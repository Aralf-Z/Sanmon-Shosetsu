/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Stack bool. </summary>
    public class StackBool : StackValue
    {
        public static StackBool True => new(true);

        public static StackBool False => new(false);

        public override Type ValueType => typeof(bool);

        public readonly bool boolValue;

        public StackBool(bool value) : base(value)
        {
            boolValue = value;
        }
    }
}
