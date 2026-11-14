using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameConsole.Extension
{
    /// <summary> 参数信息. </summary>
    public class ParamDefine
    {
        /// <summary> Param. </summary>
        public readonly string param;

        /// <summary> Tag. </summary>
        public readonly string tag;

        /// <summary> Name. </summary>
        public readonly string name;

        /// <summary> Desc. </summary>
        public readonly string desc;

        /// <summary> Icon asset. </summary>
        public readonly string iconAsset;

        /// <summary> Icon loader. </summary>
        public readonly Func<string, Sprite> iconLoader;

        /// <summary> Icon. </summary>
        public Sprite Icon => iconLoader?.Invoke(iconAsset);

        
        /// <summary> Create a ParamDefine instance. </summary>
        /// <param name="param">所有参数以空格分隔</param>
        /// <param name="name">显示用的名称</param>
        /// <param name="desc">描述</param>
        /// <param name="tag">标签, 支持多标签, 以'|'分隔</param>
        /// <param name="iconAsset">显示用的图标资源名称</param>
        /// <param name="iconLoader">显示用的图标加载方式,仅支持同步加载</param>
        public ParamDefine(string param, string name = null, string desc = null, string tag = null, string iconAsset = null, Func<string, Sprite> iconLoader = null)
        {
            this.param = param;
            this.name = name;
            this.desc = desc;
            this.tag = tag;
            this.iconAsset = iconAsset;
            this.iconLoader = iconLoader;
        }

        /// <summary> Get infos. </summary>
        public static List<ParamDefine> GetInfos(MethodInfo methodInfo)
        {
            var filter = methodInfo.GetCustomAttribute<ParamDefineAttribute>();
            if (filter == null)
                return null;
            return GetInfos(filter);
        }

        /// <summary> Get infos. </summary>
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
