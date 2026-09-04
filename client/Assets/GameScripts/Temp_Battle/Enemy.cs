using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
using Sanmon.GameEntity;
using UnityEngine;

namespace GameScripts.Temp_Battle
{
    public class Enemy: MonoBehaviour
        , IGetEntity
    {
        public Unit self;

        [SerializeField] private BindUnitCollider collider;
        
        private void Awake()
        {
            var en = this.Entity().Require("enemy");

            en.AddComponent<CmAttribute>();
            en.AddComponent<CmResource>();
            en.AddComponent<CmBlackboard>();
            en.AddComponent<CmTag>();
            en.AddComponent<CmGroup>();
            en.AddComponent<CmEffect>();
            
            self = new Unit(en);
            
            var health = self.attri.AddValue(Attribute.Health, 10000f);
            self.attri.AddValue(Attribute.Attack, 10f);
            
            self.resource.Add(Attribute.Health, health);

            var model = en.AddComponent<CmModel>();
            var trans =  en.AddComponent<CmTransform>();
            model.SetModel(gameObject);
            trans.SetTransform(gameObject.AddComponent<BindTransform>());
            
            collider.unit = self;
        }
    }
}