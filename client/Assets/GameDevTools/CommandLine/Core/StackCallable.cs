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
    /// <summary> Stack callable. </summary>
    public abstract class StackCallable : StackObject
    {
        /// <summary> callable identifier. </summary>
        public abstract string Name { get; }

        /// <summary> the return type of callable. </summary>
        public abstract Type ReturnType { get; }

        /// <summary> parameter count. </summary>
        public abstract int ParameterCount { get; }

        /// <summary> check if callable has return value. </summary>
        public bool HasReturnValue => ReturnType != typeof(void);

        /// <summary> MethodInfo or Delegate. </summary>
        public abstract object Instance { get; }

        /// <summary> Create a StackCallable instance. </summary>
        public StackCallable() : base(StackObjectType.Callable)
        {
        }

        /// <summary> get parameter information. </summary>
        public abstract ParameterInfo GetParameter(int index);

        /// <summary> try get parameter default value if target parameter is optional. </summary>
        public abstract bool TryGetParameterDefaultValue(int index, out object defaultValue);

        /// <summary> invoke callable. </summary>
        public abstract object Invoke(object[] args);
    }
}
