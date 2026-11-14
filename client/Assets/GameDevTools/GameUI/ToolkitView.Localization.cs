using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitView
    {
        private Label _shortcutText; // 动态按键提示不从已经省略的文本还原.
        private (KeyCode key, ShortcutModifier modifier, bool minimize)? _shortcutSettings; // 无关配置保存不重复更新帮助和按键提示.

        // 所有显示订阅随窗口解除, 不延长旧窗口的生命周期.
        private void InitializeLocalization(VisualElement root)
        {
            _shortcutText = Require<Label>(root, "shortcutHint");
            ToolkitLocalization.e_onChanged += RefreshLanguage;
            _unbind.Add(() => ToolkitLocalization.e_onChanged -= RefreshLanguage);
            Config.e_onChanged += RefreshShortcutSettings;
            _unbind.Add(() => Config.e_onChanged -= RefreshShortcutSettings);
            RefreshShortcutSettings();
        }

        // 只更新展示, 不切页, 不重新提交命令或写入宏输入框.
        private void RefreshLanguage()
        {
            RefreshShortcutHint();
            _renderer.RefreshLanguage();
            _cheats.RefreshLanguage();
            _macros?.RefreshLanguage();
            RefreshMacroFailure();
            _options.RefreshLanguage();
            _cheatLogs.RefreshLanguage();
            _windowController.RefreshLanguage();
            _help.RefreshLanguage();
        }

        // 修改组合键或开关行为后同步提示, 不重建窗口或修改输入内容.
        private void RefreshShortcutSettings()
        {
            if (_disposed) return;
            var snapshot = (Config.KeyCode, Config.KeyModifier, Config.MinimizeOnShortcut);
            if (_shortcutSettings.HasValue && _shortcutSettings.Value == snapshot) return;
            var refreshHelp = _shortcutSettings.HasValue;
            _shortcutSettings = snapshot;
            RefreshShortcutHint();
            if (refreshHelp) _help.RefreshLanguage();
        }

        // 关闭和最小化只影响打开窗口时的按键行为, 浮标仍始终还原.
        private void RefreshShortcutHint() => _text.SetText(_shortcutText, ToolkitLocalization.Format("ui.View.labels", ("p0", (object)ToolShortcut.DisplayName), ("p1", (object)ToolkitLocalization.Get(Config.MinimizeOnShortcut ? "options.shortcut.action.minimize" : "options.shortcut.action.close"))));
    }
}
