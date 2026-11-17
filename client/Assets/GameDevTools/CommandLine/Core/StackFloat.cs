/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Stack float. </summary>
    public class StackFloat : StackValue
    {
        public override Type ValueType => typeof(float);

        public readonly float floatValue;

        public StackFloat(float value) : base(value)
        {
            floatValue = value;
        }
    }
}
