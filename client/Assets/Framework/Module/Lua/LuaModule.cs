using System.Collections.Generic;
using System.IO;
using System.Text;
using Sanmon.Helper;
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

        public IEnumerable<LuaInfo> GetLuaModule(string workspace)
        {
            var path = Path.Combine(RootPath, workspace);

            if (!Directory.Exists(path))
                throw new InvalidPathException(path);

            foreach (var file in Directory.EnumerateFiles(path, "*.lua", SearchOption.AllDirectories))
            {
                var fullPath = file.PathFormat();
                var relativePath = Path.GetRelativePath(path, file);
                var moduleName = Path.Combine(workspace, Path.ChangeExtension(relativePath, null));
                var fileName = Path.GetFileNameWithoutExtension(file);

                yield return new LuaInfo(fullPath, moduleName, fileName);
            }  
        }

        public struct LuaInfo
        {
            public readonly string fullPath;
            public readonly string module;
            public readonly string file;

            public LuaInfo(string fullPath, string module, string file)
            {
                this.fullPath = fullPath;
                this.module = module;
                this.file = file;
            }

            public override string ToString()
            {
                return $"fullPath: {fullPath}, module: {module}, file: {file}";
            }
        }
    }
}