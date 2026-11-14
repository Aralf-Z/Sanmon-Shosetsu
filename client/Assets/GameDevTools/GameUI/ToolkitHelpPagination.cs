using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameConsole.UI
{
    /// <summary> 根据显示层提供的实际文字高度分页, 不依赖 UI 或文案存储. </summary>
    internal static class ToolkitHelpPagination
    {
        /// <summary> 各特性独立分页, 分页文本拼接后保持原始正文不变. </summary>
        internal static List<ToolkitHelpPage> Build(IReadOnlyList<ToolkitHelpFeature> features, float availableHeight, Func<string, float> measureHeight)
        {
            var pages = new List<ToolkitHelpPage>();
            for (var featureIndex = 0; featureIndex < features.Count; featureIndex++)
            {
                var text = features[featureIndex].Body ?? string.Empty;
                var starts = StringInfo.ParseCombiningCharacters(text);
                var boundaries = new int[starts.Length + 1];
                Array.Copy(starts, boundaries, starts.Length);
                boundaries[boundaries.Length - 1] = text.Length;
                var firstPage = pages.Count;
                var element = 0;
                do
                {
                    var offset = boundaries[element];
                    var low = element;
                    var high = boundaries.Length - 1;
                    while (low < high)
                    {
                        var middle = low + (high - low + 1) / 2;
                        if (measureHeight(text.Substring(offset, boundaries[middle] - offset)) <= availableHeight) low = middle;
                        else high = middle - 1;
                    }
                    // 视口至少能容纳一行时, 总能向前推进一个完整文本元素.
                    var end = Math.Max(element + 1, low);
                    end = Math.Min(end, boundaries.Length - 1);
                    if (end < boundaries.Length - 1)
                        end = PreferredBoundary(text, boundaries, element, end);
                    pages.Add(new ToolkitHelpPage(featureIndex, offset, text.Substring(offset, boundaries[end] - offset), pages.Count - firstPage + 1));
                    element = end;
                } while (element < boundaries.Length - 1);
                var count = pages.Count - firstPage;
                for (var i = firstPage; i < pages.Count; i++) pages[i].PartCount = count;
            }
            return pages;
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
                    if (priority == 0 && paragraph ||
                        priority == 1 && (last == '\n' || last == '\r') ||
                        priority == 2 && (last == '。' || last == '！' || last == '？' || last == '!' || last == '?' || last == ';' || last == '；' || last == '.'))
                        return i;
                }
            }
            return end;
        }
    }

    /// <summary> 一页纯文本及其在原始特性中的位置, 用于缩放后恢复阅读位置. </summary>
    internal sealed class ToolkitHelpPage
    {
        /// <summary> 所属特性的索引. </summary>
        internal int FeatureIndex { get; }
        /// <summary> 本页在特性正文中的字符偏移. </summary>
        internal int Offset { get; }
        /// <summary> 未截断或转义的页面正文. </summary>
        internal string Text { get; }
        /// <summary> 当前特性内的页序号, 从一开始. </summary>
        internal int PartIndex { get; }
        /// <summary> 当前特性的总页数. </summary>
        internal int PartCount { get; set; }

        /// <summary> 创建保留源位置的分页结果. </summary>
        internal ToolkitHelpPage(int featureIndex, int offset, string text, int partIndex)
        {
            FeatureIndex = featureIndex;
            Offset = offset;
            Text = text;
            PartIndex = partIndex;
        }
    }
}
