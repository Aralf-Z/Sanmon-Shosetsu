using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace Sanmon.Editor
{
    [InitializeOnLoad]
    public static class PlayModeModifier
    {
        static PlayModeModifier()
        {
            // PlayModeStateChanged 回调
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            var config = PlayModeConfig.instance;
            if (config == null || !config.isActive) return;

            // 编辑器将进入 Play
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (config == null || config.entryScene == null)
                    return;

                string entryScenePath = AssetDatabase.GetAssetPath(config.entryScene);
                if (string.IsNullOrEmpty(entryScenePath))
                    return;

                // 如果当前编辑的场景未保存，提示保存
                if (EditorSceneManager.GetActiveScene().isDirty)
                {
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        Debug.Log("取消进入播放模式（因为当前场景未保存）");
                        EditorApplication.isPlaying = false;
                        return;
                    }
                }

                // 切换到入口场景
                EditorSceneManager.OpenScene(entryScenePath);
                Debug.Log($"进入 PlayMode, 自动切换到入口场景: {config.entryScene.name}");
            }
        }
    }

    //
    // [InitializeOnLoad]
    // public static class PlayModeSceneBootstrap
    // {
    //     private const string ConfigPath = "Assets/Map/Temp/PlayModeSceneConfig.asset";
    //     private const string SceneSetupKey = "PlayMode_PrevSceneSetup";
    //
    //     private static PlayModeSceneConfig _config;
    //
    //     static PlayModeSceneBootstrap()
    //     {
    //         LoadConfig();
    //         EditorApplication.playModeStateChanged += OnPlayModeChanged;
    //     }
    //
    //     private static void LoadConfig()
    //     {
    //         _config = AssetDatabase.LoadAssetAtPath<PlayModeSceneConfig>(ConfigPath);
    //     }
    //
    //     private static void OnPlayModeChanged(PlayModeStateChange state)
    //     {
    //         if (_config == null || !_config.enable)
    //             return;
    //
    //         switch (state)
    //         {
    //             //  进入 Play 前
    //             case PlayModeStateChange.ExitingEditMode:
    //                 EnterPlayMode();
    //                 break;
    //
    //             //  退出 Play 后
    //             case PlayModeStateChange.EnteredEditMode:
    //                 ExitPlayMode();
    //                 break;
    //         }
    //     }
    //
    //     private static void EnterPlayMode()
    //     {
    //         if (_config.startScene == null)
    //             return;
    //
    //         // 记录当前 Scene Setup（支持多场景）
    //         var setup = EditorSceneManager.GetSceneManagerSetup();
    //         string json = JsonUtility.ToJson(new SceneSetupWrapper(setup));
    //         EditorPrefs.SetString(SceneSetupKey, json);
    //
    //         if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
    //         {
    //             EditorApplication.isPlaying = false;
    //             return;
    //         }
    //
    //         string path = AssetDatabase.GetAssetPath(_config.startScene);
    //         EditorSceneManager.OpenScene(path);
    //     }
    //
    //     private static void ExitPlayMode()
    //     {
    //         if (!_config.restorePreviousScene)
    //             return;
    //
    //         string json = EditorPrefs.GetString(SceneSetupKey, "");
    //         if (string.IsNullOrEmpty(json))
    //             return;
    //
    //         var wrapper = JsonUtility.FromJson<SceneSetupWrapper>(json);
    //         if (wrapper != null && wrapper.sceneSetups != null)
    //         {
    //             EditorSceneManager.RestoreSceneManagerSetup(wrapper.sceneSetups);
    //         }
    //     }
    //
    //     // Unity 的 SceneSetup 不能直接序列化，需要包一层
    //     [System.Serializable]
    //     private class SceneSetupWrapper
    //     {
    //         public SceneSetup[] sceneSetups;
    //
    //         public SceneSetupWrapper(SceneSetup[] setups)
    //         {
    //             sceneSetups = setups;
    //         }
    //     }
    // }
    //
    //
    // public static class PlayModeSceneMenu
    // {
    //     private const string ConfigPath = "Assets/Editor/PlayModeSceneConfig.asset";
    //
    //     [MenuItem("Tools/PlayMode/Select Config")]
    //     public static void SelectConfig()
    //     {
    //         var config = AssetDatabase.LoadAssetAtPath<PlayModeSceneConfig>(ConfigPath);
    //         Selection.activeObject = config;
    //     }
    //
    //     [MenuItem("Tools/PlayMode/Use Current Scene As Start Scene")]
    //     public static void UseCurrentScene()
    //     {
    //         var config = AssetDatabase.LoadAssetAtPath<PlayModeSceneConfig>(ConfigPath);
    //
    //         var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
    //         var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
    //
    //         config.startScene = sceneAsset;
    //         EditorUtility.SetDirty(config);
    //
    //         Debug.Log("Start scene set to: " + scene.path);
    //     }
    //
    //     [MenuItem("Tools/PlayMode/Toggle Enable")]
    //     public static void ToggleEnable()
    //     {
    //         var config = AssetDatabase.LoadAssetAtPath<PlayModeSceneConfig>(ConfigPath);
    //         config.enable = !config.enable;
    //
    //         EditorUtility.SetDirty(config);
    //         Debug.Log("PlayMode Scene Redirect: " + (config.enable ? "ON" : "OFF"));
    //     }
    // }
    //
    // [CreateAssetMenu(menuName = "Editor/PlayMode Scene Config")]
    // public class PlayModeSceneConfig : ScriptableObject
    // {
    //     public bool enable = true;
    //
    //     [Header("启动场景")] public SceneAsset startScene;
    //
    //     [Header("是否恢复原场景")] public bool restorePreviousScene = true;
    // }
    //
    // public static class PlayModeSceneConfigCreator
    // {
    //     private const string Path = "Assets/Map/Temp/PlayModeSceneConfig.asset";
    //
    //     [InitializeOnLoadMethod]
    //     private static void CreateIfNotExists()
    //     {
    //         var config = AssetDatabase.LoadAssetAtPath<PlayModeSceneConfig>(Path);
    //         if (config == null)
    //         {
    //             var instance = ScriptableObject.CreateInstance<PlayModeSceneConfig>();
    //
    //             AssetDatabase.CreateAsset(instance, Path);
    //             AssetDatabase.SaveAssets();
    //
    //             Debug.Log("PlayModeSceneConfig created at: " + Path);
    //         }
    //     }
    // }
}