# 代码风格

## 命名规范

### 字段（Field）

| 访问级别 / 修饰符 | 命名风格 | 示例 |
|-------------------|----------|------|
| `private`、`protected` | `_camelCase`（以下划线开头的小驼峰） | `_userName` |
| 其他（`internal`、`public` 等） | `camelCase`（小驼峰） | `userName` |
| `const`、`static readonly` | `ALL_UPPER_SNAKE`（全大写, 下划线分隔） | `MAX_RETRY_COUNT` |

> **注1**：只有 `static` 或者 `readonly` 修饰使用普通字段规则（`private`/`protected` 用 `_camel`, 其他用 `camel`）.  
> **注2**：**所有实例字段应明确访问修饰符, 避免隐式 `private`**.

### 属性（Property）

统一使用 `PascalCase`（大驼峰）  
```csharp
public string UserName { get; set; }
```

### 事件（Event）

event Action 格式：`e_camelCase`（以 `e_` 前缀 + 小驼峰）, 其余使用Pascal格式
```csharp
public event Action e_onTakeDamage;
```
> **注3**：event Action 为项目常见成员, 特殊标记用以区别方便调试.

### 方法（Method）

统一使用 `PascalCase`（大驼峰）  
```csharp
public void GetUserInfo() { }
```

> **注4**：异步方法需要添加 `Async` 后缀（如 `LoadAsync`）.

### 参数（Parameter）

统一使用 `camelCase`（小驼峰）  
```csharp
public void SetName(string userName) { }
```

### 局部变量（Local Variable）

统一使用 `camelCase`（小驼峰）  
```csharp
var userCount = 10;
```

### 类、结构、接口、枚举

| 类型 | 命名风格 | 前缀/后缀建议 |
|------|----------|--------------|
| 类（Class） | `PascalCase` | 无 |
| 结构（Struct） | `PascalCase` | 无 |
| 接口（Interface） | `PascalCase` | 以 `I` 前缀（如 `IUserService`） |
| 枚举（Enum） | `PascalCase` | 无；枚举值同样 `PascalCase` |

> **注5**：泛型类型参数建议使用 `T` 或 `TKey`、`TValue` 等, 遵循 `PascalCase`.

#### 命名空间（Namespace）

统一使用 `PascalCase`, 按层级用点分隔  
```csharp
namespace GameScript.ModuleName.SubModule
```

## 代码布局与格式

- **花括号**：推荐换行.
  ```csharp
  if (condition)
  {
        // code
  }
  ```
  局部代码块只有一行可以忽略{}, 且可以不用换行
  ```csharp
  if (condition)
        return null;
  ```

  ```csharp
  foreach(var item in itemList) Handle(item);
  ```
- **空行**：类成员之间建议保留一个空行；不同逻辑块之间可加空行.
- **using 语句**：置于命名空间声明之外；按系统、第三方、内部顺序排列.
- **行数**：行数建议不要超过400行, 超过建议拆分

---

## UI命名前缀：

以下为 Unity UI 及常用组件的**前缀命名规范**, 均作为变量/字段名称的前缀使用（驼峰式, 首字母小写）.

| 组件类型 | 前缀 | 示例 |
|---------|------|------|
| UIView | `view` | `viewHeader` |
| Image/RawImage | `img` | `imgIcon` |
| Text/TextMeshPro | `txt` | `txtTitle` |
| Button | `btn` | `btnConfirm` |
| Transform | `tf` | `tfRoot` |
| RectTransform | `rect` | `rectPanel` |
| Animator / Animation | `anim` | `animFade` |
| CanvasGroup | `cg` | `cgPopup` |
| Canvas | `cv` | `cvMain` |
| ScrollRect | `scr` | `scrList` |
| Scrollbar | `scb` | `scbVertical` |
| Slider | `sld` | `sldVolume` |
| Toggle | `tog` | `togMusic` |
| InputField | `input` | `inputName` |
| Dropdown| `drop` | `dropLanguage` |
| Mask | `mask` | `maskAvatar` |
| HorizontalLayoutGroup | `hl` | `hlButtons` |
| VerticalLayoutGroup | `vl` | `vlItems` |
| GridLayoutGroup | `gl` | `glIcons` |


> **注6** 所有前缀均为**小写**, 与后续单词构成驼峰命名（如 `btnConfirm`）.  
> **注7** `UIView` 统一使用 `view` 前缀, 便于识别为局部视图模块.

# 文件路径

## Client

### Assets
  - Editor #``开发期间的配置文件``
  - Framework #``代码框架, 需用程序集定义``
  - GameAsset #``加载资源包, 路径下文件均以英文、snake格式命名``
    - Atlas #``图集``
    - Audio #``音频``
    - Prefab #``预制体``
      - UI #``ui预制体``
      - Vfx #``特效``
      - ...
    - Sprite #``精灵图``
        - UI
        - ...
    - Video #``视频``
    - ...
  - GameRaw #``资产原始文件, 路径下文件均以英文、snake格式命名``
    - Font #``字体``
    - Material #``材质``
    - Mesh #``模型``
    - Shader #``着色器``
    - Texture #``材质``
    - ...
  - GameConfig #``配置程序代码, 需用程序集定义``
    - Tables #``表格配置``
    - App #``软件设置相关``
  - GameScript #``脚本代码, 需用程序集定义``
  - Plugins #``第三方插件``
  - StreamingAssets
    - Tables
    - ...
  - Temp
  - Test
  - ...
### Library
### Logs
### obj
### CustomPackages #``第三方自定义包，便于修改代码查看索引``
### Packages
### ProjectSettings
### Temp
### UserSettings
### ...

## Config
### _Tables
### ...

# 资源命名规范

## 基础命名

### 前缀
  - 所有的资源都需要有前缀, 前缀的意义是方便检索和识别
  - 资源前缀保持在四个字母以内, 全小写, 下划线链接名称

### 名称
  - 必须是英文名
  - 全小写, snake格式

### 常见前缀.....
| 类型 | 前缀 | 示例 |
|---------|------|------|
|animation clip/controller|``a``|``a_player_walk``|0...
|audio|``sfx``|``sfx_on_click_confirm``|
|mesh|``mesh``|``mesh_player``|
|material|``mat``|``mat_ui_common``|
|prefab|``pr``|``pr_player``|
|prefab ui|``ui``|``ui_hud``|
|prefab vfx|``vfx``|``vfx_on_enemy_spawn``|
|shader|``s``|``s_dissolve``|
|sprite|``pic``|``pic_player``|
|sprite ui|``ui_xxx``|``ui_hud_hp_bar``|
|texture|``t``|``t_gound_grass``|
|timeline|``tl``|``tl_fire_ball_loop``|
|vidio|``vd``|``vd_on_boss1_challenged``|