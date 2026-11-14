## 命名规范

### 字段 (Field)

| 访问级别 / 修饰符 | 命名风格 | 示例 |
|---|---|---|
| `private`, `protected`, `protected internal`, `private protected` | `_camelCase`, 以下划线开头的小驼峰 | `_userName` |
| 其他, 如 `internal`, `public` | `camelCase`, 小驼峰 | `userName` |
| 值类型(string除外)的 `const`, `static readonly` | `ALL_UPPER_SNAKE`, 全大写, 下划线分隔 | `MAX_RETRY_COUNT` |
| 标记为 `[SerializeField]` | `camelCase`, 不加下划线 | `btnCancel` |

命名规则按以下优先级应用:

1. 标记为 `[SerializeField]` 的字段统一使用 `camelCase`, 不加下划线; UI 字段保留组件前缀.
2. 值类型(string除外)的 `const` 和 `static readonly` 字段使用 `ALL_UPPER_SNAKE`, 优先于访问级别规则.
3. 其他字段按访问级别命名.

补充说明:

- 上述值类型规则是命名约定, 不表示 C# 对字段类型的限制. `const` 可以用于字符串, `static readonly` 可以用于引用类型.
- 引用类型的 `const` 和 `static readonly` 字段按访问级别命名.
- 仅有 `static` 或仅有 `readonly` 修饰时, 仍使用普通字段规则.
- 所有实例字段必须明确访问修饰符, 避免隐式 `private`.

```csharp
private string _userName;
protected int _retryCount;
protected internal int _sharedCount;
private protected int _stateId;
public int userCount;
internal static int instanceCount;
private readonly List<int> _itemIds = new();
private const int MAX_RETRY_COUNT = 3;
private const string _defaultUserName = "Guest";
private static readonly Vector3 DEFAULT_POSITION = Vector3.zero;
private static readonly List<int> _defaultItemIds = new();

[SerializeField] private Button btnCancel;
[SerializeField] private int maxRetryCount;
```

### 属性 (Property)

统一使用 `PascalCase`, 大驼峰.

```csharp
public string UserName { get; set; }
```

### 事件 (Event)

- `event Action` (包括 `Action<T>` 等泛型形式) 使用 `e_camelCase`, 即 `e_` 前缀加小驼峰.
- 其他事件使用 `PascalCase`.
- 此规则针对事件成员; 普通 `Action` 字段仍遵循字段命名规则.
- `event Action` 是项目常见成员, 特殊前缀便于识别和调试.

```csharp
public event Action e_onTakeDamage;
public event Action<int> e_onHealthChanged;
public event EventHandler StateChanged;
```

### 方法 (Method)

统一使用 `PascalCase`; 异步方法必须添加 `Async` 后缀.

```csharp
public void GetUserInfo() { }
public Task LoadAsync() { return Task.CompletedTask; }
```

### 参数 (Parameter)

统一使用 `camelCase`.

```csharp
public void SetName(string userName) { }
```

### 局部变量 (Local Variable)

统一使用 `camelCase`.

```csharp
var userCount = 10;
```

### 类, 结构, 接口, 枚举

| 类型 | 命名风格 | 前缀 / 后缀建议 |
|---|---|---|
| 类 (Class) | `PascalCase` | 无 |
| 结构 (Struct) | `PascalCase` | 无 |
| 接口 (Interface) | `PascalCase` | 使用 `I` 前缀, 如 `IUserService` |
| 枚举 (Enum) | `PascalCase` | 无; 枚举值同样使用 `PascalCase` |

泛型类型参数建议使用 `T`, `TKey`, `TValue` 等, 遵循 `PascalCase`.

### 命名空间 (Namespace)

统一使用 `PascalCase`, 按层级用点分隔.

```csharp
namespace GameScript.ModuleName.SubModule
{
}
```

## 代码布局与格式

### 成员顺序

先按特殊修饰符分组: `const` 成员置顶, `static` 成员紧随其后, 其他成员在后. 先按成员类别排序, 再按访问级别排序, 如下:

1. 属性/字段.
2. 构造函数.
3. 析构函数.
4. 抽象方法.
5. 虚方法.
6. 普通方法.
7. 父类重写方法.
8. 接口实现方法.
9. 嵌套类.

各组内的访问级别顺序:
1. public.
2. internal.
3. protected internal.
4. protected.
5. private protected.
6. private.

事件在上述列表中未指定单独位置, 可与相关成员就近组织, 保持所在类内一致.

注意: 特殊修饰符分组优先于访问级别和成员类别排序. 不要用region分割区域. region仅能用于功能区分

### 缩进与空格

- 使用 4 个空格缩进, 不使用 Tab.
- 逗号和分号后接同一行内容时, 保留一个空格.
- 括号内侧不加空格.
- 控制流关键字与左括号之间保留一个空格, 如 `if (condition)`.
- 方法名与左括号之间不加空格, 如 `Handle(item)`.
- C# 方法声明和方法调用的参数列表必须保持在同一行, 不因参数数量或行长度换行.
- 二元运算符两侧保留空格, 如 `count = total + 1`.

### 花括号

推荐花括号换行. 

```csharp
if (condition)
{
    Handle();
}
```

仅return时, 省略花括号且压行.

```csharp

if (condition) return null;

```

### 空行, using 与行数

- 类成员之间建议保留一个空行; 不同逻辑块之间可加空行.
- `using` 语句置于命名空间声明之外, 按系统, 第三方, 内部顺序排列.
- 单个代码文件建议不超过 400 行; 超过时建议按职责拆分.

### 边界与禁用

- 纯数值运算指直接对已有数值进行比较, 选择或算术运算, 不调用方法或其他外部逻辑. 三目运算符仅允许用于纯数值运算.
- 如果有非纯数值运算三目运算符, 必须换行;
- 三目运算符禁止嵌套

```csharp

var result = isSuccess ? value++ : value--;
var dmg = isCritical 
        ? DealNormal() 
        : DealCrit() ;

```

- 尽量减少if嵌套, 如果嵌套过深, 建议使用return提前退出, 或者将if逻辑拆分成方法.
- `++` 和 `--` 运算符仅允许用于纯数值运算.
- 方法行数尽量不要超过30行(包括存在局部方法的情况).
- 仅当同时满足职责单一, 用于复用所属方法内的重复逻辑, 且不捕获 `this` 或其他外部实例时, 才使用局部方法; 否则提取为类方法.

## UI 命名前缀

以下前缀用于 Unity UI 及常用组件的变量 / 字段名称. 前缀全部小写, 与后续单词组成驼峰名称.

| 组件类型 | 前缀 | 示例 |
|---|---|---|
| Image / RawImage | `img` | `imgIcon` |
| Text / TextMeshPro / TMP_Text | `txt` | `txtTitle` |
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
| InputField / TMP_InputField | `input` | `inputName` |
| Dropdown / TMP_Dropdown | `drop` | `dropLanguage` |
| Mask | `mask` | `maskAvatar` |
| HorizontalLayoutGroup | `hl` | `hlButtons` |
| VerticalLayoutGroup | `vl` | `vlItems` |
| GridLayoutGroup | `gl` | `glIcons` |

- UI 前缀与字段命名规则组合使用: 普通私有字段为 `_btnConfirm`, 公开字段为 `btnConfirm`, 局部变量为 `btnConfirm`.
- 标记为 `[SerializeField]` 的字段优先使用 `camelCase`, 不加下划线, 如 `[SerializeField] private Button btnCancel;`.
- 属性仍使用 `PascalCase`, 如 `BtnConfirm`.

## 注释

新增或修改的成员按以下规则添加注释, 重点说明用途, 单位, 边界和特殊行为. 局部变量及直观语句无需逐行注释.

注释内容使用中文汉字或者英文. 标点统一使用英文字符和英文标点习惯, 不使用 emoji, 颜文字等可读性差的符号.

### 字段, 属性

- `private`, `protected`, `protected internal`, `private protected` 成员的注释放在其后, 如:

```csharp
private int _configId; // 配置表 ID.
```

- 其他访问等级使用单行 XML 注释, 如:

```csharp
/// <summary> 配置表 ID. </summary>
public int ConfigId { get; private set; }
```

### 方法

- `private`, `protected`, `protected internal`, `private protected` 方法的注释放在其上, 通常不逐项解释参数. 参数存在特殊单位或约束时, 在注释中说明.这些注释如果命名足够明确规范, 可以省略

```csharp
// 生成新的敌人.
private void SpawnNewEnemy(int id)
{
}
```

- 其他访问等级使用单行 XML 注释, 非必要不做参数解释, 如:

```csharp
/// <summary> 生成新的敌人. </summary>
public void SpawnNewEnemy(int id)
{
}
```

### 其他备注

其他格式备注按常见备注方式处理.

示例:
```csharp
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GameScript.Examples
{
    /// <summary> 用户面板格式示例. </summary>
    public class UserPanelExample : MonoBehaviour
    {
        /// <summary> 最大重试次数. </summary>
        public const int MAX_RETRY_COUNT = 3;

        protected const string DEFAULT_USER_NAME = "Guest"; // 默认用户名称.

        private static readonly Vector3 DEFAULT_POSITION = Vector3.zero; // 默认位置.

        /// <summary> 当前用户名称. </summary>
        public string UserName { get; private set; }

        /// <summary> 面板是否已打开. </summary>
        public bool IsOpen { get; private set; }

        /// <summary> 用户配置 ID. </summary>
        public int configId;

        /// <summary> 确认操作时触发. </summary>
        public event Action e_onConfirm;

        /// <summary> 用户名称变化时触发. </summary>
        public event Action<string> e_onUserNameChanged;

        /// <summary> 打开面板并设置用户名称. </summary>
        public void Open(string userName)
        {
            var displayName = string.IsNullOrEmpty(userName) ? DEFAULT_USER_NAME : userName;

            UserName = displayName;
            IsOpen = true;

            RefreshTitle(displayName);
            e_onUserNameChanged?.Invoke(displayName);
        }

        /// <summary> 加载面板数据. </summary>
        public async Task LoadAsync()
        {
            if (_isLoading) return;

            _isLoading = true;

            try
            {
                await Task.CompletedTask;
            }
            finally
            {
                _isLoading = false;
            }
        }

        protected int _retryCount; // 当前重试次数.

        // 处理面板关闭后的扩展行为.
        protected virtual void OnClosed()
        {
        }

        [SerializeField] private Button btnConfirm; // 确认按钮.

        [SerializeField] private Text txtTitle; // 标题文本.

        [SerializeField] private RectTransform rectPanel; // 面板节点.

        private bool _isLoading; // 是否正在加载.

        // 刷新面板标题.
        private void RefreshTitle(string userName)
        {
            txtTitle.text = userName;
        }

        // 处理确认按钮点击.
        private void OnConfirmClicked()
        {
            e_onConfirm?.Invoke();
        }
    }
}
```