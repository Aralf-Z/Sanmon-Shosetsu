using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    public class PlayModeScenePanel : PanelBase
    {
        public override int Priority => PanelDefine.PLAY_MODE;
        public override string PanelName => "[编辑器] 启动场景";
        
        public override void Init()
        {
            
        }

        public override void DrawPanel(Rect windowRect)
        {
            GUILayout.Space(5);

            GUI.enabled = false;

            GUILayout.Space(5);
            EditorGUILayout.ObjectField("配置文件", PlayModeConfig.Ins, typeof(PlayModeConfig), false);
            
            GUI.enabled = true;
            
            GUILayout.Space(5);
            PlayModeConfig.Ins.enable = GUILayout.Toggle(PlayModeConfig.Ins.enable, "启用");
            PlayModeConfig.Ins.saveOnPlay = GUILayout.Toggle(PlayModeConfig.Ins.saveOnPlay, "play时保存");
            PlayModeConfig.Ins.restorePreviousScene = GUILayout.Toggle(PlayModeConfig.Ins.restorePreviousScene, "stop时返回之前编辑场景");
            GUILayout.Space(5);
            
            var oldWidth = EditorGUIUtility.labelWidth;
            
            EditorGUIUtility.labelWidth = 50;

            var scene = PlayModeConfig.Ins.scenes;

            if (scene.Count == 0)
            {
                GUILayout.Label("暂无场景快捷启动设置");
            }
            else
            {
                for (var i = 0; i < PlayModeConfig.Ins.scenes.Count; i++)
                {
                    GUILayout.Space(10);
                    var pms = PlayModeConfig.Ins.scenes[i];
                    using (new GUILayout.HorizontalScope())
                    {
                        pms.scene = (SceneAsset)EditorGUILayout.ObjectField("场景：", pms.scene, typeof(SceneAsset), false);
                        GUILayout.Space(20);
                        pms.name = EditorGUILayout.TextField("名称：", pms.name);
                        if (GUILayout.Button("X", GUILayout.Width(25)))
                            PlayModeConfig.Ins.scenes.RemoveAt(i--);
                    }
                    pms.tips =  EditorGUILayout.TextField("备注：", pms.tips);
                }
            }
            
            GUILayout.Space(12);
            
            if (GUILayout.Button("+新场景", GUILayout.Width(80)))
            {
                var copy = PlayModeConfig.Ins.scenes.Count > 0 ? PlayModeConfig.Ins.scenes[^1] : new PlayModeScene(){name = "新场景"};
                PlayModeConfig.Ins.scenes.Add(new PlayModeScene()
                {
                    scene = copy.scene,
                    name = copy.name,
                    tips = copy.tips
                });
            }
            
            EditorGUIUtility.labelWidth = oldWidth;
            
            GUILayout.Space(5);
        }
    }
}