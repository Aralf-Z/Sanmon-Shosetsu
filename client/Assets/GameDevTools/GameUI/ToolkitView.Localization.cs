using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitView
    {
        private Label _shortcutText;

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

        private void RefreshShortcutSettings()
        {
            if (_disposed) return;

            var snapshot = (Config.KeyCode, Config.KeyModifier, Config.MinimizeOnShortcut);

            if (_shortcutSettings.HasValue && _shortcutSettings.Value == snapshot) return;

            var refreshHelp = _shortcutSettings.HasValue;

            _shortcutSettings = snapshot;
            RefreshShortcutHint();
            if (refreshHelp)
            {
                _help.RefreshLanguage();
            }
        }

        private void RefreshShortcutHint() => _text.SetText(_shortcutText, ToolkitLocalization.Format("ui.View.labels", ("p0", (object)ToolShortcut.DisplayName), ("p1", (object)ToolkitLocalization.Get(Config.MinimizeOnShortcut ? "options.shortcut.action.minimize" : "options.shortcut.action.close"))));
    }
}
