using System;
using UnityEngine;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;

namespace GameConsole.Implementor
{
    /// <summary> Default command. </summary>
    public static partial class DefaultCommand
    {
        private static GameDevToolsController Gdt => GameDevToolsController.Instance;

        [Command]
        [MethodAlias("打印")]
        [CheatIgnore]
        private static void Print(string str, LogType logType = LogType.Log)
        {
            Gdt.Print(str, logType);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Command(desc: "测试大量标签, 更多弹窗, 标签与 alpha/beta/gamma 搜索组合及分页.", tag: "标签测试")]
        [MethodAlias("标签测试 / 综合预设", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetTagTestPresets))]
        private static void TagTestPresets(string value) => PrintTagTest(value);

        [Command(desc: "所有预设均无有效标签, 标签栏应只显示全部.", tag: "标签测试")]
        [MethodAlias("标签测试 / 无标签预设", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetUntaggedTestPresets))]
        private static void TagTestUntaggedPresets(string value) => PrintTagTest(value);

        [Command(desc: "只有两个业务标签, 可对照完整容纳和窗口缩窄后的更多按钮.", tag: "标签测试")]
        [MethodAlias("标签测试 / 少量标签预设", "测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetSmallTagTestPresets))]
        private static void TagTestSmallPresets(string value) => PrintTagTest(value);

        [Command(desc: "与三个预设测试入口共用标签, 去重后仍保留所有命令.", tag: "标签测试")]
        [MethodAlias("标签测试 / 重复命令标签")]
        private static void TagTestDuplicate() => PrintTagTest("duplicate");

        [Command(desc: "未提供 tag, 应包含在全部中.")]
        [MethodAlias("标签测试 / null 命令标签")]
        private static void TagTestNull() => PrintTagTest("null");

        [Command(desc: "空字符串标签不应生成业务标签按钮.", tag: "")]
        [MethodAlias("标签测试 / 空命令标签")]
        private static void TagTestEmpty() => PrintTagTest("empty");

        [Command(desc: "纯空白标签不应生成业务标签按钮.", tag: "   ")]
        [MethodAlias("标签测试 / 空白命令标签")]
        private static void TagTestWhitespace() => PrintTagTest("whitespace");

        [Command(desc: "完整匹配组合,A, 不应被拆成多个标签.", tag: "组合,A")]
        [MethodAlias("标签测试 / 逗号标签")]
        private static void TagTestCombined() => PrintTagTest("combined");

        [Command(desc: "与组合,A 是不同的完整标签.", tag: "组合")]
        [MethodAlias("标签测试 / 逗号标签对照")]
        private static void TagTestSingle() => PrintTagTest("single");

        [Command(tag: "TagCase")]
        [MethodAlias("标签测试 / 大写标签")]
        private static void TagTestUppercase() => PrintTagTest("uppercase");

        [Command(tag: "tagcase")]
        [MethodAlias("标签测试 / 小写标签")]
        private static void TagTestLowercase() => PrintTagTest("lowercase");

        [Command(desc: "非纯空白的标签保留首尾空格, 不应与去掉空格的标签合并.", tag: " 标签空格 ")]
        [MethodAlias("标签测试 / 保留首尾空格")]
        private static void TagTestPadded() => PrintTagTest("padded");

        [Command(tag: "标签空格")]
        [MethodAlias("标签测试 / 首尾空格对照")]
        private static void TagTestUnpadded() => PrintTagTest("unpadded");

        [Command(desc: "检查更多弹窗中的长名称换行, 完整 tooltip 和选中状态.", tag: DefaultParamCollector.TAG_TEST_LONG_NAME)]
        [MethodAlias("标签测试 / 超长命令标签")]
        private static void TagTestLong() => PrintTagTest("long");

        [Command(tag: "测试/01/角色")]
        [MethodAlias("标签测试 / 角色")]
        private static void TagTestCharacter() => PrintTagTest("character");

        [Command(tag: "测试/02/物品")]
        [MethodAlias("标签测试 / 物品")]
        private static void TagTestItem() => PrintTagTest("item");

        [Command(tag: "测试/03/战斗")]
        [MethodAlias("标签测试 / 战斗")]
        private static void TagTestBattle() => PrintTagTest("battle");

        [Command(tag: "测试/04/任务")]
        [MethodAlias("标签测试 / 任务")]
        private static void TagTestQuest() => PrintTagTest("quest");

        [Command(tag: "测试/05/地图")]
        [MethodAlias("标签测试 / 地图")]
        private static void TagTestMap() => PrintTagTest("map");

        [Command(tag: "测试/06/背包")]
        [MethodAlias("标签测试 / 背包")]
        private static void TagTestInventory() => PrintTagTest("inventory");

        [Command(tag: "测试/07/技能")]
        [MethodAlias("标签测试 / 技能")]
        private static void TagTestSkill() => PrintTagTest("skill");

        [Command(desc: "成就预设与大量标签分页测试, 支持 alpha/beta/gamma 搜索, 执行仅打印测试值.", tag: "测试/08/成就")]
        [MethodAlias("标签测试 / 成就", "成就测试值")]
        [ParamDefine(typeof(DefaultParamCollector), nameof(DefaultParamCollector.GetAchievementTagTestPresets))]
        private static void TagTestAchievement(string value = "achievement") => PrintTagTest(value);

        [Command(tag: "测试/09/存档")]
        [MethodAlias("标签测试 / 存档")]
        private static void TagTestSave() => PrintTagTest("save");

        [Command(tag: "测试/10/音频")]
        [MethodAlias("标签测试 / 音频")]
        private static void TagTestAudio() => PrintTagTest("audio");

        [Command(tag: "测试/11/界面")]
        [MethodAlias("标签测试 / 界面")]
        private static void TagTestInterface() => PrintTagTest("interface");

        [Command(tag: "测试/12/调试")]
        [MethodAlias("标签测试 / 调试")]
        private static void TagTestDebug() => PrintTagTest("debug");

        // 测试入口只记录日志, 不修改游戏状态.
        private static void PrintTagTest(string value) => Gdt.Print($"[TagTest] {value}", LogType.Log);
#endif
    }
}
