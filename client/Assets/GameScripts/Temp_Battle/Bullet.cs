using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScripts.Temp_Battle
{
    public class Bullet: MonoBehaviour
        , IGetSystem
        , IDamageMaker
    {
        public static GameObject template;
        
        public float speed = 5f;
        
        public BindDamageCollider box;
        
        public Unit caster;

        private Vector3 direction;
        
        private void Start()
        {
            box.onEntityEnter = OnHit;
        }

        public static void Cast(Unit caster, Vector3 start, Vector3 target)
        {
            var face = target - start;
            var instance = Game.Asset.LoadPrefabAndInstantiateNew("Assets/GameAsset/prefab/bullet").GetComponent<Bullet>();

            instance.caster = caster;
            
            instance.transform.position = start;
            instance.direction = face;
            if(face != Vector3.zero)
                instance.transform.rotation = Quaternion.LookRotation(face);
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnHit(Unit target)
        {
            var damageInfo = new DamageInfo()
            {
                maker = this,
                attacker = caster,
                defender = target,
                box = new ColliderBox()
                {
                    hitPosition = transform.position,
                    hitNormal = transform.rotation * Vector3.up,
                },
                source = DamageSource.Main,
                damage = new List<DamagePair>()
                {
                    new () {
                        type = DamageType.Magical,
                        value = Random.Range(5,10),
                    },
                    new () {
                        type = DamageType.Physical,
                        value = Random.Range(10,15),
                    },
                },
                buffsOnHitForAttacker = new List<Buff>(),
                buffsOnHitForDefender = new List<Buff>()
            };
            
            this.System().Get<BattleSystem>().OnUnitDealDamage(damageInfo);
            
            Destroy(gameObject);
        }

        public string Name => "Bullet";
    }
}