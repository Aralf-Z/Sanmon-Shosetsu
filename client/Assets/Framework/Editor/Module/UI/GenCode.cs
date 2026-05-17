using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sanmon.Module;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    internal static class GenCode
    {
        public static UICodeGenerateConfig Config => UICodeGenerateConfig.Ins;
        
        [MenuItem("Assets/CodeGen/Create Code &w", false, 1)]
        private static void CreateCode()
        {
            var time = DateTime.Now;
            foreach (var o in Selection.objects)
            {
                if (o is not GameObject go) continue;
                var contents = CollectBind(go);
                var info = CollectInfo(go, contents);
                WriteCode(info);
            }
            
            Debug.Log($"cost {(DateTime.Now - time).TotalSeconds}s");
        }
        
        [MenuItem("Assets/CodeGen/Collect Field &x", false, 2)]
        private static void CollectField()
        {
            var time = DateTime.Now;
            foreach (var o in Selection.objects)
            {
                if (o is not GameObject go) continue;
                var contents = CollectBind(go);
                CollectField(go, contents);
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"cost {(DateTime.Now - time).TotalSeconds}s");
        }

        private static List<UIElementContent> CollectBind(GameObject go)
        {
            var uiElements = go.GetComponentsInChildren<UIElement>(true);
            
            if (uiElements.Length == 0)
            {
                throw new CodeGenException($"There is no ui element at {go.name}.");
            }
            var fieldBinds = go.GetComponentsInChildren<FieldBind>(true);
            var contents = uiElements.ToDictionary(ele => ele, ele => new UIElementContent(ele));
            
            foreach (var e in uiElements)
            {
                e.FindParent();
                if (contents.TryGetValue(e.genParent, out var c))
                {
                    c.elements.Add(e);
                }
            }
            
            foreach (var f in fieldBinds)
            {
                f.FindParent();
                contents[f.genParent].fieldBinds.Add(f);
            }

            return contents.Values.ToList();
        }

        private static ElementInfo CollectInfo(GameObject root, List<UIElementContent> contents)
        {
            var elementInfos = contents.ToDictionary(x => x.root, x => new ElementInfo()
            {
                typeName = x.root.GetType().FullName,
                className = x.root.GetType().Name,
                fieldName = x.root.genElementName,
            });
            var rootContent = contents.FirstOrDefault(x => x.root.gameObject == root);
            var rootInfo = new ElementInfo()
            {
                typeName = rootContent!.root.GetType().FullName,
                className = rootContent.root.GetType().Name,
                fieldName = rootContent.root.genElementName,
                elements = CollectElementsInfo(contents, elementInfos),
                fields = CollectFieldInfo(rootContent.fieldBinds),
            };
            
            foreach (var (element, info) in elementInfos)
            {
                if(element == rootContent.root) continue;

                info.elements = CollectElementsInfo(contents, elementInfos);
                info.fields = CollectFieldInfo(rootContent.fieldBinds);
            }

            return rootInfo;
            
            List<FieldInfo> CollectFieldInfo(List<FieldBind> fields)
            {
                var infos = new List<FieldInfo>();

                foreach (var f in fields)
                {
                    if(string.IsNullOrEmpty(f.genName))
                    {
                        throw new UnityException($"bind '{f.name}' field name is null or empty");
                    }
                    infos.Add(new FieldInfo()
                    {
                        name = f.genName,
                        type = f.genComponentType,
                    });
                }
                
                return infos;
            }
            
            List<ElementInfo> CollectElementsInfo(List<UIElementContent> elements, Dictionary<UIElement, ElementInfo> map) 
                => elements.Select(e => map[e.root]).ToList();
        }

        private static void WriteCode(ElementInfo info)
        {
            if (!Directory.Exists(Config.GenCodePath))
            {
                Directory.CreateDirectory(Config.GenCodePath);
            }

            foreach (var elementInfo in info.AllElement())
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine( "namespace " + Config.namespaceName);
                sb.AppendLine( "{");
                sb.AppendLine($"   public partial class {elementInfo.className}");
                sb.AppendLine( "   {");
                foreach (var subGroup in elementInfo.elements)
                {
                    sb.AppendLine($"      [UnityEngine.SerializeField] private {subGroup.typeName} {subGroup.fieldName};");
                }
                foreach (var bind in elementInfo.fields)
                {
                    sb.AppendLine($"      [UnityEngine.SerializeField] private {bind.type} {bind.name};");
                }
                sb.AppendLine( "   }");
                sb.AppendLine( "}");
                
                File.WriteAllText(Path.Combine(Config.GenCodePath, $"{elementInfo.className}.setup.cs"), sb.ToString());
            }
            
            AssetDatabase.Refresh();
        }

        private static void CollectField(GameObject root, List<UIElementContent> contents)
        {
            foreach (var ctx in contents)
            {
                var so = new SerializedObject(ctx.root);

                foreach (var e in ctx.elements)
                {
                    var field = so.FindProperty(e.genElementName);
                    field.objectReferenceValue = e.gameObject;
                }

                foreach (var f in ctx.fieldBinds)
                {
                    var field = so.FindProperty(f.genName);
                    field.objectReferenceValue = f.genParent;
                }

                so.ApplyModifiedProperties();
                so.UpdateIfRequiredOrScript();
            }

            EditorUtility.SetDirty(root);
        }
    }
}