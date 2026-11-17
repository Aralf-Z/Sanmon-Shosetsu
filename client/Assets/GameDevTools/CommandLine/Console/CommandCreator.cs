/* MIT License - CommandSystem.cs; Created By : Prince Biscuit */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command creator. </summary>
    static class CommandCreator
    {
        public static IEnumerable<Command> CollectCommands<T>()
            where T : CommandAttribute
        {
            foreach (var type in Assembly.GetCallingAssembly()
                .GetTypes())
            {
                foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attribute = methodInfo.GetCustomAttribute<T>();

                    if (attribute != null)
                    {
                        var command = new Command(attribute.name ?? methodInfo.Name, attribute.desc, attribute.tag, methodInfo);

                        yield return command;
                    }
                }
            }
        }

        public static IEnumerable<StackProperty> CollectProperties<T>()
            where T : CommandPropertyAttribute
        {
            foreach (var type in Assembly.GetExecutingAssembly()
                .GetTypes())
            {
                foreach (var propertyInfo in type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attribute = propertyInfo.GetCustomAttribute<T>();

                    if (attribute != null)
                    {
                        var ppt = propertyInfo.CreateStackProperty(attribute.name ?? propertyInfo.Name, attribute.desc, attribute.tag);

                        if (ppt != null)
                        {
                            yield return ppt;
                        }
                    }
                }

                foreach (var fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attribute = fieldInfo.GetCustomAttribute<T>();

                    if (attribute != null)
                    {
                        var ppt = fieldInfo.CreateStackProperty(attribute.name ?? fieldInfo.Name, attribute.desc, attribute.tag);

                        if (ppt != null)
                        {
                            yield return ppt;
                        }
                    }
                }
            }
        }

        public static IEnumerable<(ValueParser, Type, string)> CollectValueParsers<T>()
            where T : CommandValueParserAttribute
        {
            foreach (var type in Assembly.GetExecutingAssembly()
                .GetTypes())
            {
                foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attr = methodInfo.GetCustomAttribute<T>();

                    if (attr == null)
                    {
                        continue;
                    }

                    if (TryConvertDelegate(methodInfo, out ValueParser parser))
                    {
                        yield return (parser, attr.type, attr.Alias);
                    }
                }
            }
        }

        private static bool TryConvertDelegate<T>(MethodInfo methodInfo, out T result)
            where T : Delegate
        {
            result = null;
            MethodInfo delegateMethodInfo = typeof(T).GetMethod("Invoke");

            if (methodInfo.ReturnType != delegateMethodInfo.ReturnType) return false;

            var methodParams = methodInfo.GetParameters();
            var delegateParams = delegateMethodInfo.GetParameters();

            if (methodParams.Length != delegateParams.Length) return false;

            for (int i = 0; i < methodParams.Length; i++)
            {
                if (methodParams[i].ParameterType != delegateParams[i].ParameterType) return false;
            }

            result = Delegate.CreateDelegate(typeof(T), null, methodInfo) as T;
            return true;
        }
    }
}
