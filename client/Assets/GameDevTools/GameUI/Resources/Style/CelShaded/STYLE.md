# 赛璐璐 UI

参考: [StyleKit Cel Shading](https://www.stylekit.top/en/styles/cel-shading) 的 Hard Prompt. 以下是本项目适配说明, 不是官网提示词原文.

提示词摘要: 使用清晰粗描边、纯色色块、无模糊的偏移阴影, 搭配柔和米灰背景与红蓝绿黄强调色. 按钮提供明确的悬停、按下和焦点反馈, 保持游戏菜单的漫画感.

浅色背景: 画布 `--cel-paper: #D4CFC5`, 面板 `--cel-surface: #DFDAD0`, 输入 `--cel-input: #E7E2D8`. 三层均使用降低亮度的暖灰, 避免大面积纯白; 深色日志与宏编辑区、强调色和错误红保持原样.

Unity 适配: 用公共装饰槽位和实体边框表达硬阴影, 不使用网页专属阴影或伪元素. 字体由 Schema 统一配置, 保留控件名称与交互. 日志和宏正文保持深色底, 兼容已有富文本及语法颜色. 浮窗日志透明度继续独立.

风格映射: 名称由 Schema.name 配置, Resources 路径为 Style/CelShaded. 在选项中选择后异步准备外观, 就绪后即时应用并保存, 不重新加载公共模板.

公共结构与加载约定见 [风格规范](../STYLE_GUIDE.md). 本目录只维护外观 USS, 页面、控件、布局、PanelSettings 和 TSS 均使用 Shared.
