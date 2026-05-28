using System;
using System.IO;
using Sanmon.Helper;
using Sanmon.Module;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sanmon.Editor
{
    public class AssetBuildPanel : PanelBase
    {
        public override int Priority => PanelDefine.ASSET;
        public override string PanelName => "[编辑器] 资源加载设置";
        
        public override void Init()
        {
            
        }

        public override void DrawPanel(Rect windowRect)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Color.green;
                
                if (GUILayout.Button("构建Resources资源目录", EditorStyles.miniButton, GUILayout.Width(150)))
                {
                    ConfigBuild();
                }

                GUI.backgroundColor = Color.white;
            }
        }
        
        private static void ConfigBuild()
        {
            try
            {
                var guids = AssetDatabase.FindAssets("", new string[] {"Assets/Resources"});
                var resConfig = new AssetMap();
                int prePathLength = "Assets/Resources/".Length;
                for (int i = 0; i < guids.Length; ++i)
                {
                    var originalPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var resPath = originalPath.Substring(prePathLength);
                    var path = resPath.Split('.');
                    if (path.Length == 1)
                    {
                        continue; //文件夹名跳过
                    }

                    var name = AssetDatabase.LoadAssetAtPath<Object>(originalPath).name;
                    Helper.Logger.LogInfo($"资源录入 - '{name}'.","Asset");
                    EditorUtility.DisplayProgressBar("ResourcesConfigBuilding", originalPath, (float) i / guids.Length);
                    resConfig.Add(name, path[0]);
                }
                
                CreateAssetConfig(resConfig);
                Helper.Logger.LogInfo("资源路径配置完成","Asset");
            }
            catch (Exception e)
            {
                Helper.Logger.LogInfo($"资源路径配置失败:{e.Message}","Asset");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void CreateAssetConfig(AssetMap data)
        {
            var dataPath = Path.Combine(Application.streamingAssetsPath, AssetModule.MAP_FILE_NAME);
            
            if (!File.Exists(dataPath))
            {
                File.Create(dataPath);
                AssetDatabase.Refresh();
            }

            var content = JsonHelper.SerializeObject(data);
            File.WriteAllText(dataPath, content);
            AssetDatabase.Refresh();
        }
    }
}