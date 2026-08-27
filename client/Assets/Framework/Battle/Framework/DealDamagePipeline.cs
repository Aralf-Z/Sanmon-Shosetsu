using System;
using Framework.Pipeline;
using Game.Config.Battle;
using Sanmon.Helper;
using Sanmon.Utility.Math;

namespace Sanmon.Battle
{
    public class DealDamagePipeline: Pipeline<DamageInfo>
    {
        public DealDamagePipeline()
        {
            SetHeader(new HandleCheckTag())
                .SetNext(new HandleCheckIsHit())
                .SetNext(new HandleCheckIsCrit())
                .SetNext(new HandleResult());
        }

        private class HandleCheckTag : Handler<DamageInfo>
        {
            protected override bool CanHandle(DamageInfo request)
            {
                return !request.defender.tag.Contains(Tag.nondamageable);
            }

            protected override void Process(DamageInfo request)
            {
                
            }
        }

        private class HandleCheckIsHit : Handler<DamageInfo>
        {
            private readonly Dice _dice = new Dice();
            
            protected override bool CanHandle(DamageInfo request)
            {
                return _dice.RollSum(new Dices(20)) > 1;
            }
            
            protected override void Process(DamageInfo request)
            {
                request.isHit = true;
            }
        }

        private class HandleCheckIsCrit : Handler<DamageInfo>
        {
            private readonly Dice _dice = new Dice();
            
            protected override void Process(DamageInfo request)
            {
                request.isCrit = _dice.RollSum(new Dices(20)) == 20;
            }
        }

        private class HandleResult : Handler<DamageInfo>
        {
            protected override void Process(DamageInfo request)
            {
                Logger.LogInfo($"伤害结束 -> {request}", "战斗");
            }
        }
    }
}