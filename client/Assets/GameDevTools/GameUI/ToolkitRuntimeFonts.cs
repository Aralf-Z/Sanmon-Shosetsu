using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace GameConsole.UI
{
    /// <summary> 开发工具独占动态字体, 避免复用 UI Toolkit 隐式 Font 缓存中的失效图集. </summary>
    internal static class ToolkitRuntimeFonts
    {
        private static readonly Dictionary<Font, Entry> _fonts = new();

        private static readonly List<Entry> _retired = new(); // 修复后的旧字体供 TextCore 暂存引用安全过渡.

        internal static int Revision { get; private set; }

        static ToolkitRuntimeFonts()
        {
            // TextCore 的共享生成器会暂存上一份字体, 关闭窗口时不能提前销毁其图集.
            Application.quitting += DisposeAll;
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += DisposeAll;
#endif
        }

        /// <summary> 显式绑定 SDF FontAsset, 所有运行时图集页均保留到程序集卸载或应用退出. </summary>
        internal static FontAsset Get(Font source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!_fonts.TryGetValue(source, out var entry))
            {
                entry = new Entry(source);
                _fonts.Add(source, entry);
            }
            else if (entry.asset == null || entry.asset.material == null)
            {
                entry.Dispose();
                entry = new Entry(source);
                _fonts[source] = entry;
                Revision++;
            }
            else if (!entry.IsValid())
            {
                // 原位清空字形与图集, 同时修复 TextCore 仍暂存的旧 FontAsset 引用.
                entry.Rebuild();
                _retired.Add(entry);
                entry = new Entry(source);
                _fonts[source] = entry;
                Revision++;
            }

            entry.ProtectTextures();
            return entry.asset;
        }

        private static void DisposeAll()
        {
            foreach (var entry in _fonts.Values)
            {
                entry.Dispose();
            }

            foreach (var entry in _retired)
            {
                entry.Dispose();
            }

            _fonts.Clear();
            _retired.Clear();
            Revision++;
        }

        private static void DestroyResource(UnityEngine.Object resource)
        {
            if (resource == null) return;

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(resource);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(resource);
            }
        }

        private sealed class Entry : IDisposable
        {
            internal readonly FontAsset asset;

            private int _checkedGlyphs = -1;

            private int _checkedAtlases = -1; // 图集页数改变时也重新检查字形引用.

            /// <summary> 使用 Unity 2022 默认动态字体规格, 保留中日韩文本的多图集能力. </summary>
            internal Entry(Font source)
            {
                asset = FontAsset.CreateFontAsset(source);
                if (asset == null)
                {
                    throw new InvalidOperationException("Cannot create GameDevTools FontAsset: " + source.name);
                }

                asset.name = source.name + " (GameDevTools Runtime)";
                asset.hideFlags = HideFlags.HideAndDontSave;
                asset.isMultiAtlasTexturesEnabled = true;
                asset.material.hideFlags = HideFlags.HideAndDontSave;
            }

            internal bool IsValid()
            {
                var textures = asset.atlasTextures;
                var count = asset.atlasTextureCount;

                if (textures == null || count <= 0 || textures.Length < count) return false;

                for (var index = 0; index < count; index++)
                {
                    if (textures[index] == null) return false;
                }

                if (asset.material.mainTexture != textures[0]) return false;

                var glyphs = asset.glyphTable;

                if (_checkedGlyphs == glyphs.Count && _checkedAtlases == count) return true;

                foreach (var glyph in glyphs)
                {
                    if (glyph.atlasIndex < 0 || glyph.atlasIndex >= count) return false;
                }

                _checkedGlyphs = glyphs.Count;
                _checkedAtlases = count;
                return true;
            }

            /// <summary> 同时重建纹理与字形索引, 避免仅补纹理后保留旧 atlasIndex. </summary>
            internal void Rebuild()
            {
                var previous = asset.atlasTextures;
                var texture = new Texture2D(1, 1, TextureFormat.Alpha8, false);

                texture.name = asset.name + " Atlas";
                texture.hideFlags = HideFlags.HideAndDontSave;
                asset.atlasTextures = new[]
                { texture };
                asset.material.mainTexture = texture;
                asset.ClearFontAssetData();
                _checkedGlyphs = -1;
                _checkedAtlases = -1;
                if (previous == null) return;

                foreach (var oldTexture in previous)
                {
                    DestroyResource(oldTexture);
                }
            }

            /// <summary> 追加图集页也保持运行时所有权, 不只保护第一页纹理. </summary>
            internal void ProtectTextures()
            {
                var textures = asset.atlasTextures;

                for (var index = 0; index < asset.atlasTextureCount; index++)
                {
                    var texture = textures[index];

                    if (texture != null && texture.hideFlags != HideFlags.HideAndDontSave)
                    {
                        texture.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
            }

            /// <summary> 仅释放工具拥有的字体资源, 不卸载原始 TTF 或 OTF. </summary>
            public void Dispose()
            {
                if (asset == null) return;

                var textures = asset.atlasTextures;

                if (textures != null)
                {
                    foreach (var texture in textures)
                    {
                        DestroyResource(texture);
                    }
                }

                DestroyResource(asset.material);
                DestroyResource(asset);
            }
        }
    }
}
