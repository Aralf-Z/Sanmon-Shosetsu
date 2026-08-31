using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sanmon.Editor
{
    public static class LeftButtonPlayMode
    {
        public static void OnToolbarGUI()
        {
            if(!PlayModeConfig.Ins.enable) return;
            
            GUI.enabled = !(EditorApplication.isCompiling || EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode);
            
            GUILayout.FlexibleSpace();
            
            foreach (var pms in PlayModeConfig.Ins.scenes)
            {
                var content = new GUIContent(pms.name,EditorGUIUtility.FindTexture("PlayButton"), pms.tips);
                
                if(GUILayout.Button(content, ToolbarStyles.TAB_BUTTON))
                    SceneHelper.StartScene(pms.scene);
            }
            
            GUI.enabled = true;
        }
    }

    [InitializeOnLoad]
    public static class SceneHelper
    {
        private const string SCENE_SETUP_KEY = "SANMON_SCENE_SETUP";
        
        private static SceneAsset _sceneAsset;

        static SceneHelper()
        { 
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange mode)
        {
            if (mode == PlayModeStateChange.EnteredEditMode)
                ExitPlayMode();
        }

        public static void StartScene(SceneAsset scene)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            
            _sceneAsset = scene;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (_sceneAsset == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.update -= OnUpdate;

            EnterPlayMode();
        }
        
        private static void EnterPlayMode()
        {
            // 记录当前 Scene Setup（支持多场景）
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var json = JsonUtility.ToJson(new SceneSetupWrapper(setup));
            
            EditorPrefs.SetString(SCENE_SETUP_KEY, json);
    
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.isPlaying = false;
                return;
            }
    
            var path = AssetDatabase.GetAssetPath(_sceneAsset);
            _sceneAsset = null;
            EditorSceneManager.OpenScene(path);
            EditorApplication.isPlaying = true;
        }
        
        private static void ExitPlayMode()
        {
            if (!PlayModeConfig.Ins.restorePreviousScene)
                return;
    
            var json = EditorPrefs.GetString(SCENE_SETUP_KEY, "");
            
            if (string.IsNullOrEmpty(json))
                return;
    
            var wrapper = JsonUtility.FromJson<SceneSetupWrapper>(json);
            if (wrapper is { sceneSetups: not null })
            {
                EditorSceneManager.RestoreSceneManagerSetup(wrapper.sceneSetups);
            }
            
            EditorPrefs.SetString(SCENE_SETUP_KEY, "");
        }
        
        // Unity 的 SceneSetup 不能直接序列化，需要包一层
        [Serializable]
        private class SceneSetupWrapper
        {
            public SceneSetup[] sceneSetups;
    
            public SceneSetupWrapper(SceneSetup[] setups)
            {
                sceneSetups = setups;
            }
        }
    }
}