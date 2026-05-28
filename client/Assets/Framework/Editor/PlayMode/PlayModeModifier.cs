using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace Sanmon.Editor
{
    [InitializeOnLoad]
    public static class PlayModeSceneBootstrap
    {
        private const string kSceneSetupKey = "SANMON_SCENE_SETUP";
        
        static PlayModeSceneBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
    
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (PlayModeConfig.Ins == null || !PlayModeConfig.Ins.enable)
                return;
    
            switch (state)
            {
                //  进入 Play 前
                case PlayModeStateChange.ExitingEditMode:
                    EnterPlayMode();
                    break;
    
                //  退出 Play 后
                case PlayModeStateChange.EnteredEditMode:
                    ExitPlayMode();
                    break;
            }
        }
    
        private static void EnterPlayMode()
        {
            if (PlayModeConfig.Ins.startScene == null)
                return;
    
            // 记录当前 Scene Setup（支持多场景）
            var setup = EditorSceneManager.GetSceneManagerSetup();
            string json = JsonUtility.ToJson(new SceneSetupWrapper(setup));
            EditorPrefs.SetString(kSceneSetupKey, json);
    
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.isPlaying = false;
                return;
            }
    
            string path = AssetDatabase.GetAssetPath(PlayModeConfig.Ins.startScene);
            EditorSceneManager.OpenScene(path);
        }
    
        private static void ExitPlayMode()
        {
            if (!PlayModeConfig.Ins.restorePreviousScene)
                return;
    
            string json = EditorPrefs.GetString(kSceneSetupKey, "");
            if (string.IsNullOrEmpty(json))
                return;
    
            var wrapper = JsonUtility.FromJson<SceneSetupWrapper>(json);
            if (wrapper != null && wrapper.sceneSetups != null)
            {
                EditorSceneManager.RestoreSceneManagerSetup(wrapper.sceneSetups);
            }
        }
    
        // Unity 的 SceneSetup 不能直接序列化，需要包一层
        [System.Serializable]
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