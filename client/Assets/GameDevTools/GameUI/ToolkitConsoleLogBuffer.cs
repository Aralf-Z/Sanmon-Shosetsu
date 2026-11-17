using System;

namespace GameConsole.UI
{
    /// <summary> 完整缓存的筛选快照和已加载区间, 不复制原始日志. </summary>
    internal sealed class ToolkitConsoleLogBuffer
    {
        internal ConsoleLogStore Store { get; private set; } = new();

        internal ConsoleLogFilter Filter { get; private set; } = ConsoleLogFilter.All;

        internal bool Collapsed { get; private set; }

        internal int First { get; private set; }

        /// <summary> 已加载区间末项之后的索引. </summary>
        internal int End { get; private set; }

        internal int Count => End - First;

        internal int Target { get; private set; }

        internal int Pending => Math.Max(0, Store.GetCount(Filter, false) - _rawAtSnapshot);

        internal bool HasOlder => First > 0;

        private int _rawAtSnapshot; // 进入当前阅读范围时接收到的匹配日志数量.

        internal void Bind(ConsoleLogStore store) => Store = store;

        /// <summary> 一次确定最近日志逻辑范围, 节点准备由视口独立控制. </summary>
        internal void Reset(ConsoleLogFilter filter, bool collapsed)
        {
            Filter = filter;
            Collapsed = collapsed;
            End = Store.GetCount(filter, collapsed);
            Target = Math.Min(Config.CONSOLE_RECENT_LOG_COUNT, End);
            First = End - Target;
            _rawAtSnapshot = Store.GetCount(filter, false);
        }

        /// <summary> 兼容原分类编号的调用, 0 表示全部. </summary>
        internal void Reset(int filter, bool collapsed) => Reset(ConsoleLogStore.FilterForCategory(filter), collapsed);

        /// <summary> 跟随时更新最近范围, 阅读历史时冻结已加载区间. </summary>
        internal void Observe(bool follow)
        {
            if (!follow) return;

            End = Store.GetCount(Filter, Collapsed);
            Target = Math.Min(Config.CONSOLE_RECENT_LOG_COUNT, End);
            First = End - Target;
            _rawAtSnapshot = Store.GetCount(Filter, false);
        }

        /// <summary> 仅用户向上接近顶部时调用, 按统一批次加载. </summary>
        internal bool LoadOlder()
        {
            var count = Math.Min(Config.CONSOLE_ROWS_PER_FRAME, First);

            First -= count;
            return count > 0;
        }

        /// <summary> 读取已加载范围中的一项. </summary>
        internal ConsoleLogItem GetItem(int index) => Store.GetItem(Filter, Collapsed, First + index);

        /// <summary> 按稳定编号定位当前已加载范围, 避免正文重复导致锚点混淆. </summary>
        internal int Find(long id)
        {
            var index = Store.Find(Filter, Collapsed, id) - First;

            return index >= 0 && index < Count
                ? index
                : -1;
        }
    }
}
