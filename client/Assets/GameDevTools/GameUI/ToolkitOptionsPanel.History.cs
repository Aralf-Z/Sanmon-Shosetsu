using System;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitOptionsPanel
    {
        private ToolkitHistoryWindow _searchHistoryWindow;

        private ToolkitHistoryWindow _inputHistoryWindow;

        private void BindHistoryActions(string clearName, string viewName, string heightReference, bool showInputHistory)
        {
            var clear = ToolkitView.Require<Button>(_root, clearName);
            var view = ToolkitView.Require<Button>(_root, viewName);
            var field = ToolkitView.Require<IntegerField>(_root, heightReference);
            var input = field.Q(className: BaseField<int>.inputUssClassName);
            EventCallback<GeometryChangedEvent> syncHeight = evt =>
            {
                var height = evt.newRect.height;

                if (height <= 0 || float.IsNaN(height) || float.IsInfinity(height)) return;
                clear.style.height = view.style.height = height;
            };

            input.RegisterCallback(syncHeight);
            _unbind.Add(() => input.UnregisterCallback(syncHeight));
            Action refresh = () =>
            {
                var history = showInputHistory ? Config.InputHistoryEntries : Config.CheatSearchHistory;

                clear.SetEnabled(history.Count > 0);
            };
            Action clearHistory = Config.ClearInputHistory;
            if (!showInputHistory)
            {
                clearHistory = () =>
                {
                    Config.ClearCheatSearchHistory();
                    refresh();
                    _searchHistoryWindow?.Refresh();
                };
            }

            Action viewHistory = () => ViewHistory(view, showInputHistory);

            clear.clicked += clearHistory;
            view.clicked += viewHistory;
            _unbind.Add(() => clear.clicked -= clearHistory);
            _unbind.Add(() => view.clicked -= viewHistory);
            refresh();
            if (showInputHistory)
            {
                Config.e_onInputHistoryChanged += refresh;
                _unbind.Add(() => Config.e_onInputHistoryChanged -= refresh);
            }
            else
            {
                _refresh.Add(refresh);
            }
        }

        private void ViewHistory(Button trigger, bool showInputHistory)
        {
            if (showInputHistory)
            {
                _inputHistoryWindow ??= new ToolkitHistoryWindow(_root, trigger, true);
                _inputHistoryWindow.Show();
                return;
            }

            _searchHistoryWindow ??= new ToolkitHistoryWindow(_root, trigger);
            _searchHistoryWindow.Show();
        }
    }
}

