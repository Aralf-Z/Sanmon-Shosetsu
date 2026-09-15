using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sanmon.Module;
using Sanmon.Core;
using Sanmon.Utility.Singleton;
using ZLua;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Battle
{
    internal class EffectManager: Singleton<EffectManager>
        , IGetModule
    { 
        private readonly IReadOnlyList<EffectEvent> DEFAULT_EVENTS = new List<EffectEvent>();
        
        public const string EFFECT_PATH = "effect";

        private Dictionary<string, EventType> _events;
        
        public readonly Dictionary<string, Effect> effects = new ();
        
        public readonly Dictionary<string, List<EffectEvent>> events = new ();
        
        public EffectManager()
        {
#if UNITY_EDITOR
            var time = UnityEngine.Time.realtimeSinceStartup;
#endif 
            LoadEffectManifest();
            LoadBuiltinEffect();
            LoadLuaEffect();
            ImportEvents();
#if UNITY_EDITOR
            Logger.LogInfo($"EffectManager 初始化 cost '{UnityEngine.Time.realtimeSinceStartup - time}s'", "战斗");
#endif
        }

        /// <summary>
        /// 注册Effect；优先级：外部注册Lua > 内部加载Lua > 外部注册CSharp > 内部加载Csharp
        /// </summary>
        public void RegisterEffect(List<Effect> newEffects)
        {
            foreach (var effect in newEffects)
            {
                if(effect.script is Script.None)
                    Logger.LogWarning($"未知来源, effect '{effect.name}' 的脚本来源为{effect.script}", "Effect");
                else
                {
                    effect.IsOverride = effects.ContainsKey(effect.name);
                    effects[effect.name] = effect;
                }
            }

            ImportEvents();
        }
        
        /// <summary>
        /// 请求Effect
        /// </summary>
        public Effect RequireEffect(string effectName)
        {
            if(effects.TryGetValue(effectName, out var effectInstance))
                return  effectInstance;
            
            throw new EffectException($"请求错误的Effect名: {effectName}");
        }
        
        /// <summary>
        /// 请求events
        /// </summary>
        public IReadOnlyList<EffectEvent> RequireEvent(string eventName)
        {
            if(events.TryGetValue(eventName, out var list))
                return list;

            return DEFAULT_EVENTS;
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
            foreach (var effect in EffectBuiltin.effects)
            {
                effect.script = Script.CSharp;
                effects[effect.name] = effect;
            }
        }

        //加载lua的Effect或者覆盖内置的Effect
        private void LoadLuaEffect()
        {
            foreach (var info in this.Module().Lua.GetLuaModule(EFFECT_PATH))
            {
                var eff = new Effect { name = info.file, script = Script.Lua, IsOverride = effects.ContainsKey(info.file)};
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

        private void ImportEvents()
        {
            events.Clear();

            foreach (var (_, effect) in effects)
            {
#if UNITY_EDITOR
                if (effect.IsOverride)
                {
                    Logger.LogWarning($"'{effect.name}' is override.", "Effect");
                }
#endif
                
                foreach (var evt in effect.events)
                {
                    if(!events.TryGetValue(evt.name, out var list))
                    {
                        list = new List<EffectEvent>();
                        events[evt.name] = list;
                    }
                    list.Add(evt);
                }
            }
        }
    }
}