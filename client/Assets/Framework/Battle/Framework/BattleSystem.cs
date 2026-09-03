using System;
using Sanmon.Core;
using Sanmon.Helper;
using Sanmon.Syztem;

namespace Sanmon.Battle
{
    public class BattleSystem: SystemBase
    {
        private const int PIPELINE_DO_LIMIT = 50;
        
        private BattleNote _note;
        private DealDamagePipeline _dealDamagePipeline;
        private DealHealPipeline _dealHealPipeline;
        
        private bool _isDealing = false;
        
        private event Action<DamageInfo> e_onUnitDealDamage;
        private event Action<HealInfo> e_onUnitHeal;
        
        protected internal override void Init()
        {
            _note = this.Note().Get<BattleNote>();
        }

        public void OnUnitDealDamage(DamageInfo damageInfo)
        {
            _note.damageInfos.Enqueue(damageInfo);
            DealOnce();
        }

        public void OnUnitDealHeal(HealInfo healInfo)
        {
            _note.healInfos.Enqueue(healInfo);
            DealOnce();
        }
        
        private void DealOnce()
        {
            if (_isDealing) return;//避免递归
            
            _isDealing = true;
            
            var count = 0;
            DamageInfo firstDamageInfo = null;
            
            if (_note.damageInfos.Count > 0)
            {
                firstDamageInfo = _note.damageInfos.Peek();
            
                while (_note.damageInfos.Count > 0)
                {
                    var info = _note.damageInfos.Dequeue();
                    _dealDamagePipeline.Do(info);
                    e_onUnitDealDamage?.Invoke(info);
                    
                    count++;
                    if (count > PIPELINE_DO_LIMIT)
                    {
                        Logger.LogWarning($"战斗处理超标, 大于{PIPELINE_DO_LIMIT} -> \n{firstDamageInfo}", "战斗");
                        _note.damageInfos.Clear();
                    }
                }
            }

            if (_note.healInfos.Count > 0)
            {
                var firstHealInfo = _note.healInfos.Peek();
            
                while (_note.healInfos.Count > 0)
                {
                    var info = _note.healInfos.Dequeue();
                    _dealHealPipeline.Do(info);
                    e_onUnitHeal?.Invoke(info);
                    count++;
                    if (count > PIPELINE_DO_LIMIT)
                    {
                        Logger.LogWarning($"战斗处理超标, 大于{PIPELINE_DO_LIMIT} -> \n{firstDamageInfo}\n{firstHealInfo}", "战斗");
                        _note.healInfos.Clear();
                    }
                }
            }
            
            _isDealing = false;
        }
    }
}