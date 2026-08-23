using System.Collections.Generic;
using Sanmon.GameEntity;

namespace Framework.Battle
{
    public class CmTag: ComponentBase
    {
        private Dictionary<string, int> _tags = new ();
        
        public int TagCount(string tagName) => _tags.GetValueOrDefault(tagName);

        public void Add(string tagName)
        {
            _tags[tagName]++;
        }

        public void Remove(string tagName)
        {
            _tags[tagName]--;
        }
    }
}