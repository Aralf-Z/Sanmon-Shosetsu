using System;
using System.Collections.Generic;
using System.Linq;
using Sanmon.Syztem;
using UnityEngine;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Core
{
    /// <summary>
    /// 实体交互逻辑处理中枢，修改Note，如果实体交互膨胀，建议对System精简，用Command来承担膨胀压力
    /// </summary>
    public class GameSystem: MonoBehaviour
    {
        private readonly Dictionary<Type, SystemBase> _systems = new Dictionary<Type, SystemBase>();
        
        internal bool IsInit { get; private set; }
        
        internal void Init()
        {
            IsInit = true;
        }

        internal void Destroy()
        {
            IsInit = false;
        }

        public T Get<T>() where T : SystemBase
        {
            var type = typeof(T);
            if (_systems.TryGetValue(type, out var sys)) return (T)sys;
            
            var @new = (T)Activator.CreateInstance(type);
            _systems.Add(type, @new);
            
            Logger.LogInfo($"create system '{type.FullName}'", "system");
            
            return @new;
        }
    }
}