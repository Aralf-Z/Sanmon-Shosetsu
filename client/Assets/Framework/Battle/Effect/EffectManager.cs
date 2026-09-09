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

        private static readonly Dictionary<string, EventPair> _eventsPairs = new();
        
        public readonly Dictionary<string, Effect> effects = new ();
        
        public EffectManager()
        {
#if UNITY_EDITOR
            var time = UnityEngine.Time.realtimeSinceStartup;
#endif 
            //加载内置的Effect
            foreach (var effect in EffectBuiltIn.Effects)
                effects[effect.name] = effect;
            
            //加载lua的Effect或者覆盖内置的Effect
            foreach (var luaModule in this.Module().Lua.GetLuaModule(EFFECT_PATH))
            {
                var effect = LoadLuaEffect(luaModule);
                effects[effect.name] = effect;
            }
            
#if UNITY_EDITOR
            Logger.LogInfo($"EffectManager 初始化 cost '{UnityEngine.Time.realtimeSinceStartup - time}s'", "战斗");
#endif
        }

        public Effect Require(string effect)
        {
            if(effects.TryGetValue(effect, out var effectInstance))
                return  effectInstance;
            
            throw new EffectException($"请求错误的Effect名: {effect}");
        }

        
        private static Effect LoadLuaEffect(string effectName)
        {
            var eff = new Effect
            {
                name = effectName
            };
                
            var luaModule = Path.Combine(EFFECT_PATH, effectName);
            var luaFullPath = Path.Combine(LuaModule.RootPath, EFFECT_PATH, effectName + ".lua");
            var luaText = File.ReadAllText(luaFullPath, Encoding.UTF8);
            var manifest = LuaUtils.AnalyzeKey(luaText);
            var effectEvents = new List<EffectEvent>();

            foreach (var method in manifest)
            {
                var ee = new EffectEvent()
                {
                    order = 1,
                    name = method,
                    effect = eff,
                    eventType = EventType.Buff,
                };
                var orderFunc = $"{method}_order";
                if(manifest.Contains(orderFunc)) 
                    ee.order = LuaAppDomain.GetFunction<Func<int>>(luaModule, orderFunc).Invoke();
                    
                ee.LoadLua();
                effectEvents.Add(ee);
            }
                
            eff.events = effectEvents.ToArray();
                
            return eff;
        }
    }
}