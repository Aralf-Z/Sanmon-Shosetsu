using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class CmTag: ComponentBase
    {
        private Dictionary<Tag, int> _tags = new ();

        public bool Contains(Tag tag) => TagCount(tag) > 0;
        
        public int TagCount(Tag tag) => _tags.GetValueOrDefault(tag, 0);

        public void Add(Tag tag)
        {
            _tags[tag]++;
        }

        public void Remove(Tag tag)
        {
            _tags[tag]--;
        }
    }
}