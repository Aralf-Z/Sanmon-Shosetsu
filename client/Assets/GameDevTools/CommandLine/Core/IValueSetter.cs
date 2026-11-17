/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> mark any object with a value setter method. </summary>
    public interface IValueSetter
    {
        string Name { get; }

        Type ValueType { get; }

        void SetValue(object value);
    }
}
