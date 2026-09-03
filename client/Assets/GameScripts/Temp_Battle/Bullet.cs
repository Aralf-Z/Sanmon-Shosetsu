using System;
using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
using Sanmon.GameEntity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScripts.Temp_Battle
{
    public class Bullet: MonoBehaviour
        , IGetSystem
        , IDamageMaker
    {
        public float speed = 10f;
        
        public BindDamageCollider box;
        
        public Unit caster;

        private Vector3 direction;
        
        private void Start()
        {
            box.onEntityEnter = OnHit;
        }

        public void Cast(Vector3 start, Vector3 face)
        {
            transform.position = start;
            direction = face;
            if(face != Vector3.zero)
                transform.rotation = Quaternion.Euler(face);
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnHit(Unit target)
        {
            Debug.Log($"{caster.unit.Info.Name} Hit On {target.unit.Info.Name}");

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
                        value = Random.Range(20,26),
                    },
                },
            };
            
            this.System().Get<BattleSystem>().OnUnitDealDamage(damageInfo);
            
            Destroy(gameObject);
        }

        public string Name => "Bullet";
    }
}