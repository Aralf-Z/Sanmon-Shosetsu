using UnityEngine;

namespace Sanmon.Editor
{
    public static class ToolbarStyles
    {
        public static readonly GUIStyle TAB_BUTTON = new GUIStyle("Tab middle")
        {
            padding = new RectOffset(2, 8, 2, 2),
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
    }
}