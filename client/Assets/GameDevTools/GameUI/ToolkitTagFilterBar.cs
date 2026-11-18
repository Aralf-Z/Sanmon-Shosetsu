using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 共用单选标签栏, 行内保留完整按钮, 溢出标签由锚定弹窗访问. </summary>
    internal sealed partial class ToolkitTagFilterBar : IDisposable
    {
        private const float GAP = 4f;

        internal string SelectedTag { get; private set; }

        internal event Action<string> e_onChanged;

        private readonly VisualElement _host;

        private readonly VisualElement _toolbar;

        private readonly VisualElement _searchGroup;

        private readonly Button _btnAll;

        private readonly Button _btnMore;

        private readonly Button _btnMeasure;

        private readonly List<Button> _buttons = new();

        private readonly Dictionary<string, float> _widths = new(StringComparer.Ordinal);

        private readonly IVisualElementScheduledItem _update;

        private readonly ToolkitTagOverflowPopup _popup;

        private string[] _tags = Array.Empty<string>();

        private bool _dirty = true;

        private bool _disposed;

        private int _fontKey;

        private int _languageVersion = -1;

        private int _appearanceVersion = -1;

        private int _layoutFrame;

        private int _visibleCount;

        internal ToolkitTagFilterBar(VisualElement host, VisualElement toolbar, VisualElement searchGroup)
        {
            _host = host;
            _toolbar = toolbar;
            _searchGroup = searchGroup;
            _host.AddToClassList("tag-filter-bar");
            _toolbar.AddToClassList("tag-filter-toolbar");
            _searchGroup.AddToClassList("tag-toolbar-search");
            _btnAll = CreateButton("tagFilterAll", () => Select(null));
            _btnAll.AddToClassList("tag-filter-first");
            _btnMore = CreateButton("tagFilterMore", TogglePopup);
            _host.Add(_btnAll);
            _host.Add(_btnMore);
            _btnMeasure = CreateButton("tagFilterMeasure", null);
            _btnMeasure.pickingMode = PickingMode.Ignore;
            _btnMeasure.focusable = false;
            _btnMeasure.AddToClassList("selected");
            _btnMeasure.AddToClassList("tag-filter-measure");
            _toolbar.Add(_btnMeasure);
            _popup = new ToolkitTagOverflowPopup(_btnMore, _host, Select);
            _host.RegisterCallback<GeometryChangedEvent>(Geometry);
            _toolbar.RegisterCallback<GeometryChangedEvent>(Geometry);
            _host.RegisterCallback<DetachFromPanelEvent>(Detached);
            _update = _host.schedule.Execute(Tick).Every(16);
            RefreshLanguage();
        }

        internal void SetTags(IEnumerable<string> tags)
        {
            var values = (tags ?? Array.Empty<string>()).Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(tag => tag, StringComparer.Ordinal)
                .ToArray();

            Close();
            SelectedTag = null;
            if (!_tags.SequenceEqual(values))
            {
                _tags = values;
                _widths.Clear();
            }

            _dirty = true;
            UpdateSelection();
        }

        internal void RefreshLanguage()
        {
            _btnAll.text = ToolkitLocalization.Get("ui.CheatPanel.EnsureCommands");
            _btnAll.tooltip = _btnAll.text;
            _btnMore.text = ToolkitLocalization.Get("ui.TagFilter.more");
            _widths.Clear();
            _dirty = true;
            UpdateSelection();
            _popup.Refresh(_tags, SelectedTag);
        }

        internal void Close() => _popup.Close();

        private static Button CreateButton(string name, Action click)
        {
            var btnTag = new Button(click) { name = name, enableRichText = false, parseEscapeSequences = false };

            btnTag.AddToClassList("category-button");
            btnTag.AddToClassList("tag-filter-button");
            return btnTag;
        }

        private void Tick()
        {
            if (_disposed || _host.panel == null || _toolbar.contentRect.width <= 0) return;

            if (_appearanceVersion != ToolkitFactory.AppearanceVersion || _languageVersion != ToolkitLocalization.Version)
            {
                _appearanceVersion = ToolkitFactory.AppearanceVersion;
                _languageVersion = ToolkitLocalization.Version;
                _layoutFrame = Time.frameCount + 1;
                RefreshLanguage();
            }

            if (Application.isPlaying && Time.frameCount < _layoutFrame) return;

            var key = ToolkitFontMetrics.GetKey(_btnMeasure);

            if (_fontKey != key)
            {
                _fontKey = key;
                _widths.Clear();
                _dirty = true;
            }

            if (!_dirty) return;

            Layout();
        }

        private float Measure(string text)
        {
            if (_widths.TryGetValue(text, out var cached)) return cached;

            var measured = _btnMeasure.MeasureTextSize(text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            var style = _btnMeasure.resolvedStyle;
            var width = measured.x + style.paddingLeft + style.paddingRight + style.borderLeftWidth + style.borderRightWidth;

            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0) return 48f;

            width = Mathf.Ceil(Mathf.Max(32f, width));
            _widths[text] = width;
            return width;
        }

        private void ApplyButtons(int count, bool overflow)
        {
            while (_buttons.Count < count)
            {
                Button btnTag = null;

                btnTag = CreateButton("tagFilterItem", () => Select(btnTag.userData as string));
                _buttons.Add(btnTag);
                _host.Insert(_host.childCount - 1, btnTag);
            }

            for (var index = 0; index < _buttons.Count; index++)
            {
                var btnTag = _buttons[index];

                btnTag.style.display = index < count ? DisplayStyle.Flex : DisplayStyle.None;
                if (index >= count) continue;

                btnTag.userData = _tags[index];
                btnTag.text = _tags[index];
                btnTag.tooltip = _tags[index];
                SetWidth(btnTag, Measure(_tags[index]));
            }

            _visibleCount = count;
            _btnMore.style.display = overflow ? DisplayStyle.Flex : DisplayStyle.None;
            if (!overflow)
            {
                _popup.Close();
            }

            UpdateSelection();
            ToolkitTextOpacity.Track(_host);
        }

        private void Select(string tag)
        {
            SelectedTag = tag;
            _dirty = true;
            _popup.Close();
            UpdateSelection();
            e_onChanged?.Invoke(tag);
        }

        private void UpdateSelection()
        {
            _btnAll.EnableInClassList("selected", SelectedTag == null);
            for (var index = 0; index < _buttons.Count; index++)
            {
                _buttons[index].EnableInClassList("selected", _buttons[index].userData as string == SelectedTag);
            }

            var hidden = SelectedTag != null && Array.IndexOf(_tags, SelectedTag) >= _visibleCount;

            _btnMore.EnableInClassList("selected", hidden);
            _btnMore.tooltip = hidden
                ? ToolkitLocalization.Format("ui.TagFilter.current", ("tag", SelectedTag))
                : ToolkitLocalization.Get("ui.TagFilter.moreTooltip");
        }

        private void TogglePopup()
        {
            if (_popup.IsOpen)
            {
                _popup.Close(true);
            }
            else
            {
                _popup.Show(_tags, SelectedTag);
            }
        }

        private void Geometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size != evt.newRect.size)
            {
                _dirty = true;
            }
        }

        private void Detached(DetachFromPanelEvent evt) => Close();

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _update.Pause();
            _popup.Dispose();
            _host.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _toolbar.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _host.UnregisterCallback<DetachFromPanelEvent>(Detached);
            _btnMeasure.RemoveFromHierarchy();
            _host.Clear();
            e_onChanged = null;
        }
    }
}
