using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Editor
{
    [InitializeOnLoad]
    public static class RightButtonSceneQuickSwitch
    {
        private static (string name, string path)[] _scene;
        
        static RightButtonSceneQuickSwitch()
        {
            AssetsWatcher.AddCheckExtension(UpdateScenes,".unity");
            UpdateScenes(null,null,null,null);
        }
        
        public static void OnToolbarGUI()
        {
            var rect = EditorGUILayout.GetControlRect();
            rect.width = 100;

            if (EditorGUI.DropdownButton(rect, new GUIContent(SceneManager.GetActiveScene().name, "场景切换"), FocusType.Keyboard, EditorStyles.popup))
            {
                var menu = new GenericMenu();

                foreach (var scene in _scene)
                {
                    menu.AddItem(new GUIContent(scene.name), false, () => SwitchScene(scene.path));
                }

                menu.DropDown(rect);
            }
        }

        private static void SwitchScene(string path)
        {
            if (SceneManager.GetActiveScene().isDirty)
            {
                if (EditorUtility.DisplayDialog("是否保存当前场景", "当前场景有未保存的更改. 你是否想保存?", "保存", "取消"))
                { 
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); 
                    EditorSceneManager.OpenScene(path);
                }
            }
            else
            {
                EditorSceneManager.OpenScene(path);
            }
        }

        [InitializeOnLoadMethod]
        public static void UpdateScenes()
        {
            UpdateScenes(null,null,null,null);
        }
       
        private static void UpdateScenes(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var folders = ScenePathConfig.Ins.paths
                .Select(x => AssetDatabase.GetAssetPath(x.path))
                .Where(AssetDatabase.IsValidFolder)
                .Distinct()
                .ToArray();

            var guids = AssetDatabase.FindAssets("t:Scene", folders);
            _scene = new (string name, string path)[guids.Length];

            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                _scene[i] = (Path.GetFileNameWithoutExtension(path), path);
            }
            
            for (var i = 0; i < _scene.Length - 1; i++)
            {
                for (var j = 0; j < _scene.Length - 1 - i; j++)
                {
                    if (string.Compare(_scene[j].name, _scene[j + 1].name, StringComparison.Ordinal) > 0)
                    {
                        (_scene[j], _scene[j + 1]) = (_scene[j + 1], _scene[j]);
                    }
                }
            }
            
            // Logger.LogInfo($"update scenes '{string.Join(", ", _scene)}'.", "Editor");
        }
    }
}