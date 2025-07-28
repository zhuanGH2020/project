using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 配置管理器
/// 提供CSV配置文件的加载、缓存和查询功能
/// 配置文件必须放在Resources/Configs/目录下，文件名必须与configName完全一致
/// </summary>
public class ConfigManager
{
    private static ConfigManager _instance;
    
    /// <summary>
    /// 单例实例
    /// </summary>
    public static ConfigManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ConfigManager();
            return _instance;
        }
    }

    /// <summary>
    /// 存储已加载的配置读取器
    /// </summary>
    private Dictionary<string, ConfigReader> _configs = new Dictionary<string, ConfigReader>();

    /// <summary>
    /// 私有构造函数，确保单例模式
    /// </summary>
    private ConfigManager() { }

    /// <summary>
    /// 加载配置文件
    /// 配置文件必须包含至少4行：列名、类型定义、中文说明和数据行
    /// </summary>
    /// <param name="configName">配置名称，必须与Resources/Configs/下的文件名完全一致（不含扩展名）</param>
    /// <returns>加载是否成功</returns>
    public bool LoadConfig(string configName)
    {
        if (string.IsNullOrEmpty(configName))
        {
            Debug.LogError("Config name cannot be null or empty");
            return false;
        }

        // Skip if already loaded
        if (_configs.ContainsKey(configName))
        {
            return true;
        }

        // Load CSV text asset
        var path = $"Configs/{configName}";
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load config file: {path}.csv");
            return false;
        }

        // Split lines and validate format
        var lines = textAsset.text.Split('\n');
        if (lines == null || lines.Length < 4)
        {
            Debug.LogError($"Invalid config file format: {path}.csv - requires at least 4 lines (headers, types, comments, data)");
            return false;
        }

        // Parse configuration definition
        var definition = new ConfigDefinition();
        var headers = ParseLine(lines[0]);
        var types = ParseLine(lines[1]);

        if (headers == null || types == null || headers.Length == 0 || types.Length == 0)
        {
            Debug.LogError($"Invalid headers or types in config file: {path}.csv");
            return false;
        }

        if (!definition.Parse(headers, types))
        {
            Debug.LogError($"Failed to parse config definition: {path}.csv");
            return false;
        }

        // Create and initialize config reader
        var reader = new ConfigReader(definition);

        // Load data (skip first 3 lines: headers, types, comments)
        var dataLines = new string[lines.Length - 3];
        System.Array.Copy(lines, 3, dataLines, 0, dataLines.Length);
        if (!reader.LoadData(dataLines))
        {
            Debug.LogError($"Failed to load config data: {path}.csv");
            return false;
        }

        // Store config reader
        _configs[configName] = reader;
        return true;
    }

    /// <summary>
    /// 获取配置读取器
    /// 如果配置未加载，会自动尝试加载
    /// </summary>
    /// <param name="configName">配置名称</param>
    /// <returns>配置读取器实例，如果加载失败则返回null</returns>
    public ConfigReader GetReader(string configName)
    {
        if (string.IsNullOrEmpty(configName))
        {
            Debug.LogError("Config name cannot be null or empty");
            return null;
        }

        if (_configs.TryGetValue(configName, out var reader))
        {
            return reader;
        }
        
        if (LoadConfig(configName))
        {
            return _configs[configName];
        }

        Debug.LogWarning($"Config '{configName}' not found and auto-load failed.");
        return null;
    }

    /// <summary>
    /// 检查配置是否已加载
    /// </summary>
    /// <param name="configName">配置名称</param>
    /// <returns>配置是否已加载</returns>
    public bool IsConfigLoaded(string configName)
    {
        if (string.IsNullOrEmpty(configName))
        {
            Debug.LogError("Config name cannot be null or empty");
            return false;
        }
        return _configs.ContainsKey(configName);
    }

    /// <summary>
    /// 清理指定配置
    /// </summary>
    /// <param name="configName">要清理的配置名称</param>
    public void ClearConfig(string configName)
    {
        if (string.IsNullOrEmpty(configName))
        {
            Debug.LogError("Config name cannot be null or empty");
            return;
        }

        if (_configs.Remove(configName))
        {
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// 清理所有已加载的配置
    /// </summary>
    public void ClearAllConfigs()
    {
        _configs.Clear();
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 获取已加载的配置数量
    /// </summary>
    /// <returns>已加载的配置数量</returns>
    public int GetLoadedConfigCount()
    {
        return _configs.Count;
    }

    /// <summary>
    /// 解析CSV行为字符串数组
    /// </summary>
    /// <param name="line">要解析的CSV行</param>
    /// <returns>解析后的字符串数组</returns>
    private string[] ParseLine(string line)
    {
        if (string.IsNullOrEmpty(line))
            return null;
            
        return line.Trim().TrimEnd('\r').Split(',');
    }
} 