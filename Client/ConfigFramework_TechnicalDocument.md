# 配置框架技术文档

**创建日期**: 2024年12月
**版本**: 1.0
**适用Unity版本**: 2021.3.37f1

## 概述

配置框架是一个基于CSV文件的通用配置管理解决方案，支持多种数据类型的动态解析和查询。框架采用单例模式设计，提供统一的配置加载、缓存和查询接口，适用于游戏中的各种配置数据管理需求。

## 系统架构

配置框架采用分层架构设计，包含以下核心组件：

```
ConfigManager (配置管理器)
    ↓
ConfigReader (配置读取器)
    ↓
ConfigDefinition (配置定义)
    ↓
CSV文件 (配置数据源)
```

- **ConfigManager**: 单例管理器，负责配置的加载、缓存和生命周期管理
- **ConfigReader**: 配置读取器，提供类型安全的数据查询接口
- **ConfigDefinition**: 配置定义解析器，解析CSV表头信息并验证数据类型
- **CSV文件**: 标准CSV格式的配置文件，支持注释和多行表头

## 文件清单

### 核心脚本（必需）
- `ConfigManager.cs` - 配置管理器，单例模式，负责配置加载和缓存
- `ConfigReader.cs` - 配置读取器，提供数据查询和类型转换功能
- `ConfigDefinition.cs` - 配置定义解析器，解析CSV表头并验证数据类型

### 示例脚本
- `Example/ConfigExample.cs` - 使用示例，展示基本用法、高级功能和验证处理

### 配置文件
- `Assets/Resources/Configs/item.csv` - 示例配置文件，展示CSV格式规范

## 核心组件设计

### ConfigManager（配置管理器）

**用途**: 配置框架的核心管理器，采用单例模式
**使用方式**: 纯C#类，无需挂载到GameObject

**主要功能**:
- 配置加载：从Resources目录加载CSV配置文件
- 配置缓存：缓存已加载的配置，避免重复加载
- 配置查询：提供统一的配置读取器获取接口
- 配置清理：支持清理单个或所有配置
- 状态检查：提供配置加载状态和数量查询

**关键方法**:
```csharp
// 加载配置
bool LoadConfig(string configName, string path)

// 获取配置读取器
ConfigReader GetReader(string configName)

// 检查配置是否已加载
bool IsConfigLoaded(string configName)

// 获取已加载配置数量
int GetLoadedConfigCount()

// 清理配置
void ClearConfig(string configName)
void ClearAllConfigs()
```

### ConfigReader（配置读取器）

**用途**: 提供类型安全的数据查询接口
**使用方式**: 纯C#类，由ConfigManager创建和管理

**主要功能**:
- 数据解析：解析CSV数据行并建立ID索引
- 类型转换：支持多种数据类型的自动转换
- 数据查询：提供泛型查询接口，支持默认值
- 批量查询：支持获取所有配置ID
- 数据验证：检查ID存在性和数据完整性

**支持的数据类型**:
- 基础类型：`int`, `float`, `string`, `bool`
- 数组类型：`int[]`, `float[]`, `string[]`
- 枚举类型：`enum<Type>`
- Unity类型：`Vector2`, `Vector3`, `Color`

**关键方法**:
```csharp
// 获取配置值
T GetValue<T>(int id, string columnName, T defaultValue = default)

// 获取所有ID
IEnumerable<int> GetAllIds()

// 获取配置条目数量
int GetEntryCount()

// 检查ID是否存在
bool HasId(int id)
```

### ConfigDefinition（配置定义）

**用途**: 解析CSV表头信息并验证数据类型
**使用方式**: 纯C#类，由ConfigManager内部使用

**主要功能**:
- 表头解析：解析CSV的表头行和类型行
- 类型验证：验证支持的数据类型
- 列管理：管理列名、列索引和列类型
- 格式验证：确保CSV格式的正确性
- 列信息查询：提供列名和类型查询功能

**关键方法**:
```csharp
// 解析配置定义
bool Parse(string[] headers, string[] types)

// 获取列信息
int GetColumnIndex(string columnName)
string GetColumnType(string columnName)
int GetColumnCount()

// 获取所有列名
string[] GetColumnNames()

// 检查列是否存在
bool HasColumn(string columnName)
```

## 具体实现

### CSV文件格式规范

配置文件采用标准CSV格式，包含以下行：

1. **表头行**: 列名定义，如 `Id,Name,Type,Attack,Defense`
2. **类型行**: 数据类型定义，如 `int,string,enum<ItemType>,int,int`
3. **注释行**: 中文说明，如 `编号,名称,类型,攻击力,防御力`
4. **数据行**: 实际配置数据

**示例文件** (`Assets/Resources/Configs/item.csv`):
```csv
Id,Name,Type,Attack,Defense,Params,Position,Color
int,string,enum<ItemType>,int,int,int[],vector3,color
编号,名称,类型,攻击力,防御力,参数组,位置,颜色
1,铁剑,1,10,5,1|2|3,0|0|0,#FF0000
2,木剑,1,8,3,2|3|4,0|1|0,#00FF00
3,布甲,2,0,10,5|6|7,1|1|0,#0000FF
```

### 数据类型支持

#### 基础类型
- `int`: 整数类型
- `float`: 浮点数类型
- `string`: 字符串类型
- `bool`: 布尔类型

#### 数组类型
- `int[]`: 整数数组，使用 `|` 分隔，如 `1|2|3`
- `float[]`: 浮点数数组，使用 `|` 分隔，如 `1.5|2.5|3.5`
- `string[]`: 字符串数组，使用 `|` 分隔，如 `a|b|c`

#### 枚举类型
- `enum<Type>`: 枚举类型，如 `enum<ItemType>`
- 枚举值使用数字表示，如 `1` 表示 `Weapon`

#### Unity类型
- `vector2`: Vector2类型，使用 `|` 分隔，如 `1.0|2.0`
- `vector3`: Vector3类型，使用 `|` 分隔，如 `1.0|2.0|3.0`
- `color`: Color类型，支持十六进制格式 `#FF0000` 或RGBA格式 `1.0|0.0|0.0|1.0`

## 使用示例

### 基本使用流程

```csharp
// 1. 加载配置
if (!ConfigManager.Instance.LoadConfig("Item", "Configs/item"))
{
    Debug.LogError("Failed to load item config");
    return;
}

// 2. 获取配置读取器
var reader = ConfigManager.Instance.GetReader("Item");
if (reader == null)
{
    Debug.LogError("Failed to get item config reader");
    return;
}

// 3. 查询单个配置
var id = 1;
var name = reader.GetValue<string>(id, "Name");
var type = reader.GetValue<ItemType>(id, "Type");
var attack = reader.GetValue<int>(id, "Attack");
var defense = reader.GetValue<int>(id, "Defense");

// 4. 查询复杂类型
var parameters = reader.GetValue<int[]>(id, "Params");
var position = reader.GetValue<Vector3>(id, "Position");
var color = reader.GetValue<Color>(id, "Color");

// 5. 遍历所有配置
foreach (var itemId in reader.GetAllIds())
{
    var itemName = reader.GetValue<string>(itemId, "Name");
    var itemType = reader.GetValue<ItemType>(itemId, "Type");
    Debug.Log($"- [{itemType}] {itemName}");
}

// 6. 错误处理和验证
var nonExistentId = 999;
var defaultName = reader.GetValue<string>(nonExistentId, "Name", "Unknown Item");
var defaultAttack = reader.GetValue<int>(nonExistentId, "Attack", 0);

Debug.Log($"Non-existent item (ID: {nonExistentId}): {defaultName}, Attack: {defaultAttack}");

// 7. 检查配置状态
Debug.Log($"Is Item config loaded: {ConfigManager.Instance.IsConfigLoaded("Item")}");
Debug.Log($"Loaded config count: {ConfigManager.Instance.GetLoadedConfigCount()}");

// 8. 清理配置
ConfigManager.Instance.ClearConfig("Item");
```

### 错误处理和验证

```csharp
// 使用默认值处理查询失败
var attack = reader.GetValue<int>(id, "Attack", 0); // 默认值为0
var name = reader.GetValue<string>(id, "Name", "Unknown"); // 默认值为"Unknown"

// 检查配置是否加载成功
if (!ConfigManager.Instance.LoadConfig("Item", "Configs/item"))
{
    // 处理加载失败
    return;
}

// 检查配置状态
if (ConfigManager.Instance.IsConfigLoaded("Item"))
{
    Debug.Log("Item config is loaded");
}

// 获取配置统计信息
Debug.Log($"Loaded configs: {ConfigManager.Instance.GetLoadedConfigCount()}");
Debug.Log($"Item entries: {reader.GetEntryCount()}");
```

### 高级功能示例

```csharp
// 加载多个配置
var configs = new[] { "Item", "Monster", "Building" };
foreach (var configName in configs)
{
    var path = $"Configs/{configName.ToLower()}";
    ConfigManager.Instance.LoadConfig(configName, path);
}

// 批量处理配置
foreach (var configName in configs)
{
    var reader = ConfigManager.Instance.GetReader(configName);
    if (reader != null)
    {
        Debug.Log($"Processing {configName}: {reader.GetEntryCount()} entries");
        // 处理配置数据
    }
}

// 清理所有配置
ConfigManager.Instance.ClearAllConfigs();
```

## 注意事项

### 文件路径规范
- 配置文件必须放在 `Assets/Resources/` 目录下
- 路径参数不包含文件扩展名，如 `"Configs/item"`
- 确保CSV文件格式正确，包含必要的表头行

### 数据类型限制
- 枚举类型必须在 `Enums.cs` 中定义
- 数组元素使用 `|` 分隔符，不支持嵌套数组
- Unity类型需要按照指定格式输入

### 错误处理
- 始终检查配置加载的返回值
- 使用默认值处理查询失败的情况
- 记录配置加载和查询的错误信息

## 总结

配置框架提供了一个简单、高效、类型安全的CSV配置管理解决方案。通过统一的接口和灵活的数据类型支持，可以满足游戏中各种配置数据的管理需求。框架的设计遵循了Unity开发的最佳实践，具有良好的性能和扩展性。 