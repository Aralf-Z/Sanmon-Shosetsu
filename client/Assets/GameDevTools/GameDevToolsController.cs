using System;
using UnityEngine;
using GameConsole.Extension;
using GameConsole.UI;
using RedSaw.CommandLineInterface;

namespace GameConsole
{
    /// <summary> 启动时初始化命令系统, 首次打开时按配置创建 UI. </summary>
    [DisallowMultipleComponent]
    public sealed class GameDevToolsController : MonoBehaviour
    {
        public static GameDevToolsController Instance { get; private set; }

        public ConsoleController<LogType> Console { get; private set; }

        public MacroSystem Macros { get; private set; }

        public bool IsViewCreated => _view != null;

        public bool IsOpen => _view?.IsOpen ?? false;

        private ConsoleViewBridge _bridge; // 连接共享日志缓存, 转发输入和界面调用.

        private ToolkitView _view;

        private bool _creatingView; // 防止创建重入, 失败后下次按键可以重试.

        private bool _changingModule;

        private bool _configSavePending;

        /// <summary> 工具输出归入 Dev, type 仅控制原有颜色, 不触发 UI 加载. </summary>
        public void Print(string msg, LogType type = LogType.Log)
        {
            if (Instance != this) return;

            var color = DevelopmentLogSystem.GetHexColor(type);

            DevelopmentLogSystem.Append(new ConsoleLogRecord(msg, msg, color, ConsoleLogType.Dev));
        }

        /// <summary> 同时清空缓存与当前界面, 不触发 UI 加载. </summary>
        public void ClearOutput()
        {
            if (Instance != this) return;

            if (_bridge == null)
            {
                DevelopmentLogSystem.Clear();
            }
            else
            {
                _bridge.Clear();
            }
        }

        /// <summary> 独立停止日志采集并释放日志缓存. </summary>
        public void ShutdownLogSystem()
        {
            if (Instance != this) return;

            DevelopmentLogSystem.Shutdown();
            _view?.ResetLogViews();
        }

        /// <summary> 注册宿主持有的字体. 名称用于显示和恢复选择, 同名同资源可重复登记. </summary>
        public bool RegisterFont(string name, Font font)
        {
            if (Instance != this || _changingModule || font == null || string.IsNullOrWhiteSpace(name)) return false;

            var key = Config.EXTERNAL_FONT_PREFIX + name.Trim();
            if (!ToolkitFactory.TryRegisterFont(key, font)) return false;

            _view?.RestoreRegisteredFont(key);
            return true;
        }

        /// <summary> 首次调用时创建界面, 禁用作弊工具时不能打开. </summary>
        public bool Open()
        {
            if (!isActiveAndEnabled || _changingModule || Console == null) return false;

            if (_view == null && !_creatingView)
            {
                InitializeView();
            }

            if (_view == null) return false;

            _view.IsOpen = true;
            return true;
        }

        public void Close()
        {
            Config.FlushInputHistory();
            if (_view != null)
            {
                _view.IsOpen = false;
            }
        }

        /// <summary> 重建命令与宏服务, 保留日志并恢复窗口及日志采集. </summary>
        public bool Restart()
        {
            if (Instance != this || _changingModule || _creatingView) return false;

            var wasOpen = IsOpen;
            var wasMinimized = _view?.IsMinimized == true;

            _changingModule = true;
            try
            {
                var settingsSaved = ReleaseModule(keepRegisteredFonts: true);
                if (!InitializeModule(settingsSaved)) return false;
            }
            finally
            {
                _changingModule = false;
            }

            if (!wasOpen) return true;
            if (!Open()) return false;

            if (wasMinimized)
            {
                _view.MinimizeWindow();
            }

            return true;
        }

        /// <summary> 释放窗口、命令、宏和非日志缓存, 日志服务继续运行. </summary>
        public void Shutdown()
        {
            if (Instance != this || _changingModule || _creatingView) return;

            _changingModule = true;
            try
            {
                ReleaseModule(keepRegisteredFonts: false);
            }
            finally
            {
                _changingModule = false;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                Destroy(this);
                return;
            }

            Instance = this;
            _changingModule = true;
            try
            {
                InitializeModule(reloadSettings: true);
            }
            finally
            {
                _changingModule = false;
            }
        }

        private bool InitializeModule(bool reloadSettings)
        {
            var stage = "日志采集初始化";

            try
            {
                DevelopmentLogSystem.Initialize();
                stage = "配置加载";
                if (reloadSettings)
                {
                    Config.Reload();
                }

                _bridge = new ConsoleViewBridge();
                stage = "命令系统初始化";
                Console = new ConsoleController<LogType>(_bridge, _bridge, inputHistoryCapacity: Config.InputHistoryCapacity, commandQueryCacheCapacity: Config.CommandQueryCacheCapacity, alternativeCommandCount: Config.AlternativeCommandCount, shouldRecordFailedCommand: Config.RecordFailedCommand, outputWithTime: false, outputStackTraceOfCommandExecution: Config.OutputVmExceptionStack);
                Config.BindInputHistory(Console.History);
                stage = "宏服务初始化";
                var console = Console;

                Macros = new MacroSystem(console.commandSystem, this, message => console.Output(message, DevelopmentLogSystem.GetHexColor(LogType.Log), ConsoleLogType.Dev));
                return true;
            }
            catch (Exception exception)
            {
                Debug.Log($"[GameDevTools] 启动失败, 阶段: {stage}.\n{exception}");
                ReleaseModule(keepRegisteredFonts: true);
                return false;
            }
        }

        private bool ReleaseModule(bool keepRegisteredFonts)
        {
            var view = _view;
            var macros = Macros;

            _configSavePending |= _bridge != null;
            _view = null;
            Console = null;
            Macros = null;
            _bridge?.Dispose();
            _bridge = null;

            ReleaseService(view);
            ReleaseService(macros);
            try
            {
                if (!_configSavePending) return true;

                _configSavePending = !Config.Save();
                return !_configSavePending;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[GameDevTools] 保存模块配置失败.\n{exception}");
                return false;
            }
            finally
            {
                Config.UnbindInputHistory();
                if (!_configSavePending)
                {
                    Config.ReleaseSnapshot();
                }

                ToolkitFactory.ReleaseViewResources(keepRegisteredFonts);
            }
        }

        private static void ReleaseService(IDisposable service)
        {
            try
            {
                service?.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[GameDevTools] 释放模块服务失败.\n{exception}");
            }
        }

        // 仅处理本工具的配置按键, 不占用其他窗口的快捷键.
        private void Update()
        {
            if (_changingModule || Console == null) return;

            if (ToolShortcut.WasPressed())
            {
                if (_view?.IsMinimized == true)
                {
                    _view.RestoreWindow();
                }
                else if (IsOpen)
                {
                    if (Config.MinimizeOnShortcut)
                    {
                        _view.MinimizeWindow();
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    Open();
                }
            }

            _view?.Tick();
            Macros?.Tick();
            Console?.Update();
            Config.TickInputHistory();
        }

        // 首次打开时直接创建唯一 UI, 失败时保留命令系统和已接收的日志.
        private void InitializeView()
        {
            if (_creatingView) return;

            _creatingView = true;
            var stage = "界面资源与控件创建";
            ToolkitView candidate = null;

            try
            {
                candidate = ToolkitFactory.Create(transform);
                if (candidate == null)
                {
                    throw new InvalidOperationException("工厂或必需资源不存在.");
                }

                stage = "界面服务绑定";
                candidate.BindConsole(Console, Macros, LoadPresetIcon);
                stage = "日志界面连接";
                _bridge.Attach(candidate);
                _view = candidate;
            }
            catch (Exception exception)
            {
                _bridge.Detach();
                try
                {
                    candidate?.Dispose();
                }
                catch (Exception cleanupException)
                {
                    Debug.Log($"[GameDevTools] 清理失败 UI 时发生异常.\n{cleanupException}");
                }

                Debug.Log($"[GameDevTools] UI 初始化失败, 阶段: {stage}. 命令系统和日志缓存继续运行, 下次开关键将重试.\n{exception}");
            }
            finally
            {
                _creatingView = false;
            }
        }

        // 项目资源接入集中在总控, 通用 UI 仅接收委托且不触发远端下载.
        private static Sprite LoadPresetIcon(string asset)
        {
            return Resources.Load<Sprite>(asset);
        }

        // 禁用时仅关闭自己的窗口.
        private void OnDisable()
        {
            if (Instance == this)
            {
                Close();
            }
        }

        private void OnApplicationQuit()
        {
            if (Instance == this)
            {
                Config.FlushInputHistory();
            }
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            _changingModule = true;
            try
            {
                ReleaseModule(keepRegisteredFonts: false);
            }
            finally
            {
                Instance = null;
            }
        }
    }
}
