using UnityEditor;
using UnityToolbarExtender;

namespace Sanmon.Editor
{
    [InitializeOnLoad]
    public class RightButton
    {
        static RightButton()
        {
            ToolbarExtender.RightToolbarGUI.Add(RightButtonSceneQuickSwitch.OnToolbarGUI);
        }
    }
}