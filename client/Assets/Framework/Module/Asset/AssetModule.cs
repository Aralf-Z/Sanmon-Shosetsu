using System.IO;
using Sanmon.Helper;
using UnityEngine;

namespace Sanmon.Module
{
    public class AssetModule: MonoBehaviour,
        IModule
    {
        int IModule.InitOrder => InitOrderDefine.ASSET;

        void IModule.Init()
        {
            var file = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, MAP_FILE_NAME));
            mAssetMap = JsonHelper.DeserializeObject<AssetMap>(file);
        }

        void IModule.Deinit()
        {
            mAssetMap = null;
        }
        
        void IModule.OnUpdate(float dt)
        {
            
        }
        
        public const string MAP_FILE_NAME = "asset_map.json";

        private AssetMap mAssetMap;
        
        public T LoadSync<T>(string assetName) where T : Object
        {
#if UNITY_EDITOR
            var timer = Time.realtimeSinceStartup;

            var asset = mAssetMap.Try(assetName, out var path) ? Resources.Load<T>(path) : null;
            
            var cost = Time.realtimeSinceStartup - timer;

            if (cost > .01f)
            {
                Helper.Logger.LogWarning($"‘{assetName}’ sync cost more than 0.01s [{cost}s], 'LoadAsync' is suggested", "AssetLoad");
            }
            
            return asset;
#else
            return mAssetMap.Try(assetName, out var path) ? Resources.TryLoad<T>(path) : null;
#endif
        }
    }
}