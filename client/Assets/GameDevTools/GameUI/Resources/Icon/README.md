# 开发工具内联图标

在各语言的 `ui.json` 中注册文件键, 在 UI 文案中引用注册项:

```json
"icon.pin": "pin",
"ui.ContextMenu.Show.2": "{icon.pin} 置顶"
```

`pin` 为资源文件名, 不是翻译文本. 文件键仅使用英文字母, 数字, 下划线和连字符, 命名时与文件名保持相同大小写.

风格 `Style/Terminal` 的查找顺序为:

1. `Icon/Terminal/pin.png`.
2. `Icon/Shared/pin.png`.
3. 均不存在时显示 `pin` 文本.

源图统一为 64x64 透明 PNG, 导入类型为 Sprite (2D and UI), Sprite Mode 为 Single. 关闭 Mipmap 和压缩, 使用 Bilinear 和 Clamp. 图标通过 `Resources.Load<Sprite>` 加载, 不生成额外的图片资源或材质配置. Shared 目录只保留 PNG 和 Unity 必需的 `.meta`.

`windowResize` 是贴边角标, 各风格和 Shared 均提供独立图片. 三角形的右边与下边不留透明外边距, 使用 Full Rect Sprite. 窗口通过独立 Image 填满 24x24 缩放手柄, 不参与字体混排; 风格提交时同步刷新, 仍沿用上述资源回退顺序.

控件通过本地化取得图标标记, 由 `ToolkitMixedText` 使用 Image 和 Label 绘制, 图片保持原比例和原色. 正文内联图标为 1em 方框; 纯图标控件统一为 24x24 面板像素方框, 不使用字体行高占位. 等宽针对 Image 显示框, PNG 的透明边距仍保留. 图文测量, 换行和省略共用同一套布局规则; 帮助分页不会拆开图片标记. 原生 tooltip 使用纯文本图标键.

动态控件使用 `ToolkitMixedText.SetText`, 可选 `ToolkitMixedTextMode.Auto`, `Inline` 或 `Control`. Auto 将单图标按钮和 Toggle 文本识别为控件模式; 其他内容保持内联. 日志类型等独立 Label 使用 Control, 模板内置顶标记使用 `toolkit-icon-indicator` 类. Foldout 使用 `SetFoldoutText`, 始终保持内联.

紧凑按钮和 Toggle 由公共控件 USS 固定为 32x30, 边框 1px, padding 2px. 公共控件 USS 在风格之后应用, 不从 TSS 导入, 避免成为低优先级默认样式. 侧栏图标预留 34px. 新增风格时保留这些几何约束, 外观颜色和装饰继续由风格负责.

不直接把图标标记写入原生 `text`. 需要取得原文时调用 `ToolkitMixedText.GetText`. 普通文本继续使用原生控件, 输入框内容不会被当作图标标记解析.
