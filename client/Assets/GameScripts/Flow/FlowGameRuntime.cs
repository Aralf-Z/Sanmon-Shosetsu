using System;
using System.Collections.Generic;
using Sanmon.Battle;
using Sanmon.Core;
using UnityEngine.SceneManagement;

namespace GameScripts
{
    public class FlowGameRuntime: FlowBase
    {
        private float mTimer;
        
        protected override void Init()
        {
            
        }

        protected override void Enter()
        {
            var effList = new List<Effect>()
            {
                DealDamageEffect.effect,
            };
            
            Game.BattleSystem.RegisterEffect(effList);
            
            SceneManager.LoadScene("GameRuntime");
        }

        protected override void LogicUpdate(float dt)
        {
            
        }

        protected override void Exit()
        {
            
        }
    }
}