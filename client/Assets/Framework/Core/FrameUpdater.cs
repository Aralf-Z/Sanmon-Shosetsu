using System;
using System.Collections.Generic;
using Sanmon.Utility.Singleton;

namespace Sanmon.Core
{
    public class FrameUpdater: Singleton<FrameUpdater>
    {
        private List<Action<float>> mUpdaters = new List<Action<float>>();
        
        public void AddUpdater(Action<float> updater) => mUpdaters.Add(updater);
        
        public void RemoveUpdater(Action<float> updater) => mUpdaters.Remove(updater);
        
        internal void FrameUpdate(float dt)
        {
            foreach (var updater in mUpdaters)
            {
                updater?.Invoke(dt);
            }
        }
    }
}