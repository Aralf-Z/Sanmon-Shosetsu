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
                .SetNext(new HandleOnHitBuff())
                .SetNext(new HandleCalculateValue())
                .SetNext(new HandleResult());
        }

        private static void DoEvent(Unit unit, string eventName, DamageInfo damageInfo)
        {
            foreach (var effectEvent in unit.effect.FindEvent(eventName))
                effectEvent.action?.Invoke(damageInfo);
        }

        private class HandleCheckHit : Handler<DamageInfo>
        {
            protected override bool Process(DamageInfo context)
            {
                //命中前
                DoEvent(context.attacker, DealDamageEvent.HIT_ATTACKER_BEFORE_HIT, context);
                DoEvent(context.defender, DealDamageEvent.HIT_DEFENDER_BEFORE_HIT, context);
                //命中检测
                DoEvent(context.attacker, DealDamageEvent.HIT_ATTACKER_CHECK_HIT, context);
                //命中后
                DoEvent(context.attacker, DealDamageEvent.HIT_ATTACKER_AFTER_HIT, context);
                DoEvent(context.defender, DealDamageEvent.HIT_DEFENDER_AFTER_HIT, context);

                return context.isHit;
            }
        }

        private class HandleOnHitBuff : Handler<DamageInfo>
        {
            protected override bool Process(DamageInfo context)
            {
                //todo
                return true;
            }
        }

        private class HandleCalculateValue : Handler<DamageInfo>
        {
            protected override bool Process(DamageInfo context)
            {
                //计算前
                DoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_BEFORE_CAL, context);
                DoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_BEFORE_CAL, context);
                //攻击者数值计算
                DoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_CHECK_CRIT, context);
                DoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_CHECK_EXTRA_DAMAGE, context);
                DoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_CHECK_RATIO, context);
                //防御者数值计算
                DoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_CHECK_DEFENCE, context);
                //衍生效果判断
                DoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_CHECK_DERIVE, context);
                DoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_CHECK_DERIVE, context);
                //计算后
                DoEvent(context.attacker, DealDamageEvent.CAL_ATTACKER_AFTER_CAL, context);
                DoEvent(context.defender, DealDamageEvent.CAL_DEFENDER_AFTER_CAL, context);

                return true;
            }
        }

        private class HandleResult : Handler<DamageInfo>
        {
            protected override bool Process(DamageInfo context)
            {
                // 结算前
                DoEvent(context.attacker, DealDamageEvent.FINAL_ATTACKER_BEFORE_FINAL, context);
                DoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_BEFORE_FINAL, context);
                // 受击者结算
                DoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_EVALUATION, context);
                DoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_CHECK_STATE, context);
                // 衍生效果生效
                DoEvent(context.attacker, DealDamageEvent.FINAL_ATTACKER_DERIVE, context);
                DoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_DERIVE, context);
                // 结算后
                DoEvent(context.attacker, DealDamageEvent.FINAL_ATTACKER_AFTER_FINAL, context);
                DoEvent(context.defender, DealDamageEvent.FINAL_DEFENDER_AFTER_FINAL, context);
                Logger.LogInfo($"伤害结束 -> {context}", "战斗");
                
                return true;
            }
        }
    }
}