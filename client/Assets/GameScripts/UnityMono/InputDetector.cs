using System;
using UnityEngine;

namespace GameScripts
{
    public class InputDetector: MonoBehaviour
    {
        public Vector3 move;

        private void Update()
        {
            if (move == Vector3.zero)
            {
                move = new Vector3(Input.GetAxis("Horizontal"), Input.GetKeyDown(KeyCode.Space) ? 10 : 0 , Input.GetAxis("Vertical"));
                
            }
        }
    }
}