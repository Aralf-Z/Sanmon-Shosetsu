using System;
using System.Collections;

namespace RedSaw.CommandLineInterface
{
    /// <summary> provide utils methods while execute AST. </summary>
    public static class VirtualMachineUtils
    {
        /// <summary> try to get element of given instance like a Array or List. arrlike: the array like instance. index: array index. </summary>
        public static object GetElement(object arrlike, int index)
        {
            var type = arrlike.GetType();
            if (type.IsSZArray)
            {
                if (arrlike is not Array array)
                    throw new CommandExecuteException("cannot index null object");
                if (index < 0 || index >= array.Length)
                    throw new CommandExecuteException($"index out of range: {index}");
                return array.GetValue(index);
            }

            if (arrlike is IList list)
            {
                if (index < 0 || index >= list.Count)
                    throw new CommandExecuteException($"index out of range: {index}");
                return list[index];
            }

            var method = type.GetGetterMethod<int>();
            if (method != null)
            {
                return method.Invoke(arrlike, new object[] { index });
            }

            throw new CommandExecuteException($"cannot index object of type {type}");
        }

        /// <summary> try to get element of given instance like a Dictionary. dictlike: the Dictionary like instance. key: dictionary key. </summary>
        public static object GetElement(object dictlike, object key)
        {
            if (key == null)
                throw new CommandExecuteException("cannot index object by null");
            var instanceType = dictlike.GetType();
            if (dictlike is IDictionary dict && instanceType.IsGenericType)
            {
                var keyType = instanceType.GenericTypeArguments[0];
                if (keyType.IsAssignableFrom(key.GetType()))
                {
                    return dict[key];
                }

                throw new CommandExecuteException($"cannot index object of type {instanceType} by {key.GetType()}");
            }

            var method = instanceType.GetGetterMethod(key.GetType());
            if (method != null)
            {
                return method.Invoke(dictlike, new object[] { key });
            }

            throw new CommandExecuteException($"cannot index object of type {instanceType}");
        }

        /// <summary> try to set element of given instance like a Array or List. arrlike: the array like instance (must not be null). index: array index. value: value to set. </summary>
        public static void SetElement(object arrlike, int index, object value)
        {
            var type = arrlike.GetType();
            if (type.IsSZArray)
            {
                if (arrlike is not Array array)
                    throw new CommandExecuteException($"cannot set element of \"{type}\" object");
                if (index < 0 || index >= array.Length)
                    throw new CommandExecuteException($"index out of range: {index}");
                var elementType = type.GetElementType() ?? throw new CommandExecuteException($"invalid element type");
                if (value == null)
                {
                    if (elementType.IsNullable())
                    {
                        array.SetValue(null, index);
                        return;
                    }

                    throw new CommandExecuteException($"cannot set null to element of \"{type}\" object");
                }

                array.SetValue(value, index);
                return;
            }

            if (arrlike is IList list)
            {
                if (index < 0 || index >= list.Count)
                    throw new CommandExecuteException($"index out of range: {index}");
                if (type.IsGenericType)
                {
                    var elementType = type.GenericTypeArguments[0];
                    if (value == null)
                    {
                        if (elementType.IsNullable())
                        {
                            list[index] = null;
                            return;
                        }

                        throw new CommandExecuteException($"cannot set null to element of \"{type}\" object");
                    }

                    var valueType = value.GetType();
                    if (elementType.IsAssignableFrom(valueType))
                    {
                        list[index] = value;
                        return;
                    }

                    throw new CommandExecuteException($"cannot set \"{valueType}\" to element of \"{type}\" object");
                }

                list[index] = value;
            }

            var method = type.GetGetterMethod<int>();
            if (method != null)
            {
                var parameters = method.GetParameters();
                if (value == null)
                {
                    if (parameters[1].ParameterType.IsNullable())
                    {
                        _ = method.Invoke(arrlike, new object[] { index, null });
                        return;
                    }

                    throw new CommandExecuteException($"cannot set null to element of \"{type}\" object");
                }

                var valueType = value.GetType();
                if (parameters[1].ParameterType.IsAssignableFrom(valueType))
                {
                    method.Invoke(arrlike, new object[] { index, value });
                    return;
                }

                throw new CommandExecuteException($"cannot set \"{valueType}\" to element of \"{type}\" object");
            }

            throw new CommandExecuteException($"cannot set element of \"{type}\" object");
        }

        /// <summary> try to set element of given instance like a Dictionary. dictlike: the Dictionary like instance (must not be null). key: dictionary key. value: value to set. </summary>
        public static void SetElement(object dictlike, object key, object value)
        {
            var instanceType = dictlike.GetType();
            var indexType = key.GetType();
            if (instanceType is IDictionary dict && instanceType.IsGenericType)
            {
                var keyType = instanceType.GenericTypeArguments[0];
                if (!keyType.IsAssignableFrom(indexType))
                {
                    throw new CommandExecuteException($"\"{indexType}\" cannot be index of \"{instanceType}\" object");
                }

                var destValueType = instanceType.GenericTypeArguments[1];
                if (value == null)
                {
                    if (destValueType.IsNullable())
                    {
                        dict[key] = null;
                        return;
                    }

                    throw new CommandExecuteException($"cannot set null to element of \"{instanceType}\" object");
                }

                if (destValueType.IsAssignableFrom(value.GetType()))
                {
                    dict[key] = value;
                    return;
                }

                throw new CommandExecuteException($"cannot set \"{value.GetType()}\" to element of \"{instanceType}\" object");
            }

            var method = instanceType.GetSetterMethod(indexType);
            if (method != null)
            {
                var parameters = method.GetParameters();
                if (value == null)
                {
                    if (parameters[1].ParameterType.IsNullable())
                    {
                        _ = method.Invoke(dictlike, new object[] { key, null });
                        return;
                    }

                    throw new CommandExecuteException($"cannot set null to element of \"{instanceType}\" object");
                }

                if (parameters[1].ParameterType.IsAssignableFrom(value.GetType()))
                {
                    method.Invoke(dictlike, new object[] { key, value });
                    return;
                }

                throw new CommandExecuteException($"cannot set \"{value.GetType()}\" to element of \"{instanceType}\" object");
            }

            throw new CommandExecuteException($"cannot set element of \"{instanceType}\" object");
        }

        /// <summary> try to get element type of given instance like a Array or List. instance: the array like instance. </summary>
        public static Type GetElementTypeOfArray(object instance)
        {
            var instanceType = instance.GetType();
            if (instanceType.IsSZArray)
            {
                return instanceType.GetElementType();
            }

            if (instance is IList && instanceType.IsGenericType)
            {
                return instanceType.GenericTypeArguments[0];
            }

            var method = instanceType.GetSetterMethod<int>();
            if (method != null)
            {
                return method.GetParameters()[1].ParameterType;
            }

            return null;
        }

        /// <summary> try to get element type of given instance like a Dictionary. instance: the Dictionary like instance. indexType: dictionary key type. </summary>
        public static Type GetElementTypeOfDict(object instance, Type indexType)
        {
            var instanceType = instance.GetType();
            if (instance is IDictionary && instanceType.IsGenericType)
            {
                var keyType = instanceType.GenericTypeArguments[0];
                if (keyType.IsAssignableFrom(indexType))
                {
                    return instanceType.GenericTypeArguments[1];
                }
            }

            var method = instanceType.GetSetterMethod(indexType);
            if (method != null)
            {
                return method.GetParameters()[1].ParameterType;
            }

            return null;
        }
    }
}
