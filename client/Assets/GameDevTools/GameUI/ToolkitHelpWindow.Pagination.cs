using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using PageKey = System.ValueTuple<GameConsole.UI.ToolkitHelpTopic, UnityEngine.Vector2, int, UnityEngine.KeyCode, GameConsole.ShortcutModifier>;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitHelpWindow
    {
        private const int MAX_CACHED_LAYOUTS = 16;

        private const double FRAME_BUDGET_MS = 2;

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private readonly Stopwatch _paginationClock = new();

        private readonly List<(PageKey key, List<ToolkitHelpPage> pages)> _pageCache = new();

        private IEnumerator<int> _pagination;

        private PageKey? _pageKey;

        private int _layoutFrame;

        internal void Tick()
        {
            if (!IsOpen || _disposed || Time.frameCount < _layoutFrame) return;

            var size = _viewport.contentRect.size;

            if (!Finite(size.x) || !Finite(size.y) || size.x <= 0 || size.y <= 0) return;

            var key = new PageKey(_topic, size, ToolkitFontMetrics.GetKey(_body), Config.KeyCode, Config.KeyModifier);

            if (!_pageKey.HasValue || !_pageKey.Value.Equals(key))
            {
                StartPagination(key, size);
            }
            else if (_pagination == null && _requestedFeature >= 0 && _pages.Count > 0)
            {
                PresentPages(_pages);
            }

            RunPaginationFrame();
        }

        private void StartPagination(PageKey key, Vector2 size)
        {
            CancelPagination();
            LoadFeatures();
            _pageKey = key;
            if (TryPresentCached(key)) return;

            var sample = ToolkitLocalization.Get("ui.HelpWindow.Tick");
            var lineHeight = _body.MeasureTextSize(sample, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).y;

            if (!Finite(lineHeight) || lineHeight <= 0)
            {
                _pageKey = null;
                return;
            }

            if (size.y < lineHeight + 2f)
            {
                ToolkitMixedText.SetText(_body, ToolkitLocalization.Get("ui.HelpWindow.Tick.2"));
                _pager.SetPage(0, 1);
                SetLoading(false);
                return;
            }

            StartMeasurements(key, size);
        }

        private void LoadFeatures()
        {
            var shortcut = ToolShortcut.DisplayName;
            var section = ToolkitHelpContents.Get(_topic);

            _features.Clear();
            foreach (var feature in section.Features)
            {
                var body = (feature.Body ?? string.Empty).Replace("{ToolKey}", shortcut);

                _features.Add(new ToolkitHelpFeature(feature.Title, body));
            }
        }

        private bool TryPresentCached(PageKey key)
        {
            var index = _pageCache.FindIndex(item => item.key.Equals(key));

            if (index < 0) return false;

            var cached = _pageCache[index];

            _pageCache.RemoveAt(index);
            _pageCache.Add(cached);
            PresentPages(cached.pages);
            return true;
        }

        private void StartMeasurements(PageKey key, Vector2 size)
        {
            var priority = _requestedFeature >= 0 ? _requestedFeature : SelectedFeature;
            var widths = new Dictionary<string, float>(System.StringComparer.Ordinal);
            var warmedGlyphs = new HashSet<string>(System.StringComparer.Ordinal);
            var availableHeight = size.y - 2f;

            SetLoading(true);
            var steps = ToolkitHelpPagination.BuildSteps(_features, priority, availableHeight,
                (text, heightLimit, complete) => ToolkitMixedText.MeasureHeightSteps(_body, text, size.x, heightLimit, widths, warmedGlyphs, complete),
                ShowPreview, pages => FinishMeasurements(key, pages));

            _pagination = steps.GetEnumerator();
        }

        private void RunPaginationFrame()
        {
            if (_pagination == null) return;

            _paginationClock.Restart();
            while (_pagination.MoveNext())
            {
                if (_paginationClock.Elapsed.TotalMilliseconds >= FRAME_BUDGET_MS) return;
            }

            _pagination.Dispose();
            _pagination = null;
        }

        private void ShowPreview(ToolkitHelpPage page)
        {
            if (_requestedFeature < 0 && _pages.Count > 0) return;

            _featureTitle.text = _features[page.FeatureIndex].Title;
            ShowBody(page);
        }

        private void ShowBody(ToolkitHelpPage page)
        {
            var text = page.Layout == null ? ToolkitIcons.Resolve(page.Text) : page.Text;

            ToolkitMixedText.SetText(_body, text, ToolkitMixedTextMode.Inline, page.Layout);
        }

        private void FinishMeasurements(PageKey key, List<ToolkitHelpPage> pages)
        {
            if (_pageCache.Count == MAX_CACHED_LAYOUTS)
            {
                _pageCache.RemoveAt(0);
            }

            _pageCache.Add((key, pages));
            PresentPages(pages);
        }

        private void PresentPages(List<ToolkitHelpPage> pages)
        {
            var feature = _requestedFeature >= 0 ? _requestedFeature : SelectedFeature;
            var offset = _requestedFeature < 0 && _pages.Count > 0 ? _pages[_pageIndex].Offset : 0;

            _pageIndex = 0;
            for (var index = 0; index < pages.Count; index++)
            {
                if (pages[index].FeatureIndex == feature && pages[index].Offset <= offset)
                {
                    _pageIndex = index;
                }
            }

            _requestedFeature = -1;
            _pages = pages;
            SetLoading(false);
            ShowPage();
        }

        private void SetLoading(bool loading)
        {
            _pagerRoot.EnableInClassList("help-pager-loading", loading);
        }

        private void CancelPagination()
        {
            _pagination?.Dispose();
            _pagination = null;
            _pageKey = null;
        }
    }
}
