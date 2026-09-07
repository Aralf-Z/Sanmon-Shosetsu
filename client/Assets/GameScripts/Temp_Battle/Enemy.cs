using System;
using System.Collections;
using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
using Sanmon.GameEntity;
using UnityEngine;
using Attribute = Game.Config.Battle.Attribute;
using Random = UnityEngine.Random;

namespace GameScripts.Temp_Battle
{
    public class Enemy: MonoBehaviour
        , IGetEntity
    {
        public Unit self;

        public float speed = .5f;
        
        [SerializeField] private BindUnitCollider unitCollider;
        [SerializeField] private Player target;
        
        private void Awake()
        {
            var en = this.Entity().Require("enemy");

            en.AddComponent<CmAttribute>();
            en.AddComponent<CmResource>();
            en.AddComponent<CmBlackboard>();
            en.AddComponent<CmTag>();
            en.AddComponent<CmGroup>();
            en.AddComponent<CmEffect>();
            var model = en.AddComponent<CmModel>();
            var trans =  en.AddComponent<CmTransform>();
            
            self = new Unit(en);
            
            var health = self.attri.AddValue(Attribute.Health, Random.Range(15, 25));
            self.attri.AddValue(Attribute.Attack, 10f);
            self.resource.Add(Attribute.Health, health);
            self.group.group = Group.Enemy;
            
            model.SetModel(gameObject);
            trans.SetTransform(gameObject.GetComponent<BindTransform>() ?? gameObject.AddComponent<BindTransform>());
            
            unitCollider.unit = self;
        }

        private void Update()
        {
            if (self.tag.Check(Tag.Dead))
            {
                unitCollider.bindCollider.enabled = false;
                StartCoroutine(TryDisappear());
            }
            else
            {
                if (!target.self.tag.Check(Tag.Dead))
                {
                    transform.position += (target.transform.position - transform.position) * speed * Time.deltaTime;
                }
            }
        }
        
        private IEnumerator TryDisappear()
        {
            yield return new WaitForSeconds(.5f);
            this.Entity().Recycle(self.unit);
            Destroy(gameObject);
        } 
    }
}