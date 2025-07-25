using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Configuration reader for parsing and querying CSV data
/// Provides type-safe data access with automatic type conversion
/// </summary>
public class ConfigReader
{
    /// <summary>
    /// Configuration definition containing column information
    /// </summary>
    private ConfigDefinition _definition;
    
    /// <summary>
    /// Dictionary storing configuration data indexed by ID
    /// </summary>
    private Dictionary<int, string[]> _data = new Dictionary<int, string[]>();

    /// <summary>
    /// Constructor for ConfigReader
    /// </summary>
    /// <param name="definition">Configuration definition to use</param>
    public ConfigReader(ConfigDefinition definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    /// <summary>
    /// Load configuration data from CSV lines
    /// </summary>
    /// <param name="lines">Array of CSV data lines</param>
    /// <returns>True if loading successful, false otherwise</returns>
    public bool LoadData(string[] lines)
    {
        if (lines == null)
        {
            Debug.LogError("Data lines array is null");
            return false;
        }

        _data.Clear();
        var loadedCount = 0;

        foreach (var line in lines)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Parse line into values
            var values = line.Trim().TrimEnd('\r').Split(',');
            
            // Validate column count
            if (values.Length != _definition.GetColumnCount())
            {
                Debug.LogError($"Invalid column count in line: {line} (expected {_definition.GetColumnCount()}, got {values.Length})");
                return false;
            }

            // Parse ID
            if (!int.TryParse(values[_definition.GetColumnIndex("Id")], out int id))
            {
                Debug.LogError($"Invalid ID in line: {line}");
                return false;
            }

            // Check for duplicate IDs
            if (_data.ContainsKey(id))
            {
                Debug.LogWarning($"Duplicate ID {id} found, overwriting previous entry");
            }

            // Store data
            _data[id] = values;
            loadedCount++;
        }

        Debug.Log($"Successfully loaded {loadedCount} configuration entries");
        return true;
    }

    /// <summary>
    /// Get typed value from configuration by ID and column name
    /// </summary>
    /// <typeparam name="T">Target type for the value</typeparam>
    /// <param name="id">Configuration entry ID</param>
    /// <param name="columnName">Name of the column</param>
    /// <param name="defaultValue">Default value to return if not found or invalid</param>
    /// <returns>Typed value or default value</returns>
    public T GetValue<T>(int id, string columnName, T defaultValue = default)
    {
        // Check if ID exists
        if (!_data.TryGetValue(id, out var values))
        {
            Debug.LogWarning($"ID {id} not found in configuration");
            return defaultValue;
        }

        // Get column information
        var columnIndex = _definition.GetColumnIndex(columnName);
        if (columnIndex < 0)
        {
            Debug.LogWarning($"Column '{columnName}' not found in configuration");
            return defaultValue;
        }

        var columnType = _definition.GetColumnType(columnName);
        if (string.IsNullOrEmpty(columnType))
        {
            Debug.LogWarning($"Column type not found for '{columnName}'");
            return defaultValue;
        }

        // Get raw value
        var rawValue = values[columnIndex].Trim();
        if (string.IsNullOrEmpty(rawValue))
        {
            return defaultValue;
        }

        try
        {
            // Parse value based on type
            var parsedValue = ParseValue(rawValue, columnType, typeof(T));
            return (T)parsedValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing value '{rawValue}' as {typeof(T)} for column '{columnName}': {e.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// Get all configuration IDs
    /// </summary>
    /// <returns>Enumerable of all configuration IDs</returns>
    public IEnumerable<int> GetAllIds()
    {
        return _data.Keys;
    }

    /// <summary>
    /// Get count of configuration entries
    /// </summary>
    /// <returns>Number of configuration entries</returns>
    public int GetEntryCount()
    {
        return _data.Count;
    }

    /// <summary>
    /// Check if configuration contains specific ID
    /// </summary>
    /// <param name="id">ID to check</param>
    /// <returns>True if ID exists, false otherwise</returns>
    public bool HasId(int id)
    {
        return _data.ContainsKey(id);
    }

    /// <summary>
    /// Parse raw string value based on type definition
    /// </summary>
    /// <param name="value">Raw string value</param>
    /// <param name="type">Type definition</param>
    /// <param name="targetType">Target type for conversion</param>
    /// <returns>Parsed object value</returns>
    private object ParseValue(string value, string type, Type targetType)
    {
        // Handle array types
        if (type.EndsWith("[]"))
        {
            return ParseArray(value, type.Substring(0, type.Length - 2));
        }

        // Handle enum types
        if (type.StartsWith("enum<") && type.EndsWith(">"))
        {
            return ParseEnum(value, targetType);
        }

        // Handle Unity types
        switch (type)
        {
            case "vector2": return ParseVector2(value);
            case "vector3": return ParseVector3(value);
            case "color": return ParseColor(value);
        }

        // Handle basic types
        switch (type)
        {
            case "int": return int.Parse(value);
            case "float": return float.Parse(value);
            case "bool": return bool.Parse(value);
            case "string": return value;
            default: throw new ArgumentException($"Unsupported type: {type}");
        }
    }

    /// <summary>
    /// Parse array value from string
    /// </summary>
    /// <param name="value">String containing array elements</param>
    /// <param name="elementType">Type of array elements</param>
    /// <returns>Array object</returns>
    private Array ParseArray(string value, string elementType)
    {
        var elements = value.Split('|');
        
        switch (elementType)
        {
            case "int":
                return elements.Select(int.Parse).ToArray();
            case "float":
                return elements.Select(float.Parse).ToArray();
            case "string":
                return elements;
            default:
                throw new ArgumentException($"Unsupported array element type: {elementType}");
        }
    }

    /// <summary>
    /// Parse enum value from string
    /// </summary>
    /// <param name="value">String enum value</param>
    /// <param name="enumType">Target enum type</param>
    /// <returns>Enum value</returns>
    private object ParseEnum(string value, Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException($"Type {enumType} is not an enum");
        return Enum.Parse(enumType, value);
    }

    /// <summary>
    /// Parse Vector2 value from string
    /// </summary>
    /// <param name="value">String containing Vector2 components</param>
    /// <returns>Vector2 value</returns>
    private Vector2 ParseVector2(string value)
    {
        var components = value.Split('|');
        if (components.Length != 2)
            throw new ArgumentException("Vector2 requires exactly 2 components (x|y)");

        return new Vector2(
            float.Parse(components[0]),
            float.Parse(components[1])
        );
    }

    /// <summary>
    /// Parse Vector3 value from string
    /// </summary>
    /// <param name="value">String containing Vector3 components</param>
    /// <returns>Vector3 value</returns>
    private Vector3 ParseVector3(string value)
    {
        var components = value.Split('|');
        if (components.Length != 3)
            throw new ArgumentException("Vector3 requires exactly 3 components (x|y|z)");

        return new Vector3(
            float.Parse(components[0]),
            float.Parse(components[1]),
            float.Parse(components[2])
        );
    }

    /// <summary>
    /// Parse Color value from string
    /// </summary>
    /// <param name="value">String containing color information</param>
    /// <returns>Color value</returns>
    private Color ParseColor(string value)
    {
        // Support hex color format
        if (value.StartsWith("#"))
        {
            if (ColorUtility.TryParseHtmlString(value, out var color))
                return color;
            throw new ArgumentException($"Invalid hex color format: {value}");
        }

        // Support RGBA format
        var components = value.Split('|');
        if (components.Length != 4)
            throw new ArgumentException("Color requires exactly 4 components (r|g|b|a)");

        return new Color(
            float.Parse(components[0]),
            float.Parse(components[1]),
            float.Parse(components[2]),
            float.Parse(components[3])
        );
    }
} 