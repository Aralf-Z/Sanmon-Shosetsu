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
                if (sInstance == null)
                {
                    var cfgGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
                    switch (cfgGuids.Length)
                    {
                        case 0:
                            sInstance = CreateInstance<T>();
                            AssetDatabase.CreateAsset(sInstance, $"Assets/Editor/Sanmon/{sInstance.GetType().Name}.asset");
                            break;
                        case 1:
                            sInstance = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(cfgGuids[0]));
                            break;
                        default:
                            throw new DevConfigException("more than one instance of " + typeof(T).Name +
                                                         $"at {string.Join(", ", cfgGuids.Select(AssetDatabase.GUIDFromAssetPath))}");
                    }
                }
                
                return sInstance;
            }
        }
        
        private static T sInstance;
    }
}