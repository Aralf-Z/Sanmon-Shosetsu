/* Auth: Prince Biscuit/王子饼干 */
using System;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> callable object based on MethodInfo, it could be static or instance method. </summary>
    public class StackMethod : StackCallable
    {
        public readonly object instance;

        public readonly MethodInfo methodInfo;

        public readonly ParameterInfo[] parameters;

        public override string Name => methodInfo.Name;

        public override int ParameterCount => parameters.Length;

        public override Type ReturnType => methodInfo.ReturnType;

        public override object Instance => methodInfo;

        public string ParameterInfo
        {
            get
            {
                if (parameters.Length == 0) return "()";

                string result = string.Empty;

                foreach (var parameter in parameters)
                {
                    result += $"{parameter.ParameterType.Name} {parameter.Name}, ";
                }

                return $"({result[..^2]})";
            }
        }

        public StackMethod(object instance, MethodInfo methodInfo) : base()
        {
            this.instance = instance;
            this.methodInfo = methodInfo;
            parameters = methodInfo.GetParameters();
        }

        public StackMethod(MethodInfo methodInfo) : base()
        {
            this.methodInfo = methodInfo;
            if (!methodInfo.IsStatic)
            {
                throw new CommandExecuteException($"require static method but get instance method \"{methodInfo.Name}\" in \"{methodInfo.DeclaringType}\"");
            }

            parameters = methodInfo.GetParameters();
        }

        public override ParameterInfo GetParameter(int idx) => parameters[idx];

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

        public override object Invoke(object[] args)
        {
            return methodInfo.Invoke(instance, args);
        }
    }
}
