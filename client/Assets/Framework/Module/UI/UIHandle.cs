using Sanmon.Core;
using UnityEngine;

namespace Sanmon.Module
{
    public sealed class UIHandle: IGetModule
    {
        public bool IsLoaded { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsDestroy { get; private set; }
        
        public IUIView View{ get; private set; }
        private GameObject _go;

        internal UIHandle() { }

        internal void Load(string path)
        {
            _go = this.Module().Asset.LoadPrefabAndInstantiateNew(path);
            View = _go.GetComponent<IUIView>();
            IsLoaded = true;
        }

        public void Destroy()
        {
            
        }
        
        public void Show()
        {
            
        }

        public void Hide()
        {
            
        }
    }
}