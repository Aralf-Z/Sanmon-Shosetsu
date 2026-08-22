using UnityEngine;

namespace Sanmon.Module
{
    public class Version
    {
        private GameVersion _gameVersion;

        public Version()
        {
            _gameVersion = GameVersion.Ins;
        }

        public string UnityVersion => Application.unityVersion;
        
        public string yooAssetVersion => AssetModule.YOO_ASSET_VERSION;
    }
}