using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class CmTag: ComponentBase
    {
        private Dictionary<int, int> _tags = new ();

        public bool Contains(int tag) => TagCount(tag) > 0;
        
        public int TagCount(int tag) => _tags.GetValueOrDefault(tag, 0);

        public void Add(int tag)
        {
            _tags[tag]++;
        }

        public void Add(Tag tag)
        {
            Add((int)tag);
        }
        
        public void Remove(int tag)
        {
            _tags[tag]--;
        }
        
        public void Remove(Tag tag)
        {
            Remove((int)tag);
        }
    }
}