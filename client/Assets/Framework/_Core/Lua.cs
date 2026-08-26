using System.IO;
using System.Text;
using UnityEngine;
using ZLua;

namespace Sanmon.Core
{
    internal static class Lua
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitZLuaOnStartup()
        {
            LuaAppDomain.Initialize(LoadLuaModule);
        }

        private static string LoadLuaModule(string module)
        {
# if UNITY_EDITOR
            var path = Path.Combine(Application.dataPath, "../..", "LuaScripts", module + ".lua");
# else
            var path = Path.Combine(Application.streamingAssetsPath, "LuaScripts", module + ".lua.txt");
# endif
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }
    }
}