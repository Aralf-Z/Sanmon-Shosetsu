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
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> StackProperty implementation based on Reflection.FieldInfo. </summary>
    public class StackPropertyField : StackProperty
    {
        /// <summary> Field info. </summary>
        public readonly FieldInfo fieldInfo;

        /// <summary> Instance. </summary>
        public readonly object instance;

        /// <summary> Value type. </summary>
        public override Type ValueType => fieldInfo.FieldType;

        /// <summary> Create a StackPropertyField instance. </summary>
        public StackPropertyField(string name, string description, string tag, object instance, FieldInfo fieldInfo) : base(name, description, tag)
        {
            this.instance = instance;
            this.fieldInfo = fieldInfo;
        }

        /// <summary> Create a StackPropertyField instance. </summary>
        public StackPropertyField(string name, object instance, FieldInfo fieldInfo) : base(name)
        {
            this.instance = instance;
            this.fieldInfo = fieldInfo;
        }

        /// <summary> Get value. </summary>
        public override object GetValue()
        {
            return fieldInfo.GetValue(instance);
        }

        /// <summary> Set value. </summary>
        public override void SetValue(object value)
        {
            fieldInfo.SetValue(instance, value);
        }
    }
}
