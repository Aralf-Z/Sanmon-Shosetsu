using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    public class SceneManagerPanel: PanelBase
    {
        public override int Priority => PanelDefine.SCENE_MANAGER;
        public override string PanelName => "[编辑器] 场景管理";
        public override void Init()
        {
            
        }

        public override void DrawPanel(Rect windowRect)
        {
            GUILayout.Space(5);

            GUI.enabled = false;

            GUILayout.Space(5);
            EditorGUILayout.ObjectField("配置文件", ScenePathConfig.Ins, typeof(ScenePathConfig), false);
            
            GUI.enabled = true;
            
            GUILayout.Space(5);

            var paths = ScenePathConfig.Ins.paths;
                
            if (ScenePathConfig.Ins.paths.Count == 0)
            {
                GUILayout.Label("暂无场景路径");
            }
            else
            {
                for (var i = 0; i < paths.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        paths[i].path = (DefaultAsset)EditorGUILayout.ObjectField(paths[i].path, typeof(DefaultAsset), false);

                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            paths.RemoveAt(i--);
                            RightButtonSceneQuickSwitch.UpdateScenes();
                        }
                    }
                }
            }

            GUILayout.Space(12);
            
            if (GUILayout.Button("+新路径", GUILayout.Width(80)))
            {
                paths.Add(new ScenePath());
                RightButtonSceneQuickSwitch.UpdateScenes();
            }
            
            GUILayout.Space(5);
        }
    }
}