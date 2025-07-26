using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 配置读取器
/// </summary>
public class ConfigReader
{
    private Dictionary<object, Dictionary<string, string>> _configData;
    private ConfigDefinition _definition;
    private Dictionary<Type, Dictionary<string, object>> _enumCache = new Dictionary<Type, Dictionary<string, object>>();

    public ConfigReader(ConfigDefinition definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _configData = new Dictionary<object, Dictionary<string, string>>();
    }

    /// <summary>
    /// 加载配置数据
    /// </summary>
    public bool LoadData(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("Data lines array is null or empty");
            return false;
        }

        _configData.Clear();
        var columnNames = _definition.GetColumnNames();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Trim().TrimEnd('\r').Split(',');
            if (values.Length != columnNames.Length)
            {
                Debug.LogError($"Invalid column count in line: {line}");
                continue;
            }

            var rowData = new Dictionary<string, string>();
            for (int i = 0; i < columnNames.Length; i++)
            {
                rowData[columnNames[i]] = values[i];
            }

            // 获取第一列的类型定义
            var keyType = _definition.GetColumnType(columnNames[0]);
            object key = ParseKeyValue(values[0], keyType);
            if (key == null)
            {
                Debug.LogError($"Failed to parse key in line: {line}");
                continue;
            }

            if (_configData.ContainsKey(key))
            {
                Debug.LogWarning($"Duplicate key {key} found, overwriting previous entry");
            }
            _configData[key] = rowData;
        }

        return _configData.Count > 0;
    }

    /// <summary>
    /// 获取配置值
    /// </summary>
    public T GetValue<T>(object key, string columnName, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            Debug.LogError("Column name cannot be null or empty");
            return defaultValue;
        }

        if (!_configData.ContainsKey(key))
        {
            Debug.LogWarning($"Key {key} not found");
            return defaultValue;
        }

        if (!_configData[key].ContainsKey(columnName))
        {
            Debug.LogWarning($"Column '{columnName}' not found for key {key}");
            return defaultValue;
        }

        string value = _configData[key][columnName];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return ParseValue<T>(value, columnName, defaultValue);
    }

    /// <summary>
    /// 获取所有键
    /// </summary>
    public IEnumerable<object> GetAllKeys()
    {
        return _configData.Keys;
    }

    /// <summary>
    /// 获取指定类型的所有键
    /// </summary>
    public IEnumerable<T> GetAllKeysOfType<T>()
    {
        return _configData.Keys.Where(k => k is T).Cast<T>();
    }

    /// <summary>
    /// 获取条目数量
    /// </summary>
    public int GetEntryCount()
    {
        return _configData.Count;
    }

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    public bool HasKey(object key)
    {
        return _configData.ContainsKey(key);
    }

    private T ParseValue<T>(string value, string columnName, T defaultValue)
    {
        try
        {
            Type targetType = typeof(T);

            // 处理枚举类型
            if (targetType.IsEnum)
            {
                return ParseEnum<T>(value, defaultValue);
            }

            // 处理枚举数组
            if (targetType.IsArray && targetType.GetElementType().IsEnum)
            {
                return ParseEnumArray<T>(value, defaultValue);
            }

            // 处理Vector2
            if (targetType == typeof(Vector2))
            {
                return ParseVector2<T>(value, defaultValue);
            }

            // 处理Vector3
            if (targetType == typeof(Vector3))
            {
                return ParseVector3<T>(value, defaultValue);
            }

            // 处理Color
            if (targetType == typeof(Color))
            {
                return ParseColor<T>(value, defaultValue);
            }

            // 处理数组类型
            if (targetType.IsArray)
            {
                return ParseArray<T>(value, defaultValue);
            }

            // 处理基础类型
            return (T)Convert.ChangeType(value, targetType);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse value '{value}' as {typeof(T)} for column '{columnName}': {e.Message}");
            return defaultValue;
        }
    }

    private T ParseEnum<T>(string value, T defaultValue)
    {
        value = value.Trim();
        
        // 使用缓存
        var enumType = typeof(T);
        if (!_enumCache.TryGetValue(enumType, out var cache))
        {
            cache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in Enum.GetNames(enumType))
            {
                cache[name] = Enum.Parse(enumType, name);
            }
            _enumCache[enumType] = cache;
        }

        if (cache.TryGetValue(value, out var enumValue))
        {
            return (T)enumValue;
        }

        Debug.LogError($"Invalid enum value '{value}' for type {enumType}");
        return defaultValue;
    }

    private T ParseEnumArray<T>(string value, T defaultValue)
    {
        var elementType = typeof(T).GetElementType();
        var values = value.Split('|');
        var result = Array.CreateInstance(elementType, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            try
            {
                result.SetValue(Enum.Parse(elementType, values[i].Trim(), true), i);
            }
            catch
            {
                Debug.LogError($"Failed to parse enum value: {values[i]}");
                return defaultValue;
            }
        }

        return (T)(object)result;
    }

    private T ParseVector2<T>(string value, T defaultValue)
    {
        var components = value.Split('|');
        if (components.Length != 2)
        {
            Debug.LogError($"Invalid Vector2 format: {value}");
            return defaultValue;
        }

        return (T)(object)new Vector2(
            float.Parse(components[0]),
            float.Parse(components[1])
        );
    }

    private T ParseVector3<T>(string value, T defaultValue)
    {
        var components = value.Split('|');
        if (components.Length != 3)
        {
            Debug.LogError($"Invalid Vector3 format: {value}");
            return defaultValue;
        }

        return (T)(object)new Vector3(
            float.Parse(components[0]),
            float.Parse(components[1]),
            float.Parse(components[2])
        );
    }

    private T ParseColor<T>(string value, T defaultValue)
    {
        if (ColorUtility.TryParseHtmlString(value, out Color color))
        {
            return (T)(object)color;
        }

        Debug.LogError($"Invalid color format: {value}");
        return defaultValue;
    }

    private T ParseArray<T>(string value, T defaultValue)
    {
        var elementType = typeof(T).GetElementType();
        var values = value.Split('|');
        var result = Array.CreateInstance(elementType, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            try
            {
                result.SetValue(Convert.ChangeType(values[i].Trim(), elementType), i);
            }
            catch
            {
                Debug.LogError($"Failed to parse array element: {values[i]}");
                return defaultValue;
            }
        }

        return (T)(object)result;
    }

    private object ParseKeyValue(string value, string type)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
                return null;

            // 处理基础类型
            switch (type)
            {
                case "int":
                    return int.Parse(value);
                case "string":
                    return value;
                default:
                    // 处理枚举类型
                    if (type.StartsWith("enum<") && type.EndsWith(">"))
                    {
                        var enumTypeName = type.Substring(5, type.Length - 6);
                        // 先尝试直接获取类型
                        var enumType = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .FirstOrDefault(t => t.Name == enumTypeName);

                        if (enumType != null && enumType.IsEnum)
                        {
                            return Enum.Parse(enumType, value, true);
                        }
                    }
                    break;
            }

            Debug.LogError($"Unsupported key type: {type}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse key value: {value}, error: {e.Message}");
            return null;
        }
    }
} 