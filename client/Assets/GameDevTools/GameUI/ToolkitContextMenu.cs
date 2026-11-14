using System;
using System.Collections.Generic;
using System.Linq;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 三类按钮共用的运行时右键菜单, 功能顺序固定为置顶后复制. </summary>
    internal sealed class ToolkitContextMenu : IDisposable
    {
        private readonly VisualElement _owner; // 不随按钮池回收的操作调度宿主.
        private readonly Dictionary<VisualElement, Binding> _bindings = new(); // 每个按钮只绑定一次右键事件.
        private GenericDropdownMenu _menu; // 当前打开的唯一菜单.
        private VisualElement _source; // 当前菜单来源, 回收时关闭.
        private VisualElement _menuRoot; // 原生菜单位于统一界面层之外, 单独同步相同透明度.
        private bool _disposed; // 丢弃销毁前排队的菜单操作.

        /// <summary> 创建当前 Cheat 页共享的菜单服务. </summary>
        internal ToolkitContextMenu(VisualElement owner)
        {
            _owner = owner;
        }

        /// <summary> 命令复制参数名称模板, 同时保留命令置顶操作. </summary>
        internal void AttachCommand(VisualElement target, Func<CommandWrapper> getCommand, Action beforeOpen = null)
        {
            Attach(target, () =>
            {
                var command = getCommand();
                return command == null ? null : new MenuData { pinId = command.Command.name, copyText = command.BuildCommandTemplate() };
            }, beforeOpen);
        }

        /// <summary> 宏复制正文, 同时保留宏置顶操作. </summary>
        internal void AttachMacro(VisualElement target, Func<Macro> getMacro, Action beforeOpen = null)
        {
            Attach(target, () =>
            {
                var macro = getMacro();
                return macro == null ? null : new MenuData { pinId = macro.Id, macros = true, copyText = macro.macro ?? string.Empty };
            }, beforeOpen);
        }

        /// <summary> 预设只提供复制, 内容与控制台实际提交文本共用生成方法. </summary>
        internal void AttachPreset(VisualElement target, Func<CommandWrapper> getCommand, Func<ParamDefine> getPreset)
        {
            Attach(target, () =>
            {
                var command = getCommand();
                var preset = getPreset();
                return command == null || preset == null ? null : new MenuData { copyText = command.BuildCommandInput(preset.param) };
            }, null);
        }

        /// <summary> 来源数据变化时关闭菜单并使排队中的旧操作失效. </summary>
        internal void Recycle(VisualElement target)
        {
            if (_bindings.TryGetValue(target, out var binding)) binding.revision++;
            if (_source == target) Close();
        }

        /// <summary> 控件永久销毁时解除绑定. </summary>
        internal void Detach(VisualElement target)
        {
            Recycle(target);
            if (!_bindings.TryGetValue(target, out var binding)) return;
            target.UnregisterCallback(binding.rightClick, TrickleDown.TrickleDown);
            _bindings.Remove(target);
        }

        /// <summary> 切页, 隐藏窗口或更换来源时关闭菜单和遮罩. </summary>
        internal void Close()
        {
            if (_menu == null) return;
            ToolkitNavigationBlocker.CloseMenu(_menu);
        }

        /// <summary> 注销所有按钮, 丢弃尚未提交的操作. </summary>
        public void Dispose()
        {
            _disposed = true;
            Close();
            foreach (var target in _bindings.Keys.ToArray()) Detach(target);
        }

        // 池项回调在交互时读取当前数据, 不捕获首次创建时的命令或预设.
        private void Attach(VisualElement target, Func<MenuData> getData, Action beforeOpen)
        {
            var binding = new Binding();
            binding.rightClick = evt =>
            {
                if (_disposed || evt.button != 1 || target.panel == null) return;
                var data = getData();
                if (data == null) return;
                Close();
                beforeOpen?.Invoke();
                Show(target, binding, data, evt.position);
                evt.StopImmediatePropagation();
            };
            target.RegisterCallback(binding.rightClick, TrickleDown.TrickleDown);
            _bindings.Add(target, binding);
        }

        // 菜单项及其添加顺序在这里固定, 不依赖注册顺序或排序配置.
        private void Show(VisualElement target, Binding binding, MenuData data, Vector2 position)
        {
            var menu = new GenericDropdownMenu();
            var revision = binding.revision;
            _menu = menu;
            _source = target;
            if (!string.IsNullOrEmpty(data.pinId))
            {
                var pinned = Config.GetPins(data.macros).Contains(data.pinId);
                menu.AddItem(pinned ? ToolkitLocalization.Get("ui.ContextMenu.Show") : ToolkitLocalization.Get("ui.ContextMenu.Show.2"), false, () => Queue(target, binding, revision, () => Config.SetPinned(data.pinId, data.macros, !pinned)));
            }
            menu.AddItem(ToolkitLocalization.Get("ui.ContextMenu.Show.3"), false, () => Queue(target, binding, revision, () => GUIUtility.systemCopyBuffer = data.copyText));
            _menuRoot = ToolkitFactory.PrepareMenu(menu);
            _menuRoot?.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (!ReferenceEquals(_menu, menu)) return;
                _menu = null;
                _source = null;
                _menuRoot = null;
            });
            menu.DropDown(new Rect(position, Vector2.zero), target, false);
        }

        // 菜单先正常关闭再提交操作, 列表重排不会打断控件清理或作用到已回收的来源.
        private void Queue(VisualElement target, Binding binding, int revision, Action action)
        {
            _owner.schedule.Execute(() =>
            {
                if (!_disposed && _bindings.TryGetValue(target, out var current) && ReferenceEquals(binding, current) && revision == current.revision)
                    action();
            });
        }

        private sealed class Binding
        {
            /// <summary> 按钮回收前后区分排队操作. </summary>
            internal int revision;
            /// <summary> 解绑时使用的右键事件回调. </summary>
            internal EventCallback<PointerUpEvent> rightClick;
        }

        private sealed class MenuData
        {
            /// <summary> 空表示该按钮不提供置顶功能. </summary>
            internal string pinId;
            /// <summary> 置顶记录是否属于宏列表. </summary>
            internal bool macros;
            /// <summary> 右键打开时取得的完整复制文本. </summary>
            internal string copyText;
        }
    }
}
