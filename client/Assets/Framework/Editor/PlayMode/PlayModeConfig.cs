using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    public class PlayModeConfig : EditorDevConfig<PlayModeConfig>
    {
        public bool enable = true;
    
        [Header("启动场景")] public SceneAsset startScene;
    
        [Header("是否恢复原场景")] public bool restorePreviousScene = true;
    }
}