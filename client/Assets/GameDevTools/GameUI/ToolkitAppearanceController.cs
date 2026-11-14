using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 分帧准备外观资源, 就绪后原位提交, 不重建控件或业务状态. </summary>
    public sealed class ToolkitAppearanceController : IDisposable
    {
        /// <summary> 资源准备与布局期间锁住风格, 字体和语言三个选择入口. </summary>
        public bool IsChanging { get; private set; }
        /// <summary> 切换开始, 完成或失败时通知选择控件. </summary>
        public event Action<bool> e_onChanging;

        private readonly VisualElement _root; // 包含浮窗日志和所有普通界面的文档根.
        private readonly PanelSettings _panel; // 当前窗口独占的主题设置, 不创建新面板.
        private IEnumerator _preparation; // 只在主线程逐帧推进资源请求.
        private object _waiting; // 等待当前异步请求完成, 不阻塞主线程.
        private ToolkitAppearanceResources _prepared; // 所有必需资源准备好后才允许提交.
        private ToolkitLanguagePack _preparedLanguage; // 语言切换只准备文本包, 不重新加载风格.
        private int _settleFrame = -1; // 提交后等待两帧样式与文本布局, 再恢复选择入口.
        private bool _disposed; // 释放后忽略尚未完成的资源加载.

        /// <summary> 绑定现有文档, 切换不影响窗口位置, 草稿或日志接收. </summary>
        internal ToolkitAppearanceController(VisualElement root, PanelSettings panel)
        {
            _root = root;
            _panel = panel;
        }

        /// <summary> 请求新风格, 未准备好前继续显示当前界面. </summary>
        internal bool RequestStyle(string path) => Request(path, ToolkitFactory.ActiveFont);

        /// <summary> 请求新字体, 继续使用当前风格. </summary>
        internal bool RequestFont(string path) => Request(ToolkitFactory.ActiveStyle, path);

        /// <summary> 与风格和字体共用切换锁, 提交前继续使用原语言. </summary>
        internal bool RequestLanguage(string code)
        {
            if (_disposed || IsChanging) return false;
            code = ToolkitLocalization.Resolve(code);
            if (code == ToolkitLocalization.ActiveLanguage) return false;
            _preparedLanguage = null;
            _preparation = ToolkitLocalization.PrepareAsync(code, pack => _preparedLanguage = pack);
            IsChanging = true;
            e_onChanging?.Invoke(true);
            return true;
        }

        /// <summary> 每帧只推进一次资源准备, 窗口隐藏时也允许完成切换. </summary>
        internal void Tick()
        {
            if (_disposed || !IsChanging) return;
            if (_settleFrame >= 0)
            {
                if (Time.frameCount >= _settleFrame) Finish();
                return;
            }
            var committed = false;
            try
            {
                if (_waiting is AsyncOperation operation && !operation.isDone) return;
                if (_preparation.MoveNext())
                {
                    _waiting = _preparation.Current;
                    return;
                }
                (_preparation as IDisposable)?.Dispose();
                _preparation = null;
                _waiting = null;
                if (_preparedLanguage != null)
                {
                    ToolkitLocalization.Commit(_preparedLanguage);
                    Config.UILanguage = ToolkitLocalization.ActiveLanguage;
                }
                else
                {
                    if (_prepared == null) throw new InvalidOperationException(ToolkitLocalization.Get("ui.AppearanceController.Tick"));
                    ToolkitFactory.CommitAppearance(_prepared, _root, _panel);
                }
                committed = true;
                _prepared = null;
                _preparedLanguage = null;
                Config.UIStyle = ToolkitFactory.ActiveStyle;
                Config.UIFont = ToolkitFactory.ActiveFont;
                Config.Save();
                _settleFrame = Time.frameCount + 2;
            }
            catch (Exception exception)
            {
                Finish();
                var message = committed ? ToolkitLocalization.Get("localization.switch.saved_failed") : ToolkitLocalization.Get("localization.switch.failed");
                Debug.LogWarning($"[GameDevTools] {message} {exception.GetBaseException().Message}");
            }
        }

        /// <summary> 丢弃未提交结果, 不允许异步请求在界面销毁后修改外观. </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Finish();
            e_onChanging = null;
        }

        // 同一时刻只允许一项切换, 不把选择提前保存到配置.
        private bool Request(string style, string font)
        {
            if (_disposed || IsChanging) return false;
            style = ToolkitFactory.ResolveStylePath(style);
            font = ToolkitFactory.ResolveFontPath(font);
            if (style == ToolkitFactory.ActiveStyle && font == ToolkitFactory.ActiveFont) return false;
            _prepared = null;
            _preparedLanguage = null;
            _waiting = null;
            _settleFrame = -1;
            _preparation = ToolkitFactory.PrepareAppearance(style, font, resources => _prepared = resources);
            IsChanging = true;
            e_onChanging?.Invoke(true);
            return true;
        }

        // 成功, 取消和失败都对称释放准备状态与按钮锁.
        private void Finish()
        {
            (_preparation as IDisposable)?.Dispose();
            _preparation = null;
            _prepared = null;
            _preparedLanguage = null;
            _waiting = null;
            _settleFrame = -1;
            var notify = IsChanging;
            IsChanging = false;
            if (notify) e_onChanging?.Invoke(false);
        }
    }
}
