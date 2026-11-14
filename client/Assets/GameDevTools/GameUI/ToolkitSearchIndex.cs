using System;
using System.Collections.Generic;

namespace GameConsole.UI
{
    /// <summary> 三类搜索共用的字段索引, 各实例只持有自己的数据, 不提供推荐或改变排列顺序. </summary>
    internal sealed class ToolkitSearchIndex<T> where T : class
    {
        private readonly Func<T, string[]> _getFields; // 仅在数据更新时提取完整字段.
        private readonly List<Entry> _entries = new(); // 来源顺序与搜索结果顺序一致.

        /// <summary> 指定当前数据类型允许参与搜索的字段. </summary>
        internal ToolkitSearchIndex(Func<T, string[]> getFields) => _getFields = getFields;

        /// <summary> 数据源变化后重建索引, 不缓存查询结果以免库更新后命中过期内容. </summary>
        internal void Rebuild(IEnumerable<T> source)
        {
            _entries.Clear();
            if (source == null) return;
            foreach (var item in source)
                if (item != null)
                    _entries.Add(new Entry(item, _getFields(item)));
        }

        /// <summary> 忽略大小写并按完整搜索词包含匹配, 分类筛选与文字搜索相互独立. </summary>
        internal void Search(string query, List<T> result, Predicate<T> include = null)
        {
            query = Normalize(query);
            result.Clear();
            foreach (var entry in _entries)
                if ((include == null || include(entry.item)) && Matches(entry.fields, query))
                    result.Add(entry.item);
        }

        /// <summary> 搜索和历史记录共用首尾空白处理, 不改动词内空格和符号. </summary>
        internal static string Normalize(string query) => (query ?? string.Empty).Trim();

        // 不拼接字段, 防止查询跨字段边界命中并避免逐次输入产生合并字符串.
        private static bool Matches(string[] fields, string query)
        {
            if (query.Length == 0) return true;
            foreach (var field in fields)
                if (field != null && field.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        private readonly struct Entry
        {
            /// <summary> 原始数据对象, 搜索不复制业务对象. </summary>
            internal readonly T item;
            /// <summary> 数据变化时取得的完整字段. </summary>
            internal readonly string[] fields;

            /// <summary> 建立字段与业务对象的关联. </summary>
            internal Entry(T item, string[] fields) { this.item = item; this.fields = fields; }
        }
    }
}
