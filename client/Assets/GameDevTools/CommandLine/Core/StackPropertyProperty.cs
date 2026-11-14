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
    /// <summary> StackProperty implementation based on Reflection.PropertyInfo,. </summary>
    public class StackPropertyProperty : StackProperty
    {
        /// <summary> Property info. </summary>
        public readonly PropertyInfo propertyInfo;

        /// <summary> Instance. </summary>
        public readonly object instance;

        /// <summary> Value type. </summary>
        public override Type ValueType => propertyInfo.PropertyType;

        /// <summary> Create a StackPropertyProperty instance. </summary>
        public StackPropertyProperty(string name, string description, string tag, object instance, PropertyInfo propertyInfo) : base(name, description, tag)
        {
            this.instance = instance;
            this.propertyInfo = propertyInfo;
        }

        /// <summary> Create a StackPropertyProperty instance. </summary>
        public StackPropertyProperty(string name, object instance, PropertyInfo propertyInfo) : base(name)
        {
            this.instance = instance;
            this.propertyInfo = propertyInfo;
        }

        /// <summary> Get value. </summary>
        public override object GetValue() => propertyInfo.GetValue(instance);

        /// <summary> Set value. </summary>
        public override void SetValue(object value) => propertyInfo.SetValue(instance, value);
    }
}
