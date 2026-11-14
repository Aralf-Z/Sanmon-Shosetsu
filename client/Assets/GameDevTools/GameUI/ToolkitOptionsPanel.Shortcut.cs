using System;
using System.Linq;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitOptionsPanel
    {
        private ToolkitDropdownPopup _shortcutModifierMenu; // 单个前置修饰键, 与风格选择使用相同的弹层.
        private ToolkitDropdownPopup _shortcutKeyMenu; // 基础键只展示受支持的键, 超过五项滚动.

        // 两个选择器构成一个开关键设置行, 不采集录制按键, 避免编辑时触发全局快捷键.
        private void BindShortcut(VisualElement root)
        {
            var modifier = ToolkitView.Require<Button>(root, "shortcutModifier");
            var key = ToolkitView.Require<Button>(root, "shortcutKey");
            var modifierTitle = ToolkitView.Require<Label>(root, "shortcutModifierName");
            var keyTitle = ToolkitView.Require<Label>(root, "shortcutKeyName");
            _text.Track(ToolkitView.Require<Label>(root, "shortcutSettingLabel"));
            modifierTitle.enableRichText = keyTitle.enableRichText = false;
            modifierTitle.parseEscapeSequences = keyTitle.parseEscapeSequences = false;
            _text.Track(modifierTitle);
            _text.Track(keyTitle);
            Action refresh = () =>
            {
                _text.SetText(modifierTitle, Config.KeyModifier == ShortcutModifier.None ? ToolkitLocalization.Get("options.shortcut.none") : Config.KeyModifier.ToString());
                _text.SetText(keyTitle, ToolShortcut.GetKeyOptionName(Config.KeyCode));
            };
            refresh();
            _refresh.Add(refresh);
            Action chooseModifier = () =>
            {
                if (_appearance.IsChanging) return;
                if (_shortcutModifierMenu?.IsOpen == true) { _shortcutModifierMenu.Close(true); return; }
                CloseSelectionMenus();
                _shortcutModifierMenu ??= new ToolkitDropdownPopup(modifier, modifier, Config.SELECTOR_VISIBLE_ITEMS);
                _shortcutModifierMenu.Show(new[] { ToolkitLocalization.Get("options.shortcut.none"), "Ctrl", "Alt", "Shift" }, index =>
                {
                    if (!_visible) return;
                    Config.KeyModifier = (ShortcutModifier)index;
                    _dirty = true;
                    Save();
                });
            };
            Action chooseKey = () =>
            {
                if (_appearance.IsChanging) return;
                if (_shortcutKeyMenu?.IsOpen == true) { _shortcutKeyMenu.Close(true); return; }
                CloseSelectionMenus();
                _shortcutKeyMenu ??= new ToolkitDropdownPopup(key, key, Config.SELECTOR_VISIBLE_ITEMS);
                _shortcutKeyMenu.Show(ToolShortcut.Keys.Select(ToolShortcut.GetKeyOptionName).ToArray(), index =>
                {
                    if (!_visible) return;
                    Config.KeyCode = ToolShortcut.Keys[index];
                    _dirty = true;
                    Save();
                });
            };
            modifier.clicked += chooseModifier;
            key.clicked += chooseKey;
            _unbind.Add(() => modifier.clicked -= chooseModifier);
            _unbind.Add(() => key.clicked -= chooseKey);
        }
    }
}
