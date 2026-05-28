using System;
using UnityEngine;

namespace GameScripts
{
    public class InputDetector: MonoBehaviour
    {
        public Vector3 Move { get; private set; }

        private void Update()
        {
            Move = new Vector3(Input.GetAxis("Horizontal"), 0 , Input.GetAxis("Vertical"));
        }
    }
}