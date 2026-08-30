using System;
using Sanmon.Battle;
using Sanmon.GameEntity;
using UnityEngine;

namespace GameScripts.Temp_Battle
{
    public class Bullet: MonoBehaviour
    {
        public float speed = 10f;
        
        public DamageColliderBind box;
        
        public Entity caster;

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

        private void OnHit(Entity target)
        {
            // Debug.Log($"{caster.GetComponent<CmModel>().Go.name} Hit On {target.GetComponent<CmModel>().Go.name}");
            Debug.Log($"OnHit");
            Destroy(gameObject);
        }
    }
}