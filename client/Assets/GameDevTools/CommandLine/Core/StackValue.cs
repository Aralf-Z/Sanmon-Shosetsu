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

namespace RedSaw.CommandLineInterface
{
    /// <summary> the object has only getter, it can be used to wrap a value it represent literal or return value which cannot assigned to. </summary>
    public class StackValue : StackObject, IValueGetter
    {
        /// <summary> Value. </summary>
        public readonly object value;

        /// <summary> Value type. </summary>
        public virtual Type ValueType => value?.GetType() ?? typeof(void);

        /// <summary> Create a StackValue instance. </summary>
        public StackValue(object value) : base(StackObjectType.ValueGetter)
        {
            this.value = value;
        }

        /// <summary> Wrap. </summary>
        public static StackValue Wrap(object value)
        {
            if (value == null)
                return StackNull.Default;
            return value switch
            {
                int intValue => new StackInt(intValue),
                float floatValue => new StackFloat(floatValue),
                string strValue => new StackString(strValue),
                bool boolValue => boolValue ? StackBool.True : StackBool.False,
                _ => new StackValue(value),
            };
        }

        /// <summary> Get value. </summary>
        public object GetValue()
        {
            return value;
        }
    }
}
