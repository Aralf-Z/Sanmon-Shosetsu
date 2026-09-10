using System.Collections.Generic;
using Framework.Pipeline;
using Sanmon.Helper;

namespace Sanmon.Battle
{
    /*
     * 1.所有参与者的事件处理顺序：攻击者、防守者
     */
    public class DealDamagePipeline : Pipeline<DamageInfo>
    {
        public DealDamagePipeline()
        {
            SetHeader(new HandleCheckHit())
                .SetNext(new HandleCalculateValue())
                .SetNext(new HandleResult());
        }
        
        private static void DoDamageInfoEvent(Unit unit, string eventName, DamageInfo damageInfo)
        {
            if(damageInfo.isAbort) return;
            
            foreach (var effectEvent in unit.effect.FindEvent(eventName))
            {
                effectEvent.damageAction?.Invoke(damageInfo);
            }
        }
        
        private static void DoBuffEvent(Unit unit, string eventName, DamageInfo damageInfo)
        {
            if(damageInfo.isAbort) return;
            
            foreach (var effectEvent in unit.effect.FindEvent(eventName))
            {
                if (unit.buff.buffOnEvent.TryGetValue(eventName, out var list))
                {
                    foreach (var buff in list) 
                        effectEvent.buffDamageAction?.Invoke(buff, damageInfo);
                }
            }
        }

        private class HandleCheckHit : Handler<DamageInfo>
        {
            protected override bool Process(DamageInfo context)
            {
                //命中前
                DoDamageInfoEvent(context.attacker, DealDamageEvent.HIT_ATTACKER_BEFORE_HIT, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.HIT_DEFENDER_BEFORE_HIT, context);
                //命中检测
                DoDamageInfoEvent(context.attacker, DealDamageEvent.HIT_ATTACKER_CHECK_HIT, context);
                //命中后
                DoDamageInfoEvent(context.attacker, DealDamageEvent.HIT_ATTACKER_AFTER_HIT, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.HIT_DEFENDER_AFTER_HIT, context);
                
                return context.isHit;
            }
        }

        private class HandleCalculateValue : Handler<DamageInfo>
        {
            protected override bool Process(DamageInfo context)
            {
                //计算前
                DoDamageInfoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_BEFORE_CAL, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_BEFORE_CAL, context);
                //攻击者数值计算
                DoDamageInfoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_CHECK_CRIT, context);
                DoDamageInfoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_CHECK_EXTRA_DAMAGE, context);
                //防御者数值计算
                DoDamageInfoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_CHECK_DEFENCE, context);
                //衍生效果判断
                DoDamageInfoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_CHECK_DERIVE, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_CHECK_DERIVE, context);
                //计算后
                DoDamageInfoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_AFTER_CAL, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_AFTER_CAL, context);
                
                return true;
            }
        }

        private class HandleResult : Handler<DamageInfo>
        {
            protected override bool Process(DamageInfo context)
            {
                // 结算前
                DoDamageInfoEvent(context.attacker, DealDamageEvent.FINAL_ATTACKER_BEFORE_FINAL, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_BEFORE_FINAL, context);
                // 受击者结算
                DoDamageInfoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_EVALUATION, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_CHECK_STATE, context);
                // 衍生效果生效
                DoDamageInfoEvent(context.attacker, DealDamageEvent.FINAL_ATTACKER_DERIVE, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_DERIVE, context);
                // 结算后
                DoDamageInfoEvent(context.attacker, DealDamageEvent.FINAL_ATTACKER_AFTER_FINAL, context);
                DoDamageInfoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_AFTER_FINAL, context);
                //buff效果
                DoBuffEvent(context.attacker, BuffEvent.ATTACKER_AFTER_HIT, context);
                DoBuffEvent(context.defender, BuffEvent.DEFENDER_AFTER_HIT, context);
                
                return true;
            }
        }
    }
}