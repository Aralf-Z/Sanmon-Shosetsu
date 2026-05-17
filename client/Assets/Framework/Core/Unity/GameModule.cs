using System.Collections.Generic;
using System.Linq;
using Sanmon.Module;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 各种模块的管理器
    /// </summary>
    public class GameModule: MonoBehaviour
    {
        internal bool IsInited { get; private set; }
        
        public AssetModule Asset { get; private set; }
        public UIModule UI { get; private set; }
        public ConfigModule Config { get; private set; }
        
        private List<IModule> modules = new();
        
        internal void Init()
        {
            modules = new List<IModule>();
            Asset = GetComponentInChildren<AssetModule>();
            modules.Add(Asset);
            UI = GetComponentInChildren<UIModule>();
            modules.Add(UI);
            Config = GetComponentInChildren<ConfigModule>();
            modules.Add(Config);
            
            foreach (var module in modules.OrderBy(m => m.InitOrder))
            {
                module.Init();
            }
        }

        internal void Destroy()
        {
            IsInited = false;
        }
        
        internal void OnUpdate(float dt)
        {
            
        }

        internal void OnLateUpdate(float dt)
        {
            
        }

        internal void OnFixedUpdate(float dt)
        {
            foreach (var module in modules)
            {
                module.OnUpdate(dt);
            }
        }
    }
}