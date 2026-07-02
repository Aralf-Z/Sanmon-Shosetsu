using System.Collections.Generic;
using Sanmon.Core;
using Sanmon.Utility.Inspector;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sanmon.Editor
{
    [UnityEditor.CustomEditor(typeof(GameNote))]
    public class GameNoteEditor : UnityEditor.Editor
    , IGetNote
    {
        private GameNote _note;

        private NodeBase _rootNode;
        private Collector _collector;
        
        private TreeViewState _viewState;
        private InspectorTreeView _view;
        
        private void OnEnable()
        {
            _note = (GameNote)target;
            _collector = new Collector("notes");

            Refresh();
        }
        
        private void Refresh()
        {
            _rootNode = _collector.Collect(_note.Notes);
            _viewState = new TreeViewState();
            _view = new InspectorTreeView(_viewState, _rootNode);
            
            _view.ExpandAll();
            
            // foreach (var VARIABLE in _view.ViewRoot.children)
            // {
            //     
            // }
        }

        public override void OnInspectorGUI()
        {
            var rect = GUILayoutUtility.GetRect(0, 1000, 0, 900);

            _view.OnGUI(rect);

            if (GUILayout.Button("Refresh"))
            {
                Refresh();
            }
        }
    }
}