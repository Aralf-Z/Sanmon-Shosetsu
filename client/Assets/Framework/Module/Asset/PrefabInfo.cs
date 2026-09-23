using UnityEngine;
using YooAsset;

namespace Sanmon.Module
{
    internal class PrefabInfo
    {
        public string assetLocation;
        public AssetHandle handle;

        public float lastUseTimestamp;
            
        public GameObject SyncNewOne(InstantiateOptions? options)
        {
            var go = handle.InstantiateSync(options ?? new InstantiateOptions(true));
            lastUseTimestamp = Time.time;
            go.AddComponent<AssetReference>();
            return go;
        }
        
        public InstantiateOperation AsyncNewOne(InstantiateOptions? options)
        {
            return handle.InstantiateAsync(options ?? new InstantiateOptions(true));
        }
    }
}