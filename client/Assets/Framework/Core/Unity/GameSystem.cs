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
        private Dictionary<Type, SystemBase> mSystems = new Dictionary<Type, SystemBase>();
        
        internal bool IsInited { get; private set; }
        
        internal void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var count = 0;

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(SystemBase).IsAssignableFrom(t)))
                {
                    var system = (SystemBase)Activator.CreateInstance(type);
                    system.Init();
                    mSystems.Add(type, system);
                    count++;
                    Logger.LogInfo($"create system '{type.FullName}'", "system");
                }
            }
            
            Logger.LogInfo($"systems loaded '{count}'.", "system");
            
            IsInited = true;
        }

        internal void Destroy()
        {
            IsInited = false;
        }

        public T Get<T>() where T : SystemBase => mSystems[typeof(T)] as T;
    }
}