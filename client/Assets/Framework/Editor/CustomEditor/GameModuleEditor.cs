using System;
using System.Collections.Generic;
using Sanmon.Core;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    [CustomEditor(typeof(GameModule))]
    public class GameModuleEditor: UnityEditor.Editor
    {
        private GameModule _self;
        private List<EditorInfo> _mdEditors = new ();
        
        private void OnEnable()
        {
            _self = (GameModule)target;
            
            foreach (var mono in _self.GetComponentsInChildren<MonoBehaviour>())
            {
                if (mono.GetType() == typeof(GameModule)) continue;
                _mdEditors.Add(new EditorInfo()
                {
                    name = mono.name,
                    editor = CreateEditor(mono),
                });
            }
        }

        private void OnDisable()
        {
            foreach (var mono in _mdEditors)
            {
                DestroyImmediate(mono.editor);
            }
            _mdEditors.Clear();
        }

        public override void OnInspectorGUI()
        {
            // GUILayout.Label("Base");
            // base.OnInspectorGUI();

            foreach (var mono in _mdEditors)
            {
                GUILayout.Label(mono.name);
                mono.editor.OnInspectorGUI();
            }
        }
        
        private class EditorInfo
        {
            public string name;
            public UnityEditor.Editor editor;
        }
    }
}