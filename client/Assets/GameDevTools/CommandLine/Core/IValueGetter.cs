/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> mark any object with a value getter method. </summary>
    public interface IValueGetter
    {
        Type ValueType { get; }

        object GetValue();
    }
}
