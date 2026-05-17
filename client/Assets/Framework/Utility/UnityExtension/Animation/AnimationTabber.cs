using System.Collections.Generic;
using UnityEngine;

namespace Sanmon.Utility.UnityExtension
{
    [DisallowMultipleComponent]
    public class AnimationTabber : MonoBehaviour
    {
        [SerializeField] private List<string> triggers = new List<string>() { "default" };

        private Animator mAnimator;

        private void Awake()
        {
            mAnimator = GetComponent<Animator>();
        }

        public void Switch(string trigger)
        {
            mAnimator.SetTrigger(trigger);
        }

        public void Switch(int triggerHash)
        {
            mAnimator.SetTrigger(triggerHash);
        }
    }
}