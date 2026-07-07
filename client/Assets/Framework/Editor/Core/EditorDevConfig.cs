using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    /// <summary>
    /// Editor：路径在 $"Assets/Editor/Sanmon/{typeof(T).Name}.asset", 会自动生成无需创建
    /// </summary>
    public abstract class EditorDevConfig<T>: ScriptableObject where T : ScriptableObject
    {
        public static T Ins
        {
            get
            {
                if (_instance == null)
                {
                    var cfgGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
                    switch (cfgGuids.Length)
                    {
                        case 0:
                            _instance = CreateInstance<T>();
                            AssetDatabase.CreateAsset(_instance, $"Assets/Editor/Sanmon/{_instance.GetType().Name}.asset");
                            break;
                        case 1:
                            _instance = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(cfgGuids[0]));
                            break;
                        default:
                            throw new DevConfigException("more than one instance of " + typeof(T).Name +
                                                         $"at {string.Join(", ", cfgGuids.Select(AssetDatabase.GUIDFromAssetPath))}");
                    }
                }
                
                return _instance;
            }
        }
        
        private static T _instance;
    }
}