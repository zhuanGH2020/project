**创建日期**: 2024年12月
**版本**: 2.0
**文档类型**: 技术实现指导文档 

# 配置框架技术文档

## 简介
配置框架是一个基于CSV文件的通用配置管理系统，提供类型安全的数据查询接口，支持基础类型、数组、枚举和Unity类型的动态解析。系统支持自动加载配置，简化使用流程。

## 详细接口

### ConfigManager.Instance
```csharp
// 加载配置文件（自动尝试从Resources/Configs/目录加载）
bool LoadConfig(string configName)

// 获取配置读取器（如果配置未加载会自动尝试加载）
ConfigReader GetReader(string configName)

// 检查配置状态
bool IsConfigLoaded(string configName)
int GetLoadedConfigCount()

// 清理配置
void ClearConfig(string configName)
void ClearAllConfigs()
```

### ConfigReader
```csharp
// 查询配置值，支持object类型的key
T GetValue<T>(object key, string columnName, T defaultValue = default)

// 获取所有键（支持多种类型的键）
IEnumerable<object> GetAllKeys()

// 获取指定类型的所有键
IEnumerable<T> GetAllKeysOfType<T>()

// 数据验证
int GetEntryCount()
bool HasKey(object key)
```

## 最佳实践

### 1. 基本使用
```csharp
// 直接获取配置读取器，系统会自动加载配置
var reader = ConfigManager.Instance.GetReader("Item");
if (reader != null)
{
    var name = reader.GetValue<string>(1, "Name", "Unknown");
    var type = reader.GetValue<ItemType>(1, "Type", ItemType.None);
    var attack = reader.GetValue<int>(1, "Attack", 0);
}
```

### 2. 遍历所有配置
```csharp
// 遍历所有键
var reader = ConfigManager.Instance.GetReader("Item");
foreach (var key in reader.GetAllKeys())
{
    var name = reader.GetValue<string>(key, "Name");
    Debug.Log($"Item {key}: {name}");
}

// 遍历指定类型的键
foreach (var intKey in reader.GetAllKeysOfType<int>())
{
    var itemName = reader.GetValue<string>(intKey, "Name");
    Debug.Log($"Item ID {intKey}: {itemName}");
}
```

### 3. 错误处理
```csharp
// 检查配置是否加载成功
if (!ConfigManager.Instance.IsConfigLoaded("Item"))
{
    Debug.LogError("Item config not loaded");
    return;
}

var reader = ConfigManager.Instance.GetReader("Item");
if (reader == null)
{
    Debug.LogError("Failed to get reader");
    return;
}

// 检查键是否存在
if (!reader.HasKey(itemId))
{
    Debug.LogWarning($"Item {itemId} not found");
    return;
}
```

### 4. 配置文件结构
配置文件必须包含至少4行：
```csv
Id,Name,Type,Attack,Defense    # 第1行：列名
int,string,enum<ItemType>,int,int    # 第2行：类型定义
编号,名称,类型,攻击力,防御力    # 第3行：中文说明
1,铁剑,Weapon,10,5    # 第4行开始：数据行
2,木盾,Shield,0,8
```

### 5. 配置路径规则
- 所有配置文件必须放在 `Resources/Configs/` 目录下
- 配置文件名必须与 configName 完全一致（区分大小写）
- 不需要包含文件扩展名（.csv）
- 调用 `GetReader("Item")` 会加载 `Resources/Configs/Item.csv`

## 支持的数据类型

### 基础类型
- `int`, `float`, `string`, `bool`

### 枚举类型
- `enum<Type>` (枚举值用名称表示，不区分大小写)

### 数组类型
- `int[]`, `float[]`, `string[]` (使用 `|` 分隔)
- `enum<Type>[]` (枚举数组，使用 `|` 分隔，值用枚举名称表示)

### Unity类型
- `Vector2` (格式：`x|y`)
- `Vector3` (格式：`x|y|z`) 
- `Color` (格式：HTML颜色代码，如 `#FF0000`)

## 配置文件格式要求
- **必须至少4行**：列名、类型定义、中文说明、数据行
- **第一列作为主键**：支持 int、string 或枚举类型
- **列名和类型定义行必须对应**：列数必须相等
- **数据行列数必须与定义一致**

## 注意事项
- 配置文件必须放在 `Resources/Configs/` 目录下
- 第一列会自动作为主键，支持int、string、enum类型
- 枚举类型必须在项目中已定义（通常在 `Enums.cs` 中）
- 枚举值使用名称表示，支持大小写不敏感匹配
- 数组和复合类型使用竖线(|)分隔元素
- 空值或解析失败时返回默认值
- 系统会自动缓存枚举解析结果以提升性能
