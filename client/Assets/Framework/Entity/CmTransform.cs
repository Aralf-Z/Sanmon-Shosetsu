using UnityEngine;

namespace Sanmon.GameEntity
{
    public class CmTransform: ComponentBase
    {
        private GameObject go;

        private BindTransform bind;
        
        public Vector3 Position
        {
            get => go.transform.position;
            set => go.transform.position = value;
        }

        public Vector3 Scale
        {
            get => go.transform.localScale;
            set => go.transform.localScale = value;
        }

        public Quaternion Quaternion
        {
            get => go.transform.rotation;
            set => go.transform.rotation = value;
        }

        public float RotationX
        {
            get => go.transform.rotation.eulerAngles.x;
            set => go.transform.rotation = Quaternion.Euler(value, 0, 0);
        }
        
        public float RotationY
        {
            get => go.transform.rotation.eulerAngles.y;
            set => go.transform.rotation = Quaternion.Euler(0, value, 0);
        }
        
        public float RotationZ
        {
            get => go.transform.rotation.eulerAngles.z;
            set => go.transform.rotation = Quaternion.Euler(0, 0, value);
        }

        public void SetTransform(BindTransform bindTransform)
        {
            bind = bindTransform;
            go = bind.gameObject;
        }
    }
}