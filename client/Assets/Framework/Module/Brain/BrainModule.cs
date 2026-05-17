using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sanmon.Module
{
    /// <summary>
    /// 用来作为接管操作的管理器，
    /// </summary>
    public class BrainModule: MonoBehaviour,
        IModule
    {
        private readonly Dictionary<Type, IBrain> mBrains = new();
        
        public void ChangeBrain<T>() where T : IBrain
        {
            
        }

        public int InitOrder => InitOrderDefine.BRAIN;
        
        public void Init()
        {
            
        }

        public void Deinit()
        {
            
        }

        public void OnUpdate(float dt)
        {
            
        }
    }
}