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
    /// <summary> created from PropertyInfo which has only setter. </summary>
    public class StackPropertyPropertyOnlySetter : StackPropertyProperty
    {
        /// <summary> Value type. </summary>
        public override Type ValueType => propertyInfo.PropertyType;

        /// <summary> Create a StackPropertyPropertyOnlySetter instance. </summary>
        public StackPropertyPropertyOnlySetter(string name, object instance, PropertyInfo propertyInfo) : base(name, instance, propertyInfo)
        {
        }

        /// <summary> Create a StackPropertyPropertyOnlySetter instance. </summary>
        public StackPropertyPropertyOnlySetter(string name, string description, string tag, object instance, PropertyInfo propertyInfo) : base(name, description, tag, instance, propertyInfo)
        {
        }

        /// <summary> Get value. </summary>
        public override object GetValue()
        {
            throw new CommandExecuteException($"property \"{propertyInfo.Name} ({propertyInfo.PropertyType})\" has no getter");
        }

        /// <summary> Set value. </summary>
        public override void SetValue(object value) => propertyInfo.SetValue(instance, value);
    }
}
