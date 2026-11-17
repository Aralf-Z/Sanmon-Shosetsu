using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> Cheat 底部读取共享的最新日志缓存, 两个行数入口即时同步并持久化. </summary>
    internal sealed class ToolkitCheatLogPreview : IDisposable
    {
        private static readonly Regex _lineBreaks = new(@"<br\s*/?>|[\r\n\v\f\u2028\u2029]", RegexOptions.IgnoreCase | RegexOptions.Compiled); // 预览强制单行, 保留颜色标签.

        private readonly VisualElement _root;

        private readonly Label[] _lines = new Label[Config.MAX_CHEAT_LOG_COUNT];

        private readonly SliderInt _cheatCount;

        private readonly SliderInt _optionCount;

        private readonly string _cheatTitle;

        private readonly string _cheatTitleKey;

        private readonly ToolkitConsoleRenderer _renderer;

        private readonly ToolkitWindowSettings _settings;

        private readonly ToolkitTextDisplay _text = new();

        private int _lineCount = -1; // 上次应用的行数, -1 确保首次读取零值时也应用隐藏状态.

        private int _logVersion = -1;

        private bool _visible;

        private bool _disposed; // 释放后不再响应设置或日志.

        internal ToolkitCheatLogPreview(VisualElement root, ToolkitConsoleRenderer renderer, ToolkitWindowSettings settings)
        {
            _root = ToolkitView.Require<VisualElement>(root, "cheatLogPreview");
            _cheatCount = ToolkitView.Require<SliderInt>(root, "cheatLogLineCount");
            _optionCount = ToolkitView.Require<SliderInt>(root, "cheatPreviewLogLines");
            _cheatCount.lowValue = _optionCount.lowValue = Config.MIN_CHEAT_LOG_COUNT;
            _cheatCount.highValue = _optionCount.highValue = Config.MAX_CHEAT_LOG_COUNT;
            _renderer = renderer;
            _settings = settings;
            _cheatTitle = _cheatCount.label;
            _cheatTitleKey = ToolkitLocalizedBindings.TextKey(_cheatCount.labelElement);
            for (var i = 0; i < _lines.Length; i++)
            {
                _lines[i] = ToolkitView.Require<Label>(root, "cheatLatestLog" + i);
                _text.Track(_lines[i]);
            }

            _cheatCount.RegisterValueChangedCallback(OnCountChanged);
            _optionCount.RegisterValueChangedCallback(OnCountChanged);
            _settings.e_onChanged += RefreshSettings;
            RefreshSettings();
            SetVisible(false);
        }

        internal void SetVisible(bool visible)
        {
            if (_disposed) return;

            _root.EnableInClassList("is-hidden", !visible || _lineCount == 0);
            if (_visible == visible) return;

            _visible = visible;
            if (visible)
            {
                _logVersion = -1;
                RefreshLogs();
            }
        }

        internal void Tick()
        {
            if (_disposed) return;

            if (_visible && _lineCount > 0)
            {
                RefreshLogs();
                _text.Tick();
            }
        }

        /// <summary> 保留日志预览的数据和位置, 只更新空提示与行数标题. </summary>
        internal void RefreshLanguage()
        {
            RefreshSettings();
            _logVersion = -1;
            if (_visible)
            {
                RefreshLogs();
            }
        }

        internal static string PreviewLine(IReadOnlyList<string> logs, int lineCount, int index)
        {
            lineCount = Mathf.Clamp(lineCount, Config.MIN_CHEAT_LOG_COUNT, Config.MAX_CHEAT_LOG_COUNT);
            if (lineCount == 0) return string.Empty;

            var count = logs?.Count ?? 0;

            if (index < 0 || index >= Math.Max(1, Math.Min(count, lineCount))) return string.Empty;

            if (count == 0) return ToolkitLocalization.Get("ui.CheatLogPreview.PreviewLine");

            return _lineBreaks.Replace(logs[Math.Max(0, count - lineCount) + index] ?? string.Empty, " ");
        }

        /// <summary> 解除双向控件和配置事件, 不接管共享设置对象的生命周期. </summary>
        public void Dispose()
        {
            if (_disposed) return;

            SetVisible(false);
            _disposed = true;
            _cheatCount.UnregisterValueChangedCallback(OnCountChanged);
            _optionCount.UnregisterValueChangedCallback(OnCountChanged);
            _settings.e_onChanged -= RefreshSettings;
            _text.Dispose();
        }

        private void OnCountChanged(ChangeEvent<int> evt)
        {
            if (!_disposed)
            {
                _settings.CheatLogLineCount = evt.newValue;
            }
        }

        // 无通知同步避免互相触发回调, 浮标行数保持独立.
        private void RefreshSettings()
        {
            if (_disposed) return;

            var value = _settings.CheatLogLineCount;

            _cheatCount.SetValueWithoutNotify(value);
            _optionCount.SetValueWithoutNotify(value);
            _cheatCount.label = ToolkitLocalization.Format("setting.value", ("name", (object)(_cheatTitleKey == null ? _cheatTitle : ToolkitLocalization.Get(_cheatTitleKey).Split(':', '：')[0])), ("value", (object)value));
            _optionCount.label = ToolkitLocalization.Format("ui.CheatLogPreview.RefreshSettings", ("p0", (object)(value)));
            if (_lineCount == value) return;

            _lineCount = value;
            _root.EnableInClassList("is-hidden", !_visible || _lineCount == 0);
            _logVersion = -1;
            if (_visible)
            {
                RefreshLogs();
            }
        }

        // 清空输出也会改变版本号, 保证旧日志不会残留在预览中.
        private void RefreshLogs()
        {
            if (_lineCount == 0 || _logVersion == _renderer.LogVersion) return;

            _logVersion = _renderer.LogVersion;
            var logs = _renderer.LatestLines;
            var count = Math.Max(1, Math.Min(logs.Count, _lineCount));

            for (var i = 0; i < _lines.Length; i++)
            {
                _lines[i].EnableInClassList("is-hidden", i >= count);
                _text.SetText(_lines[i], PreviewLine(logs, _lineCount, i));
            }
        }
    }
}
