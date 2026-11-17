using System.Collections.Generic;

namespace GameConsole.UI
{
    /// <summary> 主工具帮助的稳定标识和特性顺序, 显示文案由语言 JSON 提供. </summary>
    internal static partial class ToolkitHelpContents
    {
        private static Dictionary<ToolkitHelpTopic, ToolkitHelpSection> _topics;

        private static int _version = -1; // 仅语言提交后重建文案, 不重建窗口.

        private static Dictionary<ToolkitHelpTopic, ToolkitHelpSection> CreateTopics() => new()
        {
            [ToolkitHelpTopic.ToolSettings] = new(ToolkitLocalization.Get("help.ToolSettings.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.ToolSettings.feature1.title"), ToolkitLocalization.Get("help.ToolSettings.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.ToolSettings.feature2.title"), ToolkitLocalization.Get("help.ToolSettings.feature2.body"))),
            [ToolkitHelpTopic.InterfaceSettings] = new(ToolkitLocalization.Get("help.InterfaceSettings.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature1.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature2.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature3.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature3.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature4.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature4.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature5.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature5.body"))),
            [ToolkitHelpTopic.Window] = new(ToolkitLocalization.Get("help.Window.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature1.title"), ToolkitLocalization.Get("help.Window.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature2.title"), ToolkitLocalization.Get("help.Window.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature3.title"), ToolkitLocalization.Get("help.Window.feature3.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature4.title"), ToolkitLocalization.Get("help.Window.feature4.body"))),
            [ToolkitHelpTopic.Console] = new(ToolkitLocalization.Get("help.Console.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature1.title"), ToolkitLocalization.Get("help.Console.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature2.title"), ToolkitLocalization.Get("help.Console.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature3.title"), ToolkitLocalization.Get("help.Console.feature3.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature4.title"), ToolkitLocalization.Get("help.Console.feature4.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature5.title"), ToolkitLocalization.Get("help.Console.feature5.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature6.title"), ToolkitLocalization.Get("help.Console.feature6.body"))),
            [ToolkitHelpTopic.Stack] = new(ToolkitLocalization.Get("help.Stack.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Stack.feature1.title"), ToolkitLocalization.Get("help.Stack.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Stack.feature2.title"), ToolkitLocalization.Get("help.Stack.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.Stack.feature3.title"), ToolkitLocalization.Get("help.Stack.feature3.body"))),
            [ToolkitHelpTopic.CheatCommands] = new(ToolkitLocalization.Get("help.CheatCommands.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatCommands.feature1.title"), ToolkitLocalization.Get("help.CheatCommands.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatCommands.feature2.title"), ToolkitLocalization.Get("help.CheatCommands.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatCommands.feature3.title"), ToolkitLocalization.Get("help.CheatCommands.feature3.body"))),
            [ToolkitHelpTopic.CheatParameters] = new(ToolkitLocalization.Get("help.CheatParameters.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatParameters.feature1.title"), ToolkitLocalization.Get("help.CheatParameters.feature1.body"))),
            [ToolkitHelpTopic.CheatPresets] = new(ToolkitLocalization.Get("help.CheatPresets.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatPresets.feature1.title"), ToolkitLocalization.Get("help.CheatPresets.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatPresets.feature2.title"), ToolkitLocalization.Get("help.CheatPresets.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatPresets.feature3.title"), ToolkitLocalization.Get("help.CheatPresets.feature3.body"))),
            [ToolkitHelpTopic.CheatMacros] = new(ToolkitLocalization.Get("help.CheatMacros.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatMacros.feature1.title"), ToolkitLocalization.Get("help.CheatMacros.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatMacros.feature2.title"), ToolkitLocalization.Get("help.CheatMacros.feature2.body"))),
            [ToolkitHelpTopic.MacroLibrary] = new(ToolkitLocalization.Get("help.MacroLibrary.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroLibrary.feature1.title"), ToolkitLocalization.Get("help.MacroLibrary.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroLibrary.feature2.title"), ToolkitLocalization.Get("help.MacroLibrary.feature2.body"))),
            [ToolkitHelpTopic.MacroEditor] = new(ToolkitLocalization.Get("help.MacroEditor.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroEditor.feature1.title"), ToolkitLocalization.Get("help.MacroEditor.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroEditor.feature2.title"), ToolkitLocalization.Get("help.MacroEditor.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroEditor.feature3.title"), ToolkitLocalization.Get("help.MacroEditor.feature3.body"))),
            [ToolkitHelpTopic.MacroSyntax] = new(ToolkitLocalization.Get("help.MacroSyntax.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature1.title"), ToolkitLocalization.Get("help.MacroSyntax.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature2.title"), ToolkitLocalization.Get("help.MacroSyntax.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature3.title"), ToolkitLocalization.Get("help.MacroSyntax.feature3.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature4.title"), ToolkitLocalization.Get("help.MacroSyntax.feature4.body"))),
            [ToolkitHelpTopic.ConsoleSettings] = new(ToolkitLocalization.Get("help.ConsoleSettings.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.ConsoleSettings.feature1.title"), ToolkitLocalization.Get("help.ConsoleSettings.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.ConsoleSettings.feature2.title"), ToolkitLocalization.Get("help.ConsoleSettings.feature2.body"))),
            [ToolkitHelpTopic.CheatSettings] = new(ToolkitLocalization.Get("help.CheatSettings.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatSettings.feature1.title"), ToolkitLocalization.Get("help.CheatSettings.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatSettings.feature2.title"), ToolkitLocalization.Get("help.CheatSettings.feature2.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatSettings.feature3.title"), ToolkitLocalization.Get("help.CheatSettings.feature3.body"))),
            [ToolkitHelpTopic.FloatingSettings] = new(ToolkitLocalization.Get("help.FloatingSettings.title"), new ToolkitHelpFeature(ToolkitLocalization.Get("help.FloatingSettings.feature1.title"), ToolkitLocalization.Get("help.FloatingSettings.feature1.body")), new ToolkitHelpFeature(ToolkitLocalization.Get("help.FloatingSettings.feature2.title"), ToolkitLocalization.Get("help.FloatingSettings.feature2.body")))
        };

        internal static ToolkitHelpSection Get(ToolkitHelpTopic topic)
        {
            if (_topics == null || _version != ToolkitLocalization.Version)
            {
                _topics = CreateTopics();
                _version = ToolkitLocalization.Version;
            }

            return _topics[topic];
        }
    }

    /// <summary> 帮助入口使用的稳定标识, 不依赖显示标题. </summary>
    internal enum ToolkitHelpTopic
    {
        Window,
        Console,
        Stack,
        CheatCommands,
        CheatParameters,
        CheatPresets,
        CheatMacros,
        MacroLibrary,
        MacroEditor,
        MacroSyntax,
        ConsoleSettings,
        FloatingSettings,
        CheatSettings,
        InterfaceSettings,
        ToolSettings
    }

    /// <summary> 一个主题及其按阅读顺序排列的特性. </summary>
    internal sealed class ToolkitHelpSection
    {
        internal string Title { get; }

        internal IReadOnlyList<ToolkitHelpFeature> Features { get; }

        internal ToolkitHelpSection(string title, params ToolkitHelpFeature[] features)
        {
            Title = title;
            Features = features;
        }
    }

    /// <summary> 每项特性只维护标题与纯文本正文. </summary>
    internal readonly struct ToolkitHelpFeature
    {
        internal string Title { get; }

        internal string Body { get; }

        /// <summary> 创建特性文案, 不包含分页和 UI 信息. </summary>
        internal ToolkitHelpFeature(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }
}
