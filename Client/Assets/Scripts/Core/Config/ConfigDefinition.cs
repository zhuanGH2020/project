using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 配置定义解析器
/// 用于解析和存储CSV配置文件的列信息和类型定义
/// </summary>
public class ConfigDefinition
{
    /// <summary>
    /// 存储列名到列索引的映射
    /// </summary>
    private Dictionary<string, int> _columnIndices = new Dictionary<string, int>();
    
    /// <summary>
    /// 存储列名到列类型的映射
    /// </summary>
    private Dictionary<string, string> _columnTypes = new Dictionary<string, string>();

    /// <summary>
    /// 解析配置定义
    /// </summary>
    /// <param name="headers">列名数组</param>
    /// <param name="types">类型定义数组</param>
    /// <returns>解析是否成功</returns>
    public bool Parse(string[] headers, string[] types)
    {
        // Validate input parameters
        if (headers == null || types == null)
        {
            Debug.LogError("Headers or types array is null");
            return false;
        }

        if (headers.Length != types.Length)
        {
            Debug.LogError($"Header count ({headers.Length}) does not match type count ({types.Length})");
            return false;
        }

        if (headers.Length == 0)
        {
            Debug.LogError("Configuration must have at least one column");
            return false;
        }

        // Clear existing data
        _columnIndices.Clear();
        _columnTypes.Clear();

        // Parse headers and types
        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim();
            var type = types[i].Trim();

            // Validate header
            if (string.IsNullOrEmpty(header))
            {
                Debug.LogError($"Empty header at column {i}");
                return false;
            }

            // Check for duplicate headers
            if (_columnIndices.ContainsKey(header))
            {
                Debug.LogError($"Duplicate header found: {header}");
                return false;
            }

            // Validate type
            if (!IsValidType(type))
            {
                Debug.LogError($"Invalid type '{type}' for column '{header}'");
                return false;
            }

            // Store column information
            _columnIndices[header] = i;
            _columnTypes[header] = type;
        }

        Debug.Log($"Successfully parsed configuration definition with {_columnIndices.Count} columns");
        return true;
    }

    /// <summary>
    /// 获取列索引
    /// </summary>
    /// <param name="columnName">列名</param>
    /// <returns>列索引，如果不存在则返回-1</returns>
    public int GetColumnIndex(string columnName)
    {
        return _columnIndices.TryGetValue(columnName, out var index) ? index : -1;
    }

    /// <summary>
    /// 获取列类型
    /// </summary>
    /// <param name="columnName">列名</param>
    /// <returns>列类型，如果不存在则返回空字符串</returns>
    public string GetColumnType(string columnName)
    {
        return _columnTypes.TryGetValue(columnName, out var type) ? type : string.Empty;
    }

    /// <summary>
    /// 获取列数量
    /// </summary>
    /// <returns>配置中的列数量</returns>
    public int GetColumnCount()
    {
        return _columnIndices.Count;
    }

    /// <summary>
    /// 获取所有列名
    /// </summary>
    /// <returns>所有列名的数组</returns>
    public string[] GetColumnNames()
    {
        var names = new string[_columnIndices.Count];
        _columnIndices.Keys.CopyTo(names, 0);
        return names;
    }

    /// <summary>
    /// 检查列是否存在
    /// </summary>
    /// <param name="columnName">要检查的列名</param>
    /// <returns>列是否存在</returns>
    public bool HasColumn(string columnName)
    {
        return _columnIndices.ContainsKey(columnName);
    }

    /// <summary>
    /// 检查类型是否被配置系统支持
    /// 支持的类型：
    /// - 基础类型：int, float, string, bool
    /// - 数组类型：int[], float[], string[]
    /// - 枚举类型：enum<Type>
    /// - 枚举数组：enum<Type>[]
    /// - Unity类型：vector2, vector3, color
    /// </summary>
    private bool IsValidType(string type)
    {
        // 基础类型
        if (type == "int" || type == "float" || type == "string" || type == "bool")
            return true;

        // 数组类型
        if (type.EndsWith("[]"))
        {
            var baseType = type.Substring(0, type.Length - 2);
            // 基础类型数组
            if (baseType == "int" || baseType == "float" || baseType == "string")
                return true;
            // 枚举数组
            if (baseType.StartsWith("enum<") && baseType.EndsWith(">"))
                return true;
        }

        // 枚举类型
        if (type.StartsWith("enum<") && type.EndsWith(">"))
            return true;

        // Unity类型
        if (type == "vector2" || type == "vector3" || type == "color")
            return true;

        return false;
    }
} 