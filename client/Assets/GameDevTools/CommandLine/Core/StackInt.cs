/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Stack int. </summary>
    public class StackInt : StackValue
    {
        public override Type ValueType => typeof(int);

        public readonly int intValue;

        public StackInt(int value) : base(value)
        {
            intValue = value;
        }
    }
}
