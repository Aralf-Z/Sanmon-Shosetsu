using System;
using GameConsole.UI;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;
using UnityEngine;

namespace GameConsole
{
    /// <summary> 启动时初始化命令系统, 首次打开时按配置创建 UI. </summary>
    [DisallowMultipleComponent]
    public sealed class GameDevToolsController : MonoBehaviour
    {
        /// <summary> 当前游戏中的唯一总控. </summary>
        public static GameDevToolsController Instance { get; private set; }
        /// <summary> 开发工具使用的唯一命令控制器. </summary>
        public ConsoleController<LogType> Console { get; private set; }
        /// <summary> 宏运行和命令缓存服务. </summary>
        public MacroSystem Macros { get; private set; }
        /// <summary> 首次打开时是否已经成功创建界面. </summary>
        public bool IsViewCreated => _view != null;
        /// <summary> 整体窗口或浮标是否打开. </summary>
        public bool IsOpen => _view?.IsOpen ?? false;

        private ConsoleViewBridge _bridge; // 连接共享日志缓存, 转发输入和界面调用.
        private ToolkitView _view; // 首次打开时创建的正式 UI.
        private bool _creatingView; // 防止创建重入, 失败后下次按键可以重试.

        /// <summary> 工具输出归入 Dev, type 仅控制原有颜色, 不触发 UI 加载. </summary>
        public void Print(string msg, LogType type = LogType.Log) => Console?.Output(msg, DevelopmentLogSystem.GetHexColor(type), ConsoleLogType.Dev);

        /// <summary> 同时清空缓存与当前界面, 不触发 UI 加载. </summary>
        public void ClearOutput() => Console?.ClearOutputPanel();

        /// <summary> 首次调用时创建界面, 禁用作弊工具时不能打开. </summary>
        public bool Open()
        {
            if (!isActiveAndEnabled || Console == null) return false;
            if (_view == null && !_creatingView) InitializeView();
            if (_view == null) return false;
            _view.IsOpen = true;
            return true;
        }

        /// <summary> 关闭工具并释放界面焦点. </summary>
        public void Close()
        {
            if (_view != null) _view.IsOpen = false;
        }

        // 无需等待作弊配置打开, 启动时即准备命令系统和日志缓存.
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                Destroy(this);
                return;
            }
            Instance = this;
            var stage = "日志采集初始化";
            try
            {
                DevelopmentLogSystem.Initialize();
                stage = "配置加载";
                Config.Reload();
                _bridge = new ConsoleViewBridge();
                stage = "命令系统初始化";
                Console = new ConsoleController<LogType>(_bridge, _bridge, 
                    inputHistoryCapacity: Config.InputHistoryCapacity, 
                    commandQueryCacheCapacity: Config.CommandQueryCacheCapacity, 
                    alternativeCommandCount: Config.AlternativeCommandCount, 
                    shouldRecordFailedCommand: Config.RecordFailedCommand, 
                    outputWithTime: false, 
                    outputStackTraceOfCommandExecution: Config.OutputVmExceptionStack);
                stage = "宏服务初始化";
                Macros = new MacroSystem(Console.commandSystem, this, message => Print(message));
            }
            catch (Exception exception)
            {
                Debug.Log($"[GameDevTools] 启动失败, 阶段: {stage}.\n{exception}");
                try { Macros?.Dispose(); }
                catch (Exception cleanup) { Debug.Log($"[GameDevTools] 启动清理失败.\n{cleanup}"); }
                Macros = null;
                Console = null;
                _bridge?.Detach();
            }
        }

        // 仅处理本工具的配置按键, 不占用其他窗口的快捷键.
        private void Update()
        {
            if (ToolShortcut.WasPressed())
            {
                if (_view?.IsMinimized == true)
                {
                    _view.RestoreWindow();
                }
                else if (IsOpen)
                {
                    if (Config.MinimizeOnShortcut) _view.MinimizeWindow();
                    else Close();
                }
                else
                {
                    Open();
                }
            }

            _view?.Tick();
            Macros?.Tick();
            Console?.Update();
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
                if (candidate == null) throw new InvalidOperationException("工厂或必需资源不存在.");
                stage = "界面服务绑定";
                candidate.BindConsole(Console, Macros, LoadPresetIcon);
                stage = "日志界面连接";
                _bridge.Attach(candidate);
                _view = candidate;
            }
            catch (Exception exception)
            {
                _bridge.Detach();
                try { candidate?.Dispose(); }
                catch (Exception cleanupException) { Debug.Log($"[GameDevTools] 清理失败 UI 时发生异常.\n{cleanupException}"); }
                Debug.Log($"[GameDevTools] UI 初始化失败, 阶段: {stage}. 命令系统和日志缓存继续运行, 下次开关键将重试.\n{exception}");
            }
            finally { _creatingView = false; }
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

        // 释放窗口和宏资源, 会话日志继续由独立服务接收.
        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }
            try
            {
                try { Close(); }
                finally
                {
                    try { _view?.Dispose(); }
                    finally { Macros?.Dispose(); }
                }
            }
            finally
            {
                _bridge?.Detach();
                _view = null;
                Config.Save();
                Instance = null;
            }
        }
    }
}
