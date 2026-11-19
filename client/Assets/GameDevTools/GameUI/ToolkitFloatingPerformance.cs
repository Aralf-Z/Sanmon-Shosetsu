using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 管理浮窗帧率监控的采样、刷新和定位. </summary>
    internal sealed class ToolkitFloatingPerformance : IDisposable
    {
        private const float METRICS_WIDTH = 72f;
        private const float GRAPH_WIDTH = 156f;
        private const float MIN_GRAPH_WIDTH = 56f;
        private const float PANEL_GAP = 8f;
        private const float GRAPH_LABEL_HEIGHT = 14f;
        private const float MIN_GRAPH_MAXIMUM = 1000f / 30f;
        private const double TEXT_INTERVAL = .25d;
        private const double GRAPH_INTERVAL = .1d;

        private readonly VisualElement _panel;
        private readonly VisualElement _values;
        private readonly Label _realtime;
        private readonly Label _average;
        private readonly Label _low;
        private readonly VisualElement _graph;
        private readonly Label _scale;
        private readonly FloatingFrameSamples _samples = new();
        private readonly ToolkitWindowSettings _settings;

        private FloatingFrameMetrics _metrics;
        private bool _visible;
        private bool _placed;
        private int _lastFrame = -1;
        private double _nextTextUpdate;
        private double _nextGraphUpdate;
        private float[] _peaks = Array.Empty<float>();
        private int _peakCount;
        private float _graphMaximum = MIN_GRAPH_MAXIMUM;

        internal ToolkitFloatingPerformance(VisualElement root, ToolkitWindowSettings settings)
        {
            _settings = settings;
            _panel = ToolkitView.Require<VisualElement>(root, "floatingPerformance");
            _values = ToolkitView.Require<VisualElement>(_panel, "floatingFpsValues");
            _values.style.width = METRICS_WIDTH;
            _realtime = ToolkitView.Require<Label>(_values, "floatingRealtimeValue");
            _average = ToolkitView.Require<Label>(_values, "floatingAverageValue");
            _low = ToolkitView.Require<Label>(_values, "floatingLowValue");
            _graph = ToolkitView.Require<VisualElement>(_panel, "floatingFrameTimeGraph");
            _scale = ToolkitView.Require<Label>(_graph, "floatingFrameTimeScale");
            _graph.generateVisualContent += DrawGraph;
            _graph.RegisterCallback<GeometryChangedEvent>(OnGraphGeometry);
            Application.focusChanged += OnFocusChanged;
            _settings.e_onChanged += RefreshBackgroundOpacity;
            SetMetrics(settings.FrameMetrics);
        }

        internal void SetVisible(bool visible)
        {
            _visible = visible;
            if (!visible) StopSampling();
            else RefreshBackgroundOpacity();

            UpdateVisibility();
        }

        internal bool SetMetrics(FloatingFrameMetrics metrics)
        {
            if (_metrics == metrics) return false;

            _metrics = metrics;
            if (metrics == FloatingFrameMetrics.None) StopSampling();

            _values.EnableInClassList("is-hidden", !Has(FloatingFrameMetrics.FpsValues));
            _realtime.parent.EnableInClassList("is-hidden", !Has(FloatingFrameMetrics.RealtimeFps));
            _average.parent.EnableInClassList("is-hidden", !Has(FloatingFrameMetrics.AverageFps));
            _low.parent.EnableInClassList("is-hidden", !Has(FloatingFrameMetrics.LowFps));
            _graph.EnableInClassList("is-hidden", !Has(FloatingFrameMetrics.FrameTimeGraph));
            if (!Has(FloatingFrameMetrics.FrameTimeGraph))
            {
                _peakCount = 0;
                _scale.text = string.Empty;
            }

            RefreshValues();
            _nextTextUpdate = 0d;
            _nextGraphUpdate = 0d;
            UpdateVisibility();
            return true;
        }

        internal Rect Layout(Rect icon, Vector2 viewport, int dockEdge)
        {
            if (!_visible) return icon;

            var showValues = Has(FloatingFrameMetrics.FpsValues);
            var showGraph = Has(FloatingFrameMetrics.FrameTimeGraph);
            var (bounds, onLeft) = CalculateBounds(icon, viewport, dockEdge, showValues, showGraph);

            _placed = bounds.width > 0;
            UpdateVisibility();
            if (!_placed) return icon;

            var gap = showValues && showGraph ? PANEL_GAP : 0f;

            _panel.style.width = bounds.width;
            _panel.style.height = bounds.height;
            _panel.style.flexDirection = onLeft ? FlexDirection.RowReverse : FlexDirection.Row;
            _panel.style.translate = new Translate(bounds.x, bounds.y, 0f);
            if (showGraph)
            {
                _graph.style.width = bounds.width - (showValues ? METRICS_WIDTH : 0f) - gap;
                _graph.style.marginLeft = onLeft ? 0f : gap;
                _graph.style.marginRight = onLeft ? gap : 0f;
            }

            return Rect.MinMaxRect(Mathf.Min(icon.xMin, bounds.xMin), Mathf.Min(icon.yMin, bounds.yMin), Mathf.Max(icon.xMax, bounds.xMax), Mathf.Max(icon.yMax, bounds.yMax));
        }

        internal static (Rect bounds, bool onLeft) CalculateBounds(Rect icon, Vector2 viewport, int dockEdge, bool showValues, bool showGraph)
        {
            if (icon.width <= 0 || icon.height <= 0 || !showValues && !showGraph) return default;

            var valuesWidth = showValues ? METRICS_WIDTH + (showGraph ? PANEL_GAP : 0f) : 0f;
            var ideal = valuesWidth + (showGraph ? GRAPH_WIDTH : 0f);
            var minimum = valuesWidth + (showGraph ? MIN_GRAPH_WIDTH : 0f);
            var leftSpace = icon.xMin - PANEL_GAP;
            var rightSpace = viewport.x - icon.xMax - PANEL_GAP;
            var preferLeft = dockEdge == 2 || dockEdge != 1 && icon.center.x > viewport.x * .5f;
            var onLeft = preferLeft ? leftSpace >= ideal || rightSpace < ideal && leftSpace >= rightSpace : rightSpace < ideal && leftSpace > rightSpace;
            var sideSpace = onLeft ? leftSpace : rightSpace;
            if (sideSpace >= minimum)
            {
                var width = Mathf.Min(ideal, sideSpace);
                var x = onLeft ? icon.xMin - PANEL_GAP - width : icon.xMax + PANEL_GAP;
                return (new Rect(x, icon.yMin, width, icon.height), onLeft);
            }

            var stackedWidth = Mathf.Min(ideal, viewport.x);
            var below = icon.yMax + PANEL_GAP;
            var above = icon.yMin - PANEL_GAP - icon.height;
            if (stackedWidth < minimum || below + icon.height > viewport.y && above < 0f) return default;

            var stackedX = Mathf.Clamp(preferLeft ? icon.xMax - stackedWidth : icon.xMin, 0f, viewport.x - stackedWidth);
            var stackedY = below + icon.height <= viewport.y ? below : above;
            return (new Rect(stackedX, stackedY, stackedWidth, icon.height), preferLeft);
        }

        internal void Tick(double now, int frame, float seconds, bool focused)
        {
            if (!_visible || _metrics == FloatingFrameMetrics.None || !focused)
            {
                StopSampling();
                return;
            }

            if (frame == _lastFrame) return;

            var first = _lastFrame < 0;

            _lastFrame = frame;
            // 恢复采样的第一帧可能包含浮窗显示前或失焦期间的时间.
            if (first) return;

            var hadSamples = _samples.Count > 0;

            _samples.Add(now, seconds);
            if (now >= _nextTextUpdate || !hadSamples && _samples.Count > 0)
            {
                RefreshValues();
                _nextTextUpdate = now + TEXT_INTERVAL;
            }

            if (Has(FloatingFrameMetrics.FrameTimeGraph) && _placed && now >= _nextGraphUpdate)
            {
                RefreshGraph(now);
                _nextGraphUpdate = now + GRAPH_INTERVAL;
            }
        }

        public void Dispose()
        {
            _settings.e_onChanged -= RefreshBackgroundOpacity;
            Application.focusChanged -= OnFocusChanged;
            _graph.UnregisterCallback<GeometryChangedEvent>(OnGraphGeometry);
            _graph.generateVisualContent -= DrawGraph;
            SetVisible(false);
        }

        private bool Has(FloatingFrameMetrics metrics) => (_metrics & metrics) != 0;

        private void RefreshBackgroundOpacity()
        {
            var opacity = _settings.MonitorBackgroundOpacity;

            ToolkitTextOpacity.SetBackgroundOpacity(_values, opacity);
            ToolkitTextOpacity.SetBackgroundOpacity(_graph, opacity);
        }

        private void UpdateVisibility() => _panel.EnableInClassList("is-hidden", !_visible || !_placed || _metrics == FloatingFrameMetrics.None);

        private void OnFocusChanged(bool focused)
        {
            if (!focused) StopSampling();
        }

        private void StopSampling()
        {
            if (_lastFrame < 0) return;

            _lastFrame = -1;
            _samples.Clear();
            _nextTextUpdate = 0d;
            _nextGraphUpdate = 0d;
            _peakCount = 0;
            _scale.text = string.Empty;
            _graph.MarkDirtyRepaint();
            RefreshValues();
        }

        private void RefreshValues()
        {
            if (Has(FloatingFrameMetrics.RealtimeFps)) SetValue(_realtime, _samples.RealtimeFps);
            if (Has(FloatingFrameMetrics.AverageFps)) SetValue(_average, _samples.AverageFps);
            if (Has(FloatingFrameMetrics.LowFps)) SetValue(_low, _samples.OnePercentLow());
        }

        private static void SetValue(Label label, double value)
        {
            label.text = double.IsNaN(value) ? "—" : value.ToString("0");
        }

        private void OnGraphGeometry(GeometryChangedEvent evt)
        {
            if (evt.newRect.size != evt.oldRect.size) _nextGraphUpdate = 0d;
        }

        private void RefreshGraph(double now)
        {
            var rect = _graph.contentRect;
            if (_graph.panel == null || rect.width < 1f || rect.height <= GRAPH_LABEL_HEIGHT || float.IsNaN(rect.width) || float.IsNaN(rect.height)) return;

            var pixelsPerUnit = Screen.width / _panel.panel.visualTree.layout.width;
            if (float.IsInfinity(pixelsPerUnit) || float.IsNaN(pixelsPerUnit) || pixelsPerUnit <= 0f) pixelsPerUnit = 1f;

            _peakCount = Mathf.Max(1, Mathf.RoundToInt(rect.width * pixelsPerUnit));
            if (_peaks.Length < _peakCount) Array.Resize(ref _peaks, _peakCount);

            var peak = _samples.FillPeaks(now, _peaks, _peakCount);

            _graphMaximum = Mathf.Max(MIN_GRAPH_MAXIMUM, Mathf.Ceil(peak));
            _scale.text = peak > 0f ? _graphMaximum.ToString("0.#") + " ms" : string.Empty;
            _graph.MarkDirtyRepaint();
        }

        private void DrawGraph(MeshGenerationContext context)
        {
            var rect = _graph.contentRect;
            var height = rect.height - GRAPH_LABEL_HEIGHT;

            if (!_visible || !_placed || !Has(FloatingFrameMetrics.FrameTimeGraph) || _peakCount == 0 || rect.width <= 0f || height <= 0f) return;

            var painter = context.painter2D;
            var color = _graph.resolvedStyle.color;
            var count = 0;
            var point = Vector2.zero;

            color.a *= Mathf.Max(Config.MIN_TEXT_OPACITY, _settings.PanelOpacity);
            painter.strokeColor = color;
            painter.lineWidth = 1.25f;
            painter.BeginPath();

            for (var index = 0; index < _peakCount; index++)
            {
                var peak = _peaks[index];
                if (peak <= 0f) continue;

                point = new Vector2(rect.xMin + (index + .5f) * rect.width / _peakCount, rect.yMax - peak / _graphMaximum * height);
                if (count == 0) painter.MoveTo(point);
                else painter.LineTo(point);
                count++;
            }

            if (count == 0) return;
            if (count == 1)
            {
                painter.LineTo(new Vector2(Mathf.Max(rect.xMin, point.x - 1f), point.y));
            }

            painter.Stroke();
        }
    }
}

