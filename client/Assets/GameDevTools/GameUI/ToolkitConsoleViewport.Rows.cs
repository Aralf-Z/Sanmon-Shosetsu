using System;
using System.Collections.Generic;
using System.Diagnostics;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitConsoleViewport
    {
        private static readonly double PREPARATION_TICKS = Stopwatch.Frequency * Config.CONSOLE_PREPARATION_SECONDS; // 单条排版不可中断.

        private readonly List<Row> _pool = new(); // 只为可见区域和各一屏缓冲准备节点.
        private readonly List<Row> _activeRows = new(); // 不要求节点在层级中连续排列.
        private readonly Dictionary<long, Row> _boundRows = new(); // 按编号保留交集节点.
        private readonly Stack<Row> _freeRows = new(); // 跨筛选和切页复用模板.
        private int _budgetFrame = -1; // 同一帧的多次刷新共用预算.
        private int _preparedThisFrame; // 本帧新绑定或重新测量的行数.
        private long _budgetStart; // 第一次准备新条目时开始计时.

        // 从可见区域底部向上准备, 再补上下各一屏缓冲, 不测量整个最近范围.
        private void Render()
        {
            if (!_visible || _disposed || !Valid(_width) || !Valid(_height)) return;
            _dirty = false;
            if (_model.Count == 0) { PruneRows(0, 0); CommitRows(); return; }
            if (!EnsureMetrics()) { _dirty = true; return; }
            Bounds(0, 0, out var visibleFirst, out var visibleEnd);
            Bounds(1, 1, out var first, out var end);
            PruneRows(first, end);
            var budget = PrepareRange(visibleFirst, visibleEnd);
            Bounds(0, 0, out visibleFirst, out visibleEnd);
            Bounds(1, 1, out first, out end);
            if (budget) budget = PrepareRange(first, visibleFirst);
            Bounds(1, 1, out first, out end);
            if (budget) PrepareRange(visibleEnd, end);
            Bounds(1, 1, out first, out end);
            PruneRows(first, end);
            for (var index = first; index < end; index++)
            {
                var item = _model.GetItem(index);
                if (!_boundRows.TryGetValue(item.Id, out var row) || !RowMatches(row, item)) { _dirty = true; break; }
            }
            CommitRows();
        }

        private void Bounds(int above, int below, out int first, out int end)
        {
            if (_model.Count == 0) { first = end = 0; return; }
            var offset = CurrentOffset() - _heights.TopPadding(_height);
            first = _heights.Find(Mathf.Max(0, offset - _height * above));
            end = Mathf.Min(_model.Count, _heights.Find(Mathf.Max(0, offset + _height * (below + 1))) + 1);
        }

        // 测量长日志后即时收窄待准备区间, 避免先创建一整屏无效占位行.
        private bool PrepareRange(int first, int end)
        {
            for (var index = end - 1; index >= first; index--)
            {
                Bounds(1, 1, out var wantedFirst, out var wantedEnd);
                if (index < wantedFirst) break;
                if (index >= wantedEnd) continue;
                var item = _model.GetItem(index);
                _boundRows.TryGetValue(item.Id, out var row);
                if (row != null && RowMatches(row, item))
                {
                    row.index = index;
                    row.record = item.Record;
                    AcceptHeight(index, row.layout.height);
                    continue;
                }
                if (!ReservePreparation()) { _dirty = true; return false; }
                var layout = GetLayout(item);
                AcceptHeight(index, layout.height);
                if (row == null)
                {
                    row = _freeRows.Count > 0 ? _freeRows.Pop() : MakeRow();
                    row.id = item.Id;
                    _boundRows.Add(row.id, row);
                    _activeRows.Add(row);
                }
                row.index = index;
                row.record = item.Record;
                BindRow(row, layout);
            }
            return true;
        }

        private bool ReservePreparation()
        {
            if (_budgetFrame != Time.frameCount) { _budgetFrame = Time.frameCount; _preparedThisFrame = 0; }
            var now = Stopwatch.GetTimestamp();
            if (_preparedThisFrame >= Config.CONSOLE_ROWS_PER_FRAME || _preparedThisFrame > 0 && now - _budgetStart >= PREPARATION_TICKS) return false;
            if (_preparedThisFrame == 0) _budgetStart = now;
            _preparedThisFrame++;
            return true;
        }

        private bool RowMatches(Row row, ConsoleLogItem item) => row.ready && row.layout != null && row.layout.Matches(item, DisplayCount(item), _width, _fontKey, PresentationKey);

        // 新筛选不混入旧条目, 未准备好的行隐藏但保留节点供预算内复用.
        private void PruneRows(int first, int end)
        {
            for (var slot = _activeRows.Count - 1; slot >= 0; slot--)
            {
                var row = _activeRows[slot];
                var index = row.index;
                if (index < 0 || index >= _model.Count || _model.GetItem(index).Id != row.id) index = _model.Find(row.id);
                row.index = index;
                if (index < first || index >= end)
                {
                    row.root.RemoveFromHierarchy();
                    row.ready = false;
                    row.index = -1;
                    _boundRows.Remove(row.id);
                    _activeRows.RemoveAt(slot);
                    _freeRows.Push(row);
                }
                else if (!RowMatches(row, _model.GetItem(index)))
                {
                    row.ready = false;
                    row.root.AddToClassList("console-log-unprepared");
                }
            }
        }

        private void BindRow(Row row, RowLayout layout)
        {
            if (row.root.parent != _host)
            {
                _host.Add(row.root);
                ToolkitTextOpacity.Track(row.root);
            }
            if (row.text.text != layout.text) row.text.text = layout.text;
            if (row.count.text != layout.countText) row.count.text = layout.countText;
            row.count.EnableInClassList("is-hidden", layout.count == 0);
            row.marker.EnableInClassList("is-hidden", !_showIcons);
            row.root.style.height = layout.height;
            row.text.style.height = layout.textHeight;
            row.layout = layout;
            row.ready = true;
            row.root.RemoveFromClassList("is-hidden");
            row.root.RemoveFromClassList("console-log-unprepared");
        }

        // 可视层直接使用逻辑位置, 占位高度与滑块更新不会先移动再修正文字.
        private void CommitRows()
        {
            _offset = CurrentOffset();
            _anchor = default;
            var extent = Mathf.Max(_height, _heights.Total);
            if (_extentHeight != extent)
            {
                _extentHeight = extent;
                _extentPending = true;
                _extent.style.height = extent;
            }
            PositionRows();
            ApplyNativeOffset();
        }

        // 原生滚动回调也可立即移动已有节点, 新行仍留给共享预算准备.
        private void PositionRows()
        {
            foreach (var row in _activeRows)
            {
                if (!row.ready || row.index < 0 || row.index >= _model.Count) continue;
                var top = _heights.RowTop(row.index, _height, _offset);
                if (row.top != top) { row.top = top; row.root.style.translate = new Translate(0, top, 0); }
                var selected = row.id == SelectedId;
                if (row.selected != selected) { row.selected = selected; row.root.EnableInClassList("selected", selected); }
                var category = ConsoleLogStore.Category(row.record.Type);
                if (row.category == category && row.languageVersion == ToolkitLocalization.Version) continue;
                row.category = category;
                row.languageVersion = ToolkitLocalization.Version;
                row.marker.text = category switch { 2 => ToolkitLocalization.Get("icon.warning"), 3 => ToolkitLocalization.Get("icon.error"), 4 => ToolkitLocalization.Get("icon.dev"), _ => ToolkitLocalization.Get("icon.log") };
                row.root.EnableInClassList("console-log-warning", category == 2);
                row.root.EnableInClassList("console-log-error", category == 3);
            }
        }

        private Row MakeRow(bool pooled = true)
        {
            var row = new Row { root = new VisualElement(), marker = new Label { pickingMode = PickingMode.Ignore }, count = new Label() };
            row.root.AddToClassList("console-log-row");
            row.marker.AddToClassList("console-log-type");
            var body = ToolkitFactory.InstantiateTemplate(_template);
            body.AddToClassList("console-log-body");
            row.text = ToolkitView.Require<Label>(body, "itemText");
            row.count.AddToClassList("console-log-count");
            row.root.Add(row.marker);
            row.root.Add(body);
            row.root.Add(row.count);
            _host.Add(row.root);
            ToolkitTextOpacity.Track(row.root);
            if (!pooled)
            {
                row.root.AddToClassList("console-log-measurer");
                row.root.pickingMode = PickingMode.Ignore;
                return row;
            }
            row.root.AddToClassList("console-log-unprepared");
            row.root.RegisterCallback<PointerDownEvent>(evt => { if (evt.button == 0) { row.down = evt.position; row.downId = row.id; } });
            row.root.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.button == 0 && row.ready && row.downId == row.id && ((Vector2)evt.position - row.down).sqrMagnitude <= 16f) _select(row.id);
                row.downId = 0;
            });
            _pool.Add(row);
            return row;
        }

        private sealed class Row
        {
            /// <summary> 不参与原生内容布局的行宿主. </summary>
            internal VisualElement root;
            /// <summary> 与筛选按钮一致的类型标记, 不加入原始正文. </summary>
            internal Label marker;
            /// <summary> 沿用原始富文本. </summary>
            internal Label text;
            /// <summary> 折叠次数. </summary>
            internal Label count;
            /// <summary> 当前不可变记录引用. </summary>
            internal ConsoleLogRecord record;
            /// <summary> 字符串和行高一起复用. </summary>
            internal RowLayout layout;
            /// <summary> 稳定日志编号. </summary>
            internal long id;
            /// <summary> 行回收后不把旧按下事件当作新条目点击. </summary>
            internal long downId;
            /// <summary> 当前逻辑范围内的索引. </summary>
            internal int index = -1;
            /// <summary> 只在类型变化时更新样式. </summary>
            internal int category = -1;
            /// <summary> 图标缓存也随语言版本失效. </summary>
            internal int languageVersion = -1;
            /// <summary> 已提交的可视位置. </summary>
            internal float top = float.NaN;
            /// <summary> 正文与高度均已准备好才显示. </summary>
            internal bool ready;
            /// <summary> 已应用的选中状态. </summary>
            internal bool selected;
            /// <summary> 选择时排除滚动手势. </summary>
            internal Vector2 down;
        }
    }
}
