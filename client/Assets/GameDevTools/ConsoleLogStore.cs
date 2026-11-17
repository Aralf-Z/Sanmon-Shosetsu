using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RedSaw.CommandLineInterface;

namespace GameConsole
{
    /// <summary> 可组合的日志类型筛选, 全部关闭时不显示日志. </summary>
    [Flags]
    public enum ConsoleLogFilter
    {
        None = 0,
        Log = 1,
        Warning = 2,
        Error = 4,
        Dev = 8,
        All = Log | Warning | Error | Dev
    }

    /// <summary> 单条日志或连续折叠组的只读快照. </summary>
    public readonly struct ConsoleLogItem
    {
        /// <summary> 原始首项编号, 重复追加不会改变. </summary>
        public long Id { get; }

        public ConsoleLogRecord Record { get; }

        public int Count { get; }

        public ConsoleLogItem(long id, ConsoleLogRecord record, int count)
        {
            Id = id;
            Record = record;
            Count = count;
        }
    }

    /// <summary> UI 创建前后共用的完整运行时日志缓存, 分类型索引增量维护. </summary>
    public sealed class ConsoleLogStore
    {
        private static readonly Regex _previewTags = new(@"</?(?:b|i|size|alpha|mark)(?:=[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public int Version { get; private set; }

        /// <summary> 仅清空后递增, 用于清理视图中的过期编号. </summary>
        public int Generation { get; private set; }

        public int Count => _records[(int)ConsoleLogFilter.All].Count;

        public IReadOnlyList<string> LatestLines => _latest;

        private readonly List<ConsoleLogRecord>[] _records = CreateIndexes<ConsoleLogRecord>();

        private readonly List<Group>[] _groups = CreateIndexes<Group>();

        private readonly List<string> _latest = new(); // 预览仍只保留十条字符串.

        private long _nextId;

        public void Append(ConsoleLogRecord record)
        {
            record.Id = ++_nextId;
            var filter = Category(record.Type);
            var bit = 1 << (filter - 1);

            for (var mask = 1; mask <= (int)ConsoleLogFilter.All; mask++)
            {
                if ((mask & bit) != 0)
                {
                    _records[mask].Add(record);
                }
            }

            var all = _groups[(int)ConsoleLogFilter.All];
            var previous = all.Count == 0
                ? null
                : all[^1];

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
                {
                    if ((mask & bit) != 0 && (mask & previous.categories) == 0)
                    {
                        _groups[mask].Add(previous);
                    }
                }

                previous.categories |= bit;
                previous.last = record;
                previous.count++;
            }
            else
            {
                var group = new Group
                { id = record.Id, last = record, count = 1, categories = bit };

                for (var mask = 1; mask <= (int)ConsoleLogFilter.All; mask++)
                {
                    if ((mask & bit) != 0)
                    {
                        _groups[mask].Add(group);
                    }
                }
            }

            var text = string.IsNullOrEmpty(record.Color)
                ? record.DisplayMessage
                : $"<color={record.Color}>{record.DisplayMessage}</color>";

            text = _previewTags.Replace(text, string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .TrimEnd('\n')
                .Replace('\n', ' ');
            if (_latest.Count == Config.MAX_FLOATING_LOG_COUNT)
            {
                _latest.RemoveAt(0);
            }

            _latest.Add(text);
            Version++;
        }

        public int GetCount(ConsoleLogFilter filter, bool collapsed) => collapsed
            ? _groups[(int)filter].Count
            : _records[(int)filter].Count;

        /// <summary> 兼容原分类编号, 0 表示全部, 1 到 4 表示日志, 警告, 错误与 Dev. </summary>
        public int GetCount(int filter, bool collapsed) => GetCount(FilterForCategory(filter), collapsed);

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
                        if (((int)filter & (1 << (category - 1))) == 0)
                        {
                            continue;
                        }

                        count += group.countByCategory[category];
                        var candidate = group.lastByCategory[category];

                        if (candidate != null && (last == null || candidate.Id > last.Id))
                        {
                            last = candidate;
                        }
                    }

                    return new ConsoleLogItem(group.id, last, count);
                }

                return new ConsoleLogItem(group.id, group.last, group.count);
            }

            var record = _records[(int)filter][index];

            return new ConsoleLogItem(record.Id, record, 1);
        }

        public int Find(ConsoleLogFilter filter, bool collapsed, long id)
        {
            var low = 0;
            var high = GetCount(filter, collapsed) - 1;

            while (low <= high)
            {
                var middle = low + (high - low) / 2;
                var candidate = GetItem(filter, collapsed, middle).Id;

                if (candidate == id) return middle;

                if (candidate < id)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return -1;
        }

        /// <summary> 使用原分类编号读取, 保留旧日志适配器接口. </summary>
        public ConsoleLogItem GetItem(int filter, bool collapsed, int index) => GetItem(FilterForCategory(filter), collapsed, index);

        public int Find(int filter, bool collapsed, long id) => Find(FilterForCategory(filter), collapsed, id);

        public static ConsoleLogFilter FilterForCategory(int category) => category switch
        {
            0 => ConsoleLogFilter.All,
            1 => ConsoleLogFilter.Log,
            2 => ConsoleLogFilter.Warning,
            3 => ConsoleLogFilter.Error,
            4 => ConsoleLogFilter.Dev,
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };

        public void Clear()
        {
            foreach (var records in _records)
            {
                records.Clear();
            }

            foreach (var groups in _groups)
            {
                groups.Clear();
            }

            _latest.Clear();
            Generation++;
            Version++;
        }

        public static int Category(ConsoleLogType type) => type switch
        {
            ConsoleLogType.Log => 1,
            ConsoleLogType.Warning => 2,
            ConsoleLogType.Dev => 4,
            _ => 3
        };

        private static List<T>[] CreateIndexes<T>()
        {
            var indexes = new List<T>[(int)ConsoleLogFilter.All + 1];

            for (var index = 0; index < indexes.Length; index++)
            {
                indexes[index] = new List<T>();
            }

            return indexes;
        }

        private sealed class Group
        {
            internal long id;

            internal ConsoleLogRecord last;

            internal int count; // 仅连续重复累计.

            internal int categories;

            internal ConsoleLogRecord[] lastByCategory;

            internal int[] countByCategory; // 类型筛选中的次数仅统计匹配的原始记录.
        }
    }
}
