using System;
using System.Collections.Generic;
using System.Reflection;
using RedSaw.CommandLineInterface;
using UnityEngine;

namespace GameConsole.Extension
{
    public class CommandWrapper
    {
        public Command Command { get; private set; }
        public bool IsCheatIgnore { get; private set; }
        public IReadOnlyList<ParamDefine> ParameterDefines => mParameterDefines;
        public IReadOnlyList<ParamInfo> ParameterInfos => mParameterInfos;
        public IReadOnlyList<MacroAttribute> Macro => mMacro;
        
        private List<ParamDefine> mParameterDefines = new();
        private List<ParamInfo> mParameterInfos;
        private List<MacroAttribute> mMacro;
        
        public CommandWrapper(Command command)
        {
            Command = command;

            foreach (var attribute in command.methodInfo.GetCustomAttributes(inherit: true))
            {
                switch (attribute)
                {
                    case ParamDefineAttribute paramDefine: HandleParamDefines(paramDefine); break;
                    case CheatIgnoreAttribute cheatIgnore: HandleCheatIgnore(cheatIgnore); break;
                    case ParamAliasAttribute alias: HandleParamInfos(alias, command.methodInfo); break;
                    case MacroAttribute macro: HandleMacro(macro); break;
                    case CommandAttribute cmd: break;
                    default: Debug.Log($"attribute {attribute.GetType().FullName} is unhandleable."); break;
                }
            }
            
            if(mParameterInfos == null) HandleParamInfos(null, command.methodInfo);
        }

        private void HandleParamInfos(ParamAliasAttribute alias, MethodInfo methodInfo)
        {
            var @params = methodInfo.GetParameters();

            mParameterInfos = new();
            if (alias == null)
            {
                foreach (var paramInfo in @params)
                {
                    mParameterInfos.Add(new ParamInfo(paramInfo.Name,  paramInfo.Name, paramInfo.ParameterType
                        , paramInfo.HasDefaultValue ? paramInfo.DefaultValue?.ToString() : null));
                }
            }
            else
            {
                for (var i = 0; i < @params.Length; i++)
                {
                    var paramInfo = @params[i];
                    var aliasName = alias.alias[i];
                    mParameterInfos.Add(new ParamInfo(paramInfo.Name,  aliasName, paramInfo.ParameterType
                        , paramInfo.HasDefaultValue ? paramInfo.DefaultValue?.ToString() : null));
                }
            }
        }
        
        private void HandleCheatIgnore(CheatIgnoreAttribute attribute)
        {
            IsCheatIgnore = true;
        }
        
        private void HandleParamDefines(ParamDefineAttribute attribute)
        {
            var collectMethod = attribute.collectClass.GetMethod(attribute.collectMethod);
            if (collectMethod != null)
            {
                try
                {
                    mParameterDefines = (List<ParamDefine>)collectMethod.Invoke(null, null);
                }
                catch(Exception e)
                {
                    throw new Exception($"param collector '{attribute.collectMethod}()' in '{attribute.collectClass}' must be static and return 'List<{nameof(ParamDefine)}>'.\n{e.Message}");
                }
            }
            else
            {
                Debug.LogError($"param collector '{attribute.collectClass.Name}.{attribute.collectMethod}()' not found.");
            }
        }

        private void HandleMacro(MacroAttribute attribute)
        {
            mMacro = new();
        }
    }
}