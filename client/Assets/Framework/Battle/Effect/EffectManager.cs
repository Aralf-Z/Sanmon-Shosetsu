using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Framework.Module;
using Sanmon.Core;
using Sanmon.Utility.Singleton;
using ZLua;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Battle
{
    internal class EffectManager: Singleton<EffectManager>
        , IGetModule
    { 
        public const string EFFECT_PATH = "effect";

        private Dictionary<string, EventType> _events;
        
        public readonly Dictionary<string, Effect> effects = new ();
        
        public EffectManager()
        {
#if UNITY_EDITOR
            var time = UnityEngine.Time.realtimeSinceStartup;
#endif 
            LoadEffectManifest();
            LoadBuiltinEffect();
            LoadLuaEffect();
#if UNITY_EDITOR
            Logger.LogInfo($"EffectManager 初始化 cost '{UnityEngine.Time.realtimeSinceStartup - time}s'", "战斗");
#endif
        }

        //加载effect事件申明
        private void LoadEffectManifest()
        {
            _events = DealDamageEvent.DEAL_DAMAGE_EVENT
                .Concat(BuffEvent.BUFF_EVENT)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        //加载内置的Effect
        private void LoadBuiltinEffect()
        {
            foreach (var effect in EffectBuiltin.Effects)
                effects[effect.name] = effect;
        }

        //加载lua的Effect或者覆盖内置的Effect
        private void LoadLuaEffect()
        {
            foreach (var info in this.Module().Lua.GetLuaModule(EFFECT_PATH))
            {
                if (effects.ContainsKey(info.file))
                {
                    Logger.LogWarning($"lua effect '{info.fullPath}' 覆盖 effect '{info.file}'", "Effect");
                }
                
                var eff = new Effect { name = info.file };
                var luaText = File.ReadAllText(info.fullPath, Encoding.UTF8);
                var manifest = LuaUtils.AnalyzeKey(luaText);
                var effectEvents = new List<EffectEvent>();

                foreach (var method in manifest)
                {
                    if(method.EndsWith("_order"))
                        continue;
                    
                    if (_events.TryGetValue(method, out var eventType))
                    {
                        var ee = new EffectEvent()
                        {
                            order = 1,//C#的默认0, lua的默认1
                            name = method,
                            effect = eff,
                            eventType = eventType,
                        };
                        var orderFunc = $"{method}_order";
                        if(manifest.Contains(orderFunc)) 
                            ee.order = LuaAppDomain.GetFunction<Func<int>>(info.module, orderFunc).Invoke();
                    
                        ee.LoadLua();
                        effectEvents.Add(ee);
                    }
                    else
                    {
                        throw new EffectException($"非法事件：{method}");
                    }
                    
                }
                
                eff.events = effectEvents.ToArray();
                effects[info.file] = eff;
            }
        }
        
        public Effect Require(string effect)
        {
            if(effects.TryGetValue(effect, out var effectInstance))
                return  effectInstance;
            
            throw new EffectException($"请求错误的Effect名: {effect}");
        }
    }
}