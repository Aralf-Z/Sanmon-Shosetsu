namespace GameConsole.UI
{
    internal static partial class ToolkitHelpContents
    {
        /// <summary> 左侧导航的分组与主题顺序; 特性叶节点直接取各主题的文案列表. </summary>
        internal static ToolkitHelpGroup[] navigation => new ToolkitHelpGroup[]
        {
            new(ToolkitLocalization.Get("help.group.overview"), ToolkitHelpTopic.Window),
            new(ToolkitLocalization.Get("help.group.console"), ToolkitHelpTopic.Console, ToolkitHelpTopic.Stack),
            new(ToolkitLocalization.Get("help.group.cheat"), ToolkitHelpTopic.CheatCommands, ToolkitHelpTopic.CheatParameters, ToolkitHelpTopic.CheatPresets, ToolkitHelpTopic.CheatMacros),
            new(ToolkitLocalization.Get("help.group.macro"), ToolkitHelpTopic.MacroLibrary, ToolkitHelpTopic.MacroEditor, ToolkitHelpTopic.MacroSyntax),
            new(ToolkitLocalization.Get("help.group.options"), ToolkitHelpTopic.InterfaceSettings, ToolkitHelpTopic.ToolSettings, ToolkitHelpTopic.ConsoleSettings, ToolkitHelpTopic.CheatSettings, ToolkitHelpTopic.FloatingSettings)
        };
    }

    /// <summary> 只定义导航分组, 文案仍由各主题独立维护. </summary>
    internal sealed class ToolkitHelpGroup
    {
        /// <summary> 一级页签名称. </summary>
        internal string Title { get; }
        /// <summary> 组内主题的展示顺序. </summary>
        internal ToolkitHelpTopic[] Topics { get; }

        /// <summary> 配置分组标题及其稳定主题标识. </summary>
        internal ToolkitHelpGroup(string title, params ToolkitHelpTopic[] topics) { Title = title; Topics = topics; }
    }
}
