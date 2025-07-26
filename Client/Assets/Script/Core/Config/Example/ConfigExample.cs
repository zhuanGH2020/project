using UnityEngine;

/// <summary>
/// Configuration system usage examples
/// Demonstrates how to load, query and manage CSV configuration data
/// </summary>
public static class ConfigExample
{
    /// <summary>
    /// Example demonstrating basic configuration usage
    /// </summary>
    public static void Example()
    {
        Debug.Log("=== Configuration System Example ===");

        // 1. Load configuration
        if (!ConfigManager.Instance.LoadConfig("Item", "Configs/Item"))
        {
            Debug.LogError("Failed to load item config");
            return;
        }

        // 2. Get configuration reader
        var reader = ConfigManager.Instance.GetReader("Item");
        if (reader == null)
        {
            Debug.LogError("Failed to get item config reader");
            return;
        }

        Debug.Log($"Loaded {reader.GetEntryCount()} item configurations");

        // 3. Query single configuration entry
        var id = 1;
        var name = reader.GetValue<string>(id, "Name");
        var type = reader.GetValue<ItemType>(id, "Type");
        var attack = reader.GetValue<int>(id, "Attack");
        var defense = reader.GetValue<int>(id, "Defense");

        Debug.Log($"Item ID: {id}");
        Debug.Log($"Name: {name}");
        Debug.Log($"Type: {type}");
        Debug.Log($"Attack: {attack}");
        Debug.Log($"Defense: {defense}");

        // 4. Query complex data types
        var parameters = reader.GetValue<int[]>(id, "Params");
        var position = reader.GetValue<Vector3>(id, "Position");
        var color = reader.GetValue<Color>(id, "Color");

        Debug.Log($"Parameters: {string.Join(", ", parameters)}");
        Debug.Log($"Position: {position}");
        Debug.Log($"Color: {color}");

        // 5. Iterate through all configurations
        Debug.Log("All items:");
        foreach (var itemId in reader.GetAllIds())
        {
            var itemName = reader.GetValue<string>(itemId, "Name");
            var itemType = reader.GetValue<ItemType>(itemId, "Type");
            Debug.Log($"- [{itemType}] {itemName} (ID: {itemId})");
        }

        // 6. Demonstrate error handling with default values
        var nonExistentId = 999;
        var defaultName = reader.GetValue<string>(nonExistentId, "Name", "Unknown Item");
        var defaultAttack = reader.GetValue<int>(nonExistentId, "Attack", 0);
        
        Debug.Log($"Non-existent item (ID: {nonExistentId}): {defaultName}, Attack: {defaultAttack}");

        // 7. Check configuration status
        Debug.Log($"Is Item config loaded: {ConfigManager.Instance.IsConfigLoaded("Item")}");
        Debug.Log($"Loaded config count: {ConfigManager.Instance.GetLoadedConfigCount()}");

        // 8. Clean up configuration
        ConfigManager.Instance.ClearConfig("Item");
        Debug.Log("Configuration cleared");

        Debug.Log("=== Example completed ===");
    }

    /// <summary>
    /// Example demonstrating advanced configuration features
    /// </summary>
    public static void AdvancedExample()
    {
        Debug.Log("=== Advanced Configuration Example ===");

        // Load multiple configurations
        var configs = new[] { "Item", "Monster", "Building" };
        
        foreach (var configName in configs)
        {
            var path = $"Configs/{configName.ToLower()}";
            if (ConfigManager.Instance.LoadConfig(configName, path))
            {
                var reader = ConfigManager.Instance.GetReader(configName);
                Debug.Log($"Successfully loaded {configName} config with {reader.GetEntryCount()} entries");
            }
            else
            {
                Debug.LogWarning($"Failed to load {configName} config (file may not exist)");
            }
        }

        // Demonstrate configuration management
        Debug.Log($"Total loaded configurations: {ConfigManager.Instance.GetLoadedConfigCount()}");

        // Clear all configurations
        ConfigManager.Instance.ClearAllConfigs();
        Debug.Log("All configurations cleared");

        Debug.Log("=== Advanced example completed ===");
    }

    /// <summary>
    /// Example demonstrating data validation and error handling
    /// </summary>
    public static void ValidationExample()
    {
        Debug.Log("=== Validation Example ===");

        // Try to load non-existent configuration
        if (!ConfigManager.Instance.LoadConfig("NonExistent", "Configs/nonexistent"))
        {
            Debug.Log("Expected: Failed to load non-existent config");
        }

        // Try to get reader for non-loaded config
        var reader = ConfigManager.Instance.GetReader("NonExistent");
        if (reader == null)
        {
            Debug.Log("Expected: Reader is null for non-loaded config");
        }

        // Load valid config and test data validation
        if (ConfigManager.Instance.LoadConfig("Item", "Configs/Item"))
        {
            reader = ConfigManager.Instance.GetReader("Item");
            
            // Test with invalid ID
            var invalidId = 999;
            var name = reader.GetValue<string>(invalidId, "Name", "Default Name");
            Debug.Log($"Invalid ID result: {name}");

            // Test with invalid column
            var invalidColumn = reader.GetValue<string>(1, "InvalidColumn", "Default Value");
            Debug.Log($"Invalid column result: {invalidColumn}");

            ConfigManager.Instance.ClearConfig("Item");
        }

        Debug.Log("=== Validation example completed ===");
    }
} 