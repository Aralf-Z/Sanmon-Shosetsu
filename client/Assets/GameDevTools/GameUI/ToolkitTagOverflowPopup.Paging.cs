using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitTagOverflowPopup
    {
        private void BuildPages(bool paged)
        {
            _pages.Clear();
            var start = 0;
            var top = 0f;

            foreach (var row in _rows)
            {
                if (paged && row.start > start && row.top - top + row.height > _contentHeight + .01f)
                {
                    _pages.Add(new Page(start, row.start, top));
                    start = row.start;
                    top = row.top;
                }
            }

            _pages.Add(new Page(start, _buttons.Count, top));
            _page = _pages.FindIndex(page => page.start <= _anchor && _anchor < page.end);
            _page = Mathf.Max(0, _page);
        }

        private void ApplyPage()
        {
            var page = _pages[_page];
            var paddingLeft = _grid.resolvedStyle.paddingLeft;
            var paddingTop = _grid.resolvedStyle.paddingTop;

            for (var index = 0; index < _buttons.Count; index++)
            {
                var btnTag = _buttons[index];
                var visible = index >= page.start && index < page.end;

                btnTag.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (!visible) continue;

                var rect = _rects[index];

                btnTag.style.left = rect.x + paddingLeft;
                btnTag.style.top = rect.y - page.top + paddingTop;
                btnTag.style.width = rect.width;
                btnTag.style.height = rect.height;
                btnTag.style.minHeight = Mathf.Min(26f, rect.height);
                btnTag.style.maxHeight = rect.height;
            }

            _pager.SetPage(_page, _pages.Count, ToolkitLocalization.Format("ui.TagFilter.pageTooltip", ("page", _page + 1), ("pages", _pages.Count), ("count", _tags.Count)));
        }

        private void ChangePage(int page)
        {
            if (!IsOpen || page < 0 || page >= _pages.Count || page == _page) return;

            _page = page;
            _anchor = _pages[page].start;
            ApplyPage();
            _host.Focus();
        }

        private readonly struct Page
        {
            internal readonly int start;

            internal readonly int end;

            internal readonly float top;

            internal Page(int start, int end, float top)
            {
                this.start = start;
                this.end = end;
                this.top = top;
            }
        }
    }
}
