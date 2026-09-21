using System;

namespace Sanmon.Module
{
    /// <summary>
    /// ui元数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UIMetaDataAttribute : Attribute
    {
        public readonly int id;
        
        public UIMetaDataAttribute(int id)
        {
            this.id = id;
        }
    }
}