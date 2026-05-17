using System;
using UnityEngine;

namespace Sanmon.Module
{
    public class FieldBind: MonoBehaviour
    {
#if UNITY_EDITOR
        private void Reset()
        {
            FindParent();

            genName = $"{name}";

            if (transform is RectTransform) genComponentType = "UnityEngine.RectTransform";
            else throw new NullReferenceException($"'{name}' is not a valid ui component type.");
        }

        public void FindParent()
        {
            var bindGroup = transform.GetComponentInParent<UIElement>();
            if (bindGroup == null)
            {
                throw new NullReferenceException($"can not find parent for field '{name}'.");
            }

            genParent = bindGroup;
        }

        [SerializeField] public string genComponentType;
        [SerializeField] public string genName;
        [SerializeField] public UIElement genParent;
#endif
    }
}