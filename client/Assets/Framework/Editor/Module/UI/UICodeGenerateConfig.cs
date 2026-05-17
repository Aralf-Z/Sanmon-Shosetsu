using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Sanmon.Editor
{
    internal class UICodeGenerateConfig: EditorDevConfig<UICodeGenerateConfig>
    {
        public string namespaceName = "ZGame.UI";
        
        public string GenCodePath => Path.Combine(Application.dataPath, genCodePath);
        
        [SerializeField] private string genCodePath = "CodeGen/Gen";
        
        [SerializeField] public List<string> types = new ()
        {
            "UnityEngine.UI.Text",
            "UnityEngine.UI.Image",
            "UnityEngine.UI.Button",
        };
    }
}