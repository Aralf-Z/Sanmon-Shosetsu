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
        private GameNote mNote;

        private NodeBase mRootNode;
        private Collector mCollector;
        
        private TreeViewState mViewState;
        private InspectorTreeView mView;
        
        private void OnEnable()
        {
            mNote = (GameNote)target;
            mCollector = new Collector("notes");

            Refresh();
        }
        
        private void Refresh()
        {
            mRootNode = mCollector.Collect(mNote.Notes);
            mViewState = new TreeViewState();
            mView = new InspectorTreeView(mViewState, mRootNode);
            
            mView.ExpandAll();
            
            // foreach (var VARIABLE in mView.ViewRoot.children)
            // {
            //     
            // }
        }

        public override void OnInspectorGUI()
        {
            var rect = GUILayoutUtility.GetRect(0, 1000, 0, 900);

            mView.OnGUI(rect);

            if (GUILayout.Button("Refresh"))
            {
                Refresh();
            }
        }
    }
}