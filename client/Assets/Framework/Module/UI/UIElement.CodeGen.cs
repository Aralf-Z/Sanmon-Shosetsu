using System;
using UnityEngine;

namespace Sanmon.Module
{
    public partial class UIElement
    {
#if UNITY_EDITOR
        private void Reset()
        {
            FindParent();
           
            genElementName = $"{name}";
        }

        public void FindParent()
        {
            genParent = transform.parent.GetComponentInParent<UIElement>();
        }
        
        [SerializeField] public string genElementName;
        [SerializeField] public UIElement genParent;
#endif
    }
}