using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration manager for loading and managing CSV configuration files
/// Provides unified interface for config loading, caching and querying
/// </summary>
public class ConfigManager
{
    private static ConfigManager _instance;
    
    /// <summary>
    /// Singleton instance of ConfigManager
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
    /// Dictionary to store loaded configuration readers
    /// </summary>
    private Dictionary<string, ConfigReader> _configs = new Dictionary<string, ConfigReader>();

    /// <summary>
    /// Private constructor for singleton pattern
    /// </summary>
    private ConfigManager() { }

    /// <summary>
    /// Load configuration from CSV file in Resources folder
    /// </summary>
    /// <param name="configName">Unique name for the configuration</param>
    /// <param name="path">Path to CSV file in Resources folder (without extension)</param>
    /// <returns>True if loading successful, false otherwise</returns>
    public bool LoadConfig(string configName, string path)
    {
        // Skip if already loaded
        if (_configs.ContainsKey(configName))
        {
            Debug.Log($"Config '{configName}' already loaded");
            return true;
        }

        // Load CSV text asset
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load config file: {path}");
            return false;
        }

        // Split lines and validate format
        var lines = textAsset.text.Split('\n');
        if (lines.Length < 4)
        {
            Debug.LogError($"Invalid config file format: {path} - requires at least 4 lines (headers, types, comments, data)");
            return false;
        }

        // Parse configuration definition
        var definition = new ConfigDefinition();
        if (!definition.Parse(
            ParseLine(lines[0]),  // Headers
            ParseLine(lines[1])   // Types
        ))
        {
            Debug.LogError($"Failed to parse config definition: {path}");
            return false;
        }

        // Create and initialize config reader
        var reader = new ConfigReader(definition);

        // Load data (skip first 3 lines: headers, types, comments)
        var dataLines = new string[lines.Length - 3];
        System.Array.Copy(lines, 3, dataLines, 0, dataLines.Length);
        if (!reader.LoadData(dataLines))
        {
            Debug.LogError($"Failed to load config data: {path}");
            return false;
        }

        // Store config reader
        _configs[configName] = reader;
        Debug.Log($"Successfully loaded config '{configName}' from '{path}'");
        return true;
    }

    /// <summary>
    /// Get configuration reader by name
    /// </summary>
    /// <param name="configName">Name of the configuration</param>
    /// <returns>ConfigReader instance if found, null otherwise</returns>
    public ConfigReader GetReader(string configName)
    {
        if (_configs.TryGetValue(configName, out var reader))
        {
            return reader;
        }
        
        Debug.LogWarning($"Config '{configName}' not found. Make sure to load it first.");
        return null;
    }

    /// <summary>
    /// Check if configuration is loaded
    /// </summary>
    /// <param name="configName">Name of the configuration</param>
    /// <returns>True if configuration is loaded, false otherwise</returns>
    public bool IsConfigLoaded(string configName)
    {
        return _configs.ContainsKey(configName);
    }

    /// <summary>
    /// Clear specific configuration from memory
    /// </summary>
    /// <param name="configName">Name of the configuration to clear</param>
    public void ClearConfig(string configName)
    {
        if (_configs.Remove(configName))
        {
            Debug.Log($"Cleared config '{configName}'");
        }
        else
        {
            Debug.LogWarning($"Config '{configName}' not found for clearing");
        }
    }

    /// <summary>
    /// Clear all loaded configurations from memory
    /// </summary>
    public void ClearAllConfigs()
    {
        var count = _configs.Count;
        _configs.Clear();
        Debug.Log($"Cleared {count} loaded configurations");
    }

    /// <summary>
    /// Get count of loaded configurations
    /// </summary>
    /// <returns>Number of loaded configurations</returns>
    public int GetLoadedConfigCount()
    {
        return _configs.Count;
    }

    /// <summary>
    /// Parse CSV line into string array
    /// </summary>
    /// <param name="line">CSV line to parse</param>
    /// <returns>Array of string values</returns>
    private string[] ParseLine(string line)
    {
        return line.Trim().TrimEnd('\r').Split(',');
    }
} 