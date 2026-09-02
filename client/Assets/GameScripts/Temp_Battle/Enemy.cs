using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
using UnityEngine;

namespace GameScripts.Temp_Battle
{
    public class Enemy: MonoBehaviour
        , IGetEntity
    {
        public Unit self;

        [SerializeField] private UnitColliderBind colliderBind;
        
        private void Awake()
        {
            var en = this.Entity().Require();

            en.AddComponent<CmAttribute>();
            en.AddComponent<CmResource>();
            en.AddComponent<CmBlackboard>();
            en.AddComponent<CmTag>();
            en.AddComponent<CmGroup>();
            
            self = new Unit(en);
            
            var health = self.attri.AddValue(Attribute.Health, 100f);
            self.attri.AddValue(Attribute.Attack, 10f);
            
            self.resource.Add(Attribute.Health, health);

            var model = en.AddComponent<CmModel>();
            model.SetModel(gameObject);
            
            colliderBind.host = en;
        }
    }
}