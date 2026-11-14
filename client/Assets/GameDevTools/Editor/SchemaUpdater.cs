using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameConsole.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GameConsole.Editor
{
    /// <summary> 自动追加风格, 字体和语言, 保留 Schema 中的手动名称和排列, 构建前再次同步. </summary>
    [InitializeOnLoad]
    public sealed class SchemaUpdater : AssetPostprocessor, IPreprocessBuildWithReport
    {
        private const string _resourceDirectory = "Assets/GameScripts/GameDevTools/GameUI/Resources"; // 风格目录的唯一位置.
        private const string _catalogPath = _resourceDirectory + "/Schema.asset"; // 所有风格共用的可编辑映射.
        private const string _fontDirectory = _resourceDirectory + "/Font"; // 所有风格共用的字体资源目录.
        private const string _styleFile = "GameDevToolStyles.uss"; // 风格仅提供外观, 模板和面板位于 Shared.
        private static bool _queued; // 导入批次只合并一次索引更新.

        /// <summary> 在普通构建预处理阶段同步目录. </summary>
        public int callbackOrder => 0;

        static SchemaUpdater() => QueueRefresh();

        /// <summary> Player 构建使用已生成的目录索引, 不依赖打包后的磁盘目录. </summary>
        public void OnPreprocessBuild(BuildReport report) => Refresh();

        // 新增, 删除或改名风格目录后自动刷新, 忽略索引自身以免循环导入.
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Concat(deleted).Concat(moved).Concat(movedFrom).Any(path => path.StartsWith(_resourceDirectory + "/", StringComparison.Ordinal) && path != _catalogPath && path != _catalogPath + ".meta"))
                QueueRefresh();
        }

        // 等待导入和编译完成, 不在资源导入回调里写入索引.
        private static void QueueRefresh()
        {
            if (_queued) return;
            _queued = true;
            EditorApplication.delayCall += RefreshWhenReady;
        }

        // Unity 的导入事务结束后再检查是否有实际变化.
        private static void RefreshWhenReady()
        {
            _queued = false;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { QueueRefresh(); return; }
            Refresh();
        }

        // 仅追加尚未登记的目录, 不覆盖显示名称, 不删除手动配置的映射或改变其顺序.
        private static void Refresh()
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", _resourceDirectory));
            if (!Directory.Exists(root)) return;
            var styleRoot = Path.Combine(root, "Style");
            var paths = Directory.Exists(styleRoot) ? Directory.EnumerateDirectories(styleRoot).Where(directory => File.Exists(Path.Combine(directory, _styleFile))).Select(directory => "Style/" + Path.GetFileName(directory)).OrderBy(path => path, StringComparer.Ordinal).ToArray() : Array.Empty<string>();
            var catalog = AssetDatabase.LoadAssetAtPath<Schema>(_catalogPath);
            var entries = (catalog?.styles ?? Array.Empty<ToolkitStyleEntry>()).ToList();
            var fonts = (catalog?.fonts ?? Array.Empty<ToolkitFontEntry>()).ToList();
            var languages = (catalog?.languages ?? Array.Empty<ToolkitLanguageEntry>()).ToList();
            var registered = new HashSet<string>(entries.Where(entry => entry != null).Select(entry => PathKey(entry.path)), StringComparer.Ordinal);
            var changed = false;
            foreach (var path in paths)
                if (registered.Add(path))
                {
                    entries.Add(new ToolkitStyleEntry { name = Path.GetFileName(path), path = path });
                    changed = true;
                }
            var fontPaths = AssetDatabase.IsValidFolder(_fontDirectory) ? AssetDatabase.FindAssets("t:Font", new[] { _fontDirectory }).Select(AssetDatabase.GUIDToAssetPath).Select(path => Path.ChangeExtension(path.Substring(_resourceDirectory.Length + 1), null).Replace('\\', '/')).OrderBy(path => path, StringComparer.Ordinal).ToArray() : Array.Empty<string>();
            var registeredFonts = new HashSet<string>(fonts.Where(entry => entry != null).Select(entry => PathKey(entry.path)), StringComparer.Ordinal);
            foreach (var path in fontPaths)
                if (registeredFonts.Add(path))
                {
                    fonts.Add(new ToolkitFontEntry { name = Path.GetFileName(path), path = path });
                    changed = true;
                }
            var languageRoot = Directory.EnumerateDirectories(root).FirstOrDefault(directory => string.Equals(Path.GetFileName(directory), "localization", StringComparison.OrdinalIgnoreCase));
            if (languageRoot != null)
                foreach (var directory in Directory.EnumerateDirectories(languageRoot).OrderBy(path => path, StringComparer.Ordinal))
                {
                    var code = Path.GetFileName(directory);
                    if (!new[] { "ui", "help", "macro" }.All(module => File.Exists(Path.Combine(directory, module + ".json"))) || languages.Any(entry => entry != null && entry.code == code)) continue;
                    languages.Add(new ToolkitLanguageEntry { code = code, name = code, path = Path.GetFileName(languageRoot) + "/" + code });
                    changed = true;
                }
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<Schema>();
                catalog.styles = entries.ToArray();
                catalog.fonts = fonts.ToArray();
                catalog.languages = languages.ToArray();
                AssetDatabase.CreateAsset(catalog, _catalogPath);
            }
            else if (changed)
            {
                catalog.styles = entries.ToArray();
                catalog.fonts = fonts.ToArray();
                catalog.languages = languages.ToArray();
                EditorUtility.SetDirty(catalog);
            }
            AssetDatabase.SaveAssetIfDirty(catalog);
        }

        // 与运行时使用相同的路径分隔符约定, 保留资源中作者填写的原始值.
        private static string PathKey(string path) => (path ?? string.Empty).Trim().Replace('\\', '/').Trim('/');
    }
}
