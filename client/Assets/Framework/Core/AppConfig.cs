using System.IO;
using System.Text.RegularExpressions;
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
                if (_instance == null)
                {
                    var fileName = $"{ToSnakeCase(typeof(T).Name)}.json";
                    var path = Path.Combine(PathHelper.ConfigPath, fileName);
                    if (File.Exists(path))
                    {
                        var file = File.ReadAllText(path);
                        _instance = JsonHelper.DeserializeObject<T>(file);
                    }
                    else
                    {
                        _instance = new T();
                        Save();
                    }
                }
                
                return _instance;
            }
        }
        
        private static T _instance;

        private static void Save()
        {
            var fileName = $"{ToSnakeCase(typeof(T).Name)}.json";
            var path = Path.Combine(PathHelper.ConfigPath, fileName);
            var json = JsonHelper.SerializeObject(_instance);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();            
#endif
        }
        
        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // 使用正则表达式在适当位置插入下划线
            // 模式说明：
            //   (?<=[a-z0-9])(?=[A-Z])        : 小写/数字 后紧跟大写（如 "aB" -> "a_B"）
            //   (?<=[A-Z])(?=[A-Z][a-z])      : 大写字母后紧跟大写+小写（如 "XMLParser" 中的 "L" 与 "P" 之间）
            //   (?<=[0-9])(?=[A-Z])           : 数字后紧跟大写（如 "2Update" -> "2_Update"）
            //   (?<=[A-Z])(?=[0-9])           : 大写后紧跟数字（如 "Version2" -> "Version_2"）
            var pattern = @"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=[0-9])(?=[A-Z])|(?<=[A-Z])(?=[0-9])";
            var result = Regex.Replace(input, pattern, "_");
            return result.ToLowerInvariant();
        }
    }
}