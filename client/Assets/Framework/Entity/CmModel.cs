using System;
using Sanmon.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sanmon.GameEntity
{
    public class CmModel: ComponentBase
        , IGetModule
        , IGetEntity
    {
        public GameObject Go { get; private set; }

        public ModelBind Bind { get; private set; }
        
        public Transform Transform => Go.transform;
        
        public Vector3 Position
        {
            get => Go.transform.position;
            set => Go.transform.position = value;
        }

        public Vector3 Scale
        {
            get => Go.transform.localScale;
            set => Go.transform.localScale = value;
        }

        public Quaternion Quaternion
        {
            get => Go.transform.rotation;
            set => Go.transform.rotation = value;
        }

        public float RotationX
        {
            get => Go.transform.rotation.eulerAngles.x;
            set => Go.transform.rotation = Quaternion.Euler(value, 0, 0);
        }
        
        public float RotationY
        {
            get => Go.transform.rotation.eulerAngles.y;
            set => Go.transform.rotation = Quaternion.Euler(0, value, 0);
        }
        
        public float RotationZ
        {
            get => Go.transform.rotation.eulerAngles.z;
            set => Go.transform.rotation = Quaternion.Euler(0, 0, value);
        }
        
        public string Name { get; private set; }
        
        public void TryLoad(string name)
        {
            var template = this.Module().Asset.LoadSync<GameObject>(name);
            var parent = this.Entity().transform;
            
            if (template)
            {
                Go = Object.Instantiate(template, parent);
            }
            else
            {
                Go = new GameObject(name);
                Go.transform.SetParent(parent);
            }
            
            Go.name = name;
            Bind = Go.AddComponent<ModelBind>();
            Bind.Bind(this);
        }
        
        public void SetModel(GameObject go, string newName)
        {
            Go = go;
            Go.name = Name = newName;
            Bind = Go.AddComponent<ModelBind>();
            Bind.Bind(this);
        }
    }
}