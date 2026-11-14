using System;

namespace GameConsole.UI
{
    /// <summary> 日志行高的前缀索引, 支持测量更新和按滚动位置定位. </summary>
    internal sealed class ToolkitConsoleHeightIndex
    {
        /// <summary> 当前记录数. </summary>
        internal int Count { get; private set; }
        /// <summary> 当前估算与已测量行高的总和. </summary>
        internal float Total => Prefix(Count);
        private float[] _values = Array.Empty<float>(); // 每条已加载逻辑行的高度.
        private float[] _tree = new float[1]; // Fenwick 树, 滚动查询不线性扫描历史.

        /// <summary> 加载区间变化时重建, 采用上次有效测量或最低行高. </summary>
        internal void Reset(int count, Func<int, float> height)
        {
            if (_values.Length < count)
            {
                var capacity = Math.Max(count, Math.Max(16, _values.Length * 2));
                _values = new float[capacity];
                _tree = new float[capacity + 1];
            }
            Count = count;
            Array.Clear(_tree, 0, count + 1);
            for (var index = 0; index < count; index++)
            {
                var value = Math.Max(1, height(index));
                _values[index] = value;
                _tree[index + 1] += value;
                var parent = index + 1 + ((index + 1) & -(index + 1));
                if (parent <= count) _tree[parent] += _tree[index + 1];
            }
        }

        /// <summary> 更新已测量行高, 返回是否发生有效变化. </summary>
        internal bool Set(int index, float height)
        {
            if (index < 0 || index >= Count || float.IsNaN(height) || float.IsInfinity(height) || height <= 0 || Math.Abs(_values[index] - height) < .25f) return false;
            var delta = height - _values[index];
            _values[index] = height;
            for (var node = index + 1; node <= Count; node += node & -node) _tree[node] += delta;
            return true;
        }

        /// <summary> 索引之前的累计高度. </summary>
        internal float Prefix(int end)
        {
            var sum = 0f;
            for (var node = Math.Min(end, Count); node > 0; node -= node & -node) sum += _tree[node];
            return sum;
        }

        /// <summary> 对应滚动位置所在的行, 空区间返回 0. </summary>
        internal int Find(float offset)
        {
            var index = 0;
            var sum = 0f;
            var bit = 1;
            while (bit <= Count / 2) bit <<= 1;
            for (; bit > 0; bit >>= 1)
            {
                var next = index + bit;
                if (next <= Count && sum + _tree[next] <= offset) { index = next; sum += _tree[next]; }
            }
            return Math.Min(index, Math.Max(0, Count - 1));
        }

        /// <summary> 不足一屏时把空白留在顶部. </summary>
        internal float TopPadding(float viewportHeight) => Math.Max(0, viewportHeight - Total);

        /// <summary> 原生滚动条的有效滚动范围. </summary>
        internal float MaxOffset(float viewportHeight) => Math.Max(0, Total - viewportHeight);

        /// <summary> 日志相对视口的位置, 与原生内容布局更新时机解耦. </summary>
        internal float RowTop(int index, float viewportHeight, float offset)
        {
            // 跟随时从尾部累计少量行高, 避免长历史的大浮点数相减影响最新行位置.
            if (Math.Abs(offset - MaxOffset(viewportHeight)) < .01f) return viewportHeight - Suffix(index);
            return TopPadding(viewportHeight) + Prefix(index) - offset;
        }

        // 直接累加覆盖尾部的树节点, 不使用 Total - Prefix 的大数相减.
        private float Suffix(int start)
        {
            var sum = 0f;
            for (var end = Count; end > start;)
            {
                var previous = end - (end & -end);
                if (previous >= start) { sum += _tree[end]; end = previous; }
                else { sum += _values[end - 1]; end--; }
            }
            return sum;
        }
    }
}
