using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 三处搜索共用的历史入口, 保留十项视口及原有窄宽度. </summary>
    internal sealed class ToolkitSearchHistoryPopup : IDisposable
    {
        /// <summary> 当前是否显示搜索历史. </summary>
        internal bool IsOpen => _popup.IsOpen;

        private readonly ToolkitDropdownPopup _popup; // 与风格选择共用模板和滚动交互.
        private readonly Action<string> _select; // 仅填入搜索词, 不执行命令.

        /// <summary> 绑定搜索框和独立历史按钮. </summary>
        internal ToolkitSearchHistoryPopup(TextField field, Button trigger, Action<string> select)
        {
            _popup = new ToolkitDropdownPopup(trigger, field, Config.SEARCH_HISTORY_VISIBLE_ITEMS, true);
            _select = select;
        }

        /// <summary> 按当前历史建立快照, 选中后仍返回完整搜索词. </summary>
        internal void Show(IReadOnlyList<string> history)
        {
            var choices = new List<string>(history);
            _popup.Show(choices, index => _select(choices[index]), ToolkitLocalization.Get("ui.SearchHistoryPopup.Show"));
        }

        /// <summary> 关闭当前菜单, 可选择恢复搜索焦点. </summary>
        internal void Close(bool restoreFocus = false) => _popup.Close(restoreFocus);

        /// <summary> 释放弹层及其事件. </summary>
        public void Dispose() => _popup.Dispose();

        /// <summary> 搜索历史保留十项和三分之一宽度的边界规则. </summary>
        internal static Rect CalculateBounds(Rect viewport, Rect anchor, float preferredWidth, int count) => ToolkitDropdownPopup.CalculateBounds(viewport, anchor, preferredWidth, count, 10, true);
    }
}
