using System;
using UnityEngine;
using YooAsset;

namespace Sanmon.Module
{
    public class GameObjectAsyncHandle
    {
        internal PrefabInfo info;

        internal InstantiateOptions? options;
        
        public event Action<GameObject> e_onLoaded
        {
            add
            {
                if (info.handle.IsDone)
                {
                    value?.Invoke(info.NewOne(options));
                }
                else
                {
                    _callback += value;
                }
            }
            remove => _callback -= value;
        }
        
        private Action<GameObject> _callback;
        
        internal GameObjectAsyncHandle(PrefabInfo info, InstantiateOptions? options)
        {
            this.info = info;
            this.options = options;
            info.handle.Completed += OnGameObjectLoaded;
        }

        private void OnGameObjectLoaded(AssetHandle handle)
        {
            _callback?.Invoke(info.NewOne(options));
        }
    }
}