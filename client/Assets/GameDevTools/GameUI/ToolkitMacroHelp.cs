using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using GameConsole.Extension;

namespace GameConsole.UI
{
    /// <summary> 宏语法参考和两组有限颜色选择, 不修改编辑中的正文. </summary>
    internal sealed class ToolkitMacroHelp : IDisposable
    {
        private readonly Label _title;

        private readonly Label _description;

        private readonly Label _example;

        private readonly VisualElement _colors;

        private readonly ScrollView _details;

        private readonly Button _reset;

        private readonly List<Button> _keywords = new();

        private readonly List<Button> _palette = new();

        private readonly List<Action> _unbind = new();

        private Entry[] _entries = CreateEntries();

        private static Entry[] CreateEntries() => new Entry[]
        {
            new(ToolkitLocalization.Get("macro.syntax.command.title"), ToolkitLocalization.Get("macro.syntax.command.description"), ToolkitLocalization.Get("macro.syntax.command.example")),
            new(ToolkitLocalization.Get("macro.syntax.string.title"), ToolkitLocalization.Get("macro.syntax.string.description"), ToolkitLocalization.Get("macro.syntax.string.example")),
            new(ToolkitLocalization.Get("macro.syntax.comment.title"), ToolkitLocalization.Get("macro.syntax.comment.description"), ToolkitLocalization.Get("macro.syntax.comment.example")),
            new(ToolkitLocalization.Get("macro.syntax.repeat.title"), ToolkitLocalization.Get("macro.syntax.repeat.description"), ToolkitLocalization.Get("macro.syntax.repeat.example"), "Repeat"),
            new(ToolkitLocalization.Get("macro.syntax.endRepeat.title"), ToolkitLocalization.Get("macro.syntax.endRepeat.description"), ToolkitLocalization.Get("macro.syntax.endRepeat.example"), "Repeat"),
            new(ToolkitLocalization.Get("macro.syntax.waitFrame.title"), ToolkitLocalization.Get("macro.syntax.waitFrame.description"), ToolkitLocalization.Get("macro.syntax.waitFrame.example"), "WaitFor"),
            new(ToolkitLocalization.Get("macro.syntax.waitSeconds.title"), ToolkitLocalization.Get("macro.syntax.waitSeconds.description"), ToolkitLocalization.Get("macro.syntax.waitSeconds.example"), "WaitFor"),
            new(ToolkitLocalization.Get("macro.syntax.waitCommand.title"), ToolkitLocalization.Get("macro.syntax.waitCommand.description"), ToolkitLocalization.Get("macro.syntax.waitCommand.example"), "WaitFor")
        };

        private int _selected = 3;

        internal ToolkitMacroHelp(VisualElement root)
        {
            _title = ToolkitView.Require<Label>(root, "macroHelpTitle");
            _description = ToolkitView.Require<Label>(root, "macroHelpDescription");
            _example = ToolkitView.Require<Label>(root, "macroHelpExample");
            _colors = ToolkitView.Require<VisualElement>(root, "macroColorOptions");
            _details = ToolkitView.Require<ScrollView>(root, "macroHelpDetailsScroll");
            _reset = ToolkitView.Require<Button>(root, "resetMacroColor");
            var list = ToolkitView.Require<VisualElement>(root, "macroKeywords");

            try
            {
                for (var i = 0; i < _entries.Length; i++)
                {
                    var index = i;
                    var button = new Button
                    { text = _entries[i].title };

                    button.AddToClassList("macro-keyword");
                    Action click = () =>
                    {
                        _selected = index;
                        Refresh();
                        _details.scrollOffset = UnityEngine.Vector2.zero;
                    };

                    button.clicked += click;
                    _unbind.Add(() => button.clicked -= click);
                    list.Add(button);
                    _keywords.Add(button);
                }

                for (var i = 0; i < MacroSyntaxColors.palette.Length; i++)
                {
                    var color = MacroSyntaxColors.palette[i];
                    var button = ToolkitView.Require<Button>(root, "macroColor" + i);

                    button.tooltip = MacroSyntaxColors.names[i] + " " + color;
                    Action click = () => SetColor(color);

                    button.clicked += click;
                    _unbind.Add(() => button.clicked -= click);
                    _palette.Add(button);
                }

                _reset.clicked += Reset;
                Config.e_onChanged += Refresh;
                Refresh();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary> 更新参考文字与预选颜色名称, 保留当前关键字和编辑正文. </summary>
        internal void RefreshLanguage()
        {
            _entries = CreateEntries();
            for (var i = 0; i < _keywords.Count; i++)
            {
                _keywords[i].text = _entries[i].title;
            }

            for (var i = 0; i < _palette.Count; i++)
            {
                _palette[i].tooltip = MacroSyntaxColors.names[i] + " " + MacroSyntaxColors.palette[i];
            }

            Refresh();
        }

        public void Dispose()
        {
            Config.e_onChanged -= Refresh;
            _reset.clicked -= Reset;
            foreach (var unbind in _unbind)
            {
                unbind();
            }

            foreach (var keyword in _keywords)
            {
                keyword.RemoveFromHierarchy();
            }

            _unbind.Clear();
            _keywords.Clear();
        }

        private void SetColor(string color)
        {
            var group = _entries[_selected].colorGroup;

            if (group == null) return;

            Config.Edit(data =>
            {
                if (group == "WaitFor")
                {
                    data.macroWaitForColor = color;
                }
                else
                {
                    data.macroRepeatColor = color;
                }
            });
        }

        private void Reset() => SetColor(_entries[_selected].colorGroup == "WaitFor" ? MacroSyntaxColors.defaultWait : MacroSyntaxColors.defaultRepeat);

        private void Refresh()
        {
            var entry = _entries[_selected];

            _title.text = entry.title;
            _description.text = entry.description;
            _example.text = entry.example;
            _colors.EnableInClassList("is-hidden", entry.colorGroup == null);
            var color = entry.colorGroup == "WaitFor"
                ? Config.MacroWaitForColor
                : Config.MacroRepeatColor;

            for (var i = 0; i < _palette.Count; i++)
            {
                _palette[i].EnableInClassList("selected", MacroSyntaxColors.palette[i] == color);
            }

            for (var i = 0; i < _keywords.Count; i++)
            {
                _keywords[i].EnableInClassList("selected", i == _selected);
            }
        }

        private sealed class Entry
        {
            internal readonly string title;

            internal readonly string description;

            internal readonly string example;

            internal readonly string colorGroup;

            internal Entry(string title, string description, string example, string colorGroup = null)
            {
                this.title = title;
                this.description = description;
                this.example = example;
                this.colorGroup = colorGroup;
            }
        }
    }
}
