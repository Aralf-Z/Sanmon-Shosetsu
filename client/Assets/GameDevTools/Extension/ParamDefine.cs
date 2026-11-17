using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameConsole.Extension
{
    /// <summary> 参数信息. </summary>
    public class ParamDefine
    {
        public readonly string param;

        public readonly string tag;

        public readonly string name;

        public readonly string desc;

        public readonly string iconAsset;

        public readonly Func<string, Sprite> iconLoader;

        public Sprite Icon => iconLoader?.Invoke(iconAsset);

        /// <summary> Create a ParamDefine instance. </summary>
        public ParamDefine(string param, string name = null, string desc = null, string tag = null, string iconAsset = null, Func<string, Sprite> iconLoader = null)
        {
            this.param = param;
            this.name = name;
            this.desc = desc;
            this.tag = tag;
            this.iconAsset = iconAsset;
            this.iconLoader = iconLoader;
        }

        public static List<ParamDefine> GetInfos(MethodInfo methodInfo)
        {
            var filter = methodInfo.GetCustomAttribute<ParamDefineAttribute>();

            if (filter == null) return null;

            return GetInfos(filter);
        }

        public static List<ParamDefine> GetInfos(ParamDefineAttribute attribute)
        {
            var collectMethod = attribute.collectClass.GetMethod(attribute.collectMethod);

            if (collectMethod != null)
            {
                try
                {
                    return (List<ParamDefine>)collectMethod.Invoke(null, null);
                }
                catch (Exception e)
                {
                    throw new Exception($"param collector '{attribute.collectMethod}()' in '{attribute.collectClass}' must be static and return 'List<{nameof(ParamDefine)}>'.\n{e.Message}");
                }
            }

            Debug.LogError($"param collector '{attribute.collectClass.Name}.{attribute.collectMethod}()' not found.");
            return null;
        }
    }
}
