using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RedSaw.CommandLineInterface;

namespace GameConsole
{
    /// <summary> 可组合的日志类型筛选, 全部关闭时不显示日志. </summary>
    [Flags]
    public enum ConsoleLogFilter { None = 0, Log = 1, Warning = 2, Error = 4, Dev = 8, All = Log | Warning | Error | Dev }

    /// <summary> 单条日志或连续折叠组的只读快照. </summary>
    public readonly struct ConsoleLogItem
    {
        /// <summary> 原始首项编号, 重复追加不会改变. </summary>
        public long Id { get; }
        /// <summary> 最近一次出现的正文, 时间与堆栈. </summary>
        public ConsoleLogRecord Record { get; }
        /// <summary> 当前连续组的累计次数. </summary>
        public int Count { get; }
        /// <summary> 创建显示快照. </summary>
        public ConsoleLogItem(long id, ConsoleLogRecord record, int count) { Id = id; Record = record; Count = count; }
    }

    /// <summary> UI 创建前后共用的完整运行时日志缓存, 分类型索引增量维护. </summary>
    public sealed class ConsoleLogStore
    {
        private static readonly Regex _previewTags = new(@"</?(?:b|i|size|alpha|mark)(?:=[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled); // 保持现有预览的单行样式.
        /// <summary> 接收或清空后递增. </summary>
        public int Version { get; private set; }
        /// <summary> 仅清空后递增, 用于清理视图中的过期编号. </summary>
        public int Generation { get; private set; }
        /// <summary> 清空以来原始接收次数. </summary>
        public int Count => _records[(int)ConsoleLogFilter.All].Count;
        /// <summary> 不受 Console 筛选与折叠影响的最近十条预览. </summary>
        public IReadOnlyList<string> LatestLines => _latest;

        private readonly List<ConsoleLogRecord>[] _records = CreateIndexes<ConsoleLogRecord>(); // 按四种类型的组合掩码维护索引.
        private readonly List<Group>[] _groups = CreateIndexes<Group>(); // 先按原始顺序成组, 同一组在组合索引中只出现一次.
        private readonly List<string> _latest = new(); // 预览仍只保留十条字符串.
        private long _nextId; // 清空后不复用旧编号.

        /// <summary> 追加原始记录, 只更新尾组及对应类型索引. </summary>
        public void Append(ConsoleLogRecord record)
        {
            record.Id = ++_nextId;
            var filter = Category(record.Type);
            var bit = 1 << (filter - 1);
            for (var mask = 1; mask <= (int)ConsoleLogFilter.All; mask++)
                if ((mask & bit) != 0) _records[mask].Add(record);
            var all = _groups[(int)ConsoleLogFilter.All];
            var previous = all.Count == 0 ? null : all[^1];
            if (previous != null && previous.last.Message == record.Message && previous.last.StackTrace == record.StackTrace)
            {
                if (previous.lastByCategory != null || Category(previous.last.Type) != filter)
                {
                    if (previous.lastByCategory == null)
                    {
                        var previousFilter = Category(previous.last.Type);
                        previous.lastByCategory = new ConsoleLogRecord[5];
                        previous.countByCategory = new int[5];
                        previous.lastByCategory[previousFilter] = previous.last;
                        previous.countByCategory[previousFilter] = previous.count;
                    }
                    previous.lastByCategory[filter] = record;
                    previous.countByCategory[filter]++;
                }
                for (var mask = 1; mask < (int)ConsoleLogFilter.All; mask++)
                    if ((mask & bit) != 0 && (mask & previous.categories) == 0) _groups[mask].Add(previous);
                previous.categories |= bit;
                previous.last = record;
                previous.count++;
            }
            else
            {
                var group = new Group { id = record.Id, last = record, count = 1, categories = bit };
                for (var mask = 1; mask <= (int)ConsoleLogFilter.All; mask++)
                    if ((mask & bit) != 0) _groups[mask].Add(group);
            }
            var text = string.IsNullOrEmpty(record.Color) ? record.DisplayMessage : $"<color={record.Color}>{record.DisplayMessage}</color>";
            text = _previewTags.Replace(text, string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n').Replace('\n', ' ');
            if (_latest.Count == Config.MAX_FLOATING_LOG_COUNT) _latest.RemoveAt(0);
            _latest.Add(text);
            Version++;
        }

        /// <summary> 按筛选与折叠方式查询数量, 不扫描日志正文. </summary>
        public int GetCount(ConsoleLogFilter filter, bool collapsed) => collapsed ? _groups[(int)filter].Count : _records[(int)filter].Count;

        /// <summary> 兼容原分类编号, 0 表示全部, 1 到 4 表示日志, 警告, 错误与 Dev. </summary>
        public int GetCount(int filter, bool collapsed) => GetCount(FilterForCategory(filter), collapsed);

        /// <summary> 常数时间访问一条显示记录. </summary>
        public ConsoleLogItem GetItem(ConsoleLogFilter filter, bool collapsed, int index)
        {
            if (collapsed)
            {
                var group = _groups[(int)filter][index];
                if (filter != ConsoleLogFilter.All && group.lastByCategory != null)
                {
                    ConsoleLogRecord last = null;
                    var count = 0;
                    for (var category = 1; category <= 4; category++)
                    {
                        if (((int)filter & (1 << (category - 1))) == 0) continue;
                        count += group.countByCategory[category];
                        var candidate = group.lastByCategory[category];
                        if (candidate != null && (last == null || candidate.Id > last.Id)) last = candidate;
                    }
                    return new ConsoleLogItem(group.id, last, count);
                }
                return new ConsoleLogItem(group.id, group.last, group.count);
            }
            var record = _records[(int)filter][index];
            return new ConsoleLogItem(record.Id, record, 1);
        }

        /// <summary> 按稳定编号二分定位, 编号不属于当前筛选时返回 -1. </summary>
        public int Find(ConsoleLogFilter filter, bool collapsed, long id)
        {
            var low = 0;
            var high = GetCount(filter, collapsed) - 1;
            while (low <= high)
            {
                var middle = low + (high - low) / 2;
                var candidate = GetItem(filter, collapsed, middle).Id;
                if (candidate == id) return middle;
                if (candidate < id) low = middle + 1; else high = middle - 1;
            }
            return -1;
        }

        /// <summary> 使用原分类编号读取, 保留旧日志适配器接口. </summary>
        public ConsoleLogItem GetItem(int filter, bool collapsed, int index) => GetItem(FilterForCategory(filter), collapsed, index);

        /// <summary> 使用原分类编号定位稳定编号. </summary>
        public int Find(int filter, bool collapsed, long id) => Find(FilterForCategory(filter), collapsed, id);

        /// <summary> 将原来的单选分类编号转换为筛选掩码. </summary>
        public static ConsoleLogFilter FilterForCategory(int category) => category switch
        {
            0 => ConsoleLogFilter.All,
            1 => ConsoleLogFilter.Log,
            2 => ConsoleLogFilter.Warning,
            3 => ConsoleLogFilter.Error,
            4 => ConsoleLogFilter.Dev,
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };

        /// <summary> 清除所有原始日志, 分组, 索引和预览. </summary>
        public void Clear()
        {
            foreach (var records in _records) records.Clear();
            foreach (var groups in _groups) groups.Clear();
            _latest.Clear();
            Generation++;
            Version++;
        }

        /// <summary> 统一 UI 的日志类型筛选映射. </summary>
        public static int Category(ConsoleLogType type) => type switch
        {
            ConsoleLogType.Log => 1,
            ConsoleLogType.Warning => 2,
            ConsoleLogType.Dev => 4,
            _ => 3
        };

        // 每种组合都有独立索引, 新日志追加时增量维护, 筛选时不扫描完整缓存.
        private static List<T>[] CreateIndexes<T>()
        {
            var indexes = new List<T>[(int)ConsoleLogFilter.All + 1];
            for (var index = 0; index < indexes.Length; index++) indexes[index] = new List<T>();
            return indexes;
        }

        private sealed class Group
        {
            internal long id; // 首次出现的编号.
            internal ConsoleLogRecord last; // 最新一次数据, 不复制堆栈文本.
            internal int count; // 仅连续重复累计.
            internal int categories; // 该连续组实际包含的类型组合.
            internal ConsoleLogRecord[] lastByCategory; // 同一组跨类型时才分配, 筛选展示最近一次匹配记录.
            internal int[] countByCategory; // 类型筛选中的次数仅统计匹配的原始记录.
        }
    }

}
