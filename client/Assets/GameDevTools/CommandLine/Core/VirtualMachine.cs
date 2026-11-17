/* Auth: Prince Biscuit/王子饼干 */
using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> 注册命令和变量并执行命令语法树. </summary>
    public partial class VirtualMachine
    {
        public IEnumerable<StackCallable> AllCallables => _callables.Values;

        public IEnumerable<StackProperty> AllProperties
        {
            get
            {
                foreach (var p in _properties.Values)
                {
                    yield return p;
                }

                foreach (var p in _localVariables.Values)
                {
                    yield return p;
                }
            }
        }

        public StackObject TopValue
        {
            get
            {
                if (_stack.Count > 0) return _stack.Pop();

                throw new CommandExecuteException("fatal stack error");
            }
        }

        private readonly bool _ignoreInvalidSetBehaviour;

        private readonly bool _treatVoidAsNull; // while execute command input "a = myfunc()" if "myfunc" has no return value, then we should set a 'null' value to 'a'.

        private readonly bool _receiveValueFromNonStringType;

        private readonly Stack<StackObject> _stack = new();

        private readonly ValueParserManager _parsersDefault = new(DefaultTypeParserDefination.DefaultTypeParser, DefaultTypeParserDefination.DefaultTypeAlias); // readonly, you cannot custom this parser manager.

        private readonly ValueParserManager _parsersCustom = new();

        private readonly Dictionary<string, StackProperty> _properties = new();

        private readonly Dictionary<string, StackPropertyCustom> _localVariables = new();

        private readonly Dictionary<string, StackCallable> _callables = new();

        /// <summary> 通过构造选项控制无赋值接口的错误处理, 非字符串结果转换, 以及 void 结果是否视为 null. </summary>
        public VirtualMachine(bool ignoreInvalidSetBehaviour = true, bool receiveValueFromNonStringType = true, bool treatVoidAsNull = true)
        {
            this._ignoreInvalidSetBehaviour = ignoreInvalidSetBehaviour;
            this._receiveValueFromNonStringType = receiveValueFromNonStringType;
            this._treatVoidAsNull = treatVoidAsNull;
        }

        public void RegisterValueParser(Type type, ValueParser parser, string typeAlias = null)
        {
            _parsersCustom.RegisterParser(type, parser, typeAlias);
        }

        public bool RegisterCallable(StackCallable callable, bool shouldOverwrite = true)
        {
            if (_callables.ContainsKey(callable.Name))
            {
                if (shouldOverwrite)
                {
                    _callables[callable.Name] = callable;
                    return true;
                }

                return false;
            }

            _callables.Add(callable.Name, callable);
            return true;
        }

        public bool RegisterProperty(StackProperty property, bool shouldOverwrite = true)
        {
            if (_properties.ContainsKey(property.Name))
            {
                if (shouldOverwrite)
                {
                    _properties[property.Name] = property;
                    return true;
                }

                return false;
            }

            _properties.Add(property.Name, property);
            return true;
        }

        public object GetLocalVariable(string name)
        {
            if (_localVariables.TryGetValue(name, out var property)) return property.GetValue();

            return null;
        }

        public void SetLocalVariable(string name, object value)
        {
            if (_localVariables.TryGetValue(name, out var property))
            {
                property.SetValue(value);
                return;
            }

            var propertyHandle = new StackPropertyCustom(name, value);

            _localVariables.Add(name, propertyHandle);
        }

        public Type GetPropertyType(string propertyName)
        {
            if (_localVariables.TryGetValue(propertyName, out var propertyLocal)) return propertyLocal.ValueType;

            if (_properties.TryGetValue(propertyName, out var property)) return property.ValueType;

            return null;
        }

        public Type GetCallableType(string name)
        {
            if (_callables.TryGetValue(name, out var callable)) return callable.Instance?.GetType();

            return null;
        }

        private void SetProperty(IValueSetter propertyHandle, object value, string debugInfo)
        {
            if (propertyHandle is StackPropertyCustom customProperty)
            {
                customProperty.SetValue(value);
                return;
            }

            // check if target is nullable
            if (value == null)
            {
                if (propertyHandle.ValueType.IsNullable())
                {
                    propertyHandle.SetValue(null);
                    return;
                }

                throw new CommandExecuteException($"cannot assign \"null\" to \"{propertyHandle.Name}\" near \"{debugInfo}\"");
            }

            var valueType = value.GetType();

            if (propertyHandle.ValueType.IsAssignableFrom(valueType))
            {
                propertyHandle.SetValue(value);
                return;
            }

            if (propertyHandle.ValueType == typeof(string))
            {
                if (_receiveValueFromNonStringType)
                {
                    propertyHandle.SetValue(value.ToString());
                    return;
                }

                throw new CommandExecuteException($"cannot assign \"{value}({value.GetType()})\" to \"{propertyHandle.Name} (string)\" near \"{debugInfo}\"");
            }

            if (valueType == typeof(string))
            {
                string input = (string)value;

                if (TryParseInput(input, propertyHandle.ValueType, out value))
                {
                    propertyHandle.SetValue(value);
                    return;
                }

                throw new CommandExecuteException($"cannot parse \"{input}\" to type \"{propertyHandle.ValueType}\" near \"{debugInfo}\"");
            }

            throw new CommandExecuteException($"cannot assign \"{value} ({value.GetType()})\" to {propertyHandle.Name} ({propertyHandle.ValueType}) near \"{debugInfo}\"");
        }

        private bool TryParseInput(string input, Type type, out object data)
        {
            var valueType = Nullable.GetUnderlyingType(type) ?? type;

            if (valueType.IsEnum)
            {
                try
                {
                    data = Enum.Parse(valueType, input, true);
                    return true;
                }
                catch (ArgumentException)
                {
                    data = null;
                    return false;
                }
            }

            if (_parsersCustom.TryParseSafe(type, input, out data)) return true;

            return _parsersDefault.TryParse(type, input, out data);
        }

        // get type by alias, if not found, return null
        private Type TryGetTypeByAlias(string alias)
        {
            if (_parsersCustom.QueryType(alias, out var type)) return type;

            return _parsersDefault.QueryType(alias, out type)
                ? type
                : null;
        }

        private bool TryLoadVariable(string name)
        {
            if (_properties.TryGetValue(name, out var property))
            {
                _stack.Push(property);
                return true;
            }

            if (_localVariables.TryGetValue(name, out var propertyCustom))
            {
                _stack.Push(propertyCustom);
                return true;
            }

            return false;
        }

        private object UnwrapStackObjectNoRequire(StackObject container)
        {
            if (container is IValueGetter valueGetter) return valueGetter.GetValue();

            if (container is StackCallable callable) return callable.Instance;

            if (container is StackSourceInput inputContainer) return inputContainer.inputStr;

            return null;
        }

        /// <summary> indicate the target of execute one node, it would affect the behavior of Execute method. </summary>
        public enum CallTarget
        {
            Unset,
            RequireCallable,
            RequireValue,
        }

        /// <summary> pass to Execute method to indicate the target and required type of execute one node. </summary>
        public struct ExecuteStatus
        {
            public Type requiredType;

            public CallTarget target;

            public ExecuteStatus(Type requiredType, CallTarget target)
            {
                this.target = target;
                this.requiredType = requiredType;
            }
        }
    }
}
