using System.Collections.Generic;
using System.IO;
using System.Text;
using Sanmon.Module;
using Unity.Properties;
using UnityEngine;
using ZLua;

namespace Framework.Module
{
    public class LuaModule: MonoBehaviour
        , IModule
    {
        int IModule.InitOrder => InitOrderDefine.LUA;

        bool IModule.IsInit => _isInit;

        void IModule.Init()
        {
            _isInit = true;
        }

        void IModule.Deinit()
        {
            
        }

        void IModule.OnLogicUpdate(float dt)
        {
            
        }
        
        private bool _isInit = false;

        private static string RootPath 
        {
            get
            {
# if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "../..", "luaScripts");
# else      
                return Path.Combine(Application.streamingAssetsPath, "LuaScripts");
# endif    
            }

        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitZLuaOnStartup()
        {
            LuaAppDomain.Initialize(LoadLuaModule);
        }

        private static string LoadLuaModule(string module)
        {
            var path = Path.Combine(RootPath, module + ".lua");
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }

        public IEnumerable<string> GetLuaFileName(string folder)
        {
            var path = Path.Combine(RootPath, folder);
            
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories))
                    yield return Path.GetFileNameWithoutExtension(file);
            }
            else
            {
                throw new InvalidPathException(path);
            }
        }
    }
}