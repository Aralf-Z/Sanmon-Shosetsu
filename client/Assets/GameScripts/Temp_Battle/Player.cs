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
    {
        public float moveSpeed = 3f;
        public Bullet bullet;
        public Enemy enemy;

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
            
            self = new Unit(en);
            
            var health = self.attri.AddValue(Attribute.Health, 100f);
            self.attri.AddValue(Attribute.Attack, 10f);
            
            self.resource.Add(Attribute.Health, health);
            
            var model = en.AddComponent<CmModel>();
            var trans =  en.AddComponent<CmTransform>();
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
                var start = transform.position.SetY(4);
                instance.caster = self;
                instance.Cast(start, enemy.transform.position - start);
            }
        }
    }
}