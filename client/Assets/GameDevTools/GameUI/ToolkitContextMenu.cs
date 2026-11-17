using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GameConsole.Extension;

namespace GameConsole.UI
{
    /// <summary> 三类按钮共用的运行时右键菜单, 功能顺序固定为置顶后复制. </summary>
    internal sealed class ToolkitContextMenu : IDisposable
    {
        private readonly VisualElement _owner;

        private readonly Dictionary<VisualElement, Binding> _bindings = new();

        private GenericDropdownMenu _menu;

        private VisualElement _source;

        private VisualElement _menuRoot; // 原生菜单位于统一界面层之外, 单独同步相同透明度.

        private bool _disposed;

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

                return command == null ? null : new MenuData
                { pinId = command.Command.name, copyText = command.BuildCommandTemplate() };
            }, beforeOpen);
        }

        /// <summary> 宏复制正文, 同时保留宏置顶操作. </summary>
        internal void AttachMacro(VisualElement target, Func<Macro> getMacro, Action beforeOpen = null)
        {
            Attach(target, () =>
            {
                var macro = getMacro();

                return macro == null ? null : new MenuData
                { pinId = macro.Id, macros = true, copyText = macro.macro ?? string.Empty };
            }, beforeOpen);
        }

        internal void AttachPreset(VisualElement target, Func<CommandWrapper> getCommand, Func<ParamDefine> getPreset)
        {
            Attach(target, () =>
            {
                var command = getCommand();
                var preset = getPreset();

                return command == null || preset == null ? null : new MenuData
                { copyText = command.BuildCommandInput(preset.param) };
            }, null);
        }

        internal void Recycle(VisualElement target)
        {
            if (_bindings.TryGetValue(target, out var binding))
            {
                binding.revision++;
            }

            if (_source == target)
            {
                Close();
            }
        }

        internal void Detach(VisualElement target)
        {
            Recycle(target);
            if (!_bindings.TryGetValue(target, out var binding)) return;

            target.UnregisterCallback(binding.rightClick, TrickleDown.TrickleDown);
            _bindings.Remove(target);
        }

        internal void Close()
        {
            if (_menu == null) return;

            ToolkitNavigationBlocker.CloseMenu(_menu);
        }

        public void Dispose()
        {
            _disposed = true;
            Close();
            foreach (var target in _bindings.Keys.ToArray())
            {
                Detach(target);
            }
        }

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

        private void Show(VisualElement target, Binding binding, MenuData data, Vector2 position)
        {
            var menu = new GenericDropdownMenu();
            var revision = binding.revision;

            _menu = menu;
            _source = target;
            if (!string.IsNullOrEmpty(data.pinId))
            {
                var pinned = Config.GetPins(data.macros)
                    .Contains(data.pinId);

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
                {
                    action();
                }
            });
        }

        private sealed class Binding
        {
            internal int revision;

            internal EventCallback<PointerUpEvent> rightClick;
        }

        private sealed class MenuData
        {
            internal string pinId;

            internal bool macros;

            internal string copyText;
        }
    }
}
