/*
    provide an attribute to mark debug info, when we use CommandHelper.Debug(), we can get all debug info
    include parent object's debug info
*/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Debug helper. </summary>
    public static class DebugHelper
    {
        /// <summary> get all debug informations from given instance. instance: the instance to get debug info. depth: the depth of debug info. constraintNamespace: only get debug info from given namespace. </summary>
        public static (string, string)[] GetDebugInfos(this object instance, int depth = 0, int depthLimit = 5, string constraintNamespace = null)
        {
            if (depth >= depthLimit)
                return Array.Empty<(string, string)>();
            var usedKeys = new HashSet<string>();
            Type currentType = instance.GetType();
            Type[] types = GetParentTypes(currentType);
            List<(string, string)> totalDebugInfos = new();
            foreach (Type type in types)
            {
                if (constraintNamespace != null && !type.Namespace.StartsWith(constraintNamespace))
                    continue;
                totalDebugInfos.AddRange(GetDebugInfos(type, instance, usedKeys, depth));
            }

            totalDebugInfos.AddRange(GetDebugInfos(currentType, instance, usedKeys, depth));
            return totalDebugInfos.ToArray();
        }

        // Get parent types.
        private 
        /// <summary>get all parent types of given type</summary>
        static Type[] GetParentTypes(Type type)
        {
            Stack<Type> typeList = new();
            Type parent = type.BaseType;
            while (parent != null)
            {
                typeList.Push(parent);
                parent = parent.BaseType;
            }

            return typeList.ToArray();
        }

        // Get debug infos.
        private 
        /// <summary>
        /// get all debug informations from given instance
        /// </summary>
        static List<(string, string)> GetDebugInfos(Type type, object instance, HashSet<string> usedKeys, int depth)
        {
            if (type == null)
                return new List<(string, string)>();
            List<(string, string)> debugInfos = new();
            string typeName = type.Name;
            string space = new(' ', depth * 4);
            string typeSpace = new('-', depth * 4);
            debugInfos.Add(($">{typeSpace}[{typeName}]", "#ffffff"));
            /* read field infos */
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = fieldInfo.GetCustomAttribute<DebugInfoAttribute>();
                if (attr != null)
                {
                    string title = attr.Key == string.Empty ? fieldInfo.Name : attr.Key;
                    if (usedKeys.Contains(title))
                        continue;
                    usedKeys.Add(title);
                    var subTypeAttr = fieldInfo.FieldType.GetCustomAttribute<DebugObjectAttribute>();
                    if (subTypeAttr != null)
                    {
                        debugInfos.AddRange(GetDebugInfos(fieldInfo.GetValue(instance), depth + 1));
                        continue;
                    }

                    debugInfos.Add(($">{space}{typeName}.{title}: {fieldInfo.GetValue(instance)}", attr.Color));
                }
            }

            /* read property infos */
            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = propertyInfo.GetCustomAttribute<DebugInfoAttribute>();
                if (attr != null)
                {
                    string title = attr.Key == string.Empty ? propertyInfo.Name : attr.Key;
                    if (usedKeys.Contains(title))
                        continue;
                    usedKeys.Add(title);
                    var subTypeAttr = propertyInfo.PropertyType.GetCustomAttribute<DebugObjectAttribute>();
                    if (subTypeAttr != null)
                    {
                        debugInfos.AddRange(GetDebugInfos(propertyInfo.GetValue(instance), depth + 1));
                        continue;
                    }

                    debugInfos.Add(($">{space}{typeName}.{title}: {propertyInfo.GetValue(instance)}", attr.Color));
                }
            }

            return debugInfos;
        }
    }
}
