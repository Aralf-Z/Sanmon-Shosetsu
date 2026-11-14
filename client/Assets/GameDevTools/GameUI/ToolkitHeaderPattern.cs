using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 标题栏中间的平铺纹样, 两端透明渐隐, 不覆盖标题和窗口按钮. </summary>
    [Preserve]
    public sealed class ToolkitHeaderPattern : VisualElement
    {
        private const int FADE_SEGMENTS = 8;
        private static readonly CustomStyleProperty<Texture2D> IMAGE_PROPERTY = new("--header-pattern-image"); // 风格自有的纹样贴图.
        private static readonly CustomStyleProperty<float> WIDTH_PROPERTY = new("--header-pattern-width"); // 单次平铺宽度, 面板单位.
        private static readonly CustomStyleProperty<float> HEIGHT_PROPERTY = new("--header-pattern-height"); // 纹样高度, 超出部分居中裁剪.
        private static readonly CustomStyleProperty<float> FADE_PROPERTY = new("--header-pattern-fade"); // 左右端各自渐隐宽度, 面板单位.
        private readonly List<float> _edges = new(64); // 复用纹样接缝和渐隐分段, 不逐帧创建节点或贴图.
        private Texture2D _image; // 由 USS 引用持有, 不创建或销毁风格资源.
        private float _tileWidth = 128f; // 当前平铺宽度.
        private float _tileHeight = 64f; // 当前纹样高度.
        private float _fadeWidth = 32f; // 窄空间自动缩短过渡, 避免两侧越界.

        /// <summary> 只在样式或尺寸变化时重绘, 不拦截标题栏拖动. </summary>
        [Preserve]
        public ToolkitHeaderPattern()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(OnStyle);
            RegisterCallback<GeometryChangedEvent>(OnGeometry);
        }

        // 切换风格时重新解析资源, 无贴图的风格保持空白.
        private void OnStyle(CustomStyleResolvedEvent evt)
        {
            var custom = evt.customStyle;
            _image = custom.TryGetValue(IMAGE_PROPERTY, out var image) ? image : null;
            _tileWidth = ReadSize(custom, WIDTH_PROPERTY, 128f);
            _tileHeight = ReadSize(custom, HEIGHT_PROPERTY, 64f);
            _fadeWidth = ReadSize(custom, FADE_PROPERTY, 32f);
            MarkDirtyRepaint();
        }

        // 位置变化不影响局部纹样, 只在宽高改变时重绘.
        private void OnGeometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size != evt.newRect.size) MarkDirtyRepaint();
        }

        // 统一使用纹样顶点透明度, 与整体透明度相乘, 不叠加标题栏底色.
        private void Draw(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (_image == null || rect.width < .01f || rect.height < .01f || float.IsNaN(rect.width) || float.IsInfinity(rect.width)) return;
            var fade = Mathf.Min(_fadeWidth, rect.width * .5f);
            _edges.Clear();
            _edges.Add(0f);
            _edges.Add(rect.width);
            for (var x = _tileWidth; x < rect.width; x += _tileWidth) _edges.Add(x);
            for (var step = 1; step <= FADE_SEGMENTS; step++)
            {
                var x = fade * step / FADE_SEGMENTS;
                _edges.Add(x);
                _edges.Add(rect.width - x);
            }
            _edges.Sort();
            for (var index = _edges.Count - 1; index > 0; index--)
                if (_edges[index] - _edges[index - 1] < .001f) _edges.RemoveAt(index);

            var mesh = context.Allocate((_edges.Count - 1) * 4, (_edges.Count - 1) * 6, _image);
            var uv = mesh.uvRegion;
            var imageTop = (rect.height - _tileHeight) * .5f;
            var top = Mathf.Max(0f, imageTop);
            var bottom = Mathf.Min(rect.height, imageTop + _tileHeight);
            var vTop = uv.yMin + (1f - (top - imageTop) / _tileHeight) * uv.height;
            var vBottom = uv.yMin + (1f - (bottom - imageTop) / _tileHeight) * uv.height;
            var tint = resolvedStyle.unityBackgroundImageTintColor;
            for (var index = 0; index < _edges.Count - 1; index++)
            {
                var left = _edges[index];
                var right = _edges[index + 1];
                var tileStart = Mathf.Floor((left + right) * .5f / _tileWidth) * _tileWidth;
                var uLeft = uv.xMin + Mathf.Clamp01((left - tileStart) / _tileWidth) * uv.width;
                var uRight = uv.xMin + Mathf.Clamp01((right - tileStart) / _tileWidth) * uv.width;
                var leftTint = FadeTint(tint, left, rect.width, fade);
                var rightTint = FadeTint(tint, right, rect.width, fade);
                mesh.SetNextVertex(MakeVertex(rect.xMin + left, rect.yMin + bottom, uLeft, vBottom, leftTint));
                mesh.SetNextVertex(MakeVertex(rect.xMin + left, rect.yMin + top, uLeft, vTop, leftTint));
                mesh.SetNextVertex(MakeVertex(rect.xMin + right, rect.yMin + top, uRight, vTop, rightTint));
                mesh.SetNextVertex(MakeVertex(rect.xMin + right, rect.yMin + bottom, uRight, vBottom, rightTint));
                var first = (ushort)(index * 4);
                mesh.SetNextIndex(first);
                mesh.SetNextIndex((ushort)(first + 1));
                mesh.SetNextIndex((ushort)(first + 2));
                mesh.SetNextIndex((ushort)(first + 2));
                mesh.SetNextIndex((ushort)(first + 3));
                mesh.SetNextIndex(first);
            }
        }

        // 平滑过渡保持两端完全透明, 中间沿用主题原始颜色和透明度.
        private static Color32 FadeTint(Color tint, float x, float width, float fade)
        {
            tint.a *= Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Mathf.Min(x, width - x) / fade));
            return tint;
        }

        // UV 映射到 Unity 分配的图集区域, 避免平铺采样到其他 UI 图片.
        private static Vertex MakeVertex(float x, float y, float u, float v, Color32 tint) => new() { position = new Vector3(x, y, Vertex.nearZ), uv = new Vector2(u, v), tint = tint };

        // 无效尺寸使用内置默认值, 不产生零除或无限平铺.
        private static float ReadSize(ICustomStyle custom, CustomStyleProperty<float> property, float fallback)
        {
            return custom.TryGetValue(property, out var value) && !float.IsNaN(value) && !float.IsInfinity(value) && value >= 1f ? value : fallback;
        }

        /// <summary> 使用 Unity 2022.3 的 UXML 工厂注册方式. </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ToolkitHeaderPattern, UxmlTraits> { }
    }
}
