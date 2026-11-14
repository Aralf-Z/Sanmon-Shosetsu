using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedSaw.CommandLineInterface;
using UnityEngine;

namespace GameConsole.Extension
{
    /// <summary> 汇总命令的别名, 参数定义和作弊显示信息. </summary>
    public class CommandWrapper
    {
        /// <summary> Command. </summary>
        public Command Command { get; private set; }

        /// <summary> Is cheat ignore. </summary>
        public bool IsCheatIgnore { get; private set; }

        /// <summary> Alias. </summary>
        public string Alias { get; private set; }

        /// <summary> Parameter defines. </summary>
        public IReadOnlyList<ParamDefine> ParameterDefines => _parameterDefines;

        /// <summary> Parameter infos. </summary>
        public IReadOnlyList<ParamInfo> ParameterInfos => _parameterInfos;

        /// <summary> Macro. </summary>
        public IReadOnlyList<MacroAttribute> Macro => _macro;

        private List<ParamDefine> _parameterDefines = new(); // Parameter defines.

        private List<ParamInfo> _parameterInfos; // Parameter infos.

        private List<MacroAttribute> _macro = new (); // Macro.

        /// <summary> Create a CommandWrapper instance. </summary>
        public CommandWrapper(Command command)
        {
            Command = command;
            foreach (var attribute in command.methodInfo.GetCustomAttributes(inherit: true))
            {
                switch (attribute)
                {
                    case ParamDefineAttribute paramDefine:
                        HandleParamDefines(paramDefine);
                        break;
                    case CheatIgnoreAttribute cheatIgnore:
                        HandleCheatIgnore(cheatIgnore);
                        break;
                    case MethodAliasAttribute alias:
                        HandleAliasInfos(alias, command.methodInfo);
                        break;
                    case MacroAttribute macro:
                        HandleMacro(macro);
                        break;
                    case CommandAttribute cmd:
                        break;
                    default:
                        Debug.Log($"attribute {attribute.GetType().FullName} is unhandleable.");
                        break;
                }
            }

            if (_parameterInfos == null)
                HandleAliasInfos(null, command.methodInfo);
        }

        /// <summary> 由当前工具的共享包装缓存向宏系统注册快捷入口. </summary>
        public void RegisterMacros(MacroSystem system) => system.Register(this);

        /// <summary> 构建实际提交到控制台的完整文本, 保留参数内部的引号和空白. </summary>
        public string BuildCommandInput(string parameters) => $"{Command.name} {parameters}".TrimEnd();

        /// <summary> 构建带参数显示名称的调用模板, 始终使用真实命令名. </summary>
        public string BuildCommandTemplate() => $"{Command.name}({string.Join(", ", _parameterInfos.Select(info => info.alias))})";

        // 收集方法参数并应用显示别名.
        private void HandleAliasInfos(MethodAliasAttribute alias, MethodInfo methodInfo)
        {
            var @params = methodInfo.GetParameters();
            Alias = alias?.methodAlias ?? methodInfo.Name;
            _parameterInfos = new();
            for (var i = 0; i < @params.Length; i++)
            {
                var paramInfo = @params[i];
                var aliasName = string.Empty;
                
                if(alias?.ParamAlias?.Count == @params.Length) aliasName = alias.ParamAlias[i];
                if (string.IsNullOrWhiteSpace(aliasName)) aliasName = paramInfo.Name;
                
                _parameterInfos.Add(new ParamInfo(paramInfo.Name, aliasName, paramInfo.ParameterType, paramInfo.HasDefaultValue ? paramInfo.DefaultValue?.ToString() : null));
            }
        }

        // 将命令标记为不显示在作弊面板中.
        private void HandleCheatIgnore(CheatIgnoreAttribute attribute)
        {
            IsCheatIgnore = true;
        }

        // 调用参数收集方法并缓存预设参数.
        private void HandleParamDefines(ParamDefineAttribute attribute)
        {
            var collectMethod = attribute.collectClass.GetMethod(attribute.collectMethod);
            if (collectMethod != null)
            {
                try
                {
                    _parameterDefines = (List<ParamDefine>)collectMethod.Invoke(null, null);
                }
                catch (Exception e)
                {
                    throw new Exception($"param collector '{attribute.collectMethod}()' in '{attribute.collectClass}' must be static and return 'List<{nameof(ParamDefine)}>'.\n{e.Message}");
                }
            }
            else
            {
                Debug.LogError($"param collector '{attribute.collectClass.Name}.{attribute.collectMethod}()' not found.");
            }
        }

        // 初始化宏信息容器
        private void HandleMacro(MacroAttribute attribute)
        {
            _macro.Add(attribute);
        }
    }
}
