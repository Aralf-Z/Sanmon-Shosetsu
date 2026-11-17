/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> the object has only getter, it can be used to wrap a value it represent literal or return value which cannot assigned to. </summary>
    public class StackValue : StackObject, IValueGetter
    {
        public readonly object value;

        public virtual Type ValueType => value?.GetType() ?? typeof(void);

        public StackValue(object value) : base(StackObjectType.ValueGetter)
        {
            this.value = value;
        }

        public static StackValue Wrap(object value)
        {
            if (value == null) return StackNull.Default;

            return value switch
            {
                int intValue => new StackInt(intValue),
                float floatValue => new StackFloat(floatValue),
                string strValue => new StackString(strValue),
                bool boolValue => boolValue
                    ? StackBool.True
                    : StackBool.False,
                _ => new StackValue(value),
            };
        }

        public object GetValue()
        {
            return value;
        }
    }
}
