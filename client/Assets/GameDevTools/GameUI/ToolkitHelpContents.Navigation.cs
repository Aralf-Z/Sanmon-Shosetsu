namespace GameConsole.UI
{
    internal static partial class ToolkitHelpContents
    {
        internal static ToolkitHelpGroup[] navigation => new ToolkitHelpGroup[]
        { new(ToolkitLocalization.Get("help.group.overview"), ToolkitHelpTopic.Window), new(ToolkitLocalization.Get("help.group.console"), ToolkitHelpTopic.Console, ToolkitHelpTopic.Stack), new(ToolkitLocalization.Get("help.group.cheat"), ToolkitHelpTopic.CheatCommands, ToolkitHelpTopic.CheatParameters, ToolkitHelpTopic.CheatPresets, ToolkitHelpTopic.CheatMacros), new(ToolkitLocalization.Get("help.group.macro"), ToolkitHelpTopic.MacroLibrary, ToolkitHelpTopic.MacroEditor, ToolkitHelpTopic.MacroSyntax), new(ToolkitLocalization.Get("help.group.options"), ToolkitHelpTopic.InterfaceSettings, ToolkitHelpTopic.ToolSettings, ToolkitHelpTopic.ConsoleSettings, ToolkitHelpTopic.CheatSettings, ToolkitHelpTopic.FloatingSettings) };
    }

    /// <summary> 只定义导航分组, 文案仍由各主题独立维护. </summary>
    internal sealed class ToolkitHelpGroup
    {
        internal string Title { get; }

        internal ToolkitHelpTopic[] Topics { get; }

        internal ToolkitHelpGroup(string title, params ToolkitHelpTopic[] topics)
        {
            Title = title;
            Topics = topics;
        }
    }
}
