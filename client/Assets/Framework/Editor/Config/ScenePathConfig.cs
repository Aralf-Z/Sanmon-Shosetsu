using System;
using System.Collections.Generic;
using UnityEditor;

namespace Sanmon.Editor
{
    [Serializable]
    public class ScenePath
    {
        public DefaultAsset path;
    }
    
    public class ScenePathConfig: EditorDevConfig<ScenePathConfig>
    {
        public List<ScenePath> paths = new List<ScenePath>();
    }
}