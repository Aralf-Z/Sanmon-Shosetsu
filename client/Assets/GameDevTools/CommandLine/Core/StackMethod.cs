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
    /// <summary> callable object based on MethodInfo, it could be static or instance method. </summary>
    public class StackMethod : StackCallable
    {
        /// <summary> Instance. </summary>
        public readonly object instance;

        /// <summary> Method info. </summary>
        public readonly MethodInfo methodInfo;

        /// <summary> Parameters. </summary>
        public readonly ParameterInfo[] parameters;

        /// <summary> Name. </summary>
        public override string Name => methodInfo.Name;

        /// <summary> Parameter count. </summary>
        public override int ParameterCount => parameters.Length;

        /// <summary> Return type. </summary>
        public override Type ReturnType => methodInfo.ReturnType;

        /// <summary> Instance. </summary>
        public override object Instance => methodInfo;

        /// <summary> command parameter information. </summary>
        public string ParameterInfo
        {
            get
            {
                if (parameters.Length == 0)
                    return "()";
                string result = string.Empty;
                foreach (var parameter in parameters)
                {
                    result += $"{parameter.ParameterType.Name} {parameter.Name}, ";
                }

                return $"({result[..^2]})";
            }
        }

        /// <summary> instance method. </summary>
        public StackMethod(object instance, MethodInfo methodInfo) : base()
        {
            this.instance = instance;
            this.methodInfo = methodInfo;
            parameters = methodInfo.GetParameters();
        }

        /// <summary> static method. </summary>
        public StackMethod(MethodInfo methodInfo) : base()
        {
            this.methodInfo = methodInfo;
            if (!methodInfo.IsStatic)
            {
                throw new CommandExecuteException($"require static method but get instance method \"{methodInfo.Name}\" in \"{methodInfo.DeclaringType}\"");
            }

            parameters = methodInfo.GetParameters();
        }

        /// <summary> Get parameter. </summary>
        public override ParameterInfo GetParameter(int idx) => parameters[idx];

        /// <summary> Try get parameter default value. </summary>
        public override bool TryGetParameterDefaultValue(int index, out object defaultValue)
        {
            if (index < 0 || index >= parameters.Length || !parameters[index].HasDefaultValue)
            {
                defaultValue = default;
                return false;
            }

            defaultValue = parameters[index].DefaultValue;
            return true;
        }

        /// <summary> Invoke. </summary>
        public override object Invoke(object[] args)
        {
            return methodInfo.Invoke(instance, args);
        }
    }
}
