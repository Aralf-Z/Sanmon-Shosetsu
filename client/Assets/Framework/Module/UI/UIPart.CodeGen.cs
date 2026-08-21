using System;
using UnityEngine;

namespace Sanmon.Module
{
    public partial class UIPart
    {
#if UNITY_EDITOR
        private void Reset()
        {
            FindParent();
           
            genPartName = $"{name}";
        }

        public void FindParent()
        {
            genParent = transform.parent.GetComponentInParent<UIPart>();
        }
        
        [SerializeField] public string genPartName;
        [SerializeField] public UIPart genParent;
#endif
    }
}