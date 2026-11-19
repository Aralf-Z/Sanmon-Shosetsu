using System;
using UnityEngine;

namespace GameConsole.UI
{
    /// <summary> 通过公共 Config 持久化界面及日志预览设置, 同时支持 Editor 和 Player. </summary>
    public sealed class ToolkitWindowSettings : IDisposable
    {
        public event Action e_onChanged;

        public float PanelOpacity
        {
            get => Config.PanelOpacity;
            set
            {
                var previous = Config.PanelOpacity;

                Config.PanelOpacity = value;
                if (previous != Config.PanelOpacity)
                {
                    MarkChanged();
                }
            }
        }

        public float WheelScrollSensitivity
        {
            get => Config.WheelScrollSensitivity;
            set
            {
                var previous = Config.WheelScrollSensitivity;

                Config.WheelScrollSensitivity = value;
                if (previous != Config.WheelScrollSensitivity)
                {
                    MarkChanged();
                }
            }
        }

        /// <summary> Cheat 页最新日志行数, 零隐藏预览, 两个设置入口即时同步. </summary>
        public int CheatLogLineCount
        {
            get => Config.CheatLogLineCount;
            set
            {
                value = Mathf.Clamp(value, Config.MIN_CHEAT_LOG_COUNT, Config.MAX_CHEAT_LOG_COUNT);
                if (value == CheatLogLineCount) return;

                Config.CheatLogLineCount = value;
                MarkChanged();
            }
        }

        public bool ShowLatestLogs
        {
            get => Config.FloatingShowLatestLogs;
            set
            {
                if (value == ShowLatestLogs) return;

                Config.FloatingShowLatestLogs = value;
                MarkChanged();
            }
        }

        internal bool HasLogPreview => ShowLatestLogs && LogLineCount > 0;

        internal FloatingFrameMetrics FrameMetrics
        {
            get => Config.FloatingMetrics;
            set
            {
                if (FrameMetrics == value) return;

                Config.FloatingMetrics = value;
                MarkChanged();
            }
        }

        internal float MonitorBackgroundOpacity
        {
            get => Config.FloatingMonitorBackgroundOpacity;
            set
            {
                var previous = MonitorBackgroundOpacity;

                Config.FloatingMonitorBackgroundOpacity = value;
                if (previous != MonitorBackgroundOpacity)
                {
                    MarkChanged();
                }
            }
        }

        public int LogLineCount
        {
            get => Config.FloatingLogLineCount;
            set
            {
                value = Mathf.Clamp(value, Config.MIN_FLOATING_LOG_COUNT, Config.MAX_FLOATING_LOG_COUNT);
                if (value == LogLineCount) return;

                Config.FloatingLogLineCount = value;
                MarkChanged();
            }
        }

        /// <summary> 背景 Alpha, UI 百分比直接对应此值, 1 表示全显示. </summary>
        public float BackgroundOpacity
        {
            get => Config.FloatingLogBackgroundOpacity;
            set
            {
                if (value == BackgroundOpacity) return;

                Config.FloatingLogBackgroundOpacity = value;
                MarkChanged();
            }
        }

        /// <summary> 文字 Alpha, UI 百分比直接对应此值, 1 表示全显示. </summary>
        public float TextOpacity
        {
            get => Config.FloatingLogTextOpacity;
            set
            {
                if (value == TextOpacity) return;

                Config.FloatingLogTextOpacity = value;
                MarkChanged();
            }
        }

        private bool _savePending;

        private float _saveAfterTime; // 停止修改后延迟保存, 使用真实时间.

        /// <summary> 视图构建成功后再连接全局事件, 避免构造失败遗留订阅. </summary>
        public void Bind() => Config.e_onChanged += OnChanged;

        public void Tick()
        {
            if (_savePending && Time.realtimeSinceStartup >= _saveAfterTime)
            {
                Save();
            }
        }

        public void Save()
        {
            if (!_savePending) return;

            if (Config.Save())
            {
                _savePending = false;
            }
            else
            {
                _saveAfterTime = Time.realtimeSinceStartup + Config.CONFIG_SAVE_RETRY_DELAY;
            }
        }

        /// <summary> 解除配置订阅, 避免退出 Play 后持有旧窗口. </summary>
        public void Dispose()
        {
            Save();
            Config.e_onChanged -= OnChanged;
            e_onChanged = null;
        }

        private void MarkChanged()
        {
            _savePending = true;
            _saveAfterTime = Time.realtimeSinceStartup + Config.CONFIG_SAVE_DELAY;
            e_onChanged?.Invoke();
        }

        private void OnChanged()
        {
            _savePending = false;
            e_onChanged?.Invoke();
        }
    }
}
