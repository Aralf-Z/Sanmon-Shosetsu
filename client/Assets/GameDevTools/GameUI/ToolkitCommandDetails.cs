using System;
using System.Collections.Generic;
using System.Linq;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 在主窗口外侧统一显示命令, 参数预设与宏的字段信息, 缓存图标解析结果. </summary>
    internal sealed class ToolkitCommandDetails
    {
        private readonly VisualElement _root; // 页签下方的独立详情卡片.
        private readonly ToolkitDetailsLayout _layout; // 普通内容容器按主框底边计算高度并省略.
        private readonly Image _icon; // 预设图标, 缺失时隐藏.
        private readonly Label _name; // 不截断的完整名称.
        private readonly Label _kind; // 命令与参数预设的来源标识.
        private readonly Label _body; // 原始元数据的可读说明.
        private readonly Dictionary<ParamDefine, Sprite> _icons = new(); // 成功和失败结果均缓存到工具销毁.
        private Func<string, Sprite> _iconLoader; // 宿主可注入项目资源加载, 通用 UI 不认识资源框架.
        private CommandWrapper _selected; // 指针移出按钮后恢复的当前命令.
        private object _hovered; // 防止旧按钮退出事件覆盖新按钮的详情.
        private bool _visible; // 仅 Cheat 页面可见时显示.

        /// <summary> 连接可由 UI Builder 编辑的详情模板. </summary>
        internal ToolkitCommandDetails(VisualElement root)
        {
            _root = ToolkitView.Require<VisualElement>(root, "commandHoverDetails");
            _icon = ToolkitView.Require<Image>(root, "hoverDetailIcon");
            _name = ToolkitView.Require<Label>(root, "hoverDetailName");
            _kind = ToolkitView.Require<Label>(root, "hoverDetailKind");
            _body = ToolkitView.Require<Label>(root, "hoverDetailBody");
            _layout = new ToolkitDetailsLayout(root);
        }

        /// <summary> 由宿主提供默认图标加载器, 预设自身的加载器仍优先. </summary>
        internal void SetIconLoader(Func<string, Sprite> iconLoader)
        {
            _iconLoader = iconLoader;
            _icons.Clear();
        }

        /// <summary> 切页或最小化时清除悬停状态. </summary>
        internal void SetVisible(bool visible)
        {
            _visible = visible;
            _hovered = null;
            _root.EnableInClassList("is-hidden", !visible);
            RenderCommand(_selected);
        }

        /// <summary> 选中命令变化后更新默认详情. </summary>
        internal void Select(CommandWrapper command)
        {
            _selected = command;
            _hovered = null;
            RenderCommand(command);
        }

        /// <summary> 命令按钮悬停只显示详情, 不切换右侧参数. </summary>
        internal void Hover(CommandWrapper command)
        {
            if (!_visible || command == null || ReferenceEquals(_hovered, command))
                return;
            _hovered = command;
            RenderCommand(command);
        }

        /// <summary> 显示预设原始参数, 标签和说明. </summary>
        internal void Hover(ParamDefine define)
        {
            if (!_visible || define == null || ReferenceEquals(_hovered, define))
                return;
            _hovered = define;
            _name.text = DisplayName(define);
            _kind.text = ToolkitLocalization.Get("ui.CommandDetails.Hover");
            _body.text = FormatFields(("ui.CommandDetails.field.parameters", define.param), ("ui.CommandDetails.field.tags", define.tag), ("ui.CommandDetails.field.description", define.desc));
            _icon.sprite = ResolveIcon(define);
            _icon.EnableInClassList("is-hidden", _icon.sprite == null);
            _layout.SetText(_name.text, _kind.text, _body.text);
        }

        /// <summary> 宏复用现有外侧详情窗, 不创建第二个悬停面板. </summary>
        internal void Hover(Macro macro)
        {
            if (!_visible || macro == null || ReferenceEquals(_hovered, macro)) return;
            _hovered = macro;
            _icon.sprite = null;
            _icon.AddToClassList("is-hidden");
            _name.text = macro.name;
            _kind.text = macro.IsBuiltIn ? ToolkitLocalization.Get("ui.CommandDetails.Hover.4") : ToolkitLocalization.Get("ui.CommandDetails.Hover.5");
            _body.text = FormatFields(("ui.CommandDetails.field.description", macro.desc), ("ui.CommandDetails.field.body", macro.macro));
            _layout.SetText(_name.text, _kind.text, _body.text);
        }

        /// <summary> 仅当前悬停来源退出或回收时恢复默认详情. </summary>
        internal void Restore(object source)
        {
            if (source == null || !ReferenceEquals(_hovered, source))
                return;
            _hovered = null;
            RenderCommand(_selected);
        }

        /// <summary> 尊重自定义加载器, 不存在的图标回退到文字且不会反复尝试. </summary>
        internal Sprite ResolveIcon(ParamDefine define)
        {
            if (_icons.TryGetValue(define, out var sprite))
                return sprite;
            try
            {
                if (!string.IsNullOrWhiteSpace(define.iconAsset))
                    sprite = define.iconLoader != null ? define.Icon : _iconLoader != null ? _iconLoader(define.iconAsset) : Resources.Load<Sprite>(define.iconAsset);
            }
            catch (Exception)
            {
                sprite = null;
            }
            _icons[define] = sprite;
            return sprite;
        }

        /// <summary> 未命名预设仍可通过原始参数辨认. </summary>
        internal static string DisplayName(ParamDefine define) => string.IsNullOrWhiteSpace(define.name) ? define.param ?? string.Empty : define.name;

        /// <summary> 加载器保留图标所有权, 此处只释放引用. </summary>
        internal void Clear()
        {
            SetVisible(false);
            _selected = null;
            _icon.sprite = null;
            _icons.Clear();
        }

        /// <summary> 可见时仅在内容和几何变化后更新详情高度. </summary>
        internal void Tick()
        {
            if (_visible)
                _layout.Tick();
        }

        /// <summary> 按原悬停来源更新文案, 不改变所选命令或参数. </summary>
        internal void RefreshLanguage()
        {
            var source = _hovered;
            _hovered = null;
            if (source is ParamDefine preset) Hover(preset);
            else if (source is Macro macro) Hover(macro);
            else if (source is CommandWrapper command) Hover(command);
            else RenderCommand(_selected);
        }

        // 参数只展示别名或声明名, 顺序沿用方法定义, 不改变执行元数据.
        private void RenderCommand(CommandWrapper command)
        {
            _icon.sprite = null;
            _icon.AddToClassList("is-hidden");
            _name.text = command?.Alias ?? ToolkitLocalization.Get("ui.CommandDetails.RenderCommand");
            _kind.text = command == null ? ToolkitLocalization.Get("ui.CommandDetails.RenderCommand.2") : ToolkitLocalization.Get("ui.CommandDetails.RenderCommand.3");
            _body.text = command == null ? string.Empty : FormatFields(("ui.CommandDetails.field.command", command.Command.name), ("ui.CommandDetails.field.tags", command.Command.tag), ("ui.CommandDetails.field.description", command.Command.description), ("ui.CommandDetails.field.parameters", string.Join(", ", command.ParameterInfos.Select(info => string.IsNullOrWhiteSpace(info.alias) ? info.name : info.alias))));
            _layout.SetText(_name.text, _kind.text, _body.text);
        }

        // 所有字段使用同一冒号格式, 空白值显示占位, 非空内容保留原始换行与空格.
        private static string FormatFields(params (string labelKey, string value)[] fields)
        {
            var empty = ToolkitLocalization.Get("ui.CommandDetails.field.empty");
            return string.Join("\n", fields.Select(field => ToolkitLocalization.Format("ui.CommandDetails.field.format", ("label", (object)ToolkitLocalization.Get(field.labelKey)), ("value", (object)(string.IsNullOrWhiteSpace(field.value) ? empty : field.value)))));
        }
    }
}
