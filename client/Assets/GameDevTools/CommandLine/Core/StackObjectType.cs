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

namespace RedSaw.CommandLineInterface
{
    /// <summary> Stack object type. </summary>
    public enum StackObjectType
    {
        /// <summary>
        /// an object which maintain a value and provide only getter
        /// </summary>
        ValueGetter,
        /// <summary>
        /// an object which maintain a value and provide only setter
        /// </summary>
        ValueSetter,
        /// <summary>
        /// an object maintance a value and provide both getter and setter
        /// </summary>
        ValueProperty,
        /// <summary>
        /// an object which can be called
        /// </summary>
        Callable,
        /// <summary>
        /// unknown type
        /// </summary>
        Any
    }
}
