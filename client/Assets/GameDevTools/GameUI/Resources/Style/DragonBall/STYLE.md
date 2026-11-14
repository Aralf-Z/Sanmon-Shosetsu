# 七龙珠 / 深蓝战斗

采用 B 方向的午夜深蓝、橙色选中态和金色描边, 使用当前 DevTools 公共布局. 显示名称由 Schema.name 配置为七龙珠, Resources 路径为 Style/DragonBall.

## 配色与控件

- 主背景 #101A2C, 面板 #1B2C43, 输入 #0D1729, 正文 #F2EEE3, 次要文字 #A9B7C9.
- 橙色 #F28C28 用于固定宏入口和当前页签, 金色 #F4B83F 用于边框、悬停与焦点提示. 普通命令、预设和宏按钮保持深蓝.
- Console、宏正文和堆栈保持深色可读区域, 原始日志颜色与语法配色沿用工具设置. 帮助、Options、历史下拉和右键菜单使用同一套配色.
- 浮窗日志背景 #102039, 橙金色细边框; 背景和文字透明度继续使用原有独立设置.
- 不绑定独立字体, 不改变输入、排序、分页、拖拽或命令执行逻辑. 维护规则见 [公共风格规范](../STYLE_GUIDE.md).

## 素材与通用入口

| 素材 | 源图规格 | 导入 Max Size | 用法 |
|---|---|---|---|
| HeaderDecoration.png | 2172 x 724, RGBA8, 透明 | 1024 | 筋斗云, 在现有标题装饰槽位单张居中 contain, 不平铺, 不在标题栏背景重复绘制 |
| FloatingBadge.png | 1254 x 1254, RGBA8, 透明 | 256 | 四星珠, 四颗红星, 显示于原有 56 x 56 装饰区域, 76 x 56 命中区与独立设置按钮不变 |

两张图均由内置 imagegen 生成, 原始文件保留, 项目内图片未裁剪或重绘. 导入设置使用 Bilinear、Clamp、sRGB、Alpha Is Transparency, 关闭 Mip Maps 和有损压缩. 高分辨率源图通过导入设置缩小使用.

HeaderDecoration.png 是所有风格都可选择使用的通用素材名称. 未配置该用途的风格默认不显示单张装饰, 不新增占位图片或运行时提示. 本风格不设置平铺花纹的 --header-pattern-image, 避免和单张筋斗云重合.

## 生成提示词

### HeaderDecoration.png

Create ONE finished production UI texture asset, not a UI mockup. Flying Nimbus / 筋斗云 from Dragon Ball, matching a dark navy-and-orange developer tool skin: golden-yellow curled cloud with a sleek trailing golden swoosh extending to the right. Clear 2D anime cel shading, warm #ffd15b highlights, #e6a324 midtones, #9c5a15 restrained outlines, small soft cream highlight, no photorealism. Horizontal silhouette, compact rounded billows clustered on the left half, graceful tapering tail on the right, entirely readable at only 30 panel pixels tall. Output wide 4:1 canvas, intended source 1024x256. IMPORTANT tight framing: cloud and tail together nearly fill the width and height with only 3% transparent safety margin, no huge empty margins. Genuine transparent alpha background. No scenery, no blue background, no text, no letters, no icons, no stars, no border, no frame, no checkerboard, no ground shadow. A single isolated cloud asset, flat front view.

### FloatingBadge.png

Create ONE finished production UI icon asset on a genuine transparent alpha background. A single four-star Dragon Ball / 四星珠, front view, a glossy translucent amber-orange sphere with EXACTLY FOUR dark-red five-point stars on its front, arranged in a balanced 2 by 2 square. Count precisely FOUR stars. All four stars complete, clearly separated and equal size. Rich deep amber bottom edge, #ffb72f sunlit center, warm orange #f58b18, a restrained dark ink outer contour, one compact white curved specular highlight near upper-left which does NOT cover any star. 2D anime cel shading suitable for a dark-navy game developer tool. Strong simple round silhouette, crisp at 56x56 UI pixels. Square canvas, intended source512x512. Ball occupies 90% of image width and height, centered, enough transparent padding for antialiasing. NO ground or cast shadow outside the ball. No label, no text or lettering, no DEV, no settings gear, no frame, no decorative ribbons, no other objects, no background, no checkerboard. Exactly one round four-star ball, not a collection.
