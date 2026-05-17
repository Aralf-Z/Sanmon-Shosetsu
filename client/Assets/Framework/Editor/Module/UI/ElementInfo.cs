using System.Collections.Generic;

namespace Sanmon.Editor
{
    internal class ElementInfo
    {
        public string typeName;
        public string className;
        public string fieldName;
        public List<ElementInfo> elements = new ();
        public List<FieldInfo> fields = new ();
        
        public IEnumerable<ElementInfo> AllElement()
        {
            var stack = new Stack<ElementInfo>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                yield return cur;

                var subs = cur.elements;
                if (subs is not { Count: > 0 }) continue;
                
                for (var i = subs.Count - 1; i >= 0; i--)
                    stack.Push(subs[i]);
            }
        }
    }
}