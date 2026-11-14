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
    /// <summary> a custom property which has no target, it only maintain a value, use to store local variables. </summary>
    public class StackPropertyCustom : StackProperty
    {
        /// <summary> Value type. </summary>
        public override Type ValueType
        {
            get
            {
                if (_value == null)
                    return typeof(void);
                return _value.GetType();
            }
        }

        private object _value; // Value.

        /// <summary> Create a StackPropertyCustom instance. </summary>
        public StackPropertyCustom(string name) : base(name)
        {
            this._value = null;
        }

        /// <summary> Create a StackPropertyCustom instance. </summary>
        public StackPropertyCustom(string name, object initValue) : base(name)
        {
            this._value = initValue;
        }

        /// <summary> Get value. </summary>
        public override object GetValue()
        {
            return _value;
        }

        /// <summary> Set value. </summary>
        public override void SetValue(object value)
        {
            this._value = value;
        }
    }
}
