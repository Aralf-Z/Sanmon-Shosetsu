using UnityEditor;
using UnityEngine;
using YooAsset;

namespace Sanmon.Editor
{
    public static class RightButtonAssetMode
    {
        private const string KEY = "EditorAssetMode";

        private static string[] _mode = new[]
        {
            "编辑器模拟模式",
            "离线运行模式",
            "Web模式"
        };

        private static int _modeIndex = 0;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            switch ((EPlayMode)EditorPrefs.GetInt(KEY, (int)EPlayMode.EditorSimulateMode))
            {
                case EPlayMode.OfflinePlayMode:
                    _modeIndex = 1;
                    break;
                case EPlayMode.WebPlayMode:
                    _modeIndex = 2;
                    break;
                case EPlayMode.None:
                case EPlayMode.EditorSimulateMode:
                case EPlayMode.HostPlayMode:
                case EPlayMode.CustomPlayMode:
                default:
                    _modeIndex = 0;
                    EditorPrefs.SetInt(KEY, (int)EPlayMode.EditorSimulateMode);
                    break;
            }

            ;
        }

        public static void OnToolbarGUI()
        {
            GUI.enabled = !(EditorApplication.isCompiling || EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode);
            
            var rect = EditorGUILayout.GetControlRect();
            rect.width = 200;
            
            EditorGUI.BeginChangeCheck();

            _modeIndex = EditorGUILayout.Popup(_modeIndex, _mode, EditorStyles.toolbarPopup);

            if (EditorGUI.EndChangeCheck())
            {
                switch (_modeIndex)
                {
                    case 0:
                        EditorPrefs.SetInt(KEY, (int)EPlayMode.EditorSimulateMode); break;
                    case 1:
                        EditorPrefs.SetInt(KEY, (int)EPlayMode.OfflinePlayMode); break;
                    case 2:
                        EditorPrefs.SetInt(KEY, (int)EPlayMode.WebPlayMode); break;
                }
            }

            GUI.enabled = true;
        }
    }
}