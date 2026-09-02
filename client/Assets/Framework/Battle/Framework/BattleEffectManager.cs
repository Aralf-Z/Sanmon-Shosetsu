using System;
using System.Collections.Generic;
using Sanmon.Core;
using Sanmon.GameEntity;
using Sanmon.Utility.Singleton;
using ZLua;

namespace Sanmon.Battle
{
    internal class BattleEffectManager: Singleton<BattleEffectManager>
        , IGetModule
    {
        public const string ON_DEAL_DAMAGE_CHECK_TAG = "on_deal_damage_check_tag";
        
        private const string EFFECT_PATH = "effects";
        
        private static readonly List<string> LUA_FUNCTION_NAME = new List<string>()
        {
            ON_DEAL_DAMAGE_CHECK_TAG,
        };
        
        public readonly Dictionary<string, BattleEffect> effects = new ();
        
        public BattleEffectManager()
        {
            foreach (var effect in this.Module().Lua.GetLuaFileName(EFFECT_PATH))
            {
                foreach (var method in LUA_FUNCTION_NAME)
                {
                    var methodInstance = LuaAppDomain.GetFunction<Action<DamageInfo>>(effect, method);
                    if (methodInstance != null)
                    {
                        // var newEffect = new BattleEffect
                        // {
                        //     order = LuaAppDomain.GetFunction<Func<int>>(effect, "order")?.Invoke() ?? 0,
                        //     methodName = method,
                        //     method = methodInstance
                        // };
                        // effects.Add(effect, newEffect);
                    }
                }
            }
        }

        public BattleEffect Require(string effect)
        {
            if(effects.TryGetValue(effect, out var effectInstance))
                return  effectInstance;
            
            throw new KeyNotFoundException($"错误的Effect名: {effect}");
        }
    }
}