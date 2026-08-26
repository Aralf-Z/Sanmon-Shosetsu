using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class CmBlackboard: ComponentBase
    {
        private readonly Dictionary<Blackboard, float> _blackboard = new Dictionary<Blackboard, float>();
        
        public float GetOrDefault(Blackboard blackboard, float defaultValue = 0)
        {
            if(_blackboard.TryGetValue(blackboard, out var value))
                return value;
            
            _blackboard[blackboard] = defaultValue;
            return defaultValue;
        }

        public float AddValue(Blackboard blackboard, float value)
        {
            if (_blackboard.TryGetValue(blackboard, out var current))
            {
                current += value;
                _blackboard[blackboard] = current;
                return current;
            }

            _blackboard[blackboard] = value;
            return value;
        }
    }
}