using System;
using System.Linq;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitOptionsPanel
    {
        private ToolkitDropdownPopup _languageMenu; // 与风格和字体共用的五项滚动选择器.

        /// <summary> 语言变更不重新赋值输入框, 只刷新显示和设置标题. </summary>
        internal void RefreshLanguage()
        {
            foreach (var refresh in _refresh) refresh();
            _floatingSettings.RefreshLanguage();
            _historyWindow?.Refresh();
        }

        // 语言菜单显示 Schema 中的原生语言名称, 保存代码而非标题.
        private void BindLanguage(VisualElement root)
        {
            var field = ToolkitView.Require<Button>(root, "uiLanguage");
            var title = ToolkitView.Require<Label>(root, "uiLanguageName");
            _text.Track(field.parent.Q<Label>(className: "unity-base-field__label"));
            _text.Track(title);
            Action refresh = () => _text.SetText(title, ToolkitLocalization.Languages.First(entry => entry.code == ToolkitLocalization.ActiveLanguage).name);
            refresh();
            _refresh.Add(refresh);
            Action click = () =>
            {
                if (_appearance.IsChanging) return;
                if (_languageMenu?.IsOpen == true) { _languageMenu.Close(true); return; }
                _languageMenu ??= new ToolkitDropdownPopup(field, field, Config.SELECTOR_VISIBLE_ITEMS);
                CloseSelectionMenus();
                var languages = ToolkitLocalization.Languages;
                _languageMenu.Show(languages.Select(entry => entry.name).ToArray(), index =>
                {
                    if (_visible && !_appearance.IsChanging) _appearance.RequestLanguage(languages[index].code);
                });
            };
            field.clicked += click;
            _unbind.Add(() => field.clicked -= click);
        }
    }
}
