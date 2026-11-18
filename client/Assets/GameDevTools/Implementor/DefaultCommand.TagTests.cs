using GameConsole.Extension;
using RedSaw.CommandLineInterface;

namespace GameConsole.Implementor
{
    public static partial class DefaultCommand
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 大量预设压力", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetManyTagTestPresets))]
        private static void TagTestManyPresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 空预设集合", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetEmptyTagTestPresets))]
        private static void TagTestEmptyPresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 单一标签预设", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetSingleTagTestPresets))]
        private static void TagTestSinglePresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 重复标签预设", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetDuplicateTagTestPresets))]
        private static void TagTestDuplicatesPresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 超长名称预设", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetLongTagTestPresets))]
        private static void TagTestLongNamesPresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 完整标签匹配", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetExactTagTestPresets))]
        private static void TagTestExactPresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 标签分页边界", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetPagingTagTestPresets))]
        private static void TagTestPagingPresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 预设代码文本", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetCodeTagTestPresets))]
        private static void TagTestCodePresets(string value) => PrintTagTest(value);

        [Command(desc: "标签分页与参数展示测试, 执行仅打印测试值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 多语言预设", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetUnicodeTagTestPresets))]
        private static void TagTestUnicodePresets(string value) => PrintTagTest(value);

        [Command(desc: "测试 int, float, bool, string 的组合预设参数.", tag: "标签测试")]
        [MethodAlias("标签测试 / 多类型参数预设", "整数", "小数", "开关", "文本")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetTypedTagTestPresets))]
        private static void TagTestTypedPresets(int count, float amount, bool enabled, string text) => PrintTagTest($"count={count}, amount={amount}, enabled={enabled}, text={text}");

        [Command(desc: "测试预设省略可选参数及默认值.", tag: "标签测试")]
        [MethodAlias("标签测试 / 默认参数预设", "文本", "数量")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetOptionalTagTestPresets))]
        private static void TagTestOptionalPresets(string text = "default", int count = 3) => PrintTagTest($"text={text}, count={count}");

        [Command(tag: "测试/13/扩展标签13")]
        [MethodAlias("标签测试 / 扩展命令 13")]
        private static void TagTestExtended13() => PrintTagTest("extended_13");

        [Command(tag: "测试/14/扩展标签14")]
        [MethodAlias("标签测试 / 扩展命令 14")]
        private static void TagTestExtended14() => PrintTagTest("extended_14");

        [Command(tag: "测试/15/扩展标签15")]
        [MethodAlias("标签测试 / 扩展命令 15")]
        private static void TagTestExtended15() => PrintTagTest("extended_15");

        [Command(tag: "测试/16/扩展标签16")]
        [MethodAlias("标签测试 / 扩展命令 16")]
        private static void TagTestExtended16() => PrintTagTest("extended_16");

        [Command(tag: "测试/17/扩展标签17")]
        [MethodAlias("标签测试 / 扩展命令 17")]
        private static void TagTestExtended17() => PrintTagTest("extended_17");

        [Command(tag: "测试/18/扩展标签18")]
        [MethodAlias("标签测试 / 扩展命令 18")]
        private static void TagTestExtended18() => PrintTagTest("extended_18");

        [Command(tag: "测试/19/扩展标签19")]
        [MethodAlias("标签测试 / 扩展命令 19")]
        private static void TagTestExtended19() => PrintTagTest("extended_19");

        [Command(tag: "测试/20/扩展标签20")]
        [MethodAlias("标签测试 / 扩展命令 20")]
        private static void TagTestExtended20() => PrintTagTest("extended_20");

        [Command(tag: "测试/21/扩展标签21")]
        [MethodAlias("标签测试 / 扩展命令 21")]
        private static void TagTestExtended21() => PrintTagTest("extended_21");

        [Command(tag: "测试/22/扩展标签22")]
        [MethodAlias("标签测试 / 扩展命令 22")]
        private static void TagTestExtended22() => PrintTagTest("extended_22");

        [Command(tag: "测试/23/扩展标签23")]
        [MethodAlias("标签测试 / 扩展命令 23")]
        private static void TagTestExtended23() => PrintTagTest("extended_23");

        [Command(tag: "测试/24/扩展标签24")]
        [MethodAlias("标签测试 / 扩展命令 24")]
        private static void TagTestExtended24() => PrintTagTest("extended_24");

        [Command(tag: "测试/25/扩展标签25")]
        [MethodAlias("标签测试 / 扩展命令 25")]
        private static void TagTestExtended25() => PrintTagTest("extended_25");

        [Command(tag: "测试/26/扩展标签26")]
        [MethodAlias("标签测试 / 扩展命令 26")]
        private static void TagTestExtended26() => PrintTagTest("extended_26");

        [Command(tag: "测试/27/扩展标签27")]
        [MethodAlias("标签测试 / 扩展命令 27")]
        private static void TagTestExtended27() => PrintTagTest("extended_27");

        [Command(tag: "测试/28/扩展标签28")]
        [MethodAlias("标签测试 / 扩展命令 28")]
        private static void TagTestExtended28() => PrintTagTest("extended_28");

        [Command(tag: "测试/29/扩展标签29")]
        [MethodAlias("标签测试 / 扩展命令 29")]
        private static void TagTestExtended29() => PrintTagTest("extended_29");

        [Command(tag: "测试/30/扩展标签30")]
        [MethodAlias("标签测试 / 扩展命令 30")]
        private static void TagTestExtended30() => PrintTagTest("extended_30");

        [Command(tag: "测试/31/扩展标签31")]
        [MethodAlias("标签测试 / 扩展命令 31")]
        private static void TagTestExtended31() => PrintTagTest("extended_31");

        [Command(tag: "测试/32/扩展标签32")]
        [MethodAlias("标签测试 / 扩展命令 32")]
        private static void TagTestExtended32() => PrintTagTest("extended_32");

        [Command(tag: "测试/33/扩展标签33")]
        [MethodAlias("标签测试 / 扩展命令 33")]
        private static void TagTestExtended33() => PrintTagTest("extended_33");

        [Command(tag: "测试/34/扩展标签34")]
        [MethodAlias("标签测试 / 扩展命令 34")]
        private static void TagTestExtended34() => PrintTagTest("extended_34");

        [Command(tag: "测试/35/扩展标签35")]
        [MethodAlias("标签测试 / 扩展命令 35")]
        private static void TagTestExtended35() => PrintTagTest("extended_35");

        [Command(tag: "测试/36/扩展标签36")]
        [MethodAlias("标签测试 / 扩展命令 36")]
        private static void TagTestExtended36() => PrintTagTest("extended_36");

        [Command(tag: "测试/37/扩展标签37")]
        [MethodAlias("标签测试 / 扩展命令 37")]
        private static void TagTestExtended37() => PrintTagTest("extended_37");

        [Command(tag: "测试/38/扩展标签38")]
        [MethodAlias("标签测试 / 扩展命令 38")]
        private static void TagTestExtended38() => PrintTagTest("extended_38");

        [Command(tag: "测试/39/扩展标签39")]
        [MethodAlias("标签测试 / 扩展命令 39")]
        private static void TagTestExtended39() => PrintTagTest("extended_39");

        [Command(tag: "测试/40/扩展标签40")]
        [MethodAlias("标签测试 / 扩展命令 40")]
        private static void TagTestExtended40() => PrintTagTest("extended_40");

        [Command(tag: "测试/41/扩展标签41")]
        [MethodAlias("标签测试 / 扩展命令 41")]
        private static void TagTestExtended41() => PrintTagTest("extended_41");

        [Command(tag: "测试/42/扩展标签42")]
        [MethodAlias("标签测试 / 扩展命令 42")]
        private static void TagTestExtended42() => PrintTagTest("extended_42");

        [Command(tag: "测试/43/扩展标签43")]
        [MethodAlias("标签测试 / 扩展命令 43")]
        private static void TagTestExtended43() => PrintTagTest("extended_43");

        [Command(tag: "测试/44/扩展标签44")]
        [MethodAlias("标签测试 / 扩展命令 44")]
        private static void TagTestExtended44() => PrintTagTest("extended_44");

        [Command(tag: "测试/45/扩展标签45")]
        [MethodAlias("标签测试 / 扩展命令 45")]
        private static void TagTestExtended45() => PrintTagTest("extended_45");

        [Command(tag: "测试/46/扩展标签46")]
        [MethodAlias("标签测试 / 扩展命令 46")]
        private static void TagTestExtended46() => PrintTagTest("extended_46");

        [Command(tag: "测试/47/扩展标签47")]
        [MethodAlias("标签测试 / 扩展命令 47")]
        private static void TagTestExtended47() => PrintTagTest("extended_47");

        [Command(tag: "测试/48/扩展标签48")]
        [MethodAlias("标签测试 / 扩展命令 48")]
        private static void TagTestExtended48() => PrintTagTest("extended_48");

        [Command(tag: "测试/49/扩展标签49")]
        [MethodAlias("标签测试 / 扩展命令 49")]
        private static void TagTestExtended49() => PrintTagTest("extended_49");

        [Command(tag: "测试/50/扩展标签50")]
        [MethodAlias("标签测试 / 扩展命令 50")]
        private static void TagTestExtended50() => PrintTagTest("extended_50");

        [Command(tag: "测试/51/扩展标签51")]
        [MethodAlias("标签测试 / 扩展命令 51")]
        private static void TagTestExtended51() => PrintTagTest("extended_51");

        [Command(tag: "测试/52/扩展标签52")]
        [MethodAlias("标签测试 / 扩展命令 52")]
        private static void TagTestExtended52() => PrintTagTest("extended_52");

        [Command(tag: "测试/53/扩展标签53")]
        [MethodAlias("标签测试 / 扩展命令 53")]
        private static void TagTestExtended53() => PrintTagTest("extended_53");

        [Command(tag: "测试/54/扩展标签54")]
        [MethodAlias("标签测试 / 扩展命令 54")]
        private static void TagTestExtended54() => PrintTagTest("extended_54");

        [Command(tag: "测试/55/扩展标签55")]
        [MethodAlias("标签测试 / 扩展命令 55")]
        private static void TagTestExtended55() => PrintTagTest("extended_55");

        [Command(tag: "测试/56/扩展标签56")]
        [MethodAlias("标签测试 / 扩展命令 56")]
        private static void TagTestExtended56() => PrintTagTest("extended_56");

        [Command(tag: "测试/57/扩展标签57")]
        [MethodAlias("标签测试 / 扩展命令 57")]
        private static void TagTestExtended57() => PrintTagTest("extended_57");

        [Command(tag: "测试/58/扩展标签58")]
        [MethodAlias("标签测试 / 扩展命令 58")]
        private static void TagTestExtended58() => PrintTagTest("extended_58");

        [Command(tag: "测试/59/扩展标签59")]
        [MethodAlias("标签测试 / 扩展命令 59")]
        private static void TagTestExtended59() => PrintTagTest("extended_59");

        [Command(tag: "测试/60/扩展标签60")]
        [MethodAlias("标签测试 / 扩展命令 60")]
        private static void TagTestExtended60() => PrintTagTest("extended_60");

#endif
    }
}
