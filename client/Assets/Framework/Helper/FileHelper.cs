using UnityEngine.Device;

namespace Sanmon.Helper
{
    public static class PathHelper
    {
        public static string ConfigPath => Application.streamingAssetsPath;
        
        public static string SavePath => Application.persistentDataPath;
    }
}