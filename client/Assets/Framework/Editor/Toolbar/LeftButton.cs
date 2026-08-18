using UnityEditor;
using UnityToolbarExtender;

namespace Sanmon.Editor
{
    [InitializeOnLoad]
    public class LeftButton
    {
        static LeftButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(LeftButtonPlayMode.OnToolbarGUI);
        }
    }
}