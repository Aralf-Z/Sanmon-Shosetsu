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
        
        private List<IModule> _modules = new();
        
        internal void Init()
        {
            _modules = new List<IModule>();
            Asset = GetComponentInChildren<AssetModule>();
            _modules.Add(Asset);
            UI = GetComponentInChildren<UIModule>();
            _modules.Add(UI);
            Config = GetComponentInChildren<ConfigModule>();
            _modules.Add(Config);
            
            foreach (var module in _modules.OrderBy(m => m.InitOrder))
            {
                module.Init();
            }
        }

        internal void Destroy()
        {
            IsInited = false;
        }

        internal void OnLogicUpdate(float dt)
        {
            foreach (var module in _modules)
            {
                module.OnLogicUpdate(dt);
            }
        }
    }
}