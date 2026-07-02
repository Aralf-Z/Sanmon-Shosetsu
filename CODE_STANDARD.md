# 代码风格

## 1.命名规范

### 字段（Field）

| 访问级别 / 修饰符 | 命名风格 | 示例 |
|-------------------|----------|------|
| `private`、`protected` | `_camelCase`（以下划线开头的小驼峰） | `_userName` |
| 其他（`internal`、`public` 等） | `camelCase`（小驼峰） | `userName` |
| `const`、`static readonly` | `ALL_UPPER_SNAKE`（全大写，下划线分隔） | `MAX_RETRY_COUNT` |
| `private static`、`protected static` | `camelCase`（小驼峰） | `defaultTimeout` |
| 其他（`internal static`、`public static` 等） | `PascalCase`（大驼峰） | `AppConfig` |

> **注1**：明确 `readonly`（非静态）建议遵循普通字段规则（`private`/`protected` 用 `_camel`，其他用 `camel`）。  
> **注2**：**所有实例字段应明确访问修饰符，避免隐式 `private`**。

### 属性（Property）

统一使用 `PascalCase`（大驼峰）  
```csharp
public string UserName { get; set; }
```

### 事件（Event）

格式：`Evt_PascalCase`（以 `Evt_` 前缀 + 大驼峰）  
```csharp
public event EventHandler Evt_DataLoaded;
```
> **注3**：若事件为委托类型，建议遵循相同命名，同时明确 `sender` 和 `args` 参数命名（见下文参数规范）。

### 方法（Method）

统一使用 `PascalCase`（大驼峰）  
```csharp
public void GetUserInfo() { }
```

> **注4**：异步方法建议添加 `Async` 后缀（如 `FetchDataAsync`），非强制但推荐。

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
| 接口（Interface） | `PascalCase` | 以 `I` 开头（如 `IUserService`） |
| 枚举（Enum） | `PascalCase` | 无；枚举值同样 `PascalCase` |

> **注5**：泛型类型参数建议使用 `T` 或 `TKey`、`TValue` 等，遵循 `PascalCase`。

#### 命名空间（Namespace）

统一使用 `PascalCase`，按层级用点分隔  
```csharp
namespace CompanyName.ModuleName.SubModule
```

## 2.代码布局与格式

- **缩进**：使用 Tab。
- **大括号**：推荐 **换行**：
  ```csharp
  if (condition)
  {
        // code
  }
  ```
  局部代码块只有一行可以忽略{}
  ```csharp
  if (condition)
        return null;
  ```
    ```csharp
  if (condition) return null;
  ```
- **空行**：类成员之间建议保留一个空行；不同逻辑块之间可加空行。
- **using 语句**：置于命名空间声明之外；按系统、第三方、内部顺序排列。

---

## 3. UI命名前缀：

以下为 Unity UI 及常用组件的**前缀命名规范**，均作为变量/字段名称的前缀使用（驼峰式，首字母小写）。

| 组件类型 | 前缀 | 示例 |
|---------|------|------|
| UIView | `view` | `viewHeader` |
| Image/RawImage | `img` | `imgIcon` |
| Text/TextMeshPro | `txt` | `txtTitle` |
| Button | `btn` | `btnConfirm` |
| Transform | `transf` | `transfRoot` |
| RectTransform | `rect` | `rectPanel` |
| Animator / Animation | `anim` | `animFade` |
| CanvasGroup | `cg` | `cgPopup` |
| Canvas | `cv` | `cvMain` |
| ScrollRect | `sr` | `srList` |
| Scrollbar | `srb` | `srbVertical` |
| Slider | `sld` | `sldVolume` |
| Toggle | `tog` | `togMusic` |
| InputField | `input` | `inputName` |
| Dropdown (UI) | `drop` | `dropLanguage` |
| Mask | `mask` | `maskAvatar` |
| ContentSizeFitter | `fitter` | `fitterContent` |
| HorizontalLayoutGroup | `hLayout` | `hLayoutButtons` |
| VerticalLayoutGroup | `vLayout` | `vLayoutItems` |
| GridLayoutGroup | `gridLayout` | `gridLayoutIcons` |


> **注6** 所有前缀均为**小写**，与后续单词构成驼峰命名（如 `btnConfirm`）。  
> **注7** `UIView` 统一使用 `view` 前缀，便于识别为局部视图模块。