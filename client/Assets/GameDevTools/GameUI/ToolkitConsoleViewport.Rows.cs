using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using RedSaw.CommandLineInterface;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitConsoleViewport
    {
        private static readonly double PREPARATION_TICKS = Stopwatch.Frequency * Config.CONSOLE_PREPARATION_SECONDS;

        private readonly List<Row> _pool = new();

        private readonly List<Row> _activeRows = new(); // 不要求节点在层级中连续排列.

        private readonly Dictionary<long, Row> _boundRows = new(); // 按编号保留交集节点.

        private readonly Stack<Row> _freeRows = new();

        private int _budgetFrame = -1;

        private int _preparedThisFrame;

        private long _budgetStart;

        // 从可见区域底部向上准备, 再补上下各一屏缓冲, 不测量整个最近范围.
        private void Render()
        {
            if (!_visible || _disposed || !Valid(_width) || !Valid(_height)) return;

            _dirty = false;
            if (_model.Count == 0)
            {
                PruneRows(0, 0);
                CommitRows();
                return;
            }

            if (!EnsureMetrics())
            {
                _dirty = true;
                return;
            }

            Bounds(0, 0, out var visibleFirst, out var visibleEnd);
            Bounds(1, 1, out var first, out var end);
            PruneRows(first, end);
            var budget = PrepareRange(visibleFirst, visibleEnd);

            Bounds(0, 0, out visibleFirst, out visibleEnd);
            Bounds(1, 1, out first, out end);
            if (budget)
            {
                budget = PrepareRange(first, visibleFirst);
            }

            Bounds(1, 1, out first, out end);
            if (budget)
            {
                PrepareRange(visibleEnd, end);
            }

            Bounds(1, 1, out first, out end);
            PruneRows(first, end);
            for (var index = first; index < end; index++)
            {
                var item = _model.GetItem(index);

                if (!_boundRows.TryGetValue(item.Id, out var row) || !RowMatches(row, item))
                {
                    _dirty = true;
                    break;
                }
            }

            CommitRows();
        }

        private void Bounds(int above, int below, out int first, out int end)
        {
            if (_model.Count == 0)
            {
                first = end = 0;
                return;
            }

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
                if (index < wantedFirst)
                {
                    break;
                }

                if (index >= wantedEnd)
                {
                    continue;
                }

                var item = _model.GetItem(index);

                _boundRows.TryGetValue(item.Id, out var row);
                if (row != null && RowMatches(row, item))
                {
                    row.index = index;
                    row.record = item.Record;
                    AcceptHeight(index, row.layout.height);
                    continue;
                }

                if (!ReservePreparation())
                {
                    _dirty = true;
                    return false;
                }

                var layout = GetLayout(item);

                AcceptHeight(index, layout.height);
                if (row == null)
                {
                    row = _freeRows.Count > 0
                        ? _freeRows.Pop()
                        : MakeRow();
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
            if (_budgetFrame != Time.frameCount)
            {
                _budgetFrame = Time.frameCount;
                _preparedThisFrame = 0;
            }

            var now = Stopwatch.GetTimestamp();

            if (_preparedThisFrame >= Config.CONSOLE_ROWS_PER_FRAME || _preparedThisFrame > 0 && now - _budgetStart >= PREPARATION_TICKS) return false;

            if (_preparedThisFrame == 0)
            {
                _budgetStart = now;
            }

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

                if (index < 0 || index >= _model.Count || _model.GetItem(index).Id != row.id)
                {
                    index = _model.Find(row.id);
                }

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

            if (row.text.text != layout.text)
            {
                row.text.text = layout.text;
            }

            if (row.count.text != layout.countText)
            {
                row.count.text = layout.countText;
            }

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
                if (!row.ready || row.index < 0 || row.index >= _model.Count)
                {
                    continue;
                }

                var top = _heights.RowTop(row.index, _height, _offset);

                if (row.top != top)
                {
                    row.top = top;
                    row.root.style.translate = new Translate(0, top, 0);
                }

                var selected = row.id == SelectedId;

                if (row.selected != selected)
                {
                    row.selected = selected;
                    row.root.EnableInClassList("selected", selected);
                }

                var category = ConsoleLogStore.Category(row.record.Type);

                if (row.category == category && row.languageVersion == ToolkitLocalization.Version)
                {
                    continue;
                }

                row.category = category;
                row.languageVersion = ToolkitLocalization.Version;
                var marker = category switch
                {
                    2 => ToolkitLocalization.Get("icon.warning"),
                    3 => ToolkitLocalization.Get("icon.error"),
                    4 => ToolkitLocalization.Get("icon.dev"),
                    _ => ToolkitLocalization.Get("icon.log")
                };

                ToolkitMixedText.SetText(row.marker, marker, ToolkitMixedTextMode.Control);
                row.root.EnableInClassList("console-log-warning", category == 2);
                row.root.EnableInClassList("console-log-error", category == 3);
            }
        }

        private Row MakeRow(bool pooled = true)
        {
            var row = new Row
            {
                root = new VisualElement(),
                marker = new Label
                { pickingMode = PickingMode.Ignore },
                count = new Label()
            };

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
            row.root.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    row.down = evt.position;
                    row.downId = row.id;
                }
            });
            row.root.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.button == 0 && row.ready && row.downId == row.id && ((Vector2)evt.position - row.down).sqrMagnitude <= 16f)
                {
                    _select(row.id);
                }

                row.downId = 0;
            });
            _pool.Add(row);
            return row;
        }

        private sealed class Row
        {
            internal VisualElement root;

            internal Label marker;

            internal Label text;

            internal Label count;

            internal ConsoleLogRecord record;

            internal RowLayout layout;

            internal long id;

            internal long downId;

            /// <summary> 当前逻辑范围内的索引. </summary>
            internal int index = -1;

            internal int category = -1;

            internal int languageVersion = -1;

            internal float top = float.NaN;

            internal bool ready;

            internal bool selected;

            internal Vector2 down;
        }
    }
}
