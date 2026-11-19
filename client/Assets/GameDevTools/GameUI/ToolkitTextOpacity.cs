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

        private static readonly Dictionary<VisualElement, ToolkitTextOpacity> _roots = new();

        private readonly VisualElement _root;

        private readonly Dictionary<VisualElement, Node> _nodes = new(); // 仅持有仍挂载的节点, 回收时恢复原样式并释放引用.

        private readonly ConditionalWeakTable<VisualElement, Node> _cache = new(); // 池项复用状态, 已删除的非池项不会被缓存阻止回收.

        private float _opacity;

        private bool _disposed; // 避免销毁后的排队回调重新登记.

        internal ToolkitTextOpacity(VisualElement root)
        {
            _root = root;
            _opacity = Config.PanelOpacity;
            _roots.Add(root, this);
            try
            {
                TrackTree(root);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary> 动态内容附加后只处理该分支, 已登记节点不会重复创建状态或回调. </summary>
        internal static void Track(VisualElement element)
        {
            for (var ancestor = element; ancestor != null; ancestor = ancestor.hierarchy.parent)
            {
                if (!_roots.TryGetValue(ancestor, out var owner))
                {
                    continue;
                }

                owner.TrackTree(element);
                return;
            }
        }

        internal static void SetOpacity(float opacity)
        {
            foreach (var owner in _roots.Values)
            {
                if (owner._opacity == opacity)
                {
                    continue;
                }

                owner._opacity = opacity;
                owner.RefreshTree(owner._root);
            }
        }

        internal static void SetBackgroundOpacity(VisualElement element, float opacity)
        {
            for (var ancestor = element; ancestor != null; ancestor = ancestor.hierarchy.parent)
            {
                if (_roots.TryGetValue(ancestor, out var owner) && owner._nodes.TryGetValue(element, out var node))
                {
                    node.SetBackgroundOpacity(opacity);
                    return;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _roots.Remove(_root);
            foreach (var node in _nodes.Values)
            {
                node.Dispose();
            }

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
                else
                {
                    node.Resume();
                }

                _nodes.Add(element, node);
            }

            for (var index = 0; index < element.hierarchy.childCount; index++)
            {
                TrackTree(element.hierarchy[index]);
            }
        }

        private void RefreshTree(VisualElement element)
        {
            if (_nodes.TryGetValue(element, out var node))
            {
                node.Apply();
            }

            for (var index = 0; index < element.hierarchy.childCount; index++)
            {
                RefreshTree(element.hierarchy[index]);
            }
        }

        private void Release(Node node)
        {
            if (!_nodes.Remove(node.element)) return;

            node.Suspend();
        }
    }
}
