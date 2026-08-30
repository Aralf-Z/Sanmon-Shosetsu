using UnityEngine;

namespace Sanmon.Editor
{
    public class ZLuaPanel: PanelBase
    {
        public override int Priority => PanelDefine.ZLua;
        public override string PanelName => "ZLua";
        
        public override void Init()
        {
            
        }

        public override void DrawPanel(Rect windowRect)
        {
            if (GUILayout.Button("同步脚本到StreamingAssets", GUILayout.Width(200)))
            {
                SyncLuaScriptsToStreamingAssets.SyncLuaScripts();
            }
        }
    }
}