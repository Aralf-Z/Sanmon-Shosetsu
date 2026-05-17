using System;
using Sanmon.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sanmon.Entity
{
    public class WorldModel: ComponentBase
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

        public string name = string.Empty;

        public event Action Evt_OnLoaded;

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
            Evt_OnLoaded?.Invoke();
        }
    }
}