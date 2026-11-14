# GameDevTools UI 风格规范

本规范适用于 GameDevTools 的所有 UI Toolkit 风格, 不包含独立 GM. 修改功能或布局时只维护公共 UI; 新增风格时只维护外观.

## 1. 目录与职责

```text
Resources/
├─ Schema.asset
├─ Font/
├─ Localization/
└─ Style/
   ├─ STYLE_GUIDE.md
   ├─ Shared/
   │  ├─ GameDevTool.uxml              # 主入口, 同目录包含全部页面和控件模板
   │  ├─ GameDevToolLayout.uss         # 公共布局与基础外观
   │  ├─ ToolkitOpacity.uss            # 每个节点的透明度标记初始化
   │  ├─ ToolkitControls.uss           # 所有风格一致的控件状态与行为约束
   │  ├─ GameDevToolPanel.asset        # 所有风格共用的面板设置
   │  └─ GameDevToolTheme.tss          # 基础布局与公共控件样式
   └─ 风格目录/
      ├─ GameDevToolStyles.uss         # 必需, 风格唯一的外观入口
      ├─ SurfaceTexture.png           # 可选, 通用铺底纹理
      ├─ HeaderPattern.png            # 可选, 标题栏花纹
      ├─ HeaderDecoration.png         # 可选, 标题栏单张装饰
      ├─ WindowFrame.png              # 可选, 装饰边框
      ├─ FloatingBadge.png            # 可选, 浮标图片
      └─ STYLE.md                     # 可选, 风格配色与美术说明
```

- `Shared` 是公共资源目录, 不作为 Schema 中的一项风格.
- 公共 UXML 定义层级、控件名称、装饰槽位和交互区域. 不为风格复制 UXML、布局、PanelSettings 或 TSS.
- 公共布局维护区域比例、对齐、省略、分页容器、滚动约束、隐藏状态和点击范围. Options 每行仍为名称 40%, 设置内容 60%.
- 风格 USS 负责配色、纹理、装饰、字体外观、边框、圆角和控件内边距. 不改变功能层级、页面比例、排序、分页或滚动策略.
- 装饰使用已有 `theme-*`、`floating-badge-*`、`window-header-pattern` 槽位. 装饰不参与点击或推动内容布局, 不修改浮标实际点击范围.
- C# 负责交互、数据绑定和实际运行时测量, 如窗口拖动、虚拟日志高度、光标可见性和网格容量. 不为某套风格编写分支.

## 2. 样式加载顺序

1. 公共 PanelSettings 使用公共 TSS, 提供基础布局和控件样式.
2. 每份公共 UXML 引用 `GameDevToolLayout.uss`, 其首先导入 `ToolkitOpacity.uss`.
3. `ToolkitFactory` 在公共布局之后挂载所选风格的 `GameDevToolStyles.uss`.
4. UXML 最后挂载 `ToolkitControls.uss`, 保留透明原生宏文字、帮助选中状态、滚动约束和浮窗留白等共同行为.

风格 USS 不重复导入上述公共文件. 原生右键菜单也按相同顺序挂载. 字体、语言和外观通过工厂的现有异步切换流程更新; 切换风格不重新加载模板, 不重建窗口或池项.

在 UI Builder 中直接打开 `Shared` 下的模板可以检查公共结构和基础布局. 需要预览风格时, 临时将对应风格 USS 放在布局之后、公共控件样式之前; 不将预览绑定保存为某套风格专用模板.

## 3. 控件、状态与文本

- UXML `name` 使用 camelCase, 在当前模板查询范围内保持唯一; USS class 使用 kebab-case. 调整名称时同步检查 C# 查询和嵌套模板.
- 动态控件通过 `ToolkitFactory.RequireTemplate` 和 `InstantiateTemplate` 创建, 保留统一的本地化、字体、透明度及外观更新入口.
- 隐藏使用公共 `is-hidden`; 选中、悬停、禁用和置顶等沿用控件已有的状态 class 或伪类. 风格只改变这些状态的视觉反馈.
- 导航事件由 `ToolkitNavigationBlocker` 在工具独占面板统一拦截, 普通键盘事件和鼠标事件保留. 程序关闭原生菜单使用其 `CloseMenu` 方法, 避免取消事件被当作输入导航屏蔽.
- 固定文字和 emoji 使用 `@loc:键`; 动态文字使用 `ToolkitLocalization` 的稳定键与占位符. 不修改输入值、日志原文、业务命令、宏正文或用户数据.
- 风格、字体的展示名称取 `Schema` 条目的 `name`, 不做本地化. 默认取配置列表首项; 保存选择失效时按配置顺序回退, 不硬编码某个风格.
- 字体统一来自 `Resources/Font`, 由 Schema 和工厂管理. 风格内不绑定独立字体或修改用户选择.

## 4. 图片规范

同一用途使用同一名称, 不再在文件名中包含风格前缀. 图片放在所属风格目录, USS 使用相对路径引用. 无图片的风格不放空白占位图.

### 4.1. 新素材用途统一接入

- 任意风格需求新增一种素材用途时, 同步把该用途纳入所有风格共用的素材配置约定、装饰槽位和样式接口. 不为某套风格添加专用文件名、加载分支或独立模板.
- 有素材的风格按统一名称配置并使用; 没有素材的风格不配置、不引用, 对应装饰默认隐藏或使用 `background-image: none`. 不要求为其他风格补图, 不跨风格借用图片.
- 现有名称能够表达用途时直接复用. 确需新增用途时, 先在本规范补充统一名称、源图尺寸、透明区域、显示方式和导入要求, 再接入公共槽位. 不提前建立未使用的素材文件或配置项.
- 可选装饰不得占据额外布局空间或改变命中区. 切换到不使用该素材的风格时, 必须清除上一风格的图片和相关装饰状态.
- 有无素材由风格的有效配置决定, 不给缺少可选素材的风格添加运行时提醒或占位图. 已明确引用的文件路径应在开发期间正确维护.

### 4.2. 命名与用途

| 名称 | 用途 | 当前示例 |
|---|---|---|
| `SurfaceTexture.png` | 通用背景、标题栏、页脚或浮标铺底 | Blueprint 网格, RetroVintage 纸纹 |
| `HeaderPattern.png` | 标题栏中间的平铺花纹 | BronzeAshes 云雷纹 |
| `HeaderDecoration.png` | 标题栏中间的单张装饰, 居中等比显示 | DragonBall 筋斗云 |
| `WindowFrame.png` | 外框装饰, 可设置九宫格切片 | BronzeAshes 青铜边框 |
| `FloatingBadge.png` | 浮标外观, 可包含透明异形轮廓 | BronzeAshes 神鸟 |

同一图片可以被多个区域引用, 不为复用而复制文件. 标题栏可复用 `SurfaceTexture.png`, 不要求另建 `HeaderPattern.png`. 平铺尺寸、切片和纹理裁剪由所属风格 USS 指定.

`HeaderPattern.png` 与 `HeaderDecoration.png` 共用已有 `.window-header-pattern` 装饰槽位, 同一区域选择一种绘制方式:

- 平铺花纹通过 `--header-pattern-image` 和逻辑宽高配置, 由公共控件绘制并处理边缘渐隐.
- 单张装饰通过该槽位的 `background-image` 引用 `HeaderDecoration.png`, 使用 `no-repeat`、`contain` 和居中定位, 不设置 `--header-pattern-image`. 不使用该用途的风格沿用公共 `background-image: none`, 不需要补素材或新增配置字段.
- 主窗口与帮助标题栏共用这一入口, 不在标题栏容器上再次平铺同一张图. 单张装饰与平铺花纹切换时, 由公共默认样式和移除旧风格样式清除旧图片; 保持标题、窗口按钮及实际布局不变.

### 4.3. 素材规格

以下为新增素材的默认制作规格, 尺寸单位为源图像素. 源图尺寸与 UI Toolkit 面板单位分开维护, 不直接把源图像素作为控件宽高.

| 名称 | 默认源图尺寸 | 透明区域与制作要求 | 显示约定 |
|---|---|---|---|
| `SurfaceTexture.png` | 128 x 128; 简单纹理可用 64 x 64, 较细纹理可用 256 x 256 | 双轴平铺时四边无缝; 透明底或低对比铺底, 不包含文字和按钮 | 由 USS 指定平铺逻辑尺寸, 避免放大后线条模糊或干扰正文 |
| `HeaderPattern.png` | 512 x 256 | 透明底, 水平方向无缝; 上下保留至少 16px 安全区, 不包含标题文字 | 使用公共标题花纹槽位; USS 明确 `--header-pattern-width` 和 `--header-pattern-height`, 保留两端渐隐 |
| `HeaderDecoration.png` | 1024 x 256; 单张图案可按构图调整比例 | 透明底, 图案完整, 四边建议至少 8px 安全区, 不包含标题文字或窗口按钮; 不要求无缝 | 现有标题装饰槽位内单张居中 `contain`, 默认高度约 30 面板单位; 不重复、不拉伸、不覆盖两侧文字和按钮 |
| `WindowFrame.png` | 256 x 256 | 中心透明; 四角完整, 边缘适合拉伸; 默认四边各 32px 切片区 | USS 明确四边 `-unity-slice-*`; 若边框厚度不同, 切片值跟随素材并记录在风格说明中 |
| `FloatingBadge.png` | 256 x 256 | 透明底, 图案居中, 主要轮廓距四边至少 8px; 允许异形, 不烘焙设置按钮 | 使用 `contain` 保持比例; 当前通常显示约 56 x 56 面板单位, 浮标实际命中区继续为 76 x 56 |

- 文件使用 PNG, RGB 每通道 8 bit; 需要透明背景时使用 RGBA 和真实 alpha, 不烘焙棋盘格、白底或黑底. 透明边缘不得带明显色边.
- 使用 sRGB 颜色, `Texture Type = Default`, 2D 纹理. 默认关闭 Mip Maps 和非二次幂缩放, 开启透明素材的 Alpha Is Transparency. 导入 Max Size 按实际显示像素及目标缩放倍率配置; 高分辨率源图可以通过导入设置缩小使用, 不必修改原文件. 标题单张装饰通常可用 1024, 浮标通常可用 256, 特殊值记录在风格说明中.
- 普通 UI 素材默认 Bilinear, 像素风素材使用 Point. 需要纹理采样平铺的底纹使用 Repeat; 独立徽标和切片边框使用 Clamp. 标题花纹按实际铺设方式设置采样, 不依赖越界采样消除接缝.
- 细线、透明边缘和小图默认不使用有损压缩, 防止边框、星形或纹样失真. 像素风图案按整数倍率显示, 不用模糊缩放弥补尺寸不合适.
- 特殊素材可调整源图尺寸、比例和切片, 在对应 `STYLE.md` 中记录源图尺寸、逻辑显示尺寸、裁剪或平铺策略及导入差异; 新增通用用途仍需先补充本规范.
- 现有素材保留当前尺寸和导入设置, 不因新增规格批量缩放或重新导入. 上述规格供后续制作与替换参考, 不新增运行时尺寸检查.

### 4.4. 替换与维护

移动或改名图片必须连同原 `.meta` 一起移动, 保留 GUID. 替换内容时保持名称和 `.meta`, 检查透明边缘、纹理导入设置、切片以及实际显示尺寸. 不在运行时生成替代纹理或按风格名猜测文件名.

## 5. 透明度与版本基线

- 普通界面使用 `ToolkitTextOpacity` 的统一流程, 字体最低透明度保留现有 0.6 规则. 不叠加父节点 opacity 导致文字重复变淡.
- 背景、边框和图片颜色按现有 `--dev-opacity-*` 标记声明; 悬停等状态只覆盖颜色值. 参考现有风格的同类控件写法.
- 浮窗日志背景使用 `--floating-log-background`, 背景和文字继续由独立选项控制, 不改为整体透明度.
- 使用 Unity 2022.3 公开的 UI Toolkit API. 不增加版本兼容宏、私有接口反射或 Unity 6 专属属性.
- 使用 USS 支持的属性, 不引入网页 CSS 的渐变、阴影、gap、z-index 或伪元素. 现有渐隐花纹由公共 `ToolkitHeaderPattern` 实现.

## 6. 新增与维护流程

1. 新建 `Style/风格目录`, 添加 `GameDevToolStyles.uss`; 从现有风格复制外观规则后修改, 不复制 Shared.
2. 按上述规格添加可选图片; 新用途先补充统一规范和公共可选槽位, 所有风格均可选择使用, 无素材时不显示. 保持输入、正文、错误信息和状态反馈清晰可读.
3. SchemaUpdater 自动追加发现的风格. 在 `Schema.asset` 中维护 `name` 和 `path`, 路径形如 `Style/BronzeAshes`; 手动名称和顺序保留.
4. 更新本风格的 `STYLE.md`, 记录配色、装饰和图片用途, 不在其中维护另一份布局规范.
5. 静态检查路径、GUID 和模板引用, 完成 Runtime 与 Editor 编译. 在游戏内检查所有页面、弹层、菜单、窄窗口、字体与透明度切换、浮标和日志背景, 覆盖有素材与无素材风格之间的切换, 确认无残留装饰.

资源完整性在开发期间处理. 不额外添加运行时资源检查弹窗、修复提示或占位外观.
