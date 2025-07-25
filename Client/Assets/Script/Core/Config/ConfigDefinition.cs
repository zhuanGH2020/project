using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration definition parser for CSV headers and types
/// Validates and stores column information for configuration files
/// </summary>
public class ConfigDefinition
{
    /// <summary>
    /// Dictionary mapping column names to their indices
    /// </summary>
    private Dictionary<string, int> _columnIndices = new Dictionary<string, int>();
    
    /// <summary>
    /// Dictionary mapping column names to their types
    /// </summary>
    private Dictionary<string, string> _columnTypes = new Dictionary<string, string>();

    /// <summary>
    /// Parse configuration definition from CSV header lines
    /// </summary>
    /// <param name="headers">Array of column header names</param>
    /// <param name="types">Array of column type definitions</param>
    /// <returns>True if parsing successful, false otherwise</returns>
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

        // Clear existing data
        _columnIndices.Clear();
        _columnTypes.Clear();

        // Parse headers and types
        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim();
            var type = types[i].Trim().ToLower();

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

        // Ensure required Id column exists
        if (!_columnIndices.ContainsKey("Id"))
        {
            Debug.LogError("Missing required 'Id' column in configuration");
            return false;
        }

        Debug.Log($"Successfully parsed configuration definition with {_columnIndices.Count} columns");
        return true;
    }

    /// <summary>
    /// Get column index by name
    /// </summary>
    /// <param name="columnName">Name of the column</param>
    /// <returns>Column index if found, -1 otherwise</returns>
    public int GetColumnIndex(string columnName)
    {
        return _columnIndices.TryGetValue(columnName, out var index) ? index : -1;
    }

    /// <summary>
    /// Get column type by name
    /// </summary>
    /// <param name="columnName">Name of the column</param>
    /// <returns>Column type if found, empty string otherwise</returns>
    public string GetColumnType(string columnName)
    {
        return _columnTypes.TryGetValue(columnName, out var type) ? type : string.Empty;
    }

    /// <summary>
    /// Get total number of columns
    /// </summary>
    /// <returns>Number of columns in the configuration</returns>
    public int GetColumnCount()
    {
        return _columnIndices.Count;
    }

    /// <summary>
    /// Get all column names
    /// </summary>
    /// <returns>Array of all column names</returns>
    public string[] GetColumnNames()
    {
        var names = new string[_columnIndices.Count];
        _columnIndices.Keys.CopyTo(names, 0);
        return names;
    }

    /// <summary>
    /// Check if column exists
    /// </summary>
    /// <param name="columnName">Name of the column to check</param>
    /// <returns>True if column exists, false otherwise</returns>
    public bool HasColumn(string columnName)
    {
        return _columnIndices.ContainsKey(columnName);
    }

    /// <summary>
    /// Check if type is supported by the configuration system
    /// </summary>
    /// <param name="type">Type string to validate</param>
    /// <returns>True if type is supported, false otherwise</returns>
    private bool IsValidType(string type)
    {
        // Basic types
        if (type == "int" || type == "float" || type == "string" || type == "bool")
            return true;

        // Array types
        if (type.EndsWith("[]"))
        {
            var baseType = type.Substring(0, type.Length - 2);
            return baseType == "int" || baseType == "float" || baseType == "string";
        }

        // Enum types
        if (type.StartsWith("enum<") && type.EndsWith(">"))
            return true;

        // Unity types
        if (type == "vector2" || type == "vector3" || type == "color")
            return true;

        return false;
    }
} 