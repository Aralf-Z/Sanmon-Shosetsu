using System;
using UnityEngine;

namespace GameConsole.UI
{
    /// <summary> 通过公共 Config 持久化界面及日志预览设置, 同时支持 Editor 和 Player. </summary>
    public sealed class ToolkitWindowSettings : IDisposable
    {
        /// <summary> Inspector 或运行时修改设置后刷新预览和控件. </summary>
        public event Action e_onChanged;
        /// <summary> 整体界面不透明度, 即时预览并合并保存, 浮窗日志保持独立. </summary>
        public float PanelOpacity
        {
            get => Config.PanelOpacity;
            set
            {
                var previous = Config.PanelOpacity;
                Config.PanelOpacity = value;
                if (previous != Config.PanelOpacity) MarkChanged();
            }
        }
        /// <summary> Cheat 页最新日志行数, 零隐藏预览, 两个设置入口即时同步. </summary>
        public int CheatLogLineCount
        {
            get => Config.CheatLogLineCount;
            set
            {
                value = Mathf.Clamp(value, Config.MIN_CHEAT_LOG_COUNT, Config.MAX_CHEAT_LOG_COUNT);
                if (value == CheatLogLineCount)
                    return;
                Config.CheatLogLineCount = value;
                MarkChanged();
            }
        }
        /// <summary> 浮标日志显示开关, 即时应用. </summary>
        public bool ShowLatestLogs
        {
            get => Config.FloatingShowLatestLogs;
            set { if (value == ShowLatestLogs) return; Config.FloatingShowLatestLogs = value; MarkChanged(); }
        }
        /// <summary> 浮标日志条数, 即时应用. </summary>
        public int LogLineCount
        {
            get => Config.FloatingLogLineCount;
            set { if (value == LogLineCount) return; Config.FloatingLogLineCount = value; MarkChanged(); }
        }
        /// <summary> 背景 Alpha, UI 百分比直接对应此值, 1 表示全显示. </summary>
        public float BackgroundOpacity
        {
            get => Config.FloatingLogBackgroundOpacity;
            set { if (value == BackgroundOpacity) return; Config.FloatingLogBackgroundOpacity = value; MarkChanged(); }
        }
        /// <summary> 文字 Alpha, UI 百分比直接对应此值, 1 表示全显示. </summary>
        public float TextOpacity
        {
            get => Config.FloatingLogTextOpacity;
            set { if (value == TextOpacity) return; Config.FloatingLogTextOpacity = value; MarkChanged(); }
        }

        private bool _savePending; // 连续拖动只合并为一次配置写入.
        private float _saveAfterTime; // 停止修改后延迟保存, 使用真实时间.

        /// <summary> 视图构建成功后再连接全局事件, 避免构造失败遗留订阅. </summary>
        public void Bind() => Config.e_onChanged += OnChanged;

        /// <summary> 连续修改结束后再写磁盘, 不影响即时预览. </summary>
        public void Tick()
        {
            if (_savePending && Time.realtimeSinceStartup >= _saveAfterTime)
                Save();
        }

        /// <summary> 关闭弹窗或退出时立即提交剩余修改. </summary>
        public void Save()
        {
            if (!_savePending)
                return;
            if (Config.Save())
                _savePending = false;
            else
                _saveAfterTime = Time.realtimeSinceStartup + Config.CONFIG_SAVE_RETRY_DELAY;
        }

        /// <summary> 解除配置订阅, 避免退出 Play 后持有旧窗口. </summary>
        public void Dispose() { Save(); Config.e_onChanged -= OnChanged; e_onChanged = null; }

        // 内存变化先通知预览与另一组滑动条, 推迟实际写入.
        private void MarkChanged()
        {
            _savePending = true;
            _saveAfterTime = Time.realtimeSinceStartup + Config.CONFIG_SAVE_DELAY;
            e_onChanged?.Invoke();
        }

        // 公共配置保存或重载后无需重复写入, 继续刷新两种入口.
        private void OnChanged()
        {
            _savePending = false;
            e_onChanged?.Invoke();
        }
    }
}
