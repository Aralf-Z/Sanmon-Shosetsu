using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 共用图标分页栏, 页号在组件内从零开始, 到达边界后再次操作才循环. </summary>
    internal sealed partial class ToolkitPager : IDisposable
    {
        private const float CONTROL_WIDTH = 32f;

        private const float CONTROL_HEIGHT = 30f;

        private const float GAP = 4f;

        private readonly VisualElement _root;

        private readonly VisualElement _navigation;

        private readonly VisualElement _jumpGroup;

        private readonly Button[] _buttons;

        private readonly Label _txtPage;

        private readonly TextField _inputJump;

        private readonly Action<int> _changePage;

        private readonly IVisualElementScheduledItem _update;

        private int _page = -1;

        private int _pages = 1;

        private int _pageCountHint = 1;

        private int _fontKey;

        private float _width = -1f;

        private bool _dirty = true;

        private bool _disposed;

        internal ToolkitPager(VisualElement root, Action<int> changePage)
        {
            _root = root;
            _changePage = changePage;
            _root.AddToClassList("unified-pager");
            _navigation = CreateGroup("pageNavigation");
            _jumpGroup = CreateGroup("pageJumpGroup");
            _buttons = new[] { CreateButton("pagePreviousTen", () => Move(-10)), CreateButton("pagePrevious", () => Move(-1)), CreateButton("pageNext", () => Move(1)), CreateButton("pageNextTen", () => Move(10)), CreateButton("pageJump", Jump) };
            _txtPage = new Label { name = "pageInfo", pickingMode = PickingMode.Ignore, enableRichText = false };
            _txtPage.AddToClassList("unified-page-info");
            _navigation.Add(_buttons[0]);
            _navigation.Add(_buttons[1]);
            _navigation.Add(_txtPage);
            _navigation.Add(_buttons[2]);
            _navigation.Add(_buttons[3]);
            _inputJump = new TextField { name = "pageJumpInput", value = "1" };
            _inputJump.AddToClassList("unified-page-input");
            _inputJump.RegisterValueChangedCallback(InputChanged);
            _inputJump.RegisterCallback<KeyDownEvent>(InputKeyDown, TrickleDown.TrickleDown);
            _inputJump.RegisterCallback<FocusOutEvent>(InputFocusOut);
            _jumpGroup.Add(_inputJump);
            _jumpGroup.Add(_buttons[4]);
            _root.Add(_navigation);
            _root.Add(_jumpGroup);
            _update = _root.schedule.Execute(Tick).Every(16);
            ToolkitLocalization.e_onChanged += RefreshLanguage;
            RefreshLanguage();
            SetPage(0, 1);
        }

        internal static int Step(int page, int pages, int delta)
        {
            pages = Math.Max(1, pages);
            page = Mathf.Clamp(page, 0, pages - 1);
            if (delta == 0 || pages == 1) return page;

            if (delta < 0 && page == 0) return pages - 1;

            if (delta > 0 && page == pages - 1) return 0;

            return (int)Math.Max(0L, Math.Min(pages - 1L, (long)page + delta));
        }

        internal void SetPage(int page, int pages, string tooltip = null)
        {
            pages = Math.Max(1, pages);
            page = Mathf.Clamp(page, 0, pages - 1);
            var changed = _page != page || _pages != pages;

            _page = page;
            _pages = pages;
            _txtPage.text = $"{page + 1} / {pages}";
            _txtPage.tooltip = tooltip ?? ToolkitLocalization.Format("ui.Pager.pageInfo", ("page", page + 1), ("pages", pages));
            foreach (var button in _buttons)
            {
                button.SetEnabled(pages > 1);
            }

            _inputJump.SetEnabled(pages > 1);
            if (changed)
            {
                ResetInput();
                _dirty = true;
            }
        }

        internal void ReservePages(int pages)
        {
            _pageCountHint = Math.Max(1, pages);
            _dirty = true;
        }

        internal void Move(int delta) => ChangePage(Step(_page, _pages, delta));

        internal bool ContainsInput(VisualElement element) => element == _inputJump || _inputJump.Contains(element);

        private static VisualElement CreateGroup(string name)
        {
            var group = new VisualElement { name = name, pickingMode = PickingMode.Ignore };

            group.AddToClassList("unified-page-group");
            return group;
        }

        private static Button CreateButton(string key, Action click)
        {
            var button = new Button(click) { name = key, enableRichText = false };

            button.AddToClassList("unified-page-button");
            return button;
        }

        private void RefreshLanguage()
        {
            foreach (var button in _buttons)
            {
                ToolkitMixedText.SetText(button, ToolkitLocalization.Get("icon." + button.name), ToolkitMixedTextMode.Control);
                button.tooltip = ToolkitLocalization.Get("ui.Pager." + button.name);
            }

            _inputJump.tooltip = ToolkitLocalization.Get("ui.Pager.input");
            _dirty = true;
        }

        private void ChangePage(int page)
        {
            if (_disposed || _pages <= 1 || page == _page) return;

            SetPage(page, _pages);
            _changePage(page);
        }

        private void ResetInput() => _inputJump.SetValueWithoutNotify((_page + 1 < _pages ? _page + 2 : 1).ToString());

        private int ReadInput()
        {
            var page = 0L;

            foreach (var digit in _inputJump.value)
            {
                page = Math.Min(_pages, page * 10 + digit - '0');
            }

            return (int)Math.Max(1, page) - 1;
        }

        private void Jump()
        {
            if (_disposed || _pages <= 1) return;

            if (!string.IsNullOrEmpty(_inputJump.value))
            {
                ChangePage(ReadInput());
            }

            ResetInput();
        }

        private void InputChanged(ChangeEvent<string> evt)
        {
            if (evt.target != _inputJump) return;

            var digits = new string((evt.newValue ?? string.Empty).Where(character => character >= '0' && character <= '9').ToArray());

            if (digits != evt.newValue)
            {
                _inputJump.SetValueWithoutNotify(digits);
            }
        }

        private void InputKeyDown(KeyDownEvent evt)
        {
            if (!string.IsNullOrEmpty(Input.compositionString)) return;

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                Jump();
                evt.StopPropagation();
                evt.PreventDefault();
            }
            else if (!evt.ctrlKey && !evt.commandKey && evt.character >= ' ' && (evt.character < '0' || evt.character > '9'))
            {
                evt.StopPropagation();
                evt.PreventDefault();
            }
        }

        private void InputFocusOut(FocusOutEvent evt)
        {
            if (!string.IsNullOrEmpty(_inputJump.value))
            {
                _inputJump.SetValueWithoutNotify((ReadInput() + 1).ToString());
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _update.Pause();
            ToolkitLocalization.e_onChanged -= RefreshLanguage;
            _inputJump.UnregisterValueChangedCallback(InputChanged);
            _inputJump.UnregisterCallback<KeyDownEvent>(InputKeyDown, TrickleDown.TrickleDown);
            _inputJump.UnregisterCallback<FocusOutEvent>(InputFocusOut);
        }
    }
}
