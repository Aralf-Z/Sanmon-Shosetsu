using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Core;
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
            var en = this.Entity().Require();

            en.AddComponent<CmAttribute>();
            en.AddComponent<CmResource>();
            en.AddComponent<CmBlackboard>();
            en.AddComponent<CmTag>();
            en.AddComponent<CmGroup>();
            
            self = new Unit(en);
            
            var health = self.attri.AddValue(Attribute.health, 100f);
            self.attri.AddValue(Attribute.attack, 10f);
            
            self.resource.Add(Attribute.health, health);
            
            var model = en.AddComponent<CmModel>();
            model.SetModel(gameObject);

            
        }

        private void Update()
        {
            var direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var instance = Instantiate(bullet.gameObject).GetComponent<Bullet>();
                var start = transform.position.SetY(4);
                instance.caster = self.unit;
                instance.Cast(start, enemy.transform.position - start);
            }
        }
    }
}