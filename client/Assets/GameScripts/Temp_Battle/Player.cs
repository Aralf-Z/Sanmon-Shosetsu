using System;
using System.Collections;
using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Helper;
using UnityEngine;

namespace GameScripts.Temp_Battle
{
    public class Player : MonoBehaviour
    {
        public float moveSpeed = 3f;
        public Bullet bullet;
        public Enemy enemy;

        public Entity self;
        
        private void Update()
        {
            var direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var instance = Instantiate(bullet.gameObject).GetComponent<Bullet>();
                var start = transform.position.SetY(4);
                instance.Cast(start, enemy.transform.position - start);
            }
        }
    }
}