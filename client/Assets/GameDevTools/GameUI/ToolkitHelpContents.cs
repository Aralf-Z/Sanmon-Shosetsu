using System.Collections.Generic;

namespace GameConsole.UI
{
    // 文案维护: Resources/localization/语言代码/help.json, 按 help.主题.feature序号.title/body 查找.
    // 本文件只维护主题标识和特性顺序, 分组在 ToolkitHelpContents.Navigation 中配置.
    // JSON 正文使用转义换行, {ToolKey} 由显示层填入当前按键, 不需要手工拆页.
    /// <summary> 主工具帮助的稳定标识和特性顺序, 显示文案由语言 JSON 提供. </summary>
    internal static partial class ToolkitHelpContents
    {
        private static Dictionary<ToolkitHelpTopic, ToolkitHelpSection> _topics; // 当前语言的帮助文案快照.
        private static int _version = -1; // 仅语言提交后重建文案, 不重建窗口.

        private static Dictionary<ToolkitHelpTopic, ToolkitHelpSection> CreateTopics() => new()
        {
            [ToolkitHelpTopic.ToolSettings] = new(ToolkitLocalization.Get("help.ToolSettings.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.ToolSettings.feature1.title"), ToolkitLocalization.Get("help.ToolSettings.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.ToolSettings.feature2.title"), ToolkitLocalization.Get("help.ToolSettings.feature2.body"))),
            [ToolkitHelpTopic.InterfaceSettings] = new(ToolkitLocalization.Get("help.InterfaceSettings.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature1.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature2.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature3.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature3.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature4.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature4.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.InterfaceSettings.feature5.title"), ToolkitLocalization.Get("help.InterfaceSettings.feature5.body"))),
            [ToolkitHelpTopic.Window] = new(ToolkitLocalization.Get("help.Window.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature1.title"), ToolkitLocalization.Get("help.Window.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature2.title"), ToolkitLocalization.Get("help.Window.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature3.title"), ToolkitLocalization.Get("help.Window.feature3.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Window.feature4.title"), ToolkitLocalization.Get("help.Window.feature4.body"))),
            [ToolkitHelpTopic.Console] = new(ToolkitLocalization.Get("help.Console.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature1.title"), ToolkitLocalization.Get("help.Console.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature2.title"), ToolkitLocalization.Get("help.Console.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature3.title"), ToolkitLocalization.Get("help.Console.feature3.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature4.title"), ToolkitLocalization.Get("help.Console.feature4.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature5.title"), ToolkitLocalization.Get("help.Console.feature5.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Console.feature6.title"), ToolkitLocalization.Get("help.Console.feature6.body"))),
            [ToolkitHelpTopic.Stack] = new(ToolkitLocalization.Get("help.Stack.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Stack.feature1.title"), ToolkitLocalization.Get("help.Stack.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Stack.feature2.title"), ToolkitLocalization.Get("help.Stack.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.Stack.feature3.title"), ToolkitLocalization.Get("help.Stack.feature3.body"))),
            [ToolkitHelpTopic.CheatCommands] = new(ToolkitLocalization.Get("help.CheatCommands.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatCommands.feature1.title"), ToolkitLocalization.Get("help.CheatCommands.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatCommands.feature2.title"), ToolkitLocalization.Get("help.CheatCommands.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatCommands.feature3.title"), ToolkitLocalization.Get("help.CheatCommands.feature3.body"))),
            [ToolkitHelpTopic.CheatParameters] = new(ToolkitLocalization.Get("help.CheatParameters.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatParameters.feature1.title"), ToolkitLocalization.Get("help.CheatParameters.feature1.body"))),
            [ToolkitHelpTopic.CheatPresets] = new(ToolkitLocalization.Get("help.CheatPresets.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatPresets.feature1.title"), ToolkitLocalization.Get("help.CheatPresets.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatPresets.feature2.title"), ToolkitLocalization.Get("help.CheatPresets.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatPresets.feature3.title"), ToolkitLocalization.Get("help.CheatPresets.feature3.body"))),
            [ToolkitHelpTopic.CheatMacros] = new(ToolkitLocalization.Get("help.CheatMacros.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatMacros.feature1.title"), ToolkitLocalization.Get("help.CheatMacros.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatMacros.feature2.title"), ToolkitLocalization.Get("help.CheatMacros.feature2.body"))),
            [ToolkitHelpTopic.MacroLibrary] = new(ToolkitLocalization.Get("help.MacroLibrary.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroLibrary.feature1.title"), ToolkitLocalization.Get("help.MacroLibrary.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroLibrary.feature2.title"), ToolkitLocalization.Get("help.MacroLibrary.feature2.body"))),
            [ToolkitHelpTopic.MacroEditor] = new(ToolkitLocalization.Get("help.MacroEditor.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroEditor.feature1.title"), ToolkitLocalization.Get("help.MacroEditor.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroEditor.feature2.title"), ToolkitLocalization.Get("help.MacroEditor.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroEditor.feature3.title"), ToolkitLocalization.Get("help.MacroEditor.feature3.body"))),
            [ToolkitHelpTopic.MacroSyntax] = new(ToolkitLocalization.Get("help.MacroSyntax.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature1.title"), ToolkitLocalization.Get("help.MacroSyntax.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature2.title"), ToolkitLocalization.Get("help.MacroSyntax.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature3.title"), ToolkitLocalization.Get("help.MacroSyntax.feature3.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.MacroSyntax.feature4.title"), ToolkitLocalization.Get("help.MacroSyntax.feature4.body"))),
            [ToolkitHelpTopic.ConsoleSettings] = new(ToolkitLocalization.Get("help.ConsoleSettings.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.ConsoleSettings.feature1.title"), ToolkitLocalization.Get("help.ConsoleSettings.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.ConsoleSettings.feature2.title"), ToolkitLocalization.Get("help.ConsoleSettings.feature2.body"))),
            [ToolkitHelpTopic.CheatSettings] = new(ToolkitLocalization.Get("help.CheatSettings.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatSettings.feature1.title"), ToolkitLocalization.Get("help.CheatSettings.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatSettings.feature2.title"), ToolkitLocalization.Get("help.CheatSettings.feature2.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.CheatSettings.feature3.title"), ToolkitLocalization.Get("help.CheatSettings.feature3.body"))),
            [ToolkitHelpTopic.FloatingSettings] = new(ToolkitLocalization.Get("help.FloatingSettings.title"),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.FloatingSettings.feature1.title"), ToolkitLocalization.Get("help.FloatingSettings.feature1.body")),
                new ToolkitHelpFeature(ToolkitLocalization.Get("help.FloatingSettings.feature2.title"), ToolkitLocalization.Get("help.FloatingSettings.feature2.body")))
        }; // 在此直接增删特性, 不需要修改分页器.

        /// <summary> 按稳定主题标识取得文案. </summary>
        internal static ToolkitHelpSection Get(ToolkitHelpTopic topic)
        {
            if (_topics == null || _version != ToolkitLocalization.Version) { _topics = CreateTopics(); _version = ToolkitLocalization.Version; }
            return _topics[topic];
        }
    }

    /// <summary> 帮助入口使用的稳定标识, 不依赖显示标题. </summary>
    internal enum ToolkitHelpTopic
    {
        /// <summary> 主窗口操作. </summary>
        Window,
        /// <summary> 控制台日志与输入. </summary>
        Console,
        /// <summary> 日志堆栈侧栏. </summary>
        Stack,
        /// <summary> 命令列表. </summary>
        CheatCommands,
        /// <summary> 参数表单. </summary>
        CheatParameters,
        /// <summary> 参数预设网格. </summary>
        CheatPresets,
        /// <summary> 宏按钮网格. </summary>
        CheatMacros,
        /// <summary> 宏库与草稿. </summary>
        MacroLibrary,
        /// <summary> 宏编辑与运行. </summary>
        MacroEditor,
        /// <summary> 语法说明侧栏. </summary>
        MacroSyntax,
        /// <summary> 控制台配置. </summary>
        ConsoleSettings,
        /// <summary> Options 与浮标弹窗共用的设置说明. </summary>
        FloatingSettings,
        /// <summary> Cheat 配置. </summary>
        CheatSettings,
        /// <summary> 全工具统一透明度. </summary>
        InterfaceSettings,
        /// <summary> 工具开关键与窗口开关行为. </summary>
        ToolSettings
    }

    /// <summary> 一个主题及其按阅读顺序排列的特性. </summary>
    internal sealed class ToolkitHelpSection
    {
        /// <summary> 区域名称. </summary>
        internal string Title { get; }
        /// <summary> 各特性的展示顺序. </summary>
        internal IReadOnlyList<ToolkitHelpFeature> Features { get; }

        /// <summary> 文案定义只需要主题标题和特性列表. </summary>
        internal ToolkitHelpSection(string title, params ToolkitHelpFeature[] features) { Title = title; Features = features; }
    }

    /// <summary> 每项特性只维护标题与纯文本正文. </summary>
    internal readonly struct ToolkitHelpFeature
    {
        /// <summary> 特性标题, 多页编号由显示层自动添加. </summary>
        internal string Title { get; }
        /// <summary> 可直接编辑的多行正文. </summary>
        internal string Body { get; }

        /// <summary> 创建特性文案, 不包含分页和 UI 信息. </summary>
        internal ToolkitHelpFeature(string title, string body) { Title = title; Body = body; }
    }
}
