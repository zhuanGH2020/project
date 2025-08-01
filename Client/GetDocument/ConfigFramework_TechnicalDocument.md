# Config Framework 接口文档

## 概述

Config Framework 是一个基于CSV配置文件的数据管理系统，提供类型安全的配置数据查询功能。

## 核心接口

### ConfigManager - 配置管理器

```csharp
public class ConfigManager
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static ConfigManager Instance { get; }
    
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="configName">配置名称</param>
    /// <returns>加载是否成功</returns>
    public bool LoadConfig(string configName)
    
    /// <summary>
    /// 获取配置读取器（自动加载）
    /// </summary>
    /// <param name="configName">配置名称</param>
    /// <returns>配置读取器实例</returns>
    public ConfigReader GetReader(string configName)
    
    /// <summary>
    /// 检查配置是否已加载
    /// </summary>
    /// <param name="configName">配置名称</param>
    /// <returns>是否已加载</returns>
    public bool IsConfigLoaded(string configName)
    
    /// <summary>
    /// 清理指定配置
    /// </summary>
    /// <param name="configName">配置名称</param>
    public void ClearConfig(string configName)
    
    /// <summary>
    /// 清理所有配置
    /// </summary>
    public void ClearAllConfigs()
    
    /// <summary>
    /// 获取已加载配置数量
    /// </summary>
    /// <returns>配置数量</returns>
    public int GetLoadedConfigCount()
}
```

### ConfigReader - 配置读取器

```csharp
public class ConfigReader
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="key">主键</param>
    /// <param name="columnName">列名</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值</returns>
    public T GetValue<T>(object key, string columnName, T defaultValue = default)
    
    /// <summary>
    /// 获取所有键
    /// </summary>
    /// <returns>所有键的集合</returns>
    public IEnumerable<object> GetAllKeys()
    
    /// <summary>
    /// 获取指定类型的所有键
    /// </summary>
    /// <typeparam name="T">键类型</typeparam>
    /// <returns>指定类型的键集合</returns>
    public IEnumerable<T> GetAllKeysOfType<T>()
    
    /// <summary>
    /// 获取条目数量
    /// </summary>
    /// <returns>条目数量</returns>
    public int GetEntryCount()
    
    /// <summary>
    /// 检查键是否存在
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>是否存在</returns>
    public bool HasKey(object key)
}
```

## 支持的数据类型

### 基础类型
- `int` - 整数
- `float` - 浮点数
- `string` - 字符串
- `bool` - 布尔值

### 数组类型
- `int[]` - 整数数组，使用 `|` 分隔
- `float[]` - 浮点数数组，使用 `|` 分隔
- `string[]` - 字符串数组，使用 `|` 分隔

### 枚举类型
- `enum<EnumType>` - 单个枚举值
- `enum<EnumType>[]` - 枚举数组，使用 `|` 分隔

### Unity类型
- `vector2` - Vector2，格式：`x|y`
- `vector3` - Vector3，格式：`x|y|z`
- `color` - Color，支持HTML颜色格式（如 `#FF0000`）

## 配置文件格式

配置文件必须是CSV格式，包含以下结构：

```csv
列名1,列名2,列名3,列名4
int,string,enum<ToolType>,int[]
ID,名称,类型,属性数组
1001,斧头,Axe,10|20|30
1002,锤子,Hammer,15|25|35
```

**文件格式要求：**
1. 第1行：列名定义
2. 第2行：类型定义
3. 第3行：中文说明（注释行）
4. 第4行及以后：实际数据

**文件位置：**
- 配置文件必须放在 `Resources/Configs/` 目录下
- 文件名必须与配置名完全一致（不含扩展名）

## 使用示例

```csharp
// 1. 获取配置读取器（自动加载）
var reader = ConfigManager.Instance.GetReader("Tool");

// 2. 查询配置值
string toolName = reader.GetValue<string>(1001, "Name", "默认名称");
int[] attributes = reader.GetValue<int[]>(1001, "Attributes");
ToolType toolType = reader.GetValue<ToolType>(1001, "Type", ToolType.None);

// 3. 遍历所有配置
foreach (var key in reader.GetAllKeysOfType<int>())
{
    var name = reader.GetValue<string>(key, "Name");
    Debug.Log($"ID: {key}, 名称: {name}");
}

// 4. 检查键是否存在
if (reader.HasKey(1001))
{
    Debug.Log("配置项存在");
}
```
 