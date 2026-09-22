using UnityEngine;

namespace Sanmon.Module
{
    public static class Version
    {
        private static GameVersion _gameVersion;

        static Version()
        {
            _gameVersion = Sanmon.Module.GameVersion.Ins;

            var versionCode = _gameVersion.versionCode;

            var v1 = versionCode / 100_00_0000;
            var v2 = versionCode / 100_0000 % 100;
            var v3 = versionCode / 1_0000 % 100;
            var v4 = versionCode % 1_0000;

            GameVersion = $"v{v1:#0}.{v2:00}.{v3:00}";
            GameVersionWithBuild = $"v{v1:#0}.{v2:00}.{v3:00}.{v4:0000}";
        }

        public static string UnityVersion => Application.unityVersion;
        
        public static string YooAssetVersion => AssetModule.YOO_ASSET_VERSION;
        
        public static string GameVersion { get; }
        
        public static string GameVersionWithBuild { get; }
    }
}