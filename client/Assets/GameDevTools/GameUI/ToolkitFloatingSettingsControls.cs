using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> Options 与浮标弹窗共用的滑动条绑定, 即时预览并合并保存. </summary>
    public sealed class ToolkitFloatingSettingsControls : IDisposable
    {
        private readonly ToolkitWindowSettings _settings; // 唯一的配置入口.
        private readonly Toggle _show; // 日志显示开关.
        private readonly SliderInt _count; // 日志条数.
        private readonly SliderInt _background; // 背景显示比例, 100 表示全显示, 0 表示全透明.
        private readonly SliderInt _text; // 文字显示比例, 100 表示全显示, 0 表示全透明.
        private readonly string _countTitle; // 保留 UXML 中可编辑的滑动条标题.
        private readonly string _backgroundTitle; // 背景透明度标题.
        private readonly string _textTitle; // 文字透明度标题.
        private readonly string _countKey; // UXML 绑定的语言键, 与初始数值分离.
        private readonly string _backgroundKey; // 背景透明度标题键.
        private readonly string _textKey; // 文字透明度标题键.
        private readonly List<Action> _unbind = new(); // 解除所有字段回调.
        private readonly ToolkitTextDisplay _labels = new(); // 只省略字段标题, 保留设置值与标题原文.

        /// <summary> 按前缀连接 UXML 控件, 避免弹窗与 Options 控件同名. </summary>
        public ToolkitFloatingSettingsControls(VisualElement root, ToolkitWindowSettings settings, string prefix)
        {
            _settings = settings;
            _show = ToolkitView.Require<Toggle>(root, prefix + "ShowLogs");
            _count = ToolkitView.Require<SliderInt>(root, prefix + "LogLines");
            _background = ToolkitView.Require<SliderInt>(root, prefix + "BackgroundOpacity");
            _text = ToolkitView.Require<SliderInt>(root, prefix + "TextOpacity");
            _count.lowValue = Config.MIN_FLOATING_LOG_COUNT;
            _count.highValue = Config.MAX_FLOATING_LOG_COUNT;
            _background.lowValue = _text.lowValue = Config.MIN_PERCENTAGE;
            _background.highValue = _text.highValue = Config.MAX_PERCENTAGE;
            _countKey = ToolkitLocalizedBindings.TextKey(_count.labelElement);
            _backgroundKey = ToolkitLocalizedBindings.TextKey(_background.labelElement);
            _textKey = ToolkitLocalizedBindings.TextKey(_text.labelElement);
            _countTitle = SettingTitle(_count.label);
            _backgroundTitle = SettingTitle(_background.label);
            _textTitle = SettingTitle(_text.label);
            _labels.Track(_show.labelElement);
            _labels.Track(_count.labelElement);
            _labels.Track(_background.labelElement);
            _labels.Track(_text.labelElement);
            EventCallback<ChangeEvent<bool>> changed = evt => _settings.ShowLatestLogs = evt.newValue;
            _show.RegisterValueChangedCallback(changed);
            _unbind.Add(() => _show.UnregisterValueChangedCallback(changed));
            BindInteger(_count, value => _settings.LogLineCount = value);
            BindInteger(_background, value => _settings.BackgroundOpacity = Mathf.Clamp(value, Config.MIN_PERCENTAGE, Config.MAX_PERCENTAGE) / (float)Config.MAX_PERCENTAGE);
            BindInteger(_text, value => _settings.TextOpacity = Mathf.Clamp(value, Config.MIN_PERCENTAGE, Config.MAX_PERCENTAGE) / (float)Config.MAX_PERCENTAGE);
            _settings.e_onChanged += Refresh;
            Refresh();
        }

        /// <summary> 销毁任意入口时解除设置事件. </summary>
        public void Dispose()
        {
            _settings.e_onChanged -= Refresh;
            foreach (var unbind in _unbind)
                unbind();
            _unbind.Clear();
            _labels.Dispose();
        }

        /// <summary> 由可见设置面板合并标题变化及尺寸变化. </summary>
        internal void Tick() => _labels.Tick();

        /// <summary> 不更改配置, 重新解析三项滑动条的完整标题. </summary>
        internal void RefreshLanguage() => Refresh();

        // 使用无通知同步, 不因配置刷新再次写入 JSON.
        private void Refresh()
        {
            _show.SetValueWithoutNotify(_settings.ShowLatestLogs);
            _count.SetValueWithoutNotify(_settings.LogLineCount);
            _background.SetValueWithoutNotify(Mathf.RoundToInt(_settings.BackgroundOpacity * Config.MAX_PERCENTAGE));
            _text.SetValueWithoutNotify(Mathf.RoundToInt(_settings.TextOpacity * Config.MAX_PERCENTAGE));
            _labels.SetText(_count.labelElement, ToolkitLocalization.Format("setting.value", ("name", (object)Title(_countKey, _countTitle)), ("value", (object)_count.value)));
            _labels.SetText(_background.labelElement, ToolkitLocalization.Format("setting.percentValue", ("name", (object)Title(_backgroundKey, _backgroundTitle)), ("value", (object)_background.value)));
            _labels.SetText(_text.labelElement, ToolkitLocalization.Format("setting.percentValue", ("name", (object)Title(_textKey, _textTitle)), ("value", (object)_text.value)));
            _count.SetEnabled(_settings.ShowLatestLogs);
            _background.SetEnabled(_settings.ShowLatestLogs);
            _text.SetEnabled(_settings.ShowLatestLogs);
        }

        // 仅保留标题, 兼容旧范围文字与 UXML 中的初始显示值.
        private static string SettingTitle(string label)
        {
            var title = (label ?? string.Empty).Split(':', '：')[0].Trim();
            return Regex.Replace(title, @"\s*[\(（]\s*\d+\s*[-~～]\s*\d+\s*[\)）]\s*$", string.Empty).TrimEnd();
        }

        // 没有本地化键的自定义字段沿用作者原始标题.
        private static string Title(string key, string fallback) => key == null ? fallback : SettingTitle(ToolkitLocalization.Get(key));

        // 拖动时立即更新预览, 磁盘写入由设置模型合并处理.
        private void BindInteger(SliderInt field, Action<int> setter)
        {
            EventCallback<ChangeEvent<int>> changed = evt => { setter(evt.newValue); Refresh(); };
            field.RegisterValueChangedCallback(changed);
            _unbind.Add(() => field.UnregisterValueChangedCallback(changed));
        }
    }
}
