using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    public class GitPanel : PanelBase
    {
        public override int Priority => PanelDefine.GIT;
        
        public override string PanelName => "[编辑器] git";

        public override void Init()
        {
            
        }

        public override void DrawPanel(Rect windowRect)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Color.green;

                if (GUILayout.Button("打开SourceTree", EditorStyles.miniButton, GUILayout.Width(150)))
                {
                    Process.Start("C:\\Users\\Ein\\AppData\\Local\\SourceTree\\SourceTree.exe");
                }

                GUI.backgroundColor = Color.white;
            }
        }
    }
}
