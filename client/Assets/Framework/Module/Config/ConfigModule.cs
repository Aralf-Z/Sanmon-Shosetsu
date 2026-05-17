using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Game.Config;
using Luban;
using Luban.SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Sanmon.Module
{
    public class ConfigModule: MonoBehaviour,
        IModule
    {
        public static string ConfigFilePath => Path.Combine(Application.streamingAssetsPath, "tables");
        public static string CodeFilePath => Path.Combine(Application.dataPath, "Script/Table/CodeGen");
        
        int IModule.InitOrder => InitOrderDefine.CONFIG;

        public Tables Tables { get; private set; }
        
        void IModule.Init()
        {
            var tablesCtor = typeof(Tables).GetConstructors()[0];
            var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            
#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
            //只有这里采取异步的方式，因为UniTask不能使用.GetAwaiter().GetResult();故懒加载是不支持web和安卓的
            try
            {
                using var request = UnityWebRequest.Get(fileListPath);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var fileContent = request.downloadHandler.text;
                    var fileList = JsonConvert.DeserializeObject<List<string>>(fileContent);
                    var byteMaps = new Dictionary<string, ByteBuf>();
                    var jsonMaps = new Dictionary<string, JSONNode>();


                    if (loaderReturnType == typeof(ByteBuf))
                    {
                        byteMaps = await LoadByteBuf_Web(fileList);
                    }
                    else
                    {
                        jsonMaps = await LoadJson_Web(fileList);
                    }

                    var loader = loaderReturnType == typeof(ByteBuf)
                        ? new Func<string, ByteBuf>(file => byteMaps[file])
                        : (Delegate) new Func<string, JSONNode>(file => jsonMaps[file]);

                    sTables = (Tables) tablesCtor.Invoke(new object[] {loader});
                }
            }
            catch (Exception e)
            {
                LogDebug.LogError(e);
                throw;
            }
            
#else
            try
            { 
                Delegate loader = loaderReturnType == typeof(ByteBuf) 
                    ? new Func<string, ByteBuf>(LoadByteBuf)
                    : new Func<string, JSONNode>(LoadJson);
            
                Tables = (Tables)tablesCtor.Invoke(new object[] {loader});
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
        
        void IModule.OnUpdate(float dt)
        {
            
        }
        
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