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
        internal bool IsInit { get; private set; }
        
        public AssetModule Asset { get; private set; }
        public UIModule UI { get; private set; }
        public ConfigModule Config { get; private set; }
        
        private List<IModule> _modules = new();
        private int _initIndex = 0;
        
        internal void Init()
        {
            _modules = new List<IModule>();
            Asset = GetComponentInChildren<AssetModule>();
            _modules.Add(Asset);
            UI = GetComponentInChildren<UIModule>();
            _modules.Add(UI);
            Config = GetComponentInChildren<ConfigModule>();
            _modules.Add(Config);

            _modules = _modules.OrderBy(m => m.InitOrder).ToList();
            
            _modules[_initIndex].Init();
        }

        internal void Destroy()
        {
            IsInit = false;
        }

        internal void OnLogicUpdate(float dt)
        {
            if (IsInit)
            {
                foreach (var module in _modules)
                    module.OnLogicUpdate(dt);
            }
            else
            {
                if (_modules[_initIndex].IsInit)
                {
                    _initIndex++;
                    if (_initIndex >= _modules.Count) 
                        IsInit = true;
                    else 
                        _modules[_initIndex].Init();
                }
            }
        }
    }
}