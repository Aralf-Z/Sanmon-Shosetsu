using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 按各面板原生步长缩放滚轮速度, 保留原生事件传播和边界处理. </summary>
    internal sealed class ToolkitWheelScrollController : IDisposable
    {
        private readonly VisualElement _root;

        private readonly Dictionary<ScrollView, float> _originalSteps = new();

        internal ToolkitWheelScrollController(VisualElement root)
        {
            _root = root;
            _root.RegisterCallback<WheelEvent>(Wheel, TrickleDown.TrickleDown);
        }

        private void Wheel(WheelEvent evt)
        {
            if (!(evt.target is VisualElement target) || evt.delta == Vector3.zero) return;

            var sensitivity = Config.WheelScrollSensitivity;

            for (var element = target; element != null; element = element.parent)
            {
                if (element is ScrollView scroll && !scroll.contentContainer.ClassListContains("context-menu") && !scroll.ClassListContains("macro-inner-scroll"))
                {
                    ApplyStep(scroll, sensitivity);
                }

                if (element == _root) break;
            }
        }

        private void ApplyStep(ScrollView scroll, float sensitivity)
        {
            if (!_originalSteps.TryGetValue(scroll, out var original))
            {
                original = scroll.mouseWheelScrollSize;
                _originalSteps.Add(scroll, original);
                scroll.RegisterCallback<DetachFromPanelEvent>(Detached);
            }

            scroll.mouseWheelScrollSize = original * sensitivity * sensitivity;
        }

        private void Detached(DetachFromPanelEvent evt)
        {
            if (!(evt.target is ScrollView scroll) || !_originalSteps.TryGetValue(scroll, out var original)) return;

            _originalSteps.Remove(scroll);
            Restore(scroll, original);
        }

        private void Restore(ScrollView scroll, float original)
        {
            scroll.UnregisterCallback<DetachFromPanelEvent>(Detached);
            scroll.mouseWheelScrollSize = original;
        }

        public void Dispose()
        {
            _root.UnregisterCallback<WheelEvent>(Wheel, TrickleDown.TrickleDown);
            foreach (var pair in _originalSteps)
            {
                Restore(pair.Key, pair.Value);
            }

            _originalSteps.Clear();
        }
    }
}
