using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameConsole.Extension
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ParamDefineAttribute: Attribute
    {
        public readonly Type collectClass;
        public readonly string collectMethod;

        public ParamDefineAttribute(Type collectClass, string collectMethod)
        {
            this.collectClass = collectClass;
            this.collectMethod = collectMethod;
        }
    }
    
    /// <summary>
    /// 参数信息
    /// </summary>
    public class ParamDefine
    {
        public readonly string param;
        public readonly string tag;
        public readonly string name;
        public readonly string desc;
        public readonly string iconAsset;
        public readonly Func<string, Sprite> iconLoader;
        public Sprite Icon => iconLoader?.Invoke(iconAsset);
        
        /// <param name="param"> 所有参数以空格分隔 </param>
        /// <param name="name"> 显示用的名称 </param>
        /// <param name="desc"> 显示用的描述 </param>
        /// <param name="tag"> 标签、支持多标签，以"|"分隔 </param>
        /// <param name="iconAsset"> 显示用的图标资源名称 </param>
        /// <param name="iconLoader"> 显示用的图标加载方式，仅支持同步加载 </param>
        public ParamDefine(string param, string name = null, string desc = null, string tag = null, string iconAsset  = null, Func<string, Sprite> iconLoader = null)
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
                catch(Exception e)
                {
                    throw new Exception($"param collector '{attribute.collectMethod}()' in '{attribute.collectClass}' must be static and return 'List<{nameof(ParamDefine)}>'.\n{e.Message}");
                }
            }
            
            Debug.LogError($"param collector '{attribute.collectClass.Name}.{attribute.collectMethod}()' not found.");
            return null;
        }
    }
}