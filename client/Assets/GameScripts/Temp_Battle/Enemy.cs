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
            self = Game.Sys<BattleSystem>().RegisterUnit("player", transform);
            
            var health = self.attri.AddValue(Attribute.Health, Random.Range(15, 25));
            self.attri.AddValue(Attribute.Attack, 10f);
            self.resource.Add(Attribute.Health, health);
            self.group.ServeFor = Group.Enemy;
            unitCollider.unit = self;

            var model = self.entity.AddComponent<CmModel>();
            model.SetModel(gameObject);
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
            Game.Sys<BattleSystem>().UnregisterUnit(self);
            yield return new WaitForSeconds(.5f);
            Destroy(gameObject);
        } 
    }
}