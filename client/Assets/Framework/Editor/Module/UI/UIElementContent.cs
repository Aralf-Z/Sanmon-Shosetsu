using System.Collections.Generic;
using Sanmon.Module;

namespace Sanmon.Editor
{
    internal class UIElementContent
    {
        public readonly UIPart root;
        public readonly List<UIPart> elements = new ();
        public readonly List<FieldBind> fieldBinds = new ();
        
        public UIElementContent(UIPart root)
        {
            this.root = root;
        }
    }
}