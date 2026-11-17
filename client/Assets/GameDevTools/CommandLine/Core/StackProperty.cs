/* Auth: Prince Biscuit/王子饼干 */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> the stack object which maintain a value and provide both getter and setter,. </summary>
    public abstract class StackProperty : StackObject, IValueGetter, IValueSetter
    {
        public readonly string name;

        public readonly string description;

        public readonly string tag;

        public virtual string Name => name;

        public abstract Type ValueType { get; }

        public StackProperty(string name, string description, string tag) : base(StackObjectType.ValueProperty)
        {
            this.name = name;
            this.description = description;
            this.tag = tag;
        }

        public StackProperty(string name) : base(StackObjectType.ValueProperty)
        {
            this.name = name;
            this.description = string.Empty;
            this.tag = null;
        }

        public abstract object GetValue();

        public abstract void SetValue(object value);

        /// <summary> check if target tag is same as this tag, if this tag is null, then return true. </summary>
        public bool CompareTag(string tag)
        {
            if (this.tag == null) return true;

            return this.tag == tag;
        }
    }
}
