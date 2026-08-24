using UnityEngine;

namespace Sanmon.Module
{
    public class Version
    {
        private GameVersion _gameVersion;

        public Version()
        {
            _gameVersion = Sanmon.Module.GameVersion.Ins;

            var v1 = _gameVersion.versionCode / 100_000_000;
            var v2 = v1 == 0 ? _gameVersion.versionCode / 1000_000 : _gameVersion.versionCode % v1 / 1000_000;
            var v3 = v2 == 0 ? _gameVersion.versionCode / 1000 : _gameVersion.versionCode % v2 / 1000;
            var v4 = v3 == 0 ? _gameVersion.versionCode : _gameVersion.versionCode % v3;

            GameVersion = $"version {v1:00}.{v2:00}.{v3:000}";
            GameVersionInteral = $"version {v1:00}.{v2:00}.{v3:000}.{v4:000}";
        }

        public string UnityVersion => Application.unityVersion;
        
        public string YooAssetVersion => AssetModule.YOO_ASSET_VERSION;
        
        public string GameVersion { get; }
        
        public string GameVersionInteral { get; }
    }
}