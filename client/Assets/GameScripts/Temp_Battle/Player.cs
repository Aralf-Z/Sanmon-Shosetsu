using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
using Sanmon.GameEntity;
using Sanmon.Helper;
using UnityEngine;

namespace GameScripts.Temp_Battle
{
    public class Player : MonoBehaviour
    {
        public float moveSpeed = 3f;

        public Unit self;

        public float castInterval = 0.5f;
        private float castTimer;
        
        private void Awake()
        {
            self = Game.Sys<BattleSystem>().RegisterUnit("player", transform);
            
            var health = self.attri.AddValue(Attribute.Health, 100f);
            self.attri.AddValue(Attribute.Attack, 10f);
            
            self.resource.Add(Attribute.Health, health);
            self.group.ServeFor = Group.Player;

            var model = self.entity.AddComponent<CmModel>();
            model.SetModel(gameObject);
        }

        private void Update()
        {
            var direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            
            transform.position += direction * moveSpeed * Time.deltaTime;

            castTimer -= Time.deltaTime;
            
            if (Input.GetKey(KeyCode.Space) && castTimer <= 0)
            {
                var start = transform.position.SetY(3);
                var target = Game.Sys<BattleSystem>().SearchNearestUnit(self, Group.Enemy);
                Bullet.Cast(self, start, target.transform.Position);
                castTimer = castInterval;
            }
        }
    }
}