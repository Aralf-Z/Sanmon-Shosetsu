using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    [Serializable]
    public class PlayModeScene
    {
        public SceneAsset scene;
        public string name;
        public string tips;
    }
    
    public class PlayModeConfig : EditorDevConfig<PlayModeConfig>
    {
        public bool enable = true;
        
        public bool saveOnPlay = true;
        
        public bool restorePreviousScene = true;

        public List<PlayModeScene> scenes;
    }
}