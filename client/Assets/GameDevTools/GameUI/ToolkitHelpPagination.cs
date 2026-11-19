using System;
using System.Collections.Generic;

namespace GameConsole.UI
{
    /// <summary> 根据显示层提供的实际文字高度分页, 不依赖 UI 或文案存储. </summary>
    internal static class ToolkitHelpPagination
    {
        internal delegate IEnumerable<int> Measure(string text, float heightLimit, Action<float, ToolkitInlineLayout.Result> complete);

        internal static IEnumerable<int> BuildSteps(IReadOnlyList<ToolkitHelpFeature> features, int priority, float availableHeight, Measure measure, Action<ToolkitHelpPage> preview, Action<List<ToolkitHelpPage>> complete)
        {
            var sections = new List<ToolkitHelpPage>[features.Count];
            for (var step = 0; step < sections.Length; step++)
            {
                var index = (priority + step) % sections.Length;
                var pages = new List<ToolkitHelpPage>();
                var firstPage = step == 0 ? preview : null;

                sections[index] = pages;
                foreach (var measured in BuildFeature(features[index].Body ?? string.Empty, index, availableHeight, measure, firstPage, pages))
                {
                    yield return measured;
                }

                foreach (var page in pages)
                {
                    page.PartCount = pages.Count;
                }
            }

            var result = new List<ToolkitHelpPage>();
            foreach (var section in sections)
            {
                result.AddRange(section);
            }

            complete(result);
        }

        private static IEnumerable<int> BuildFeature(string text, int featureIndex, float availableHeight, Measure measure, Action<ToolkitHelpPage> preview, List<ToolkitHelpPage> pages)
        {
            var boundaries = ToolkitIcons.TextBoundaries(text);
            var element = 0;
            var last = boundaries.Length - 1;
            do
            {
                var end = last;
                ToolkitInlineLayout.Result layout = null;
                foreach (var step in FindEnd(text, boundaries, element, availableHeight, measure, (result, measuredLayout) =>
                {
                    end = result;
                    layout = measuredLayout;
                }))
                {
                    yield return step;
                }

                var offset = boundaries[element];
                var content = text.Substring(offset, boundaries[end] - offset);
                if (layout == null && ToolkitIcons.tokens.IsMatch(content))
                {
                    foreach (var step in measure(content, float.PositiveInfinity, (_, measuredLayout) => layout = measuredLayout))
                    {
                        yield return step;
                    }
                }

                var page = new ToolkitHelpPage(featureIndex, offset, content, pages.Count + 1, layout);

                pages.Add(page);
                if (pages.Count == 1)
                {
                    preview?.Invoke(page);
                }

                element = end;
                yield return 0;
            }
            while (element < last);
        }

        private static IEnumerable<int> FindEnd(string text, int[] boundaries, int start, float availableHeight, Measure measure, Action<int, ToolkitInlineLayout.Result> complete)
        {
            var offset = boundaries[start];
            var last = boundaries.Length - 1;
            var height = 0f;
            ToolkitInlineLayout.Result layout = null;
            void Measured(float value, ToolkitInlineLayout.Result measuredLayout)
            {
                height = value;
                layout = measuredLayout;
            }

            foreach (var step in measure(text.Substring(offset), availableHeight, Measured))
            {
                yield return step;
            }

            if (height <= availableHeight || start == last)
            {
                complete(last, layout);
                yield break;
            }

            var low = start;
            var high = last - 1;
            ToolkitInlineLayout.Result fittingLayout = null;
            while (low < high)
            {
                var middle = low + (high - low + 1) / 2;
                var prefix = text.Substring(offset, boundaries[middle] - offset);

                foreach (var step in measure(prefix, availableHeight, Measured))
                {
                    yield return step;
                }

                if (height <= availableHeight)
                {
                    low = middle;
                    fittingLayout = layout;
                }
                else
                {
                    high = middle - 1;
                }
            }

            var furthest = Math.Max(start + 1, low);
            var end = furthest == last ? furthest : PreferredBoundary(text, boundaries, start, furthest);
            complete(end, end == low ? fittingLayout : null);
        }

        // 在当前页后半部分优先找段落, 然后换行或句末, 避免过早拆出大量短页.
        private static int PreferredBoundary(string text, int[] boundaries, int start, int end)
        {
            var minimum = boundaries[start] + (boundaries[end] - boundaries[start]) / 2;

            for (var priority = 0; priority < 3; priority++)
            {
                for (var i = end; i > start && boundaries[i] >= minimum; i--)
                {
                    var offset = boundaries[i];
                    var last = text[offset - 1];
                    var paragraph = last == '\n' && (offset >= 2 && text[offset - 2] == '\n' || offset >= 3 && text[offset - 2] == '\r' && text[offset - 3] == '\n') || last == '\r' && offset >= 2 && text[offset - 2] == '\r';

                    if (priority == 0 && paragraph || priority == 1 && (last == '\n' || last == '\r') || priority == 2 && (last == '。' || last == '！' || last == '？' || last == '!' || last == '?' || last == ';' || last == '；' || last == '.')) return i;
                }
            }

            return end;
        }
    }

    /// <summary> 一页图文及其在原始特性中的位置, 用于缩放后恢复阅读位置. </summary>
    internal sealed class ToolkitHelpPage
    {
        internal int FeatureIndex { get; }

        internal int Offset { get; }

        internal string Text { get; }

        internal int PartIndex { get; }

        internal ToolkitInlineLayout.Result Layout { get; }

        /// <summary> 当前特性的总页数. </summary>
        internal int PartCount { get; set; }

        /// <summary> 创建保留源位置的分页结果. </summary>
        internal ToolkitHelpPage(int featureIndex, int offset, string text, int partIndex, ToolkitInlineLayout.Result layout)
        {
            FeatureIndex = featureIndex;
            Offset = offset;
            Text = text;
            PartIndex = partIndex;
            Layout = layout;
        }
    }
}
