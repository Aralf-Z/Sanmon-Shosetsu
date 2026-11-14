using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 开发工具的唯一 UI 工厂, 资源依赖由 Resources 纳入 Player. </summary>
    [Preserve]
    public static partial class ToolkitFactory
    {
        private const string _catalogPath = "Schema"; // Resources 下可编辑的名称与路径映射.
        private const string _sharedRoot = "Style/Shared/"; // 页面结构和基础面板独立于所选风格.

        /// <summary> 当前已提交的风格资源路径, 准备中的选择不会改变此值. </summary>
        public static string ActiveStyle { get; private set; } = string.Empty;
        /// <summary> 当前已提交的字体路径, 准备中的选择不会改变此值. </summary>
        public static string ActiveFont { get; private set; } = string.Empty;
        /// <summary> 当前启动时加载的名称与路径快照. </summary>
        internal static IReadOnlyList<ToolkitStyleEntry> Styles => _styles;
        /// <summary> 全部 UI 风格共用的字体配置快照. </summary>
        internal static IReadOnlyList<ToolkitFontEntry> Fonts => _fonts;

        private static ThemeStyleSheet _sharedTheme; // 全部风格共用的基础主题.
        private static PanelSettings _sharedPanel; // 创建窗口时克隆, 不被风格切换替换.
        private static StyleSheet _layoutSheet; // 原生菜单也使用同一份公共布局和透明度标记.
        private static StyleSheet _controlsSheet; // 公共状态样式在外观后覆盖, 与模板挂载顺序一致.
        private static ToolkitStyleEntry[] _styles = Array.Empty<ToolkitStyleEntry>(); // 名称独立于路径, 每次创建工具时重新加载.
        private static StyleSheet _styleSheet; // 当前已提交的共用样式.
        private static ToolkitFontEntry[] _fonts = Array.Empty<ToolkitFontEntry>(); // 字体显示名独立于保存路径.
        private static Font _font; // 当前界面与原生菜单共用, 准备阶段不替换.

        /// <summary> 配置无可用资源时报告初始化错误, 创建异常时释放临时对象. </summary>
        [Preserve]
        public static ToolkitView Create(Transform owner)
        {
            if (!Application.isPlaying || owner == null) return null;
            var catalog = Resources.Load<Schema>(_catalogPath);
            _styles = BuildStyles(catalog?.styles);
            _fonts = BuildFonts(catalog?.fonts);
            ToolkitLocalization.Initialize(catalog?.languages, Config.UILanguage);
            if (string.IsNullOrEmpty(ToolkitLocalization.ActiveLanguage)) throw new InvalidOperationException(ToolkitLocalization.Get("schema.languages.unavailable", "Schema.languages has no loadable language package."));
            LoadSharedResources(out var tree, out var theme, out var settings);
            if (!TryLoadInitialStyle(Config.UIStyle, out var selected, out var sheet)) throw new InvalidOperationException(ToolkitLocalization.Get("schema.styles.unavailable"));
            if (!LoadFont(Config.UIFont)) throw new InvalidOperationException(ToolkitLocalization.Get("schema.fonts.unavailable"));
            var languageChanged = Config.UILanguage != ToolkitLocalization.ActiveLanguage;
            Config.UILanguage = ToolkitLocalization.ActiveLanguage;
            if (languageChanged || Config.UIStyle != selected || Config.UIFont != ActiveFont)
            {
                Config.UIStyle = selected;
                Config.UIFont = ActiveFont;
                Config.Save();
            }
            ActiveStyle = selected;
            _styleSheet = sheet;
            InitializeAppearanceCache(sheet);

            GameObject window = null;
            PanelSettings instance = null;
            try
            {
                instance = UnityEngine.Object.Instantiate(settings);
                instance.hideFlags = HideFlags.DontSave;
                instance.themeStyleSheet = theme;
                window = new GameObject("GameDevTools (UI Toolkit)") { hideFlags = HideFlags.DontSave };
                window.SetActive(false);
                window.transform.SetParent(owner, false);
                var document = window.AddComponent<UIDocument>();
                document.panelSettings = instance;
                document.visualTreeAsset = tree;
                document.sortingOrder = 30001;
                window.SetActive(true);
                NormalizeThemeSheets(document.rootVisualElement);
                ApplyFont(document.rootVisualElement);
                ToolkitLocalizedBindings.Bind(document.rootVisualElement);
                return new ToolkitView(window, document, instance);
            }
            catch
            {
                if (window != null) { window.SetActive(false); UnityEngine.Object.Destroy(window); }
                if (instance != null) UnityEngine.Object.Destroy(instance);
                throw;
            }
        }

        /// <summary> 加载可在 UI Builder 中编辑的动态控件模板, 缺失时报告初始化错误. </summary>
        internal static VisualTreeAsset RequireTemplate(string name)
        {
            if (_templates.TryGetValue(name, out var cached) && cached != null) return cached;
            var template = Resources.Load<VisualTreeAsset>(_sharedRoot + name);
            if (template == null) throw new InvalidOperationException(ToolkitLocalization.Format("ui.Factory.RequireTemplate", ("p0", (object)(name))));
            _templates[name] = template;
            return template;
        }

        /// <summary> 优先保留有效的保存选择, 未设置或未登记时使用 Schema 列表首项. </summary>
        internal static string ResolveStylePath(string path)
        {
            path = NormalizeStylePath(path);
            return _styles.Any(style => style.path == path) ? path : _styles.FirstOrDefault()?.path ?? string.Empty;
        }

        /// <summary> 获取路径对应的显示名称, 不将名称作为资源路径拼接. </summary>
        internal static string GetStyleName(string path)
        {
            var selected = ResolveStylePath(path);
            var entry = _styles.FirstOrDefault(style => style.path == selected);
            return entry?.name ?? string.Empty;
        }

        /// <summary> 只建立配置中的独立快照, 保留首项作为默认, 不补入固定风格. </summary>
        internal static ToolkitStyleEntry[] BuildStyles(IEnumerable<ToolkitStyleEntry> source)
        {
            var result = new List<ToolkitStyleEntry>();
            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in source ?? Array.Empty<ToolkitStyleEntry>())
            {
                var path = NormalizeStylePath(entry?.path);
                if (path == null || !paths.Add(path)) continue;
                var name = string.IsNullOrWhiteSpace(entry.name) ? path.Substring(path.LastIndexOf('/') + 1) : entry.name.Trim();
                result.Add(new ToolkitStyleEntry { name = name, path = path });
            }
            return result.ToArray();
        }

        /// <summary> 只接受 Font 目录内不带扩展名的资源键, 保留首项作为默认. </summary>
        internal static ToolkitFontEntry[] BuildFonts(IEnumerable<ToolkitFontEntry> source)
        {
            var result = new List<ToolkitFontEntry>();
            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in source ?? Array.Empty<ToolkitFontEntry>())
            {
                var path = NormalizeStylePath(entry?.path);
                if (path == null || !path.StartsWith("Font/", StringComparison.Ordinal) || path.Length <= 5 || System.IO.Path.HasExtension(path) || !paths.Add(path)) continue;
                var name = string.IsNullOrWhiteSpace(entry.name) ? path.Substring(path.LastIndexOf('/') + 1) : entry.name.Trim();
                result.Add(new ToolkitFontEntry { name = name, path = path });
            }
            return result.ToArray();
        }

        /// <summary> 未登记或非法字体键使用列表首项, 不使用可修改的显示名称匹配. </summary>
        internal static string ResolveFontPath(string path)
        {
            path = NormalizeStylePath(path);
            return _fonts.Any(entry => entry.path == path) ? path : _fonts.FirstOrDefault()?.path ?? string.Empty;
        }

        /// <summary> 返回字体资源键对应的菜单名称. </summary>
        internal static string GetFontName(string path)
        {
            var selected = ResolveFontPath(path);
            var entry = _fonts.FirstOrDefault(value => value.path == selected);
            return entry?.name ?? string.Empty;
        }

        /// <summary> 所有普通界面及独立原生菜单从同一个字体配置继承. </summary>
        internal static void ApplyFont(VisualElement root)
        {
            if (_font == null || root == null) return;
            root.style.unityFont = _font;
            root.style.unityFontDefinition = FontDefinition.FromFont(_font);
        }

        /// <summary> 给原生菜单应用当前已提交的风格, 并登记文档树之外的菜单根. </summary>
        internal static VisualElement PrepareMenu(GenericDropdownMenu menu)
        {
            menu.contentContainer.AddToClassList("context-menu");
            var root = menu.contentContainer;
            while (root != null && !root.ClassListContains(GenericDropdownMenu.ussClassName)) root = root.hierarchy.parent;
            if (root == null) return null;
            root.AddToClassList("context-menu-root");
            if (_layoutSheet != null) root.styleSheets.Add(_layoutSheet);
            var sheet = _styleSheet;
            if (sheet != null) root.styleSheets.Add(sheet);
            if (_controlsSheet != null) root.styleSheets.Add(_controlsSheet);
            NormalizeThemeSheets(root);
            ApplyFont(root);
            ApplyMenuOpacity(root);
            TrackMenu(root);
            return root;
        }

        // 先尝试保存选择, 失败后仅从 Schema 首项依次查找可用风格.
        private static bool TryLoadInitialStyle(string preferred, out string selected, out StyleSheet sheet)
        {
            foreach (var path in CandidatePaths(ResolveStylePath(preferred), _styles.Select(entry => entry.path)))
            {
                sheet = Resources.Load<StyleSheet>(path + "/GameDevToolStyles");
                if (sheet == null) continue;
                selected = path;
                return true;
            }
            selected = null;
            sheet = null;
            return false;
        }

        // 公共模板只缓存一份, 初次创建检查基础面板, 切换风格不再加载这些资源.
        private static void LoadSharedResources(out VisualTreeAsset tree, out ThemeStyleSheet theme, out PanelSettings settings)
        {
            foreach (var name in _templateNames) RequireTemplate(name);
            tree = RequireTemplate("GameDevTool");
            if (_sharedTheme == null) _sharedTheme = Resources.Load<ThemeStyleSheet>(_sharedRoot + "GameDevToolTheme");
            if (_sharedPanel == null) _sharedPanel = Resources.Load<PanelSettings>(_sharedRoot + "GameDevToolPanel");
            if (_layoutSheet == null) _layoutSheet = Resources.Load<StyleSheet>(_sharedRoot + "GameDevToolLayout");
            if (_controlsSheet == null) _controlsSheet = Resources.Load<StyleSheet>(_sharedRoot + "ToolkitControls");
            theme = _sharedTheme;
            settings = _sharedPanel;
            if (theme == null || settings == null || _layoutSheet == null || _controlsSheet == null) throw new InvalidOperationException(ToolkitLocalization.Get("schema.styles.unavailable"));
        }

        // 优先所选字体, 缺失时仅依配置顺序尝试, 不回退到未登记字体.
        private static bool LoadFont(string selected)
        {
            foreach (var path in CandidatePaths(ResolveFontPath(selected), _fonts.Select(entry => entry.path)))
            {
                var font = Resources.Load<Font>(path);
                if (font == null) continue;
                _font = font;
                ActiveFont = path;
                return true;
            }
            _font = null;
            ActiveFont = string.Empty;
            return false;
        }

        // 所选项优先, 失败后从配置首项开始, 每个路径只尝试一次且不引入固定后备.
        private static IEnumerable<string> CandidatePaths(string selected, IEnumerable<string> configured) => new[] { selected }.Concat(configured).Where(path => !string.IsNullOrEmpty(path)).Distinct(StringComparer.Ordinal);

        // 资源路径使用正斜线, 支持相对目录, 不接受上级目录和绝对文件系统路径.
        private static string NormalizeStylePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            path = path.Trim().Replace('\\', '/').Trim('/');
            return path.Split('/').Any(part => part.Length == 0 || part == "." || part == ".." || part.Contains(':')) ? null : path;
        }
    }
}
