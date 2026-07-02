using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    public class PanelWindow : EditorWindow
    {
        private PanelBase[] _panels;
        private Vector2 _scrollBtn;

        [MenuItem("Tools/Panel Window #Z", false, 1)]
        private static void OpenSelf()
        {
            var w = GetWindow<PanelWindow>("ConfigPanelWindow", true, WindowDefine.DOCKED_WINDOW_TYPES);
            w.maxSize = new Vector2(900, 900);
            w.minSize = new Vector2(630, 450);
        }

        private void OnEnable()
        {
            var panels = new List<PanelBase>();
            var ts = typeof(PanelBase).Assembly
                .GetTypes()
                .Where(t => typeof(PanelBase).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var t in ts)
            {
                if (Activator.CreateInstance(t) is not PanelBase panel) continue;
                panel.Init();
                panels.Add(panel);
            }

            _panels = new PanelBase[panels.Count];
            _panels = panels.OrderBy(p => p.Priority).ToArray();
        }

        private void OnGUI()
        {
            _scrollBtn = GUILayout.BeginScrollView(_scrollBtn, GUILayout.Width(position.width), GUILayout.Height(position.height));
            
            var titleFont = new GUIStyle {fontSize = 15, normal = new GUIStyleState{textColor = Color.cyan}};
            var curWinRect = position;

            foreach (var p in _panels)
            {
                using (new GUILayout.VerticalScope("HelpBox"))
                {
                    GUILayout.Space(5);
                    GUILayout.Label(p.PanelName, titleFont);
                    p.DrawPanel(curWinRect);
                    GUILayout.Space(5);
                }
                GUILayout.Space(10);
            }
            
            GUILayout.EndScrollView();
        }
    }
}