using Sanmon.Utility.Monitor;
using UnityEditor.IMGUI.Controls;

namespace Sanmon.Editor
{
    public class Monitor : TreeView
    {
        public NodeBase Root { get; }
        public TreeViewItem ViewRoot { get; private set; }
        
        public Monitor(TreeViewState state, NodeBase inspectorRoot) : base(state)
        {
            Root = inspectorRoot;
            Reload();
        }
    
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(0, -1, "MyNote");
        
            ViewRoot = new TreeViewItem (Root.Id, Root.depth, Root.adapter.Content(Root));
        
            root.AddChild(ViewRoot);
            AddChildren(Root, ViewRoot);
            SetupDepthsFromParentsAndChildren(root);
        
            return root;
        }

        private void AddChildren(NodeBase root, TreeViewItem tvtRoot)
        {
            foreach (var child in root.children)
            {
                var tvt = new TreeViewItem(child.Id, child.depth, child.adapter.Content(child));
                tvtRoot.AddChild(tvt);
                AddChildren(child, tvt);
            }
        }
    }

}