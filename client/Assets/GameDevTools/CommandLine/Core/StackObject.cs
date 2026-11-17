/* Auth: Prince Biscuit/王子饼干 */
namespace RedSaw.CommandLineInterface
{
    /// <summary> represent the base value type of VirtualMachine stack, use to wrap a value or a callable object. </summary>
    public abstract class StackObject
    {
        public readonly StackObjectType stackType;

        public StackObject(StackObjectType stackType)
        {
            this.stackType = stackType;
        }
    }
}
