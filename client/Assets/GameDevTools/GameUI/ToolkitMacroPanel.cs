using System;
using System.Collections.Generic;
using System.Linq;
using GameConsole.Extension;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 会话内保留各宏草稿的编辑页, 元数据与正文统一传入 Macro. </summary>
    internal sealed class ToolkitMacroPanel : IDisposable
    {
        private readonly VisualElement _customList; // 自定义宏与未保存草稿.
        private readonly VisualElement _builtInList; // 只读内置宏.
        private readonly TextField _name; // 显示名称.
        private readonly TextField _description; // 显示描述.
        private readonly TextField _body; // 多行命令编辑器.
        private readonly Label _lineNumbers; // 正文行号及错误位置.
        private readonly Label _diagnostics; // 可点击定位首个错误.
        private readonly Label _status; // 当前运行状态.
        private readonly Button _create; // 创建自定义草稿.
        private readonly Button _copy; // 复制当前宏.
        private readonly Button _save; // 保存校验通过的自定义宏.
        private readonly Button _delete; // 删除当前自定义宏.
        private readonly Button _format; // 无损格式整理.
        private readonly Button _run; // 运行当前编辑快照.
        private readonly Button _stop; // 停止后续调度.
        private readonly ToolkitMacroHighlight _highlight; // 纯文本编辑的逐行显示层.
        private readonly ToolkitMacroHelp _help; // 语法说明和持久化配色.
        private readonly ToolkitMacroLayout _layout; // 固定侧栏和窄窗口抽屉.
        private readonly ToolkitTextDisplay _listText = new(); // 按实际可用宽度显示英文省略号, 原文独立保留.
        private readonly Dictionary<string, Macro> _drafts = new(); // 改名不改变字典键.
        private readonly HashSet<string> _modified = new(); // 未保存提示.
        private readonly Dictionary<string, (string name, string description, string body)> _savedSnapshots = new(); // 最近一次成功保存的编辑内容, 用于撤销后的精确比较.
        private readonly List<Action> _rowUnbind = new(); // 列表重建前注销回调.
        private readonly List<Action> _groupUnbind = new(); // 分组折叠事件随整个面板注销.
        private readonly List<Action> _groupRefresh = new(); // 语言变化时原位更新分组标题.
        private MacroSystem _system; // 共用宏服务.
        private Macro _selected; // 当前可编辑副本或只读定义.
        private List<MacroDiagnostic> _errors = new(); // 最近一次校验结果.
        private bool _visible; // 隐藏时保留表单, 暂停视图刷新.
        private bool _listDirty = true; // 合并库与草稿变化.
        private bool _validateDirty; // 合并一次输入引起的多字段变化.

        /// <summary> 编辑控件来自独立 UXML, 不在代码中硬编码布局. </summary>
        internal ToolkitMacroPanel(VisualElement root)
        {
            _customList = ToolkitView.Require<VisualElement>(root, "customMacroList");
            _builtInList = ToolkitView.Require<VisualElement>(root, "builtInMacroList");
            _name = ToolkitView.Require<TextField>(root, "macroName");
            _description = ToolkitView.Require<TextField>(root, "macroDescription");
            _body = ToolkitView.Require<TextField>(root, "macroBody");
            _lineNumbers = ToolkitView.Require<Label>(root, "macroLineNumbers");
            _diagnostics = ToolkitView.Require<Label>(root, "macroDiagnostics");
            _status = ToolkitView.Require<Label>(root, "macroEditorStatus");
            _create = ToolkitView.Require<Button>(root, "newMacro");
            _copy = ToolkitView.Require<Button>(root, "copyMacro");
            _save = ToolkitView.Require<Button>(root, "saveMacro");
            _delete = ToolkitView.Require<Button>(root, "deleteMacro");
            _format = ToolkitView.Require<Button>(root, "formatMacro");
            _run = ToolkitView.Require<Button>(root, "runMacro");
            _stop = ToolkitView.Require<Button>(root, "stopMacroEditor");
            try
            {
                _highlight = new ToolkitMacroHighlight(_body, _lineNumbers, ToolkitView.Require<ScrollView>(root, "macroSourceScroll"));
                _help = new ToolkitMacroHelp(root);
                _layout = new ToolkitMacroLayout(root);
                _name.RegisterValueChangedCallback(Edit);
                _description.RegisterValueChangedCallback(Edit);
                _body.RegisterValueChangedCallback(Edit);
                _diagnostics.RegisterCallback<PointerDownEvent>(LocateError);
                _create.clicked += Create;
                _copy.clicked += Copy;
                _save.clicked += Save;
                _delete.clicked += Delete;
                _format.clicked += Format;
                _run.clicked += Run;
                _stop.clicked += Stop;
                foreach (var group in ToolkitView.Require<VisualElement>(root, "macroContent").Query<Foldout>().ToList())
                {
                    var title = group.text;
                    var label = group.Q<TextElement>(className: "unity-toggle__text");
                    var key = ToolkitLocalizedBindings.TextKey(label);
                    Action refresh = () => group.text = ToolkitLocalization.Get(group.value ? "icon.open" : "icon.collapsed") + " " + (key == null ? title : ToolkitLocalization.Get(key));
                    _groupRefresh.Add(refresh);
                    refresh();
                    EventCallback<ChangeEvent<bool>> change = evt => { if (evt.target == group) refresh(); };
                    group.RegisterValueChangedCallback(change);
                    _groupUnbind.Add(() => group.UnregisterValueChangedCallback(change));
                }
                Select(null);
            }
            catch { Dispose(); throw; }
        }

        /// <summary> 连接共用服务, 编辑页先打开也能触发注册. </summary>
        internal void Bind(MacroSystem system)
        {
            _system = system;
            system.e_onChanged += Changed;
        }

        /// <summary> 切换时保留每个宏的草稿内容. </summary>
        internal void SetVisible(bool visible)
        {
            _visible = visible;
            _layout.SetVisible(visible);
            if (visible) _system?.RequestCommands();
        }

        /// <summary> 校验只读当前草稿, 不修改输入字段或脏标记. </summary>
        internal void RefreshLanguage()
        {
            foreach (var refresh in _groupRefresh) refresh();
            _help.RefreshLanguage();
            _validateDirty = true;
        }

        /// <summary> 仅可见时刷新列表和校验, 不执行编辑器中的命令. </summary>
        internal void Tick()
        {
            if (!_visible || _system == null) return;
            if (_listDirty) RebuildList();
            if (_validateDirty)
            {
                _errors = _selected == null ? new List<MacroDiagnostic>() : _system.Validate(_selected);
                _diagnostics.text = _selected == null ? ToolkitLocalization.Get("ui.MacroPanel.Tick") : _errors.Count == 0 ? ToolkitLocalization.Get("ui.MacroPanel.Tick.2") : string.Join("\n", _errors);
                _diagnostics.EnableInClassList("macro-error", _errors.Count > 0);
                _highlight.SetDiagnostics(_errors);
                _name.EnableInClassList("macro-invalid-field", _errors.Any(error => error.Line == 0 && string.IsNullOrWhiteSpace(_name.value)));
                _validateDirty = false;
            }
            _layout.Tick();
            _listText.Tick();
            _highlight.Tick();
            _save.SetEnabled(_selected != null && !_selected.IsBuiltIn && _errors.Count == 0);
            _run.SetEnabled(_selected != null && _errors.Count == 0 && _system.RunningId == null);
            _stop.SetEnabled(_system.RunningId != null);
            _status.text = _system.Status + (_selected != null && _modified.Contains(_selected.Id) ? ToolkitLocalization.Get("ui.MacroPanel.Tick.3") : string.Empty);
        }

        /// <summary> 取消所有控件及宏库事件订阅. </summary>
        public void Dispose()
        {
            if (_system != null) _system.e_onChanged -= Changed;
            _layout?.Dispose();
            _help?.Dispose();
            _highlight?.Dispose();
            foreach (var unbind in _rowUnbind) unbind();
            _listText.Dispose();
            foreach (var unbind in _groupUnbind) unbind();
            _name.UnregisterValueChangedCallback(Edit);
            _description.UnregisterValueChangedCallback(Edit);
            _body.UnregisterValueChangedCallback(Edit);
            _diagnostics.UnregisterCallback<PointerDownEvent>(LocateError);
            _create.clicked -= Create;
            _copy.clicked -= Copy;
            _save.clicked -= Save;
            _delete.clicked -= Delete;
            _format.clicked -= Format;
            _run.clicked -= Run;
            _stop.clicked -= Stop;
        }

        // 库变化不会覆盖正在编辑的副本.
        private void Changed() => _listDirty = true;

        // 自定义草稿与保存记录合并显示, 两个来源分组内默认按名称排序.
        private void RebuildList()
        {
            foreach (var unbind in _rowUnbind) unbind();
            _rowUnbind.Clear();
            _customList.Clear();
            _builtInList.Clear();
            foreach (var saved in _system.Macros.Where(macro => !macro.IsBuiltIn))
            {
                if (_drafts.ContainsKey(saved.Id)) continue;
                var draft = new Macro(saved.ToSource()) { Id = saved.Id };
                _drafts.Add(saved.Id, draft);
                _savedSnapshots[saved.Id] = Snapshot(draft);
            }
            foreach (var macro in _drafts.Values.OrderBy(macro => macro.name, StringComparer.OrdinalIgnoreCase)) AddRow(_customList, macro);
            foreach (var macro in _system.Macros.Where(macro => macro.IsBuiltIn).OrderBy(macro => macro.name, StringComparer.OrdinalIgnoreCase)) AddRow(_builtInList, macro);
            _listDirty = false;
        }

        // 列表按钮只选择, 不执行宏.
        private void AddRow(VisualElement parent, Macro macro)
        {
            var button = new Button { text = (_modified.Contains(macro.Id) ? "*" : string.Empty) + macro.name, tooltip = macro.name + "\n" + macro.desc, enableRichText = false };
            button.AddToClassList("macro-library-button");
            _listText.Track(button);
            button.EnableInClassList("selected", _selected?.Id == macro.Id);
            Action click = () => Select(macro);
            button.clicked += click;
            _rowUnbind.Add(() => { button.clicked -= click; _listText.Untrack(button); });
            parent.Add(button);
            ToolkitTextOpacity.Track(button);
        }

        // 使用无通知赋值, 切换宏不触发脏标记.
        private void Select(Macro macro)
        {
            _selected = macro;
            _name.SetValueWithoutNotify(macro?.name ?? string.Empty);
            _description.SetValueWithoutNotify(macro?.desc ?? string.Empty);
            _body.SetValueWithoutNotify(macro?.macro ?? string.Empty);
            var readOnly = macro == null || macro.IsBuiltIn;
            _name.isReadOnly = readOnly;
            _description.isReadOnly = readOnly;
            _body.isReadOnly = readOnly;
            _format.SetEnabled(!readOnly);
            _delete.SetEnabled(!readOnly);
            _copy.SetEnabled(macro != null);
            _listDirty = true;
            _validateDirty = true;
            _save.SetEnabled(false);
            _run.SetEnabled(false);
        }

        // 立即保存草稿字段, 即使关闭窗口也不会丢失本次会话内容.
        private void Edit(ChangeEvent<string> evt)
        {
            if (_selected == null || _selected.IsBuiltIn) return;
            _selected.name = _name.value;
            _selected.desc = _description.value;
            _selected.macro = _body.value;
            UpdateModified(_selected);
            _listDirty = true;
            _validateDirty = true;
            _save.SetEnabled(false);
            _run.SetEnabled(false);
        }

        private void Create() => AddDraft(ToolkitLocalization.Get("ui.MacroPanel.Create"), string.Empty, string.Empty); // 新建默认草稿.
        private void Copy() { if (_selected != null) AddDraft(_selected.name + ToolkitLocalization.Get("ui.MacroPanel.Copy"), _selected.desc, _selected.macro); } // 内置宏可复制.

        // 比较实际内容而非输入事件; 改回原文或撤销后移除脏标记, 新建项在保存前保持未保存状态.
        private void UpdateModified(Macro macro)
        {
            var modified = !macro.IsBuiltIn && (!_savedSnapshots.TryGetValue(macro.Id, out var saved) || Snapshot(macro) != saved);
            if (modified) _modified.Add(macro.Id); else _modified.Remove(macro.Id);
        }

        // 保存独立的字段值, 不与可变的草稿对象共享引用状态.
        private static (string name, string description, string body) Snapshot(Macro macro) => (macro.name ?? string.Empty, macro.desc ?? string.Empty, macro.macro ?? string.Empty);

        // 稳定标识在创建草稿时分配, 后续改名不更换.
        private void AddDraft(string name, string desc, string body)
        {
            var macro = new Macro(Macro.Compose(name, desc, body)) { Id = Guid.NewGuid().ToString("N") };
            _drafts.Add(macro.Id, macro);
            _modified.Add(macro.Id);
            Select(macro);
        }

        // 写入成功后才移除未保存标记.
        private void Save()
        {
            if (_selected == null) return;
            if (!_system.Save(_selected, out var error)) { _diagnostics.text = error; return; }
            _savedSnapshots[_selected.Id] = Snapshot(_selected);
            UpdateModified(_selected);
            _listDirty = true;
        }

        // 新建未保存项与已保存项共用删除入口.
        private void Delete()
        {
            if (_selected == null || _selected.IsBuiltIn) return;
            if (!_system.Delete(_selected, out var error)) { _diagnostics.text = error; return; }
            _drafts.Remove(_selected.Id);
            _modified.Remove(_selected.Id);
            _savedSnapshots.Remove(_selected.Id);
            Select(null);
        }

        // 格式不完整时保留原文并显示原因.
        private void Format()
        {
            try { _body.value = _system.Format(_body.value); }
            catch (Exception exception) { _diagnostics.text = exception.Message; }
        }

        private void Run() { if (_selected != null) _system.Run(_selected); } // 校验在服务中再次执行.
        private void Stop() => _system?.Stop(); // 仅停止后续调度.

        // 定位首条正文错误, 元数据错误则聚焦名称.
        private void LocateError(PointerDownEvent evt)
        {
            var error = _errors.FirstOrDefault();
            if (error == null) return;
            if (error.Line <= 0) { _name.Focus(); return; }
            var lines = (_body.value ?? string.Empty).Split('\n');
            var index = Math.Min(lines.Length - 1, error.Line - 1);
            var offset = lines.Take(index).Sum(line => line.Length + 1);
            _body.Focus();
            _body.SelectRange(offset, offset + lines[index].Length);
        }
    }
}
