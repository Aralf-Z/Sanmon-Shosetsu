using System;
using Sanmon.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sanmon.GameEntity
{
    public class CmWorldModel: ComponentBase
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

        public Quaternion Rotation
        {
            get => Go.transform.rotation;
            set => Go.transform.rotation = value;
        }
        
        public string name = string.Empty;

        public event Action e_onLoaded;

        public void TryLoad()
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
            e_onLoaded?.Invoke();
        }
        
        public void SetModel(GameObject go, string newName = null)
        {
            Go = go;
            if (!string.IsNullOrEmpty(newName))
            {
                Go.name = name =  newName;
            }
            Bind = Go.AddComponent<ModelBind>();
            Bind.Bind(this);
            e_onLoaded?.Invoke();
        }
    }
}