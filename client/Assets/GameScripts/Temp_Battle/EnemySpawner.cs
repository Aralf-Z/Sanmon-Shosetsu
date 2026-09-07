using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScripts.Temp_Battle
{
    public class EnemySpawner: MonoBehaviour
    {
        public Enemy template;

        public float range;
        
        public float interval;

        public int count;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;
            
            if (timer < 0f)
            {
                for (var i = 0; i < count; i++)
                {
                    var en = Instantiate(template.gameObject);
                    var pos = transform.position + new Vector3(Random.Range(0, 1f), 0, Random.Range(0, 1f)) * Random.Range(-range, range);
                    en.transform.position = new Vector3(pos.x, 2f, pos.z);
                }
                
                timer = interval;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}