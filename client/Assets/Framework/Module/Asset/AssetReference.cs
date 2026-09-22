using System;
using System.Collections.Generic;
using Sanmon.Core;
using UnityEngine;
using YooAsset;

namespace Sanmon.Module
{
    [DisallowMultipleComponent]
    public class AssetReference: MonoBehaviour
        , IGetModule
    {
        private readonly HashSet<AssetHandle> handles = new ();

        public void BindGo(AssetHandle handle)
        {
            handles.Add(handle);
        }
        
        private void OnDestroy()
        {
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
        }
    }
}