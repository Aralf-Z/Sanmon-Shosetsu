/* Auth: Prince Biscuit/王子饼干 */
namespace RedSaw.CommandLineInterface
{
    /// <summary> Stack object type. </summary>
    public enum StackObjectType
    {
        /// <summary> an object which maintain a value and provide only getter </summary>
        ValueGetter,
        /// <summary> an object which maintain a value and provide only setter </summary>
        ValueSetter,
        ValueProperty,
        Callable,
        Any
    }
}
