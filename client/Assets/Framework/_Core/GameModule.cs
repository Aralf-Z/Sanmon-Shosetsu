using System.Collections.Generic;
using System.Linq;
using Framework.Module;
using Sanmon.Helper;
using Sanmon.Module;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 各种模块的管理器
    /// </summary>
    public class GameModule: MonoBehaviour
    {
        public AssetModule Asset { get; private set; }
        public UIModule UI { get; private set; }
        public ConfigModule Config { get; private set; }
        public LuaModule Lua { get; private set; }
        public InputModule Input { get; private set; }
        public CameraModule Camera { get; private set; }
        
        internal bool IsInit { get; private set; }
        
        private List<IModule> _modules = new();
        private int _initIndex = 0;
        
        internal void Init()
        {
            Asset = GetComponentInChildren<AssetModule>();
            _modules.Add(Asset);
            UI = GetComponentInChildren<UIModule>();
            _modules.Add(UI);
            Config = GetComponentInChildren<ConfigModule>();
            _modules.Add(Config);
            Lua = GetComponentInChildren<LuaModule>();
            _modules.Add(Lua);
            Input = GetComponentInChildren<InputModule>();
            _modules.Add(Input);
            Camera = GetComponentInChildren<CameraModule>();
            _modules.Add(Camera);

            _modules = _modules.OrderBy(m => m.InitOrder).ToList();
            _modules[_initIndex].Init();
        }

        internal void Destroy()
        {
            foreach (var module in _modules)
                module.Deinit();
            _modules.Clear();
            Asset = null;
            UI = null;
            Config = null;
            Lua = null;
            Input = null;
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
                    SanmonLogger.LogInfo($"'{_modules[_initIndex].GetType().Name}'初始化成功", "MODULE");
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