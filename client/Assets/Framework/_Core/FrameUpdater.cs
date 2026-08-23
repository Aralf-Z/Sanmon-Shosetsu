using System;
using System.Collections.Generic;
using Sanmon.Utility.Singleton;

namespace Sanmon.Core
{
    public class FrameUpdater: Singleton<FrameUpdater>
    {
        private readonly List<Action<float>> _updaters = new List<Action<float>>();
        
        public void Add(Action<float> updater) => _updaters.Add(updater);
        
        public void Remove(Action<float> updater) => _updaters.Remove(updater);
        
        internal void FrameUpdate(float dt)
        {
            foreach (var updater in _updaters)
            {
                updater?.Invoke(dt);
            }
        }
    }
}