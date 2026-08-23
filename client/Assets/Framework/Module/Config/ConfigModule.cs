using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Game.Config;
using Luban;
using Luban.SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Module
{
    public class ConfigModule: MonoBehaviour,
        IModule
    {
        public static string ConfigFilePath => Path.Combine(Application.streamingAssetsPath, "tables");
        public static string CodeFilePath => Path.Combine(Application.dataPath, "Script/Table/CodeGen");
        
        int IModule.InitOrder => InitOrderDefine.CONFIG;
        bool IModule.IsInit => _isInit;

        public Tables Tables { get; private set; }
        public Version Version { get; private set; }
        
        void IModule.Init()
        {
            Version = new Version();
            Logger.LogInfo("版本信息加载成功！",  "CONFIG");
            Logger.LogInfo($"游戏版本：{Version.GameVersion}",  "CONFIG");
            Logger.LogInfo($"游戏内部版本：{Version.GameVersionInteral}",  "CONFIG");
            
            var tablesCtor = typeof(Tables).GetConstructors()[0];
            var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            
#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
            Logger.LogWarning("Web和安卓模式尚未支持表格加载！", "CONFIG");
            // try
            // {
            //     using var request = UnityWebRequest.Get(fileListPath);
            //     await request.SendWebRequest();
            //
            //     if (request.result == UnityWebRequest.Result.Success)
            //     {
            //         var fileContent = request.downloadHandler.text;
            //         var fileList = JsonConvert.DeserializeObject<List<string>>(fileContent);
            //         var byteMaps = new Dictionary<string, ByteBuf>();
            //         var jsonMaps = new Dictionary<string, JSONNode>();
            //
            //
            //         if (loaderReturnType == typeof(ByteBuf))
            //         {
            //             byteMaps = await LoadByteBuf_Web(fileList);
            //         }
            //         else
            //         {
            //             jsonMaps = await LoadJson_Web(fileList);
            //         }
            //
            //         var loader = loaderReturnType == typeof(ByteBuf)
            //             ? new Func<string, ByteBuf>(file => byteMaps[file])
            //             : (Delegate) new Func<string, JSONNode>(file => jsonMaps[file]);
            //
            //         Tables = (Tables) tablesCtor.Invoke(new object[] {loader});
            //     }
            // }
            // catch (Exception e)
            // {
            //     LogDebug.LogError(e);
            //     throw;
            // }
#else
            try
            { 
                Delegate loader = loaderReturnType == typeof(ByteBuf) 
                    ? new Func<string, ByteBuf>(LoadByteBuf)
                    : new Func<string, JSONNode>(LoadJson);
            
                Tables = (Tables)tablesCtor.Invoke(new object[] {loader});
                
                Logger.LogInfo("表配置加载成功！", "CONFIG");

                
                
                _isInit = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
#endif
        }
        
        void IModule. Deinit()
        {
            
        }
        
        void IModule.OnLogicUpdate(float dt)
        {
            
        }
        
        private bool _isInit;
        
        private ByteBuf LoadByteBuf(string file)
        {
            return new (File.ReadAllBytes(Path.Combine(ConfigFilePath, $"{file}.bytes")));
        }

        private JSONNode LoadJson(string file)
        {
            return JSON.Parse(File.ReadAllText(Path.Combine(ConfigFilePath, $"{file}.json")));
        }
        
        private async UniTask<Dictionary<string, ByteBuf>> LoadByteBuf_Web(List<string> files)
        {
            var bytesMap = new Dictionary<string, ByteBuf>();
            
            foreach (var file in files)
            {
                using var request = UnityWebRequest.Get(Path.Combine(ConfigFilePath, $"{file}.bytes"));
                await request.SendWebRequest();
            
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var bytes = request.downloadHandler.data;
                    bytesMap.Add(file, new ByteBuf(bytes)); 
                }
            }

            return bytesMap;
        }

        private async UniTask<Dictionary<string, JSONNode>> LoadJson_Web(List<string> files)
        {
            var jsonMaps = new Dictionary<string, JSONNode>();
            
            foreach (var file in files)
            {
                using var request = UnityWebRequest.Get(Path.Combine(ConfigFilePath, $"{file}.json"));
                await request.SendWebRequest();
            
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var jsonStr = request.downloadHandler.text;
                    jsonMaps.Add(file, JSON.Parse(jsonStr)); 
                }
            }

            return jsonMaps;
        }
    }
}