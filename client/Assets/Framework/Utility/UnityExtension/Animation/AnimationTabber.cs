using System.Collections.Generic;
using UnityEngine;

namespace Sanmon.Utility.UnityExtension
{
    [DisallowMultipleComponent]
    public class AnimationTabber : MonoBehaviour
    {
        [SerializeField] private List<string> triggers = new List<string>() { "default" };

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Switch(string trigger)
        {
            _animator.SetTrigger(trigger);
        }

        public void Switch(int triggerHash)
        {
            _animator.SetTrigger(triggerHash);
        }
    }
}