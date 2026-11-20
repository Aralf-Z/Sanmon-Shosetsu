using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 已准备完成的主题资源, 提交前不改变界面或配置. </summary>
    internal sealed class ToolkitAppearanceResources
    {
        internal readonly string style;

        internal readonly string fontPath;

        internal readonly Font font;

        internal readonly StyleSheet sheet;

        internal ToolkitAppearanceResources(string style, string fontPath, Font font, StyleSheet sheet)
        {
            this.style = style;
            this.fontPath = fontPath;
            this.font = font;
            this.sheet = sheet;
        }
    }

    public static partial class ToolkitFactory
    {
        private const string _mainSheetName = "GameDevToolStyles"; // 工厂挂载的风格外观入口, 其他样式保留原位.

        private const string _layoutSheetName = "GameDevToolLayout"; // 公共模板的布局标记, 外观必须在其后挂载.

        private static readonly string[] _templateNames =
        {
            "GameDevTool",
            "GameDevToolOptions",
            "GameDevToolCheat",
            "GameDevToolConsole",
            "GameDevToolMacro",
            "ToolkitCommandButton",
            "ToolkitCommandCategory",
            "ToolkitCommandDetails",
            "ToolkitCommandParameter",
            "ToolkitFloatingLog",
            "ToolkitFloatingSettings",
            "ToolkitHelpWindow",
            "ToolkitListItem",
            "ToolkitMacroButton",
            "ToolkitMacroGrid",
            "ToolkitPinnedDragPreview",
            "ToolkitPresetButton",
            "ToolkitSearchHistoryPopup",
            "ToolkitSearchHistoryWindow"
        };

        private static readonly Dictionary<string, ToolkitAppearanceResources> _appearanceCache = new(StringComparer.Ordinal);

        private static readonly Dictionary<string, Font> _fontCache = new(StringComparer.Ordinal);

        private static readonly List<WeakReference<VisualElement>> _menuRoots = new(); // 原生菜单位于 UIDocument 层级之外, 不延长其生命周期.

        private static readonly Dictionary<string, VisualTreeAsset> _templates = new(StringComparer.Ordinal);

        /// <summary> 仅初始化和成功提交后递增, 供测量缓存识别主题变化. </summary>
        internal static int AppearanceVersion { get; private set; }

        internal static IEnumerator PrepareAppearance(string style, string font, Action<ToolkitAppearanceResources> ready)
        {
            if (ready == null)
            {
                throw new ArgumentNullException(nameof(ready));
            }

            Font loadedFont = null;

            foreach (var path in CandidatePaths(ResolveFontPath(font), _fonts.Select(entry => entry.path)))
            {
                _fontCache.TryGetValue(path, out loadedFont);
                if (Config.IsExternalFont(path))
                {
                    _externalFonts.TryGetValue(path, out loadedFont);
                }
                else if (loadedFont == null)
                {
                    var request = Resources.LoadAsync<Font>(path);

                    while (!request.isDone)
                    {
                        yield return request;
                    }

                    loadedFont = request.asset as Font;
                }

                if (loadedFont == null)
                {
                    continue;
                }

                font = path;
                _fontCache[path] = loadedFont;
                break;
            }

            if (loadedFont == null)
            {
                throw new InvalidOperationException(ToolkitLocalization.Get("schema.fonts.unavailable"));
            }

            Exception failure = null;

            foreach (var path in CandidatePaths(ResolveStylePath(style), _styles.Select(entry => entry.path)))
            {
                ToolkitAppearanceResources prepared = null;
                var work = PrepareAppearanceCandidate(path, font, value => prepared = value);
                using var lifetime = work as IDisposable;

                while (true)
                {
                    bool moved;
                    object current = null;

                    try
                    {
                        moved = work.MoveNext();
                        if (moved)
                        {
                            current = work.Current;
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        break;
                    }

                    if (!moved)
                    {
                        break;
                    }

                    yield return current;
                }

                if (prepared == null)
                {
                    continue;
                }

                ready(prepared);
                yield break;
            }

            throw new InvalidOperationException(ToolkitLocalization.Get("schema.styles.unavailable"), failure);
        }

        // 仅准备风格外观与字体, 公共模板和面板不参与切换.
        private static IEnumerator PrepareAppearanceCandidate(string style, string font, Action<ToolkitAppearanceResources> ready)
        {
            if (ready == null)
            {
                throw new ArgumentNullException(nameof(ready));
            }

            style = NormalizeStylePath(style);
            font = Config.IsExternalFont(font) ? font : NormalizeStylePath(font);
            if (style == null || !_styles.Any(entry => entry.path == style))
            {
                throw new InvalidOperationException(ToolkitLocalization.Get("ui.Factory_Appearance.PrepareAppearance"));
            }

            if (font == null || ResolveFontPath(font) != font)
            {
                throw new InvalidOperationException(ToolkitLocalization.Get("ui.Factory_Appearance.PrepareAppearance.2"));
            }

            yield return null;
            _appearanceCache.TryGetValue(style, out var cached);
            var prefix = style + "/";
            var sheetRequest = cached?.sheet != null
                ? null
                : Resources.LoadAsync<StyleSheet>(prefix + "GameDevToolStyles");

            _fontCache.TryGetValue(font, out var loadedFont);
            if (font == ActiveFont && _font != null)
            {
                loadedFont = _font;
            }

            var fontRequest = loadedFont != null || Config.IsExternalFont(font)
                ? null
                : Resources.LoadAsync<Font>(font);

            foreach (var request in new[] { sheetRequest, fontRequest })
            {
                if (request != null)
                {
                    while (!request.isDone)
                    {
                        yield return request;
                    }
                }
            }

            var sheet = sheetRequest == null
                ? cached.sheet
                : sheetRequest.asset as StyleSheet;

            if (fontRequest != null)
            {
                loadedFont = fontRequest.asset as Font;
            }

            if (sheet == null)
            {
                throw new InvalidOperationException(ToolkitLocalization.Format("ui.Factory_Appearance.PrepareAppearance.4", ("p0", (object)(style))));
            }

            if (loadedFont == null)
            {
                throw new InvalidOperationException(ToolkitLocalization.Format("ui.Factory_Appearance.PrepareAppearance.5", ("p0", (object)(font))));
            }

            var prepared = new ToolkitAppearanceResources(style, font, loadedFont, sheet);

            _appearanceCache[style] = prepared;
            _fontCache[font] = loadedFont;
            ready(prepared);
        }

        internal static void CommitAppearance(ToolkitAppearanceResources prepared, VisualElement root, PanelSettings panel)
        {
            if (prepared == null || root == null || panel == null)
            {
                throw new ArgumentNullException(ToolkitLocalization.Get("ui.Factory_Appearance.CommitAppearance"));
            }

            if (prepared.sheet == null || prepared.font == null)
            {
                throw new InvalidOperationException(ToolkitLocalization.Get("ui.Factory_Appearance.CommitAppearance.2"));
            }

            var sheets = new List<SheetSnapshot>();
            var fonts = new List<FontSnapshot>();
            var roots = new List<VisualElement>
            { root };
            var fontAsset = ToolkitRuntimeFonts.Get(prepared.font);

            for (var i = _menuRoots.Count - 1; i >= 0; i--)
            {
                if (!_menuRoots[i].TryGetTarget(out var menu))
                {
                    _menuRoots.RemoveAt(i);
                    continue;
                }

                if (!root.Contains(menu) && !roots.Contains(menu))
                {
                    roots.Add(menu);
                }
            }

            try
            {
                foreach (var target in roots)
                {
                    fonts.Add(new FontSnapshot(target));
                    NormalizeThemeSheets(target, prepared.sheet, sheets);
                    target.style.unityFont = prepared.font;
                    target.style.unityFontDefinition = FontDefinition.FromSDFFont(fontAsset);
                }
            }
            catch
            {
                for (var i = sheets.Count - 1; i >= 0; i--)
                {
                    sheets[i].Restore();
                }

                foreach (var saved in fonts)
                {
                    saved.Restore();
                }

                throw;
            }

            ActiveStyle = prepared.style;
            ActiveFont = prepared.fontPath;
            _styleSheet = prepared.sheet;
            _font = prepared.font;
            _appearanceCache[prepared.style] = prepared;
            AppearanceVersion++;
            ToolkitIcons.RefreshWindowResize(root);
            ToolkitLocalization.RefreshPresentation();
        }

        internal static TemplateContainer InstantiateTemplate(string name) => InstantiateTemplate(RequireTemplate(name));

        internal static TemplateContainer InstantiateTemplate(VisualTreeAsset template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var instance = template.Instantiate();

            ToolkitLocalizedBindings.Bind(instance);
            NormalizeThemeSheets(instance);
            instance.RegisterCallback<AttachToPanelEvent>(TemplateAttached);
            return instance;
        }

        /// <summary> 只替换节点直接挂载的主题主样式, 保留无关表及其顺序. </summary>
        internal static void NormalizeThemeSheets(VisualElement root)
        {
            if (root != null && _styleSheet != null)
            {
                NormalizeThemeSheets(root, _styleSheet, null);
            }
        }

        // 初次创建重置外观缓存, 公共模板缓存独立保留.
        private static void InitializeAppearanceCache(StyleSheet sheet)
        {
            _appearanceCache.Clear();
            _fontCache.Clear();
            _appearanceCache[ActiveStyle] = new ToolkitAppearanceResources(ActiveStyle, ActiveFont, _font, sheet);
            if (_font != null)
            {
                _fontCache[ActiveFont] = _font;
            }

            AppearanceVersion++;
        }

        private static void TemplateAttached(AttachToPanelEvent evt)
        {
            var root = evt.target as VisualElement;

            NormalizeThemeSheets(root);
            ToolkitTextOpacity.Track(root);
        }

        private static void TrackMenu(VisualElement root)
        {
            for (var i = _menuRoots.Count - 1; i >= 0; i--)
            {
                if (!_menuRoots[i].TryGetTarget(out var tracked))
                {
                    _menuRoots.RemoveAt(i);
                }
                else if (ReferenceEquals(tracked, root)) return;
            }

            _menuRoots.Add(new WeakReference<VisualElement>(root));
            root.RegisterCallback<AttachToPanelEvent>(MenuAttached);
            root.RegisterCallback<DetachFromPanelEvent>(MenuDetached);
        }

        private static void ReleaseTrackedMenus()
        {
            foreach (var reference in _menuRoots)
            {
                if (reference.TryGetTarget(out var root))
                {
                    UntrackMenu(root);
                }
            }

            _menuRoots.Clear();
        }

        private static void UntrackMenu(VisualElement root)
        {
            ReleaseMenuOpacity(root);
            root.UnregisterCallback<AttachToPanelEvent>(MenuAttached);
            root.UnregisterCallback<DetachFromPanelEvent>(MenuDetached);
        }

        private static void MenuAttached(AttachToPanelEvent evt)
        {
            var root = evt.target as VisualElement;

            NormalizeThemeSheets(root);
            ApplyFont(root);
            ApplyMenuOpacity(root);
        }

        private static void MenuDetached(DetachFromPanelEvent evt)
        {
            var root = evt.target as VisualElement;

            if (root == null) return;

            UntrackMenu(root);
            _menuRoots.RemoveAll(reference => !reference.TryGetTarget(out var tracked) || ReferenceEquals(tracked, root));
        }

        // 每个公共模板在布局之后挂载外观, 原生菜单保留原样式位置和其他无关样式.
        private static void NormalizeThemeSheets(VisualElement root, StyleSheet sheet, List<SheetSnapshot> snapshots)
        {
            var themed = new List<int>();
            var layout = -1;

            for (var i = 0; i < root.styleSheets.count; i++)
            {
                if (root.styleSheets[i] != null && root.styleSheets[i].name == _mainSheetName)
                {
                    themed.Add(i);
                }

                if (root.styleSheets[i] != null && root.styleSheets[i].name == _layoutSheetName)
                {
                    layout = i;
                }
            }

            if ((themed.Count == 0 && layout >= 0) || (themed.Count > 0 && (themed.Count != 1 || root.styleSheets[themed[0]] != sheet)))
            {
                snapshots?.Add(new SheetSnapshot(root));
                var replacement = new List<StyleSheet>();

                for (var i = 0; i < root.styleSheets.count; i++)
                {
                    if (themed.Count > 0 && i == themed[0])
                    {
                        replacement.Add(sheet);
                    }

                    if (!themed.Contains(i))
                    {
                        replacement.Add(root.styleSheets[i]);
                    }

                    if (themed.Count == 0 && i == layout)
                    {
                        replacement.Add(sheet);
                    }
                }

                ReplaceSheets(root, replacement);
            }

            if (layout >= 0 || themed.Count > 0)
            {
                EnsureControlSheetLast(root, snapshots);
            }

            for (var i = 0; i < root.hierarchy.childCount; i++)
            {
                NormalizeThemeSheets(root.hierarchy[i], sheet, snapshots);
            }
        }

        private static void EnsureControlSheetLast(VisualElement root, List<SheetSnapshot> snapshots)
        {
            if (_controlsSheet == null) return;

            var count = root.styleSheets.count;

            if (count > 0 && root.styleSheets[count - 1] == _controlsSheet) return;

            snapshots?.Add(new SheetSnapshot(root));
            root.styleSheets.Remove(_controlsSheet);
            root.styleSheets.Add(_controlsSheet);
        }

        private static void ReplaceSheets(VisualElement root, List<StyleSheet> sheets)
        {
            root.styleSheets.Clear();
            foreach (var sheet in sheets)
            {
                root.styleSheets.Add(sheet);
            }
        }

        // 保存完整顺序, 失败时精确恢复包括第三方样式在内的原引用.
        private sealed class SheetSnapshot
        {
            private readonly VisualElement _root;

            private readonly List<StyleSheet> _sheets = new();

            internal SheetSnapshot(VisualElement root)
            {
                _root = root;
                for (var i = 0; i < root.styleSheets.count; i++)
                {
                    _sheets.Add(root.styleSheets[i]);
                }
            }

            internal void Restore()
            {
                ReplaceSheets(_root, _sheets);
            }
        }

        // 保留 inline 的值和关键字, 而不是把 resolved 值错误地永久写回元素.
        private readonly struct FontSnapshot
        {
            private readonly VisualElement _root;

            private readonly StyleFont _font;

            private readonly StyleFontDefinition _definition;

            internal FontSnapshot(VisualElement root)
            {
                _root = root;
                _font = root.style.unityFont;
                _definition = root.style.unityFontDefinition;
            }

            internal void Restore()
            {
                _root.style.unityFont = _font;
                _root.style.unityFontDefinition = _definition;
            }
        }
    }
}
