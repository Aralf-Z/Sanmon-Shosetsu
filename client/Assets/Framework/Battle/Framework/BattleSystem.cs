using System;
using Sanmon.Core;
using Sanmon.GameEntity;
using Sanmon.Syztem;
using UnityEngine;
using ZLinq;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Battle
{
    public class BattleSystem : SystemBase
    , ISystemUpdater
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

        public Unit RegisterUnit(Entity entity, Transform transform = null)
        {
            if(_note.allUnits.TryGetValue(entity, out var unit))
                return unit;
            
            var attri = entity.GetOrAddComponent<CmAttribute>();
            var res = entity.GetOrAddComponent<CmResource>();
            var bb = entity.GetOrAddComponent<CmBlackboard>();
            var tag = entity.GetOrAddComponent<CmTag>();
            var group = entity.GetOrAddComponent<CmGroup>();
            var eff = entity.GetOrAddComponent<CmEffect>();
            var trans =  entity.GetOrAddComponent<CmTransform>();
            var collider = entity.GetOrAddComponent<CmCollider>();
            
            if(transform)
            {
                trans.SetBind(transform);
                collider.SetBind(transform);
            }
            
            var newUnit = new Unit(entity, attri, res, bb, tag, group, eff, trans, collider);
            
            _note.allUnits.Add(entity, newUnit);
            
            return newUnit;
        }
        
        public Unit RegisterUnit(string unitName, Transform transform)
        {
            var en = this.Entity().Require(unitName);
            var attri = en.AddComponent<CmAttribute>();
            var res = en.AddComponent<CmResource>();
            var bb = en.AddComponent<CmBlackboard>();
            var tag = en.AddComponent<CmTag>();
            var group = en.AddComponent<CmGroup>();
            var eff = en.AddComponent<CmEffect>();
            var trans =  en.AddComponent<CmTransform>();
            var collider = en.AddComponent<CmCollider>();
            
            trans.SetBind(transform);
            
            var newUnit = new Unit(en, attri, res, bb, tag, group, eff, trans, collider);
            
            _note.allUnits.Add(en, newUnit);
            
            return newUnit;
        }
        
        public void UnregisterUnit(Unit unit, bool recycleEntity = true)
        {
            if (recycleEntity) this.Entity().Recycle(unit.entity);
            _note.allUnits.Remove(unit.entity);
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
            if (_isDealing) return; //避免递归
            
            var timer = Time.realtimeSinceStartup;
            
            _isDealing = true;
            
            if (_note.damageInfos.Count > 0)
            {
                while (_note.damageInfos.Count > 0)
                {
                    var info = _note.damageInfos.Dequeue();
                    _dealDamagePipeline.Do(info);
                    e_onUnitDealDamage?.Invoke(info);
                }
            }

            if (_note.healInfos.Count > 0)
            {
                while (_note.healInfos.Count > 0)
                {
                    var info = _note.healInfos.Dequeue();
                    _dealHealPipeline.Do(info);
                    e_onUnitHeal?.Invoke(info);
                }
            }

            _isDealing = false;
            
            var time = ((Time.realtimeSinceStartup - timer) * 1000).ToString("F5");
            Logger.LogDebug($"伤害处理流程花费[{time}ms]", "测试");
        }

        public Unit SearchNearestUnit(Unit self, Group group)
        {
            return _note.AllUnits
                .AsValueEnumerable()
                .Where(u => u.group.ServeFor == group && u != self)
                .OrderBy(u => (u.transform.Position - self.transform.Position).sqrMagnitude)
                .FirstOrDefault();
        }

        void ISystemUpdater.OnLogicUpdate(float dt)
        {
            
        }
    }
}