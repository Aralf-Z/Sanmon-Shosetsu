using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 从静态文案构造可展开的帮助目录, 不依赖正文分页结果. </summary>
    internal sealed class ToolkitHelpNavigation : IDisposable
    {
        private readonly ScrollView _scroll; // 只有目录滚动, 正文继续分页.
        private readonly Action<ToolkitHelpTopic, int> _select; // 点击叶节点时跳转到对应特性.
        private readonly List<Entry> _entries = new(); // 叶节点和所属折叠分支.
        private readonly List<Foldout> _branches = new(); // 语言切换时原位更新, 保留收起状态.
        private Entry _selected; // 当前正文对应的高亮叶节点.
        private int _version; // 避免旧的延迟滚动覆盖新的选择.
        private bool _disposed; // 释放后不再执行排队回调.

        /// <summary> 按文案和分组顺序生成导航, 不在切页时重建. </summary>
        internal ToolkitHelpNavigation(ScrollView scroll, Action<ToolkitHelpTopic, int> select)
        {
            _scroll = scroll;
            _select = select;
            var groups = ToolkitHelpContents.navigation;
            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var group = groups[groupIndex];
                var indexInGroups = groupIndex;
                var branch = CreateBranch(() => ToolkitHelpContents.navigation[indexInGroups].Title);
                branch.AddToClassList("help-tree-group");
                _scroll.Add(branch);
                foreach (var topic in group.Topics)
                {
                    var section = ToolkitHelpContents.Get(topic);
                    var sectionBranch = group.Topics.Length == 1 || topic == ToolkitHelpTopic.Console ? branch : CreateBranch(() => ToolkitHelpContents.Get(topic).Title);
                    if (sectionBranch != branch) branch.Add(sectionBranch);
                    for (var index = 0; index < section.Features.Count; index++)
                    {
                        var featureIndex = index;
                        var button = new Button { text = section.Features[index].Title, tooltip = section.Features[index].Title };
                        button.AddToClassList("help-tree-entry");
                        Action click = () => _select(topic, featureIndex);
                        button.clicked += click;
                        sectionBranch.Add(button);
                        _entries.Add(new Entry(topic, index, button, branch, sectionBranch, click));
                    }
                }
            }
        }

        /// <summary> 高亮并展开目标路径, 不触发正文选择事件. </summary>
        internal void Select(ToolkitHelpTopic topic, int featureIndex)
        {
            var entry = _entries.Find(value => value.topic == topic && value.featureIndex == featureIndex);
            if (entry == null) return;
            Expand(entry.group);
            Expand(entry.section);
            if (_selected != entry)
            {
                _selected?.button.RemoveFromClassList("selected");
                _selected = entry;
                entry.button.AddToClassList("selected");
            }
            var version = ++_version;
            _scroll.schedule.Execute(() =>
            {
                if (!_disposed && version == _version && entry.button.panel != null) _scroll.ScrollTo(entry.button);
            });
        }

        /// <summary> 树节点使用左右方向键展开收起时, 不翻正文页. </summary>
        internal bool Contains(VisualElement element) => _scroll.Contains(element);

        /// <summary> 目录结构和展开状态不变, 仅替换标题. </summary>
        internal void RefreshLanguage()
        {
            foreach (var branch in _branches) RenderBranch(branch);
            foreach (var entry in _entries)
            {
                entry.button.text = ToolkitHelpContents.Get(entry.topic).Features[entry.featureIndex].Title;
                entry.button.tooltip = entry.button.text;
            }
        }

        /// <summary> 解除叶节点回调, 防止窗口销毁后保留宿主. </summary>
        public void Dispose()
        {
            _disposed = true;
            foreach (var entry in _entries) entry.button.clicked -= entry.click;
            _entries.Clear();
        }

        // 折叠控件使用原生 Toggle, 点击箭头与标题均可展开.
        private Foldout CreateBranch(Func<string> title)
        {
            var foldout = new Foldout { value = false, userData = title };
            _branches.Add(foldout);
            RenderBranch(foldout);
            foldout.AddToClassList("help-tree-branch");
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.target == foldout) RenderBranch(foldout);
            });
            return foldout;
        }

        // 无通知展开也要同步文本箭头, 不依赖各风格提供原生折叠图标.
        private static void Expand(Foldout foldout)
        {
            foldout.SetValueWithoutNotify(true);
            RenderBranch(foldout);
        }

        // 箭头和标题分别本地化, 不依赖旧语言文本识别节点.
        private static void RenderBranch(Foldout foldout)
        {
            var title = ((Func<string>)foldout.userData)();
            foldout.text = ToolkitLocalization.Get(foldout.value ? "icon.open" : "icon.collapsed") + " " + title;
            foldout.tooltip = title;
        }

        private sealed class Entry
        {
            /// <summary> 稳定主题标识. </summary>
            internal readonly ToolkitHelpTopic topic;
            /// <summary> 文案中的特性顺序. </summary>
            internal readonly int featureIndex;
            /// <summary> 可点击叶节点. </summary>
            internal readonly Button button;
            /// <summary> 一级分组. </summary>
            internal readonly Foldout group;
            /// <summary> 主题分支, 简单分组与 group 相同. </summary>
            internal readonly Foldout section;
            /// <summary> 显式解除事件使用的委托. </summary>
            internal readonly Action click;

            /// <summary> 保存导航路径与清理所需的事件委托. </summary>
            internal Entry(ToolkitHelpTopic topic, int featureIndex, Button button, Foldout group, Foldout section, Action click)
            {
                this.topic = topic;
                this.featureIndex = featureIndex;
                this.button = button;
                this.group = group;
                this.section = section;
                this.click = click;
            }
        }
    }
}
