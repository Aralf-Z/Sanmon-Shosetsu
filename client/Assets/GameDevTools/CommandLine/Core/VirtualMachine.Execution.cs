/* Auth: Prince Biscuit/王子饼干 */
using System;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> 注册命令和变量并执行命令语法树. </summary>
    public partial class VirtualMachine
    {
        /// <summary> execute a syntax tree, if stack is not empty, then return the top value else return null. </summary>
        public object ExecuteRoot(SyntaxTree node)
        {
            try
            {
                return ExecuteRootCore(node);
            }
            finally
            {
                _stack.Clear();
            }
        }

        // 异常时也由入口清空临时栈, 避免下一条控制台或宏命令读取残留值.
        private object ExecuteRootCore(SyntaxTree node)
        {
            Execute(node, default);
            if (_stack.Count == 0) return null;

            var topValue = TopValue;

            _stack.Clear();
            if (topValue is IValueGetter valueGetter)
            {
                return valueGetter.GetValue();
            }
            else if (topValue is StackCallable callable)
            {
                return callable.Instance;
            }
            else if (topValue is StackSourceInput inputContainer) return inputContainer.inputStr;
            return null;
        }

        private void Execute(SyntaxTree node, ExecuteStatus status = default)
        {
            switch (node.opcode)
            {
                #region Operations
                case SyntaxTreeCode.SetField:
                    ExecuteSetField(node, status);
                    break;
                case SyntaxTreeCode.SetElement:
                    ExecuteSetElement(node, status);
                    break;
                case SyntaxTreeCode.Assign:
                    ExecuteAssign(node, status);
                    break;
                case SyntaxTreeCode.Loadvar:
                    ExecuteLoadVariable(node, status);
                    break;
                case SyntaxTreeCode.Index:
                    ExecuteIndex(node, status);
                    break;
                case SyntaxTreeCode.Call:
                    ExecuteCall(node, status);
                    break;
                case SyntaxTreeCode.Dot:
                    ExecuteDot(node, status);
                    break;
                case SyntaxTreeCode.Cvt:
                    ExecuteConvert(node, status);
                    break;
                #endregion
                #region Factors
                case SyntaxTreeCode.FactorFloat:
                    ExecuteFactorFloat(node, status);
                    break;
                case SyntaxTreeCode.FactorInt:
                    ExecuteFactorInt(node, status);
                    break;
                case SyntaxTreeCode.FactorTrue:
                    ExecuteFactorTrue(node, status);
                    break;
                case SyntaxTreeCode.FactorFalse:
                    ExecuteFactorFalse(node, status);
                    break;
                case SyntaxTreeCode.FactorNull:
                    ExecuteFactorNull(node, status);
                    break;
                case SyntaxTreeCode.FactorString:
                    ExecuteFactorString(node, status);
                    break;
                case SyntaxTreeCode.FactorInput:
                    ExecuteFactorInput(node, status);
                    break;
                    #endregion
            }
        }

        private void ExecuteFactorFloat(SyntaxTree node, ExecuteStatus status)
        {
            switch (status.target)
            {
                case CallTarget.RequireValue:
                    if (status.requiredType != null)
                    {
                        if (status.requiredType.IsAssignableFrom(typeof(float)))
                        {
                            _stack.Push(new StackFloat(float.Parse(node.data)));
                            return;
                        }

                        if (TryParseInput(node.data, status.requiredType, out var data))
                        {
                            _stack.Push(StackValue.Wrap(data));
                            return;
                        }

                        throw new CommandExecuteException($"cannot convert \"{node.data}\" to type \"{status.requiredType}\" near \"{node.DebugInfo}\"");
                    }

                    _stack.Push(new StackFloat(float.Parse(node.data)));
                    return;
                case CallTarget.RequireCallable:
                    if (_callables.TryGetValue(node.data, out var callable))
                    {
                        _stack.Push(callable);
                        return;
                    }

                    throw new CommandExecuteException($"cannot find callable named \"{node.data}\" near \"{node.DebugInfo}\"");
                default:
                    _stack.Push(new StackFloat(float.Parse(node.data)));
                    return;
            }
        }

        private void ExecuteFactorInt(SyntaxTree node, ExecuteStatus status)
        {
            switch (status.target)
            {
                case CallTarget.RequireValue:
                    if (status.requiredType != null)
                    {
                        if (status.requiredType.IsAssignableFrom(typeof(int)))
                        {
                            _stack.Push(new StackInt(int.Parse(node.data)));
                            return;
                        }

                        if (TryParseInput(node.data, status.requiredType, out var data))
                        {
                            _stack.Push(StackValue.Wrap(data));
                            return;
                        }

                        throw new CommandExecuteException($"cannot convert \"{node.data}\" to type \"{status.requiredType}\" near \"{node.DebugInfo}\"");
                    }
                    else
                    {
                        _stack.Push(new StackInt(int.Parse(node.data)));
                        return;
                    }

                case CallTarget.RequireCallable:
                    if (_callables.TryGetValue(node.data, out var callable))
                    {
                        _stack.Push(callable);
                        return;
                    }

                    throw new CommandExecuteException($"cannot find callable named \"{node.data}\" near \"{node.DebugInfo}\"");
                default:
                    _stack.Push(new StackInt(int.Parse(node.data)));
                    return;
            }
        }

        private void ExecuteFactorTrue(SyntaxTree node, ExecuteStatus status)
        {
            switch (status.target)
            {
                case CallTarget.RequireValue:
                    if (status.requiredType != null)
                    {
                        if (status.requiredType.IsAssignableFrom(typeof(bool)))
                        {
                            _stack.Push(StackBool.True);
                            return;
                        }

                        if (TryParseInput(node.data, status.requiredType, out var data))
                        {
                            _stack.Push(StackValue.Wrap(data));
                            return;
                        }

                        throw new CommandExecuteException($"cannot convert \"{node.data}\" to type \"{status.requiredType}\" near \"{node.DebugInfo}\"");
                    }
                    else
                    {
                        _stack.Push(StackBool.True);
                        return;
                    }

                case CallTarget.RequireCallable:
                    if (_callables.TryGetValue(node.data, out var callable))
                    {
                        _stack.Push(callable);
                        return;
                    }

                    throw new CommandExecuteException($"cannot find callable named \"{node.data}\" near \"{node.DebugInfo}\"");
                default:
                    _stack.Push(StackBool.True);
                    return;
            }
        }

        private void ExecuteFactorFalse(SyntaxTree node, ExecuteStatus status)
        {
            switch (status.target)
            {
                case CallTarget.RequireValue:
                    if (status.requiredType != null)
                    {
                        if (status.requiredType.IsAssignableFrom(typeof(bool)))
                        {
                            _stack.Push(StackBool.False);
                            return;
                        }

                        if (TryParseInput(node.data, status.requiredType, out var data))
                        {
                            _stack.Push(StackValue.Wrap(data));
                            return;
                        }

                        throw new CommandExecuteException($"cannot convert \"{node.data}\" to type \"{status.requiredType}\" near \"{node.DebugInfo}\"");
                    }
                    else
                    {
                        _stack.Push(StackBool.False);
                        return;
                    }

                case CallTarget.RequireCallable:
                    if (_callables.TryGetValue(node.data, out var callable))
                    {
                        _stack.Push(callable);
                        return;
                    }

                    throw new CommandExecuteException($"cannot find callable named \"{node.data}\" near \"{node.DebugInfo}\"");
                default:
                    _stack.Push(StackBool.False);
                    return;
            }
        }

        // Execute factor null.
        private void ExecuteFactorNull(SyntaxTree node, ExecuteStatus status)
        {
            switch (status.target)
            {
                case CallTarget.RequireValue:
                    if (status.requiredType != null)
                    {
                        if (status.requiredType.IsNullable())
                        {
                            _stack.Push(StackNull.Default);
                            return;
                        }

                        if (TryParseInput(node.data, status.requiredType, out var data))
                        {
                            _stack.Push(StackValue.Wrap(data));
                            return;
                        }

                        throw new CommandExecuteException($"cannot convert \"{node.data}\" to type \"{status.requiredType}\" near \"{node.DebugInfo}\"");
                    }
                    else
                    {
                        _stack.Push(StackNull.Default);
                        return;
                    }

                case CallTarget.RequireCallable:
                    if (_callables.TryGetValue(node.data, out var callable))
                    {
                        _stack.Push(callable);
                        return;
                    }

                    throw new CommandExecuteException($"cannot find callable named \"{node.data}\" near \"{node.DebugInfo}\"");
                default:
                    _stack.Push(StackNull.Default);
                    return;
            }
        }

        private bool HandleString(string input, ExecuteStatus status, string debugInfo)
        {
            if (status.target == CallTarget.RequireValue)
            {
                if (status.requiredType != null)
                {
                    if (status.requiredType.IsAssignableFrom(typeof(string)))
                    {
                        _stack.Push(new StackString(input));
                        return true;
                    }

                    if (TryParseInput(input, status.requiredType, out var data))
                    {
                        _stack.Push(StackValue.Wrap(data));
                        return true;
                    }

                    throw new CommandExecuteException($"cannot parse \"{input}\" to type \"{status.requiredType}\" near \"{debugInfo}\"");
                }

                if (_callables.TryGetValue(input, out var command))
                {
                    // the method can only call when it has no parameter
                    if (command.ParameterCount == 0)
                    {
                        object methodReturn = command.Invoke(Array.Empty<object>());

                        _stack.Push(StackValue.Wrap(methodReturn));
                        return true;
                    }
                }

                return false;
            }

            if (status.target == CallTarget.RequireCallable && _callables.TryGetValue(input, out var callable))
            {
                _stack.Push(callable);
                return true;
            }

            return false;
        }

        private void ExecuteFactorInput(SyntaxTree node, ExecuteStatus status)
        {
            if (HandleString(node.data, status, node.DebugInfo)) return;

            _stack.Push(new StackSourceInput(node.data));
        }

        private void ExecuteFactorString(SyntaxTree node, ExecuteStatus status)
        {
            if (HandleString(node.data, status, node.DebugInfo)) return;

            _stack.Push(new StackString(node.data));
        }

        private void ExecuteSetField(SyntaxTree node, ExecuteStatus status)
        {
            Execute(node.children[0], new ExecuteStatus { target = CallTarget.RequireValue });
            var topValue = TopValue;
            var instance = UnwrapStackObjectNoRequire(topValue) ?? throw new CommandExecuteException($"cannot write value to \"null.{node.children[1].data}\" near \"{node.DebugInfo}\"");
            var instanceType = instance.GetType();
            var memberName = node.children[1].data;
            var member = instanceType.GetDefaultMember(memberName) ?? throw new CommandExecuteException($"\"{node.children[0].data}\" has no member named \"{memberName}\" near \"{node.DebugInfo}\"");
            Type memberType;
            StackProperty instancePropertyHandle;

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    var fld = (FieldInfo)member;
                    memberType = fld.FieldType;
                    instancePropertyHandle = fld.CreateStackProperty(instance, memberName) ?? throw new CommandExecuteException($"cannot write value to \"{memberName}\" of \"{instanceType}\" near \"{node.DebugInfo}\"");
                    break;
                case MemberTypes.Property:
                    var ppt = (PropertyInfo)member;
                    memberType = ppt.PropertyType;
                    instancePropertyHandle = ppt.CreateStackProperty(instance, memberName) ?? throw new CommandExecuteException($"cannot write value to \"{memberName}\" of \"{instanceType}\" near \"{node.DebugInfo}\"");
                    break;
                default:
                    throw new CommandExecuteException($"cannot write value to \"{node.children[0].data}.{memberName}\" near \"{node.DebugInfo}\"");
            }

            Execute(node.children[2], new ExecuteStatus { target = CallTarget.RequireValue, requiredType = memberType });
            var stackValue = TopValue;
            object value;

            if (stackValue is IValueGetter valueGetter)
            {
                value = valueGetter.GetValue();
            }
            else if (stackValue is StackSourceInput inputContainer)
            {
                if (TryParseInput(inputContainer.inputStr, memberType, out var data))
                {
                    instancePropertyHandle.SetValue(data);
                    return;
                }

                throw new CommandExecuteException($"cannot parse \"{inputContainer.inputStr}\" to type {memberType} near \"{node.DebugInfo}\"");
            }
            else
            {
                throw new CommandExecuteException($"cannot assign <{stackValue.stackType}> to {stackValue.stackType} near \"{node.DebugInfo}\"");
            }

            SetProperty(instancePropertyHandle, value, node.DebugInfo);
        }

        private void ExecuteLoadVariable(SyntaxTree node, ExecuteStatus status)
        {
            var variableName = node.data;

            if (TryLoadVariable(variableName)) return;

            var propertyHandle = new StackPropertyCustom(variableName);

            _localVariables.Add(variableName, propertyHandle);
            _stack.Push(propertyHandle);
        }

        private void ExecuteDot(SyntaxTree node, ExecuteStatus status)
        {
            Execute(node.children[0], new ExecuteStatus { target = CallTarget.RequireValue });
            var stackValue = TopValue;
            object instance;

            if (stackValue is IValueGetter valueGetter)
            {
                instance = valueGetter.GetValue() ?? throw new CommandExecuteException($"cannot get member from null near \"{node.DebugInfo}\"");
            }
            else if (stackValue is StackCallable callable)
            {
                instance = callable.Instance;
            }
            else
            {
                if (stackValue is StackSourceInput inputContainer)
                {
                    instance = inputContainer.inputStr;
                }
                else
                {
                    throw new CommandExecuteException($"cannot get member from <{stackValue.stackType}> near \"{node.DebugInfo}\"");
                }
            }

            var instanceType = instance.GetType();
            var memberName = node.children[1].data;
            var member = instanceType.GetDefaultMember(memberName) ?? throw new CommandExecuteException($"\"{instanceType}\" has no member named \"{memberName}\" near \"{node.DebugInfo}\"");
            StackObject result = member.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)member).CreateStackProperty(instance, memberName) ?? throw new CommandExecuteException($"invalid member dot \"{memberName}\" from {stackValue.GetType()} around \"{node.DebugInfo}\""),
                MemberTypes.Property => ((PropertyInfo)member).CreateStackProperty(instance, memberName) ?? throw new CommandExecuteException($"invalid member dot \"{memberName}\" from {stackValue.GetType()} around \"{node.DebugInfo}\""),
                MemberTypes.Method => ((MethodInfo)member).CreateStackCallable(instance) ?? throw new CommandExecuteException($"invalid member dot \"{memberName}\" from {stackValue.GetType()} around \"{node.DebugInfo}\""),
                _ => throw new CommandExecuteException($"cannot get member \"{memberName}\" from {stackValue.GetType()} around \"{node.DebugInfo}\""),
            };

            _stack.Push(result);
        }

        private void ExecuteCall(SyntaxTree node, ExecuteStatus status = default)
        {
            Execute(node.children[0], new ExecuteStatus { target = CallTarget.RequireCallable });
            var stackValue = TopValue;

            if (stackValue is StackCallable callable)
            {
                var paramsNode = node.children[1];
                var paramlist = new object[callable.ParameterCount];

                for (int i = 0; i < callable.ParameterCount; i++)
                {
                    var parameter = callable.GetParameter(i);
                    var parameterType = parameter.ParameterType;

                    if (i < paramsNode.children.Length)
                    {
                        Execute(paramsNode.children[i], new ExecuteStatus { target = CallTarget.RequireValue, requiredType = parameterType });
                        stackValue = TopValue;
                        if (stackValue is IValueGetter valueGetter)
                        {
                            object value = valueGetter.GetValue();

                            if (value == null)
                            {
                                if (parameterType.IsNullable())
                                {
                                    paramlist[i] = null;
                                    continue;
                                }

                                throw new CommandExecuteException($"invalid parameter value \"null\" for \"{parameter.Name}\" ({parameterType}) near \"{node.DebugInfo}\"");
                            }

                            var valueType = value.GetType();

                            if (parameterType.IsAssignableFrom(valueType))
                            {
                                paramlist[i] = value;
                                continue;
                            }

                            string valueStr = value.ToString() ?? throw new CommandExecuteException($"invalid parameter type \"{value.GetType()}\" for \"{parameter.Name}\" ({parameterType}) near \"{node.DebugInfo}\"");

                            if (TryParseInput(valueStr, parameterType, out var data))
                            {
                                paramlist[i] = data;
                                continue;
                            }

                            throw new CommandExecuteException($"invalid parameter type \"{value.GetType()}\" for \"{parameter.Name}\" ({parameterType}) near \"{node.DebugInfo}\"");
                        }
                        else if (stackValue is StackCallable callableParamter)
                        {
                            object value = callableParamter.Instance;

                            if (parameterType.IsAssignableFrom(value.GetType()))
                            {
                                paramlist[i] = value;
                                continue;
                            }

                            throw new CommandExecuteException($"invalid parameter type \"{value.GetType()}\" for \"{parameter.Name}\" ({parameterType}) near \"{node.DebugInfo}\"");
                        }
                        else if (stackValue is StackSourceInput inputContainerParam)
                        {
                            string input = inputContainerParam.inputStr;

                            if (parameterType == typeof(string))
                            {
                                paramlist[i] = input;
                                continue;
                            }

                            if (TryParseInput(input, parameterType, out var data))
                            {
                                paramlist[i] = data;
                                continue;
                            }

                            throw new CommandExecuteException($"cannot parse \"{input}\" to type \"{parameterType}\" near \"{node.DebugInfo}\"");
                        }
                        else
                        {
                            if (parameterType.IsNullable())
                            {
                                paramlist[i] = null;
                                continue;
                            }

                            paramlist[i] = 0;
                        }
                    }
                    else
                    {
                        if (callable.TryGetParameterDefaultValue(i, out var defaultValue))
                        {
                            paramlist[i] = defaultValue;
                            continue;
                        }

                        throw new CommandExecuteException($"parameter \"{parameter.Name}\" of method \"{callable.Name}\" is not provided");
                    }
                }

                if (callable.HasReturnValue)
                {
                    _stack.Push(StackValue.Wrap(callable.Invoke(paramlist)));
                    return;
                }

                callable.Invoke(paramlist);
                if (_treatVoidAsNull)
                {
                    _stack.Push(StackNull.Default);
                }

                return;
            }

            if (stackValue is StackSourceInput inputContainer)
            {
                // while not set required type, then it should be null, but i think it maybe user has input wrong characters, so i decide to raise en error instead of taking cover by null
                string inputStr = inputContainer.inputStr;

                if (status.requiredType == null)
                {
                    throw new CommandExecuteException($"don't know how to handle input \"{inputStr}\" near \"{node.DebugInfo}\"");
                }

                // <input> would be converted to string type only if required type is string type
                if (status.requiredType == typeof(string))
                {
                    _stack.Push(new StackString(inputStr));
                    return;
                }

                if (TryParseInput(inputStr, status.requiredType, out var data))
                {
                    _stack.Push(StackValue.Wrap(data));
                    return;
                }

                throw new CommandExecuteException($"cannot parse \"{inputStr}\" to type {status.requiredType} near \"{node.DebugInfo}\"");
            }

            throw new CommandExecuteException($"<{stackValue.stackType}> is not callable near \"{node.DebugInfo}\"");
        }

        private void ExecuteAssign(SyntaxTree node, ExecuteStatus status)
        {
            Execute(node.children[0]);
            var stackValue = TopValue;

            if (stackValue is not IValueSetter valueSetter)
            {
                if (!_ignoreInvalidSetBehaviour)
                {
                    throw new CommandExecuteException($"cannot assign value to <{stackValue.stackType}> near \"{node.DebugInfo}\"");
                }

                Execute(node.children[1]);
                return;
            }

            if (valueSetter is StackPropertyCustom)
            {
                Execute(node.children[1], new ExecuteStatus { target = CallTarget.RequireValue });
            }
            else
            {
                Execute(node.children[1], new ExecuteStatus { target = CallTarget.RequireValue, requiredType = valueSetter.ValueType });
            }

            stackValue = TopValue;
            object value;

            if (stackValue is IValueGetter valueGetter)
            {
                value = valueGetter.GetValue();
            }
            else if (stackValue is StackSourceInput inputContainer)
            {
                if (valueSetter is StackPropertyCustom customPropertyHandle)
                {
                    customPropertyHandle.SetValue(inputContainer.inputStr);
                    return;
                }

                if (TryParseInput(inputContainer.inputStr, valueSetter.ValueType, out value))
                {
                    valueSetter.SetValue(value);
                    return;
                }

                throw new CommandExecuteException($"cannot parse \"{inputContainer.inputStr}\" to type \"{valueSetter.ValueType}\" near \"{node.DebugInfo}\"");
            }
            else
            {
                throw new CommandExecuteException($"cannot assign <{stackValue.stackType}> to \"{stackValue.stackType}\" near \"{node.DebugInfo}\"");
            }

            SetProperty(valueSetter, value, node.DebugInfo);
        }

        private void ExecuteIndex(SyntaxTree node, ExecuteStatus status)
        {
            Execute(node.children[0]);
            var stackValue = TopValue;

            if (stackValue is not IValueGetter valueGetter)
            {
                throw new CommandExecuteException($"cannot get element from <{stackValue.stackType}> near \"{node.DebugInfo}\"");
            }

            object instance = valueGetter.GetValue() ?? throw new CommandExecuteException($"cannot get element from \"null\" near \"{node.DebugInfo}\"");
            var instanceType = instance.GetType();

            Execute(node.children[1]);
            stackValue = TopValue;
            if (stackValue is StackInt intContainer)
            {
                object value = VirtualMachineUtils.GetElement(instance, intContainer.intValue);

                _stack.Push(StackValue.Wrap(value));
                return;
            }

            if (stackValue is StackBool boolContainer)
            {
                object value = VirtualMachineUtils.GetElement(instance, boolContainer.boolValue ? 1 : 0);

                _stack.Push(StackValue.Wrap(value));
                return;
            }

            if (stackValue is IValueGetter indexValueGetter)
            {
                object index = indexValueGetter.GetValue();
                object result = VirtualMachineUtils.GetElement(instance, index);

                _stack.Push(StackValue.Wrap(result));
                return;
            }

            throw new CommandExecuteException($"<{stackValue.stackType}> cannot be index of \"{instanceType}\" near \"{node.DebugInfo}\"");
        }

        private void ExecuteSetElement(SyntaxTree node, ExecuteStatus status)
        {
            Execute(node.children[1]);
            var stackValue = TopValue;

            if (stackValue is not IValueGetter indexGetter)
            {
                throw new CommandExecuteException($"<{stackValue.stackType}> cannot be index near \"{node.DebugInfo}\"");
            }

            object indexValue = indexGetter.GetValue() ?? throw new CommandExecuteException($"null cannot be index \"{node.DebugInfo}\"");
            bool isArray = stackValue is StackInt || stackValue is StackBool;

            Execute(node.children[0]);
            stackValue = TopValue;
            if (stackValue is not IValueGetter instanceGetter)
            {
                throw new CommandExecuteException($"cannot set element of <{stackValue.stackType}> near \"{node.DebugInfo}\"");
            }

            object instance = instanceGetter.GetValue() ?? throw new CommandExecuteException($"cannot set element of null near \"{node.DebugInfo}\"");
            Type elementType = null;

            if (isArray)
            {
                elementType = VirtualMachineUtils.GetElementTypeOfArray(instance);
            }

            if (elementType == null)
            {
                isArray = false;
                elementType = VirtualMachineUtils.GetElementTypeOfDict(instance, indexValue.GetType()) ?? throw new CommandExecuteException($"cannot set element of <{stackValue.stackType}> near \"{node.DebugInfo}\"");
            }

            Execute(node.children[2], new ExecuteStatus { requiredType = elementType });
            stackValue = TopValue;
            if (stackValue is not IValueGetter valueGetter)
            {
                throw new CommandExecuteException($"cannot set <{stackValue.stackType}> as element of {instance.GetType()} near \"{node.DebugInfo}\"");
            }

            object value = valueGetter.GetValue();

            if (isArray)
            {
                if (stackValue is StackInt intContainer)
                {
                    VirtualMachineUtils.SetElement(instance, intContainer.intValue, value);
                    return;
                }

                if (stackValue is StackBool boolContainer)
                {
                    VirtualMachineUtils.SetElement(instance, boolContainer.boolValue ? 1 : 0, value);
                    return;
                }

                throw new CommandExecuteException($"cannot convert \"{indexValue}\" to int type near \"{node.DebugInfo}\"");
            }

            VirtualMachineUtils.SetElement(instance, indexValue, value);
        }

        private void ExecuteConvert(SyntaxTree node, ExecuteStatus status)
        {
            Execute(node.children[0]);
            var stackValue = TopValue;
            string typeAlias = node.children[1].data;
            Type requiredType = TryGetTypeByAlias(typeAlias) ?? status.requiredType;

            if (requiredType == null)
            {
                _stack.Push(stackValue);
                return;
            }

            if (stackValue is IValueGetter valueGetter)
            {
                object value = valueGetter.GetValue();

                if (value == null)
                {
                    if (requiredType.IsNullable())
                    {
                        _stack.Push(StackNull.Default);
                        return;
                    }

                    throw new CommandExecuteException($"cannot convert \"null\" to \"{status.requiredType}\" near \"{node.DebugInfo}\"");
                }

                if (requiredType.IsAssignableFrom(value.GetType()))
                {
                    _stack.Push(stackValue);
                    return;
                }

                string inputStr = value.ToString();

                if (inputStr == null)
                {
                    // unknown how to handle an input with null ToString() just push it back
                    _stack.Push(stackValue);
                    return;
                }

                if (requiredType == typeof(string))
                {
                    _stack.Push(new StackString(inputStr));
                    return;
                }

                if (TryParseInput(inputStr, requiredType, out object resultValue))
                {
                    _stack.Push(StackValue.Wrap(resultValue));
                    return;
                }
            }

            if (stackValue is StackSourceInput inputGetter)
            {
                if (requiredType == typeof(string))
                {
                    _stack.Push(new StackString(inputGetter.inputStr));
                    return;
                }

                if (TryParseInput(inputGetter.inputStr, requiredType, out object resultValue))
                {
                    _stack.Push(StackValue.Wrap(resultValue));
                    return;
                }
            }

            _stack.Push(stackValue);
        }
    }
}
