using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 基于公开样式接口分开处理文字和表面, 不替换 Unity 的原生渲染材质. </summary>
    internal sealed partial class ToolkitTextOpacity : IDisposable
    {
        /// <summary> 文字层级透明度下限, 不包含字形抗锯齿覆盖率. </summary>
        internal const float MINIMUM_TEXT_OPACITY = Config.MIN_TEXT_OPACITY;

        private static readonly Dictionary<VisualElement, ToolkitTextOpacity> _roots = new(); // 普通桌面和独立原生菜单分别登记.
        private readonly VisualElement _root; // 浮窗日志不在这些根的层级内.
        private readonly Dictionary<VisualElement, Node> _nodes = new(); // 仅持有仍挂载的节点, 回收时恢复原样式并释放引用.
        private readonly ConditionalWeakTable<VisualElement, Node> _cache = new(); // 池项复用状态, 已删除的非池项不会被缓存阻止回收.
        private float _opacity; // 非文字目标透明度, 不施加到共同祖先上.
        private bool _disposed; // 避免销毁后的排队回调重新登记.

        /// <summary> 初始化时遍历一次, 后续由模板附加和动态内容创建入口登记. </summary>
        internal ToolkitTextOpacity(VisualElement root)
        {
            _root = root;
            _opacity = Config.PanelOpacity;
            _roots.Add(root, this);
            try { TrackTree(root); }
            catch { Dispose(); throw; }
        }

        /// <summary> 动态内容附加后只处理该分支, 已登记节点不会重复创建状态或回调. </summary>
        internal static void Track(VisualElement element)
        {
            for (var ancestor = element; ancestor != null; ancestor = ancestor.hierarchy.parent)
            {
                if (!_roots.TryGetValue(ancestor, out var owner)) continue;
                owner.TrackTree(element);
                return;
            }
        }

        /// <summary> 设置变化才更新现存界面, 不在 Tick 中扫描或重建日志. </summary>
        internal static void SetOpacity(float opacity)
        {
            foreach (var owner in _roots.Values)
            {
                if (owner._opacity == opacity) continue;
                owner._opacity = opacity;
                owner.RefreshTree(owner._root);
            }
        }

        /// <summary> 释放全部回调和内联覆盖, 不修改主题或字体资源. </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _roots.Remove(_root);
            foreach (var node in _nodes.Values) node.Dispose();
            _nodes.Clear();
        }

        // 子节点使用父级已经计算的文字层 alpha, 因而必须按父先子后的顺序登记.
        private void TrackTree(VisualElement element)
        {
            if (_disposed) return;
            if (!_nodes.ContainsKey(element))
            {
                if (!_cache.TryGetValue(element, out var node))
                {
                    node = new Node(this, element);
                    _cache.Add(element, node);
                }
                else node.Resume();
                _nodes.Add(element, node);
            }
            for (var index = 0; index < element.hierarchy.childCount; index++) TrackTree(element.hierarchy[index]);
        }

        // 透明度设置或局部淡化变化时, 只重算受影响分支的原生样式值.
        private void RefreshTree(VisualElement element)
        {
            if (_nodes.TryGetValue(element, out var node)) node.Apply();
            for (var index = 0; index < element.hierarchy.childCount; index++) RefreshTree(element.hierarchy[index]);
        }

        // 脱离的池项不占用管理器引用, 模板重挂载时由统一工厂再次登记.
        private void Release(Node node)
        {
            if (!_nodes.Remove(node.element)) return;
            node.Suspend();
        }
    }
}
