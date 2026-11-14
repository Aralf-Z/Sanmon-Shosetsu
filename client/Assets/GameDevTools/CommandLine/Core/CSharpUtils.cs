using System;
using System.Linq;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Csharp utils. </summary>
    static class CSharpUtils
    {
        /// <summary> indicate that given type is a nullable type. </summary>
        public static bool IsNullable(this Type type)
        {
            // return Nullable.GetUnderlyingType(type) != null;
            return !type.IsValueType;
        }

        /// <summary> Get default member. </summary>
        public static MemberInfo GetDefaultMember(this Type type, string memberName)
        {
            return type.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault();
        }

        /// <summary> Get getter method. </summary>
        public static MethodInfo GetGetterMethod<TKey>(this Type type) => GetGetterMethod(type, typeof(TKey));

        /// <summary> Get getter method. </summary>
        public static MethodInfo GetGetterMethod(this Type type, Type keyType)
        {
            var methodInfos = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name != "get_Item")
                    continue;
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(keyType))
                {
                    return methodInfo;
                }
            }

            return null;
        }

        /// <summary> Get setter method. </summary>
        public static MethodInfo GetSetterMethod<TKey>(this Type type) => GetSetterMethod(type, typeof(TKey));

        /// <summary> Get setter method. </summary>
        public static MethodInfo GetSetterMethod(this Type type, Type keyType)
        {
            var methodInfos = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name != "set_Item")
                    continue;
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 2)
                {
                    if (parameters[0].ParameterType.IsAssignableFrom(keyType))
                    {
                        return methodInfo;
                    }
                }
            }

            return null;
        }

        /// <summary> Get member type name. </summary>
        public static string GetMemberTypeName(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType.Name,
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType.Name,
                MemberTypes.Method => $"(..) -> {((MethodInfo)memberInfo).ReturnType.Name}",
                _ => string.Empty,
            };
        }

        /// <summary> check if target propertyInfo is static. </summary>
        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod?.IsStatic ?? propertyInfo.SetMethod?.IsStatic ?? false;
        }

        /// <summary> create a stack property for given propertyInfo. </summary>
        public static StackProperty CreateStackProperty(this PropertyInfo propertyInfo, string name)
        {
            if (!propertyInfo.IsStatic())
                return null;
            if (propertyInfo.GetIndexParameters().Length > 0)
                return null;
            if (propertyInfo.SetMethod == null)
            {
                if (propertyInfo.GetMethod == null)
                    return null;
                return new StackPropertyPropertyOnlyGetter(name, null, propertyInfo);
            }

            if (propertyInfo.GetMethod == null)
            {
                return new StackPropertyPropertyOnlySetter(name, null, propertyInfo);
            }

            return new StackPropertyProperty(name, null, propertyInfo);
        }

        /// <summary> create a stack property for given propertyInfo. </summary>
        public static StackProperty CreateStackProperty(this PropertyInfo propertyInfo, object instance, string name)
        {
            if (instance == null)
                return propertyInfo.CreateStackProperty(name);
            if (propertyInfo.GetIndexParameters().Length > 0)
                return null;
            if (propertyInfo.SetMethod == null)
            {
                if (propertyInfo.GetMethod == null)
                    return null;
                return new StackPropertyPropertyOnlyGetter(name, instance, propertyInfo);
            }

            if (propertyInfo.GetMethod == null)
            {
                return new StackPropertyPropertyOnlySetter(name, instance, propertyInfo);
            }

            return new StackPropertyProperty(name, instance, propertyInfo);
        }

        /// <summary> create a stack property for given propertyInfo. </summary>
        public static StackProperty CreateStackProperty(this PropertyInfo propertyInfo, string name, string description, string tag)
        {
            if (!propertyInfo.IsStatic())
                return null;
            if (propertyInfo.GetIndexParameters().Length > 0)
                return null;
            if (propertyInfo.SetMethod == null)
            {
                if (propertyInfo.GetMethod == null)
                    return null;
                return new StackPropertyPropertyOnlyGetter(name, description, tag, null, propertyInfo);
            }

            if (propertyInfo.GetMethod == null)
            {
                return new StackPropertyPropertyOnlySetter(name, description, tag, null, propertyInfo);
            }

            return new StackPropertyProperty(name, description, tag, null, propertyInfo);
        }

        /// <summary> create a stack property for given propertyInfo. </summary>
        public static StackProperty CreateStackProperty(this PropertyInfo propertyInfo, object instance, string name, string description, string tag)
        {
            if (instance == null)
                return propertyInfo.CreateStackProperty(name, description, tag);
            if (propertyInfo.GetIndexParameters().Length > 0)
                return null;
            if (propertyInfo.SetMethod == null)
            {
                if (propertyInfo.GetMethod == null)
                    return null;
                return new StackPropertyPropertyOnlyGetter(name, description, tag, instance, propertyInfo);
            }

            if (propertyInfo.GetMethod == null)
            {
                return new StackPropertyPropertyOnlySetter(name, description, tag, instance, propertyInfo);
            }

            return new StackPropertyProperty(name, description, tag, instance, propertyInfo);
        }

        /// <summary> create a stack property for given fieldInfo. </summary>
        public static StackProperty CreateStackProperty(this FieldInfo fieldInfo, string name)
        {
            if (fieldInfo.IsStatic)
                return new StackPropertyField(name, null, fieldInfo);
            return null;
        }

        /// <summary> create a stack property for given fieldInfo. </summary>
        public static StackProperty CreateStackProperty(this FieldInfo fieldInfo, object instance, string name)
        {
            if (instance == null)
                return fieldInfo.CreateStackProperty(name);
            return new StackPropertyField(name, instance, fieldInfo);
        }

        /// <summary> Create stack property. </summary>
        public static StackProperty CreateStackProperty(this FieldInfo fieldInfo, string name, string description, string tag)
        {
            if (fieldInfo.IsStatic)
                return new StackPropertyField(name, description, tag, null, fieldInfo);
            return null;
        }

        /// <summary> Create stack property. </summary>
        public static StackProperty CreateStackProperty(this FieldInfo fieldInfo, object instance, string name, string description, string tag)
        {
            if (instance == null)
                return fieldInfo.CreateStackProperty(name, description, tag);
            return new StackPropertyField(name, description, tag, instance, fieldInfo);
        }

        /// <summary> create a stack callable from given methodInfo. </summary>
        public static StackCallable CreateStackCallable(this MethodInfo methodInfo)
        {
            if (methodInfo.IsStatic)
                return new StackMethod(null, methodInfo);
            return null;
        }

        /// <summary> create a stack callable from given methodInfo. </summary>
        public static StackCallable CreateStackCallable(this MethodInfo methodInfo, object instance)
        {
            if (instance == null)
                return methodInfo.CreateStackCallable();
            return new StackMethod(instance, methodInfo);
        }
    }
}
