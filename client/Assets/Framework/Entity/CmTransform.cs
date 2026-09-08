using UnityEngine;

namespace Sanmon.GameEntity
{
    public class CmTransform: ComponentBase
    {
        private GameObject _go;

        private BindTransform _bind;
        
        public Vector3 Position
        {
            get => _go.transform.position;
            set => _go.transform.position = value;
        }

        public Vector3 Scale
        {
            get => _go.transform.localScale;
            set => _go.transform.localScale = value;
        }

        public Quaternion Quaternion
        {
            get => _go.transform.rotation;
            set => _go.transform.rotation = value;
        }

        public float RotationX
        {
            get => _go.transform.rotation.eulerAngles.x;
            set => _go.transform.rotation = Quaternion.Euler(value, 0, 0);
        }
        
        public float RotationY
        {
            get => _go.transform.rotation.eulerAngles.y;
            set => _go.transform.rotation = Quaternion.Euler(0, value, 0);
        }
        
        public float RotationZ
        {
            get => _go.transform.rotation.eulerAngles.z;
            set => _go.transform.rotation = Quaternion.Euler(0, 0, value);
        }

        public void SetBind(Transform transform)
        {
            _bind = transform.GetComponent<BindTransform>() ?? transform.gameObject.AddComponent<BindTransform>();
            _go = _bind.gameObject;
        }
    }
}