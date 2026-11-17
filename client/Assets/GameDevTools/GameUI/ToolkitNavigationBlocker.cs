using System;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 在开发工具独占的面板入口阻止导航操作, 保留鼠标和原生键盘事件. </summary>
    internal sealed class ToolkitNavigationBlocker : IDisposable
    {
        private const string _menuCloseClass = "dev-menu-close-requested"; // 仅程序发出的菜单关闭请求允许通过取消导航.

        private readonly VisualElement _documentRoot;

        private VisualElement _panelRoot; // 独占面板的根节点, 同时覆盖原生菜单和未聚焦时的导航.

        private bool _disposed; // 释放后不再绑定面板.

        internal ToolkitNavigationBlocker(VisualElement documentRoot)
        {
            _documentRoot = documentRoot ?? throw new ArgumentNullException(nameof(documentRoot));
            _documentRoot.RegisterCallback<AttachToPanelEvent>(OnAttached);
            _documentRoot.RegisterCallback<DetachFromPanelEvent>(OnDetached);
            BindPanel(_documentRoot.panel?.visualTree);
        }

        internal static void CloseMenu(GenericDropdownMenu menu)
        {
            var target = menu.contentContainer;

            target.AddToClassList(_menuCloseClass);
            using var cancel = NavigationCancelEvent.GetPooled();

            cancel.target = target;
            target.SendEvent(cancel);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _documentRoot.UnregisterCallback<AttachToPanelEvent>(OnAttached);
            _documentRoot.UnregisterCallback<DetachFromPanelEvent>(OnDetached);
            BindPanel(null);
        }

        private static void BlockNavigation(EventBase evt)
        {
            evt.PreventDefault();
            evt.StopImmediatePropagation();
        }

        // 程序关闭菜单不属于输入导航, 仅放行对应目标的一次取消请求.
        private static void OnNavigationCancel(NavigationCancelEvent evt)
        {
            if (evt.target is VisualElement target && target.ClassListContains(_menuCloseClass))
            {
                target.RemoveFromClassList(_menuCloseClass);
                return;
            }

            BlockNavigation(evt);
        }

        private void BindPanel(VisualElement root)
        {
            if (ReferenceEquals(_panelRoot, root)) return;

            if (_panelRoot != null)
            {
                _panelRoot.UnregisterCallback<NavigationMoveEvent>(BlockNavigation, TrickleDown.TrickleDown);
                _panelRoot.UnregisterCallback<NavigationSubmitEvent>(BlockNavigation, TrickleDown.TrickleDown);
                _panelRoot.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel, TrickleDown.TrickleDown);
            }

            _panelRoot = root;
            if (_panelRoot == null) return;

            _panelRoot.RegisterCallback<NavigationMoveEvent>(BlockNavigation, TrickleDown.TrickleDown);
            _panelRoot.RegisterCallback<NavigationSubmitEvent>(BlockNavigation, TrickleDown.TrickleDown);
            _panelRoot.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel, TrickleDown.TrickleDown);
        }

        // 文档延迟挂载或重新附加后, 仍在整个独占面板入口拦截.
        private void OnAttached(AttachToPanelEvent evt)
        {
            if (!_disposed)
            {
                BindPanel(evt.destinationPanel?.visualTree);
            }
        }

        private void OnDetached(DetachFromPanelEvent evt) => BindPanel(null);
    }
}
