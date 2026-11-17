/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Stack null. </summary>
    public class StackNull : StackValue
    {
        public static StackNull Default => new();

        public override Type ValueType => typeof(void);

        /// <summary> Create a StackNull instance. </summary>
        public StackNull() : base(null)
        {
        }
    }
}
