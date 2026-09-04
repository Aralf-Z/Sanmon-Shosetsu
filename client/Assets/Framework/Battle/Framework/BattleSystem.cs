using System;
using Sanmon.Core;
using Sanmon.Syztem;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Battle
{
    public class BattleSystem : SystemBase
    {
        private const float PIPELINE_COST_TIME = 0.008f;//8ms

        private BattleNote _note;
        private DealDamagePipeline _dealDamagePipeline;
        private DealHealPipeline _dealHealPipeline;

        private bool _isDealing = false;

        private event Action<DamageInfo> e_onUnitDealDamage;
        private event Action<HealInfo> e_onUnitHeal;

        protected internal override void Init()
        {
            _note = this.Note().Get<BattleNote>();
            _dealDamagePipeline = new DealDamagePipeline();
            _dealHealPipeline = new DealHealPipeline();
        }

        public void OnUnitDealDamage(DamageInfo damageInfo)
        {
            var timer = UnityEngine.Time.realtimeSinceStartup;
            _note.damageInfos.Enqueue(damageInfo);
            DealOnce();
            Logger.LogDebug($"伤害处理流程花费[{((UnityEngine.Time.realtimeSinceStartup - timer) * 1000).ToString("F5")}ms]", "测试");
        }

        public void OnUnitDealHeal(HealInfo healInfo)
        {
            _note.healInfos.Enqueue(healInfo);
            DealOnce();
        }

        private void DealOnce()
        {
            if (_isDealing) return; //避免递归

            _isDealing = true;
            
            DamageInfo firstDamageInfo = null;

            if (_note.damageInfos.Count > 0)
            {
                firstDamageInfo = _note.damageInfos.Peek();

                while (_note.damageInfos.Count > 0)
                {
                    var info = _note.damageInfos.Dequeue();
                    _dealDamagePipeline.Do(info);
                    
                    if(!info.isAbort)
                        e_onUnitDealDamage?.Invoke(info);

                    // count++;
                    // if (count > PIPELINE_DO_LIMIT)
                    // {
                    //     Logger.LogWarning($"战斗处理超标, 大于{PIPELINE_DO_LIMIT} -> \n{firstDamageInfo}", "战斗");
                    //     _note.damageInfos.Clear();
                    // }
                }
            }

            if (_note.healInfos.Count > 0)
            {
                var firstHealInfo = _note.healInfos.Peek();

                while (_note.healInfos.Count > 0)
                {
                    var info = _note.healInfos.Dequeue();
                    _dealHealPipeline.Do(info);
                    if(!info.isAbort)
                        e_onUnitHeal?.Invoke(info);
                    // count++;
                    // if (count > PIPELINE_DO_LIMIT)
                    // {
                    //     Logger.LogWarning($"战斗处理超标, 大于{PIPELINE_DO_LIMIT} -> \n{firstDamageInfo}\n{firstHealInfo}", "战斗");
                    //     _note.healInfos.Clear();
                    // }
                }
            }

            _isDealing = false;
        }
    }
}