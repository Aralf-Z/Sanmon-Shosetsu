using System.IO;
using Sanmon.Helper;
using UnityEngine;
using YooAsset;

namespace Sanmon.Module
{
    public class AssetModule: MonoBehaviour,
        IModule
    {
        int IModule.InitOrder => InitOrderDefine.ASSET;

        void IModule.Init()
        {
            YooAssets.Initialize();
            
            var package = YooAssets.CreatePackage(DEFAULT_PACKAGE);
            
        }

        void IModule.Deinit()
        {
            mAssetMap = null;
        }
        
        void IModule.OnLogicUpdate(float dt)
        {
            
        }

        public const string DEFAULT_PACKAGE = "DefaultPackage";

        public EPlayMode playMode = EPlayMode.EditorSimulateMode;
        
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