using UnityEngine;
using System.Linq;

/// <summary>
/// 配置系统使用示例
/// </summary>
public static class ConfigExample
{
    /// <summary>
    /// 基础配置使用示例
    /// </summary>
    public static void Example()
    {
        Debug.Log("=== 配置系统示例 ===");

        // 1. 获取配置读取器（会自动加载配置）
        var reader = ConfigManager.Instance.GetReader("ToolTarget");
        if (reader == null)
        {
            Debug.LogError("Failed to get tool target config reader");
            return;
        }

        Debug.Log($"加载了 {reader.GetEntryCount()} 个配置项");

        // // 2. 查询单个配置项（使用枚举作为key）
        // var key = ToolType.Axe;
        // var targetTypes = reader.GetValue<SourceType[]>(key, "SourceTypes");
        // if (targetTypes != null)
        // {
        //     Debug.Log($"工具 {key} 可作用目标: {string.Join(", ", targetTypes)}");
        // }

        // // 3. 遍历所有配置
        // Debug.Log("所有配置:");
        // // 获取指定类型的key
        // foreach (var toolType in reader.GetAllKeysOfType<ToolType>())
        // {
        //     var types = reader.GetValue<SourceType[]>(toolType, "SourceTypes");
        //     Debug.Log($"- [{toolType}] 可作用目标: {string.Join(", ", types ?? new SourceType[0])}");
        // }

        // // 4. 演示错误处理和默认值
        // var nonExistentKey = ToolType.None;
        // var defaultTypes = reader.GetValue<SourceType[]>(nonExistentKey, "SourceTypes", new SourceType[0]);
        // Debug.Log($"不存在的配置 (Key: {nonExistentKey}): {string.Join(", ", defaultTypes)}");

        Debug.Log("=== 示例完成 ===");
    }

    /// <summary>
    /// 高级配置功能示例
    /// </summary>
    public static void AdvancedExample()
    {
        Debug.Log("=== 高级配置示例 ===");

        // 1. 使用数字作为key
        var toolReader = ConfigManager.Instance.GetReader("Tool");
        if (toolReader != null)
        {
            var toolId = 1001;
            var toolName = toolReader.GetValue<string>(toolId, "Name");
            Debug.Log($"工具 {toolId}: {toolName}");

            // 遍历所有数字key
            foreach (var id in toolReader.GetAllKeysOfType<int>())
            {
                var name = toolReader.GetValue<string>(id, "Name");
                Debug.Log($"ID {id}: {name}");
            }
        }

        // 2. 使用字符串作为key - 示例已注释避免查询不存在的数据
        var itemReader = ConfigManager.Instance.GetReader("Item");
        if (itemReader != null)
        {
            // 注释掉可能不存在的测试key
            // var itemKey = "WOOD";
            // var itemName = itemReader.GetValue<string>(itemKey, "Name");
            // Debug.Log($"物品 {itemKey}: {itemName}");

            // 遍历实际存在的字符串key
            foreach (var key in itemReader.GetAllKeysOfType<string>())
            {
                var name = itemReader.GetValue<string>(key, "Name");
                Debug.Log($"Key {key}: {name}");
            }
        }

        Debug.Log("=== 高级示例完成 ===");
    }

    /// <summary>
    /// 数据验证和错误处理示例 - 已禁用避免产生警告日志
    /// </summary>
    public static void ValidationExample()
    {
        Debug.Log("=== 验证示例 (已禁用) ===");

        // 注释掉测试代码以避免产生警告日志
        // 如需测试配置系统错误处理，可以取消注释以下代码：
        
        /*
        // 加载有效配置并测试数据验证
        var reader = ConfigManager.Instance.GetReader("Tool");
        if (reader != null)
        {
            // 测试无效key
            var invalidKey = 999;
            var name = reader.GetValue<string>(invalidKey, "Name", "默认名称");
            Debug.Log($"无效key结果: {name}");

            // 测试无效列
            var validKey = reader.GetAllKeys().First();
            var invalidColumn = reader.GetValue<string>(validKey, "InvalidColumn", "默认值");
            Debug.Log($"无效列结果: {invalidColumn}");

            ConfigManager.Instance.ClearConfig("Tool");
        }
        */

        Debug.Log("=== 验证示例完成 ===");
    }
} 