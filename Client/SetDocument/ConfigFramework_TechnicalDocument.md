**创建日期**: 2024年12月
**版本**: 1.0
**文档类型**: 技术实现指导文档 

# 配置框架技术文档

## 简介
配置框架是一个基于CSV文件的通用配置管理系统，提供类型安全的数据查询接口，支持基础类型、数组、枚举和Unity类型的动态解析。

## 详细接口

### ConfigManager.Instance
```csharp
// 加载配置
bool LoadConfig(string configName, string path)

// 获取配置读取器
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
// 查询配置值
T GetValue<T>(int id, string columnName, T defaultValue = default)

// 获取所有ID
IEnumerable<int> GetAllIds()

// 数据验证
int GetEntryCount()
bool HasId(int id)
```

## 最佳实践

### 1. 配置加载
```csharp
// 游戏启动时加载所有配置
void Start()
{
    var configs = new[] { "Item", "Monster", "Building" };
    foreach (var configName in configs)
    {
        var path = $"Configs/{configName.ToLower()}";
        if (!ConfigManager.Instance.LoadConfig(configName, path))
        {
            Debug.LogError($"Failed to load {configName} config");
        }
    }
}
```

### 2. 数据查询
```csharp
// 使用默认值处理查询失败
var reader = ConfigManager.Instance.GetReader("Item");
var attack = reader.GetValue<int>(itemId, "Attack", 0);
var name = reader.GetValue<string>(itemId, "Name", "Unknown");
```

### 3. 批量处理
```csharp
// 遍历所有配置
var reader = ConfigManager.Instance.GetReader("Item");
foreach (var id in reader.GetAllIds())
{
    var name = reader.GetValue<string>(id, "Name");
    var type = reader.GetValue<ItemType>(id, "Type");
    // 处理数据
}
```

### 4. 错误处理
```csharp
// 检查配置状态
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
```

## 支持的数据类型

### 基础类型
- `int`, `float`, `string`, `bool`

### 数组类型
- `int[]`, `float[]`, `string[]` (使用 `|` 分隔)

### 枚举类型
- `enum<Type>` (枚举值用数字表示)

### Unity类型
- `Vector2`, `Vector3`, `Color`

## CSV文件格式
```csv
Id,Name,Type,Attack,Defense
int,string,enum<ItemType>,int,int
编号,名称,类型,攻击力,防御力
1,铁剑,1,10,5
2,木剑,1,8,3
```

## 注意事项
- 配置文件必须放在 `Assets/Resources/` 目录下
- 路径参数不包含文件扩展名
- 枚举类型必须在 `Enums.cs` 中定义
- 始终使用默认值处理查询失败
