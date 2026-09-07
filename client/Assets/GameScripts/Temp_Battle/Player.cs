using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
using Sanmon.GameEntity;
using Sanmon.Helper;
using UnityEngine;

namespace GameScripts.Temp_Battle
{
    public class Player : MonoBehaviour
    , IGetEntity
    , IGetSystem
    {
        public float moveSpeed = 3f;
        public Bullet bullet;

        public Unit self;

        private void Awake()
        {
            var en = this.Entity().Require("player");

            en.AddComponent<CmAttribute>();
            en.AddComponent<CmResource>();
            en.AddComponent<CmBlackboard>();
            en.AddComponent<CmTag>();
            en.AddComponent<CmGroup>();
            en.AddComponent<CmEffect>();
            var model = en.AddComponent<CmModel>();
            var trans =  en.AddComponent<CmTransform>();
            
            self = new Unit(en);
            
            var health = self.attri.AddValue(Attribute.Health, 100f);
            self.attri.AddValue(Attribute.Attack, 10f);
            
            self.resource.Add(Attribute.Health, health);
            self.group.group = Group.Player;
            
            model.SetModel(gameObject);
            trans.SetTransform(gameObject.AddComponent<BindTransform>());
        }

        private void Update()
        {
            var direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var instance = Instantiate(bullet.gameObject).GetComponent<Bullet>();
                var start = transform.position.SetY(3);
                var target = Game.Sys<BattleSystem>().SearchNearestUnit(self, Group.Enemy);
                var dir = target ? target.transform.position - start : Vector3.forward;
                instance.caster = self;
                instance.Cast(start, dir);
            }
        }
    }
}