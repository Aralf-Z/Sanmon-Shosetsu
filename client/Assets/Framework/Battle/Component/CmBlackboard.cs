using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class CmBlackboard: ComponentBase
    {
        private readonly Dictionary<int, float> _blackboard = new ();
        
        public float GetOrDefault(int blackboard, float defaultValue = 0)
        {
            if(_blackboard.TryGetValue(blackboard, out var value))
                return value;
            
            _blackboard[blackboard] = defaultValue;
            return defaultValue;
        }
        
        public float GetOrDefault(Blackboard blackboard, float defaultValue = 0)
        {
            return GetOrDefault((int)blackboard, defaultValue);
        }

        public float ChangeValue(int blackboard, float value)
        {
            var result = _blackboard[blackboard] += value;
            return result;
        }

        public float ChangeValue(Blackboard blackboard, float value)
        {
            return ChangeValue((int)blackboard, value);
        }
        
        public float AddValue(int blackboard, float value)
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
        
        public float AddValue(Blackboard blackboard, float value)
        {
            return AddValue((int)blackboard, value);
        }
    }
}