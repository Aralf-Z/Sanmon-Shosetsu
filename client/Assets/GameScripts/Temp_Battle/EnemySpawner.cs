using System;
using Sanmon.Helper;
using Sanmon.Module;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScripts.Temp_Battle
{
    public class EnemySpawner: MonoBehaviour
    {
        public float range;
        
        public float interval;

        public int count;

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;
            
            if (timer >= interval)
            {
                for (var i = 0; i < count; i++)
                {
                    var en = Game.Asset.LoadAsyncGo("pb_enemy");
                    en.e_onLoaded += go =>
                    {
                        var pos = transform.position + new Vector3(Random.Range(0, 1f), 0, Random.Range(0, 1f)) * Random.Range(-range, range);
                        go.transform.position = new Vector3(pos.x, 2f, pos.z);
                    };
                }
                
                timer = 0;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}