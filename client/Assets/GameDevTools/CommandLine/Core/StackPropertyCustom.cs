/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> a custom property which has no target, it only maintain a value, use to store local variables. </summary>
    public class StackPropertyCustom : StackProperty
    {
        public override Type ValueType
        {
            get
            {
                if (_value == null) return typeof(void);

                return _value.GetType();
            }
        }

        private object _value;

        public StackPropertyCustom(string name) : base(name)
        {
            this._value = null;
        }

        public StackPropertyCustom(string name, object initValue) : base(name)
        {
            this._value = initValue;
        }

        public override object GetValue()
        {
            return _value;
        }

        public override void SetValue(object value)
        {
            this._value = value;
        }
    }
}
