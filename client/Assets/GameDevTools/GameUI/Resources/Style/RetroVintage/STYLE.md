# 复古怀旧风 UI

参考: [StyleKit Retro Vintage](https://www.stylekit.top/en/styles/retro-vintage) 的官方 tokens 和设计规则. 本文件记录 Unity UI Toolkit 适配, 不是提示词全文.

配色: 棕 `#8B4513`、奶油纸 `#F5E6D3`、砖红 `#C94C4C`、森林绿 `#2E4A3F`, 辅以砂色 `#D4A373` 和橄榄色 `#816904`. 外壳使用轻微静态纸纹、2px 描边和低圆角; 按钮只切换颜色, 不平移、缩放, 不增加动画或阴影.

适配: 模板、控件名和尺寸链统一维护在 Shared, 本目录只维护外观 USS. 日志、堆栈、宏正文和语法示例保持深色底, 保留富文本、语法色、错误红和透明输入高亮. 侧栏详情每侧 border + padding 仍为 11; 搜索历史每行 30, 滚动条和 Options 40/60 行布局保持原样.

纸纹: `SurfaceTexture.png` 为固定种子生成的 128×128 低透明度纸纤维纹理, 不进行运行时生成. 字体继续使用 Schema 中选择的字体, 此主题不引用独立字体资源.

风格映射: 名称由 Schema.name 配置, Resources 路径为 Style/RetroVintage. 在选项中异步切换外观, 不重新加载公共模板. 维护约定见 [风格规范](../STYLE_GUIDE.md).
