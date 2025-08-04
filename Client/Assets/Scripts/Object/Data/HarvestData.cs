using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Action types for harvest interactions
/// </summary>
public enum ActionType
{
    None = 0,
    Pull = 1,        // Pulling action (grass)
    Chop = 2,        // Chopping action (trees)
    Pick = 3,        // Picking action (berries)
    Mine = 4,        // Mining action (rocks)
    Collect = 5      // Collection action (drops)
}

/// <summary>
/// Drop item information
/// </summary>
[System.Serializable]
public struct DropItem
{
    public int itemId;           // Item ID from config
    public int minCount;         // Minimum drop count
    public int maxCount;         // Maximum drop count
    public float dropRate;       // Drop probability (0.0-1.0)

    public DropItem(int itemId, int count) : this(itemId, count, count, 1.0f) { }
    
    public DropItem(int itemId, int minCount, int maxCount, float dropRate)
    {
        this.itemId = itemId;
        this.minCount = minCount;
        this.maxCount = maxCount;
        this.dropRate = dropRate;
    }

    // Calculate actual drop count based on probability
    public int GetActualDropCount()
    {
        if (Random.value > dropRate) return 0;
        return Random.Range(minCount, maxCount + 1);
    }
}

/// <summary>
/// Harvest information containing all data needed for harvest action
/// </summary>
[System.Serializable]
public struct HarvestInfo
{
    public List<DropItem> drops;                // List of possible drops
    public float harvestTime;                   // Time required to harvest
    public bool destroyAfterHarvest;            // Whether to destroy object after harvest
    public ActionType actionType;               // Action type for this harvest
    public bool requiresTool;                   // Whether a tool is required
    public ToolType requiredToolType;           // Required tool type (if any)

    public HarvestInfo(List<DropItem> drops, float harvestTime = 0f, bool destroyAfterHarvest = true, 
                      ActionType actionType = ActionType.None, bool requiresTool = false, 
                      ToolType requiredToolType = ToolType.None)
    {
        this.drops = drops ?? new List<DropItem>();
        this.harvestTime = harvestTime;
        this.destroyAfterHarvest = destroyAfterHarvest;
        this.actionType = actionType;
        this.requiresTool = requiresTool;
        this.requiredToolType = requiredToolType;
    }

    // Create simple harvest info for immediate collection
    public static HarvestInfo CreateSimple(int itemId, int count, ActionType actionType = ActionType.Pull)
    {
        var drops = new List<DropItem> { new DropItem(itemId, count) };
        return new HarvestInfo(drops, 0f, true, actionType);
    }
} 