using System.IO;
using Sanmon.Helper;

namespace Sanmon.Core
{
    /// <summary>
    /// <para> 不依赖其他模块。懒加载的配置文件：日志、画面设置等 </para>
    /// <para> 路径在 "Assets/StreamingAssets/{typeof(T).Name}.json", 没有会自动创建 </para>
    /// </summary>
    public abstract class AppConfig<T> where T : new ()
    {
        public static T Ins
        {
            get
            {
                if (sInstance == null)
                {
                    var fileName = $"{typeof(T).Name}.json";
                    var path = Path.Combine(PathHelper.ConfigPath, fileName);
                    if (File.Exists(path))
                    {
                        var file = File.ReadAllText(path);
                        sInstance = JsonHelper.DeserializeObject<T>(file);
                    }
                    else
                    {
                        sInstance = new T();
                        Save();
                    }
                }
                
                return sInstance;
            }
        }
        
        private static T sInstance;

        public static void Save()
        {
            var fileName = $"{typeof(T).Name}.json";
            var path = Path.Combine(PathHelper.ConfigPath, fileName);
            var json = JsonHelper.SerializeObject(sInstance);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();            
#endif
        }
    }
}