using System;
using System.Collections.Generic;

namespace GameConsole.UI
{
    /// <summary> 三类搜索共用的字段索引, 各实例只持有自己的数据, 不提供推荐或改变排列顺序. </summary>
    internal sealed class ToolkitSearchIndex<T>
        where T : class
    {
        private readonly Func<T, string[]> _getFields; // 仅在数据更新时提取完整字段.

        private readonly List<Entry> _entries = new();

        internal ToolkitSearchIndex(Func<T, string[]> getFields) => _getFields = getFields;

        internal void Rebuild(IEnumerable<T> source)
        {
            _entries.Clear();
            if (source == null) return;

            foreach (var item in source)
            {
                if (item != null)
                {
                    _entries.Add(new Entry(item, _getFields(item)));
                }
            }
        }

        internal void Search(string query, List<T> result, Predicate<T> include = null)
        {
            query = Normalize(query);
            result.Clear();
            foreach (var entry in _entries)
            {
                if ((include == null || include(entry.item)) && Matches(entry.fields, query))
                {
                    result.Add(entry.item);
                }
            }
        }

        internal static string Normalize(string query) => (query ?? string.Empty).Trim();

        // 不拼接字段, 防止查询跨字段边界命中并避免逐次输入产生合并字符串.
        private static bool Matches(string[] fields, string query)
        {
            if (query.Length == 0) return true;

            foreach (var field in fields)
            {
                if (field != null && field.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }

            return false;
        }

        private readonly struct Entry
        {
            internal readonly T item;

            internal readonly string[] fields;

            internal Entry(T item, string[] fields)
            {
                this.item = item;
                this.fields = fields;
            }
        }
    }
}
