using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.Editor
{
    /// <summary> 直接编辑唯一 JSON 配置, 不向总控序列化第二份配置. </summary>
    [CustomEditor(typeof(GameDevToolsController))]
    public sealed class GameDevToolsControllerEditor : UnityEditor.Editor
    {
        private readonly List<Action<GameDevToolsConfig>> _refresh = new(); // 使用无通知更新同步 Inspector 和运行时 Options.
        private Label _status; // 当前 UI 的创建和显示状态.

        /// <summary> 使用 UI Toolkit 创建总控配置 Inspector. </summary>
        public override VisualElement CreateInspectorGUI()
        {
            _refresh.Clear();
            var root = new VisualElement();
            root.Add(new HelpBox("修改完成后立即写入 JSON. 命令控制器设置在下次启动生效; 开关键, 窗口开关行为与浮标设置即时生效.", HelpBoxMessageType.Info));
            var path = new TextField("配置文件") { value = Config.FilePath, isReadOnly = true };
            var pathRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            pathRow.style.minWidth = 0;
            // 路径不参与最小内容宽度计算, 窄 Inspector 中优先为按钮保留空间.
            path.style.flexBasis = 0;
            path.style.flexGrow = 1;
            path.style.flexShrink = 1;
            path.style.minWidth = 0;
            path.style.overflow = Overflow.Hidden;
            pathRow.Add(path);
            var open = new Button(OpenConfigFile) { text = "打开", tooltip = "使用系统默认 JSON 编辑器打开配置文件" };
            open.style.width = 48;
            open.style.minWidth = 48;
            open.style.flexShrink = 0;
            pathRow.Add(open);
            root.Add(pathRow);
            _status = new Label();
            root.Add(_status);
            AddEnum(root, "开关键修饰键", data => data.keyModifier, (data, value) => data.keyModifier = (ShortcutModifier)value);
            AddShortcutKey(root);
            AddToggle(root, "打开时按开关键最小化", data => data.minimizeOnShortcut, (data, value) => data.minimizeOnShortcut = value);
            var pages = new List<string> { "console", "cheat", "macro", "option" };
            var page = new DropdownField("初始页面", pages, pages.Contains(Config.MainTagger) ? Config.MainTagger : Config.defaultMainTagger);
            page.RegisterValueChangedCallback(evt => { Config.MainTagger = evt.newValue; });
            _refresh.Add(data => page.SetValueWithoutNotify(pages.Contains(Config.MainTagger) ? Config.MainTagger : Config.defaultMainTagger));
            root.Add(page);
            AddInteger(root, "输入历史容量", data => data.inputHistoryCapacity, (data, value) => data.inputHistoryCapacity = value);
            AddInteger(root, "命令缓存容量", data => data.commandQueryCacheCapacity, (data, value) => data.commandQueryCacheCapacity = value);
            AddInteger(root, "补全数量", data => data.alternativeCommandCount, (data, value) => data.alternativeCommandCount = value);
            AddToggle(root, "Console 图标", data => data.consoleShowLogIcons, (data, value) => data.consoleShowLogIcons = value);
            AddToggle(root, "Console 时间戳", data => data.consoleShowTimestamps, (data, value) => data.consoleShowTimestamps = value);
            AddToggle(root, "记录失败命令", data => data.recordFailedCommand, (data, value) => data.recordFailedCommand = value);
            AddToggle(root, "输出命令异常堆栈", data => data.outputVmExceptionStack, (data, value) => data.outputVmExceptionStack = value);
            AddToggle(root, "浮标显示最新日志", data => data.floatingShowLatestLogs, (data, value) => data.floatingShowLatestLogs = value);
            AddLogCountSlider(root);
            AddPercentage(root, "背景透明度 (%)", data => data.floatingLogBackgroundOpacity, (data, value) => data.floatingLogBackgroundOpacity = value);
            AddPercentage(root, "日志文字透明度 (%)", data => data.floatingLogTextOpacity, (data, value) => data.floatingLogTextOpacity = value);
            var buttons = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            buttons.Add(new Button(Config.Reload) { text = "重新读取" });
            buttons.Add(new Button(Config.Reset) { text = "恢复默认" });
            root.Add(buttons);
            root.schedule.Execute(RefreshStatus).Every(250);
            Refresh();
            return root;
        }

        // 只在文件缺失时创建配置, 避免覆盖外部编辑器已经修改的内容.
        private static void OpenConfigFile()
        {
            if (!File.Exists(Config.FilePath) && !Config.Save()) return;
            try { EditorUtility.OpenWithDefaultApp(Config.FilePath); }
            catch (Exception exception) { Debug.LogWarning($"[GameDevTools] 打开配置文件失败. {exception.Message}"); }
        }

        // 无通知刷新避免写回配置形成循环.
        private void Refresh()
        {
            var data = Config.Snapshot();
            foreach (var refresh in _refresh) refresh(data);
            RefreshStatus();
        }

        // 不创建 UI 即可观察总控状态.
        private void RefreshStatus()
        {
            if (_status == null) return;
            var controller = target as GameDevToolsController;
            if (!Application.isPlaying || controller == null)
            {
                _status.text = "尚未启动. 首次打开工具时加载界面.";
                return;
            }
            _status.text = !controller.IsViewCreated ? "界面尚未创建, 首次打开时加载." : controller.IsOpen ? "界面已创建, 工具已打开." : "界面已创建, 工具已关闭.";
        }

        // 编辑单个字段时合并最新内存数据, 不覆盖 Options 刚修改的其他字段.
        private void AddToggle(VisualElement root, string label, Func<GameDevToolsConfig, bool> getter, Action<GameDevToolsConfig, bool> setter)
        {
            var field = new Toggle(label);
            field.RegisterValueChangedCallback(evt => Config.Edit(data => setter(data, evt.newValue)));
            _refresh.Add(data => field.SetValueWithoutNotify(getter(data)));
            root.Add(field);
        }

        // 数字编辑提交后由 Config 统一校正范围并保存.
        private void AddInteger(VisualElement root, string label, Func<GameDevToolsConfig, int> getter, Action<GameDevToolsConfig, int> setter)
        {
            var field = new IntegerField(label) { isDelayed = true };
            field.RegisterValueChangedCallback(evt => Config.Edit(data => setter(data, evt.newValue)));
            _refresh.Add(data => field.SetValueWithoutNotify(getter(data)));
            root.Add(field);
        }

        // 浮标条数与运行时入口使用相同的滑动范围.
        private void AddLogCountSlider(VisualElement root)
        {
            var field = new SliderInt("浮标日志条数", Config.MIN_FLOATING_LOG_COUNT, Config.MAX_FLOATING_LOG_COUNT) { showInputField = true };
            field.RegisterValueChangedCallback(evt => Config.Edit(data => data.floatingLogLineCount = evt.newValue));
            _refresh.Add(data => field.SetValueWithoutNotify(data.floatingLogLineCount));
            root.Add(field);
        }

        // 显示透明度百分比, JSON 保存归一化透明度.
        private void AddPercentage(VisualElement root, string label, Func<GameDevToolsConfig, float> getter, Action<GameDevToolsConfig, float> setter)
        {
            var field = new SliderInt(label, Config.MIN_PERCENTAGE, Config.MAX_PERCENTAGE) { showInputField = true };
            field.RegisterValueChangedCallback(evt => Config.Edit(data => setter(data, Mathf.Clamp(evt.newValue, Config.MIN_PERCENTAGE, Config.MAX_PERCENTAGE) / (float)Config.MAX_PERCENTAGE)));
            _refresh.Add(data => field.SetValueWithoutNotify(Mathf.RoundToInt(getter(data) * Config.MAX_PERCENTAGE)));
            root.Add(field);
        }

        // Inspector 与游戏内使用相同基础键列表, 不接受导航键或小键盘键.
        private void AddShortcutKey(VisualElement root)
        {
            var names = ToolShortcut.Keys.Select(ToolShortcut.GetKeyOptionName).ToList();
            var field = new DropdownField("开关键基础键", names, names.IndexOf(ToolShortcut.GetKeyOptionName(Config.KeyCode)));
            field.RegisterValueChangedCallback(evt =>
            {
                var index = names.IndexOf(evt.newValue);
                if (index >= 0) Config.Edit(data => data.keyCode = ToolShortcut.Keys[index]);
            });
            _refresh.Add(data => field.SetValueWithoutNotify(ToolShortcut.GetKeyOptionName(ToolShortcut.NormalizeKey(data.keyCode))));
            root.Add(field);
        }

        // 枚举不使用场景中的序列化配置副本.
        private void AddEnum(VisualElement root, string label, Func<GameDevToolsConfig, Enum> getter, Action<GameDevToolsConfig, Enum> setter)
        {
            var field = new EnumField(label, getter(Config.Snapshot()));
            field.RegisterValueChangedCallback(evt => Config.Edit(data => setter(data, evt.newValue)));
            _refresh.Add(data => field.SetValueWithoutNotify(getter(data)));
            root.Add(field);
        }

        // Inspector 关闭或程序集重载后不能保留旧控件引用.
        private void OnEnable() => Config.e_onChanged += Refresh;
        // 移除全局订阅, 释放所有控件引用.
        private void OnDisable() { Config.e_onChanged -= Refresh; _refresh.Clear(); }
    }
}
