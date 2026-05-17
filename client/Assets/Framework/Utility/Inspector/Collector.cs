using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Sanmon.Utility.Inspector
{
    public class Collector
    {
        public readonly string rootName;
        public readonly string tag;
        public readonly int depth;

        private readonly Adapter mNodeAdapter = new NodeAdapter();
        private readonly Adapter mLeafAdapter = new ListAdapter();
        private readonly Dictionary<Type, Adapter> mAdaptersCache = new ();
        private readonly List<Adapter> mAdapters;

        public Collector(string rootName = "", string tag = "root", int depth = 5)
        {
            this.rootName = rootName;
            this.tag = tag;
            this.depth = depth;

            mAdapters = new List<Adapter>()
            {
                new IntAdapter(),
                new FloatAdapter(),
                new BoolAdapter(),
                new StringAdapter(),
                new ListAdapter(),
                new DictionaryAdapter(),
                new EntityAdapter(),
            };
        }

        public void AddAdapter(Adapter adapter) => mAdapters.Add(adapter);
        
        public NodeBase Collect<T>(IEnumerable<T> objects)
        {
            var root = new RootNode() {name = rootName, tag = tag, depth = 0, adapter = mNodeAdapter};
            var index = 1;
            foreach (var obj in objects)
            {
                var type = obj.GetType();
                var attri = type.GetCustomAttribute<InspectableAttribute>();
                var nTag = attri?.Tag ?? type.Name;
                var node = new RootNode()
                {
                    name = $"{index++.ToString()}. ",
                    tag = nTag, 
                    depth = 1,
                    adapter = mNodeAdapter,
                    instance = obj,
                    parent = root,
                };
                
                root.children.Add(node);
                SearchNode(node, obj, 3);
            }
            
            return root;
        }

        private void SearchNode(NodeBase root, object obj, int childDepth)
        {
            root.adapter.Search(root, childDepth);
            
            foreach (var node in root.children)
            {
                var type = node switch
                {
                    RootNode rNode => rNode.instance.GetType(),
                    FieldNode fieldNode => fieldNode.info.FieldType,
                    ListElementLeaf listNode => listNode.GetValue().GetType(),
                    DictionaryElementLeaf dicNode => dicNode.GetValue().GetType(),
                    _ => null,
                };

                if (type is null)
                {
                    throw new Exception($"node [{node}] is unmatched.");
                };
                
                var adapter = RequireAdapter(type);

                if (adapter != null)
                {
                    node.adapter = adapter;
                }
                else
                {
                    
                    var attri = type.GetCustomAttribute<InspectableAttribute>();

                    if (attri == null)
                    {
                        node.adapter = RequireAdapter(type);
                        node.tag = type.Name;
                    }
                    else
                    {
                        node.adapter = mNodeAdapter;
                        node.tag = attri.Tag ?? type.Name;
                    }
                }

                if (node.adapter != null)
                {
                    switch (node)
                    {
                        case RootNode rNode: SearchNode(node, rNode.instance, childDepth + 1);
                            break;
                        case FieldNode fNode: SearchNode(node, fNode.info.GetValue(obj), childDepth + 1);
                            break;
                    }
                }
            }
        }

        private Adapter RequireAdapter(Type type)
        {
            if (mAdaptersCache.TryGetValue(type, out var adapter)) return adapter;

            foreach (var a in mAdapters)
            {
                if (a.CanHandle(type))
                {
                    mAdaptersCache.Add(type, a);
                    return a;
                }
            }

            return mLeafAdapter;
        }
    }
}