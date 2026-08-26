using Sanmon.Core;
using Sanmon.Helper;
using Sanmon.Syztem;

namespace Sanmon.Battle
{
    public class BattleSystem: SystemBase
    {
        private const int PIPELINE_DO_LIMIT = 20;
        
        private BattleNote _note;
        private DealDamagePipeline _dealDamagePipeline;
        
        protected internal override void Init()
        {
            _note = this.Note().Get<BattleNote>();
        }

        public void OnUnitDealDamage(DamageInfo damageInfo)
        {
            _note.damageInfos.Enqueue(damageInfo);

            var count = 0;
            
            while (_note.damageInfos.Count > 0)
            {
                var info = _note.damageInfos.Dequeue();
                _dealDamagePipeline.Do(info);
                
                count++;
                if (count > PIPELINE_DO_LIMIT)
                {
                    Logger.LogWarning($"DamageInfo处理超标, 大于{PIPELINE_DO_LIMIT} -> '{damageInfo}'", "战斗");
                }
            }
        }
    }
}