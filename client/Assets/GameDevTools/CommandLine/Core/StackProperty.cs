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
    /// <summary> the stack object which maintain a value and provide both getter and setter,. </summary>
    public abstract class StackProperty : StackObject, IValueGetter, IValueSetter
    {
        /// <summary> Name. </summary>
        public readonly string name;

        /// <summary> Description. </summary>
        public readonly string description;

        /// <summary> Tag. </summary>
        public readonly string tag;

        /// <summary> Name. </summary>
        public virtual string Name => name;

        /// <summary> the type of target value. </summary>
        public abstract Type ValueType { get; }

        /// <summary> Create a StackProperty instance. </summary>
        public StackProperty(string name, string description, string tag) : base(StackObjectType.ValueProperty)
        {
            this.name = name;
            this.description = description;
            this.tag = tag;
        }

        /// <summary> Create a StackProperty instance. </summary>
        public StackProperty(string name) : base(StackObjectType.ValueProperty)
        {
            this.name = name;
            this.description = string.Empty;
            this.tag = null;
        }

        /// <summary> value getter. </summary>
        public abstract object GetValue();

        /// <summary> value setter. </summary>
        public abstract void SetValue(object value);

        /// <summary> check if target tag is same as this tag, if this tag is null, then return true. </summary>
        public bool CompareTag(string tag)
        {
            if (this.tag == null)
                return true;
            return this.tag == tag;
        }
    }
}
