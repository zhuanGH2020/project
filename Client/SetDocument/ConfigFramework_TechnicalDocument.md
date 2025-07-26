**创建日期**: 2024年12月
**版本**: 1.3
**文档类型**: 技术实现指导文档 

# 配置框架技术文档

## 简介
配置框架是一个基于CSV文件的通用配置管理系统，提供类型安全的数据查询接口，支持基础类型、数组、枚举和Unity类型的动态解析。系统支持自动加载配置，简化使用流程。

## 详细接口

### ConfigManager.Instance
```csharp
// 手动加载配置（可选，一般不需要手动调用）
bool LoadConfig(string configName)

// 获取配置读取器（如果配置未加载会自动加载）
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
// 查询配置值，支持所有类型包括枚举和枚举数组
T GetValue<T>(int id, string columnName, T defaultValue = default)

// 获取所有ID
IEnumerable<int> GetAllIds()

// 数据验证
int GetEntryCount()
bool HasId(int id)
```

## 最佳实践

### 1. 配置使用
```csharp
// 直接获取配置读取器，系统会自动加载配置
var reader = ConfigManager.Instance.GetReader("Tool");
if (reader != null)
{
    var name = reader.GetValue<string>(toolId, "Name", "Unknown");
    var type = reader.GetValue<ToolType>(toolId, "Type", ToolType.None);
}
```

### 2. 配置文件结构
配置文件必须包含至少4行：
```csv
Id,Name,Type,Value    # 第1行：列名
int,string,enum<ItemType>,int    # 第2行：类型定义
编号,名称,类型,数值    # 第3行：中文说明（可选）
1,Item1,Weapon,100    # 第4行开始：数据行
```

### 3. 配置路径规则
- 所有配置文件必须放在 `Resources/Configs/` 目录下
- 配置文件名必须与 configName 完全一致（区分大小写）
- 不需要包含文件扩展名（.csv）

## 支持的数据类型

### 基础类型
- `int`, `float`, `string`, `bool`

### 数组类型
- `int[]`, `float[]`, `string[]` (使用 `|` 分隔)
- `enum<Type>[]` (枚举数组，使用 `|` 分隔，值用枚举名称表示)

### 枚举类型
- `enum<Type>` (枚举值用名称表示)

### Unity类型
- `Vector2`, `Vector3`, `Color`

## 注意事项
- 配置文件必须包含 Id 列
- 枚举类型必须在 `Enums.cs` 中定义
- 枚举值使用名称表示，不区分大小写
- 数组类型使用竖线(|)分隔元素
- 配置文件编码必须是GB2312
