/*
Name: VirtualMachine.cs
Time: @2024.01.26
Auth: Prince Biscuit/王子饼干
Desc: A simple virtual machine to execute syntax tree parsed by SyntaxAnalyzer.
      it use C#'s reflection to access properties and methods, no other dependencices.
      lightweight and easy to use. it highly decoupled with Console part, so it could be used in other projects singlely.
      also because using reflection, it's not fast enough, so it's usually used in console input to do 
      fast and simple operations, like set/get property, call method, etc.
      in this case, it's fast enough and very useful.
*/
using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> 注册命令和变量并执行命令语法树. </summary>
    public partial class VirtualMachine
    {
        /// <summary> All callables. </summary>
        public IEnumerable<StackCallable> AllCallables => _callables.Values;

        /// <summary> All properties. </summary>
        public IEnumerable<StackProperty> AllProperties
        {
            get
            {
                foreach (var p in _properties.Values)
                    yield return p;
                foreach (var p in _localVariables.Values)
                    yield return p;
            }
        }

        /// <summary> get top value of stack. </summary>
        public StackObject TopValue
        {
            get
            {
                if (_stack.Count > 0)
                    return _stack.Pop();
                throw new CommandExecuteException("fatal stack error");
            }
        }

        private readonly bool _ignoreInvalidSetBehaviour; // while execute command "func() = 999" usually, we assgin value to object with a setter interface, but if target value has no setter interface, the flag will determine if vm should ignore this error or not.

        private readonly bool _treatVoidAsNull; // while execute command input "a = myfunc()" if "myfunc" has no return value, then we should set a 'null' value to 'a'.

        private readonly bool _receiveValueFromNonStringType; // while execute command "@myValue = myFunc()" if 'myValue' is string type and 'myFunc' return a non-string value, this flag will determine if vm should convert return value to string type.

        private readonly Stack<StackObject> _stack = new(); // Stack.

        private readonly ValueParserManager _parsersDefault = new(DefaultTypeParserDefination.DefaultTypeParser, DefaultTypeParserDefination.DefaultTypeAlias); // readonly, you cannot custom this parser manager.

        private readonly ValueParserManager _parsersCustom = new(); // Parsers custom.

        private readonly Dictionary<string, StackProperty> _properties = new(); // Properties.

        private readonly Dictionary<string, StackPropertyCustom> _localVariables = new(); // Local variables.

        private readonly Dictionary<string, StackCallable> _callables = new(); // Callables.

        /// <summary> create a virtual machine. ignoreInvalidSetBehaviour: while execute command &quot;func() = 999&quot; usually, we assgin value to object with a setter interface, but if target value has no setter interface, the flag will determine if vm should ignore this error or not. receiveValueFromNonStringType: while execute command &quot;@myValue = myFunc()&quot; if &apos;myValue&apos; is string type and &apos;myFunc&apos; return a non-string value, this flag will determine if vm should convert return value to string type. treatVoidAsNull: while execute command input &quot;a = myfunc()&quot; if &quot;myfunc&quot; has no return value, then we should set a &apos;null&apos; value to &apos;a&apos;. </summary>
        public VirtualMachine(bool ignoreInvalidSetBehaviour = true, bool receiveValueFromNonStringType = true, bool treatVoidAsNull = true)
        {
            this._ignoreInvalidSetBehaviour = ignoreInvalidSetBehaviour;
            this._receiveValueFromNonStringType = receiveValueFromNonStringType;
            this._treatVoidAsNull = treatVoidAsNull;
        }

        /// <summary> register custom value parser to vm. </summary>
        public void RegisterValueParser(Type type, ValueParser parser, string typeAlias = null)
        {
            _parsersCustom.RegisterParser(type, parser, typeAlias);
        }

        /// <summary> register callable to vm. callable: the callable object. shouldOverwrite: if true, vm will overwrite callable with same name. </summary>
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

        /// <summary> register property to vm. property: the property object. shouldOverwrite: if true, vm will overwrite property with same name. </summary>
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

        /// <summary> get local variable value by name. </summary>
        public object GetLocalVariable(string name)
        {
            if (_localVariables.TryGetValue(name, out var property))
            {
                return property.GetValue();
            }

            return null;
        }

        /// <summary> set local variable value by name, if given name not existed, then create a new one. </summary>
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

        /// <summary> Get property type. </summary>
        public Type GetPropertyType(string propertyName)
        {
            if (_localVariables.TryGetValue(propertyName, out var propertyLocal))
            {
                return propertyLocal.ValueType;
            }

            if (_properties.TryGetValue(propertyName, out var property))
            {
                return property.ValueType;
            }

            return null;
        }

        /// <summary> Get callable type. </summary>
        public Type GetCallableType(string name)
        {
            if (_callables.TryGetValue(name, out var callable))
            {
                return callable.Instance?.GetType();
            }

            return null;
        }

        // Set property.
        private void SetProperty(IValueSetter propertyHandle, object value, string debugInfo)
        {
            /* in any situation, local variable would receive value */
            if (propertyHandle is StackPropertyCustom customProperty)
            {
                customProperty.SetValue(value);
                return;
            }

            /* check if target is nullable */
            if (value == null)
            {
                if (propertyHandle.ValueType.IsNullable())
                {
                    propertyHandle.SetValue(null);
                    return;
                }

                throw new CommandExecuteException($"cannot assign \"null\" to \"{propertyHandle.Name}\" near \"{debugInfo}\"");
            }

            /* while value is compatible, just write in*/
            var valueType = value.GetType();
            if (propertyHandle.ValueType.IsAssignableFrom(valueType))
            {
                propertyHandle.SetValue(value);
                return;
            }

            // 当目标类型是字符串且允许从非字符串类型接受值时，将值转换为字符串
            if (propertyHandle.ValueType == typeof(string))
            {
                if (_receiveValueFromNonStringType)
                {
                    propertyHandle.SetValue(value.ToString());
                    return;
                }

                throw new CommandExecuteException($"cannot assign \"{value}({value.GetType()})\" to \"{propertyHandle.Name} (string)\" near \"{debugInfo}\"");
            }

            // 当值类型为字符串类型且目标类型不是字符串时，尝试将字符串解析为目标类型
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

        // Try parse input.
        private bool TryParseInput(string input, Type type, out object data)
        {
            var valueType = Nullable.GetUnderlyingType(type) ?? type;
            if (valueType.IsEnum)
            {
                try { data = Enum.Parse(valueType, input, true); return true; }
                catch (ArgumentException) { data = null; return false; }
            }
            if (_parsersCustom.TryParseSafe(type, input, out data))
                return true;
            return _parsersDefault.TryParse(type, input, out data);
        }

        // Try get type by alias.
        private 
        /// <summary>
        /// get type by alias, if not found, return null
        /// </summary>
        Type TryGetTypeByAlias(string alias)
        {
            if (_parsersCustom.QueryType(alias, out var type))
                return type;
            return _parsersDefault.QueryType(alias, out type) ? type : null;
        }

        // Try load variable.
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

        // Unwrap stack object no require.
        private 
        /// <summary></summary>
        object UnwrapStackObjectNoRequire(StackObject container)
        {
            if (container is IValueGetter valueGetter)
            {
                return valueGetter.GetValue();
            }

            if (container is StackCallable callable)
            {
                return callable.Instance;
            }

            if (container is StackSourceInput inputContainer)
            {
                return inputContainer.inputStr;
            }

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
            /// <summary> Required type. </summary>
            public Type requiredType;

            /// <summary> Target. </summary>
            public CallTarget target;

            /// <summary> Create a ExecuteStatus instance. </summary>
            public ExecuteStatus(Type requiredType, CallTarget target)
            {
                this.target = target;
                this.requiredType = requiredType;
            }
        }
    }
}
