using System;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 三处搜索共用的历史入口, 只在用户主动打开时显示已用搜索词. </summary>
    internal sealed class ToolkitSearchField : IDisposable
    {
        private readonly TextField _field; // 原生输入控件, 保留输入法和编辑行为.

        private readonly Button _history;

        private readonly Label _txtPlaceholder;

        private readonly IVisualElementScheduledItem _placeholderUpdate;

        private ToolkitSearchHistoryPopup _popup;

        private bool _disposed;

        private bool _pending; // 只提交编辑后的词, 未修改的输入不会因反复失焦重新记录.

        internal ToolkitSearchField(TextField field, Button history)
        {
            _field = field;
            _history = history;
            _field.AddToClassList("toolkit-search-field");
            _txtPlaceholder = new Label { pickingMode = PickingMode.Ignore, focusable = false, enableRichText = false };
            _txtPlaceholder.AddToClassList("toolkit-search-placeholder");
            var input = _field.Q(className: "unity-text-input") ?? _field;

            input.Add(_txtPlaceholder);
            _placeholderUpdate = _field.schedule.Execute(UpdatePlaceholder).Every(16);
            ToolkitLocalization.e_onChanged += RefreshLanguage;
            _field.RegisterCallback<FocusInEvent>(OnFocusIn);
            _field.RegisterCallback<FocusOutEvent>(OnFocusOut);
            _field.RegisterValueChangedCallback(OnChanged);
            _field.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _history.clicked += OpenHistory;
            RefreshLanguage();
        }

        internal void Hide()
        {
            Commit();
            _popup?.Close();
        }

        public void Dispose()
        {
            Hide();
            _disposed = true;
            _popup?.Dispose();
            _popup = null;
            _history.clicked -= OpenHistory;
            _placeholderUpdate.Pause();
            ToolkitLocalization.e_onChanged -= RefreshLanguage;
            _txtPlaceholder.RemoveFromHierarchy();
            _field.UnregisterCallback<FocusInEvent>(OnFocusIn);
            _field.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            _field.UnregisterValueChangedCallback(OnChanged);
            _field.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void Commit()
        {
            if (!_pending || !string.IsNullOrEmpty(UnityEngine.Input.compositionString)) return;

            _pending = false;
            Config.RecordCheatSearch(_field.value);
        }

        // 标记用户实际修改, 程序静默清空预设搜索不会产生历史.
        private void OnChanged(ChangeEvent<string> evt)
        {
            _pending = true;
            UpdatePlaceholder();
        }

        private void RefreshLanguage()
        {
            // Label 的文本变更事件会冒泡到 TextField, 静默更新避免触发搜索和历史.
            ((INotifyValueChanged<string>)_txtPlaceholder).SetValueWithoutNotify(ToolkitLocalization.Get("ui.Search.placeholder"));
            UpdatePlaceholder();
        }

        // 静默清空和焦点转移也刷新覆盖层, 不修改输入值或历史.
        private void UpdatePlaceholder()
        {
            if (_disposed) return;

            var focused = _field.focusController?.focusedElement as VisualElement;
            var show = string.IsNullOrEmpty(_field.value) && focused != _field && (focused == null || !_field.Contains(focused));

            _txtPlaceholder.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnFocusIn(FocusInEvent evt) => _txtPlaceholder.style.display = DisplayStyle.None;

        private void OnFocusOut(FocusOutEvent evt)
        {
            if (evt.relatedTarget is VisualElement next && _field.Contains(next)) return;

            Commit();
            _field.schedule.Execute(UpdatePlaceholder);
        }

        // 输入法组合期间让原生文本框处理确认, 避免将半成品存入历史.
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != UnityEngine.KeyCode.Return && evt.keyCode != UnityEngine.KeyCode.KeypadEnter) return;

            if (!string.IsNullOrEmpty(UnityEngine.Input.compositionString)) return;

            Commit();
            evt.StopPropagation();
        }

        private void OpenHistory()
        {
            Commit();
            if (_disposed) return;

            _popup ??= new ToolkitSearchHistoryPopup(_field, _history, SelectHistory);
            if (_popup.IsOpen)
            {
                _popup.Close(true);
            }
            else
            {
                _popup.Show(Config.CheatSearchHistory);
            }
        }

        private void SelectHistory(string query)
        {
            if (_disposed || _field.panel == null) return;

            _field.value = query;
            Config.RecordCheatSearch(query);
            _pending = false;
        }
    }
}
