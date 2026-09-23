using System;
using UnityEngine;
using YooAsset;

namespace Sanmon.Module
{
    public class GameObjectAsyncHandle
    {
        internal PrefabInfo info;

        internal InstantiateOptions? options;
        
        public event Action<GameObject> e_onInstantiated
        {
            add
            {
                if (_instantiateOperation?.Result)
                {
                    value?.Invoke(_instantiateOperation.Result);
                }
                else
                {
                    _callback += value;
                }
            }
            remove => _callback -= value;
        }
        
        private Action<GameObject> _callback;
        private InstantiateOperation _instantiateOperation;
        
        internal GameObjectAsyncHandle(PrefabInfo info, InstantiateOptions? options)
        {
            this.info = info;
            this.options = options;
            info.handle.Completed += OnGameObjectLoaded;
        }

        private void OnGameObjectLoaded(AssetHandle handle)
        {
            _instantiateOperation = info.AsyncNewOne(options);
            _instantiateOperation.Completed += OnGameObjectInstantiated;
        }

        private void OnGameObjectInstantiated(AsyncOperationBase operation)
        {
            _callback?.Invoke(_instantiateOperation.Result);
        }
    }
}