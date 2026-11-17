/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Stack string. </summary>
    public class StackString : StackValue
    {
        public override Type ValueType => typeof(string);

        public readonly string strValue;

        public StackString(string value) : base(value)
        {
            strValue = value;
        }
    }
}
