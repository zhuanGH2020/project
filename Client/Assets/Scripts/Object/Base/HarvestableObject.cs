using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all harvestable objects
/// Provides common functionality for objects that can be clicked and harvested
/// </summary>
public abstract class HarvestableObject : DamageableObject, IHarvestable, IClickable
{
    [Header("Harvest Settings")]
    [SerializeField] protected List<DropItem> _drops = new List<DropItem>();
    [SerializeField] protected float _harvestTime = 0f;
    [SerializeField] protected bool _destroyAfterHarvest = true;
    [SerializeField] protected ActionType _actionType = ActionType.None;
    [SerializeField] protected bool _requiresTool = false;
    [SerializeField] protected ToolType _requiredToolType = ToolType.None;
    
    [Header("Interaction Settings")]
    [SerializeField] protected float _interactionRange = 2f;
    [SerializeField] protected ParticleSystem _harvestEffect;
    
    protected bool _isHarvested = false;
    protected bool _isBeingHarvested = false;

    // IHarvestable implementation
    public virtual bool CanHarvest => !_isHarvested && !_isBeingHarvested && (_currentHealth <= 0 || !_requiresTool || HasRequiredTool());
    
    public virtual HarvestInfo GetHarvestInfo()
    {
        return new HarvestInfo(_drops, _harvestTime, _destroyAfterHarvest, _actionType, _requiresTool, _requiredToolType);
    }

    public virtual void OnHarvest(IAttacker harvester)
    {
        if (!CanHarvest) return;

        _isBeingHarvested = true;
        
        // Play harvest effect
        PlayHarvestEffect();
        
        // Process drops
        ProcessDrops(harvester);
        
        // Mark as harvested
        _isHarvested = true;
        
        // Call virtual method for subclass-specific logic
        OnHarvestComplete(harvester);
        
        // Destroy if configured to do so
        if (_destroyAfterHarvest)
        {
            float destroyDelay = _harvestEffect ? _harvestEffect.main.duration : 0f;
            Destroy(gameObject, destroyDelay);
        }
    }

    // IClickable implementation
    public virtual bool CanInteract => !_isHarvested && !_isBeingHarvested;
    
    public virtual void OnClick(Vector3 clickPosition)
    {
        if (!CanInteract) return;
        
        // Publish interaction event for other systems to handle (like player movement)
        var interactionEvent = new ObjectInteractionEvent(this, clickPosition);
        EventManager.Instance.Publish(interactionEvent);
    }

    public virtual float GetInteractionRange()
    {
        return _interactionRange;
    }

    // Protected virtual methods for subclasses to override
    protected virtual void OnHarvestComplete(IAttacker harvester)
    {
        // Override in subclasses for specific harvest behavior
    }

    protected virtual void PlayHarvestEffect()
    {
        if (_harvestEffect != null)
        {
            _harvestEffect.Play();
        }
        
        // Load action data from ActionType config table
        var actionConfig = GetActionConfig();
        if (actionConfig != null)
        {
            // Play sound effect if specified
            string soundEffect = actionConfig.GetValue<string>((int)_actionType, "SoundEffect", "");
            if (!string.IsNullOrEmpty(soundEffect) && soundEffect != "none")
            {
                // TODO: Play sound effect using audio system
                Debug.Log($"[HarvestableObject] Playing sound: {soundEffect}");
            }
        }
    }

    /// <summary>
    /// Get action configuration from ActionType table
    /// </summary>
    protected virtual ConfigReader GetActionConfig()
    {
        return ConfigManager.Instance.GetReader("Action");
    }

    /// <summary>
    /// Get action display name from ActionType table
    /// </summary>
    public virtual string GetActionDisplayName()
    {
        var actionConfig = GetActionConfig();
        if (actionConfig != null)
        {
            string displayName = actionConfig.GetValue<string>(_actionType, "Name", "交互");
            return displayName;
        }
        return "";
    }

    protected virtual void ProcessDrops(IAttacker harvester)
    {
        foreach (var drop in _drops)
        {
            int actualCount = drop.GetActualDropCount();
            if (actualCount > 0)
            {
                // Try to add directly to player inventory first
                if ((Object)harvester == Player.Instance && PackageModel.Instance.AddItem(drop.itemId, actualCount))
                {
                    Debug.Log($"[{gameObject.name}] Added {actualCount}x item {drop.itemId} to inventory");
                }
                else
                {
                    // Create dropped item in world if can't add to inventory
                    CreateDroppedItem(drop.itemId, actualCount);
                }
            }
        }
    }

    protected virtual void CreateDroppedItem(int itemId, int count)
    {
        // Create a dropped item at this position using DirectHarvestable
        GameObject droppedItemGO = new GameObject($"DroppedItem_{itemId}");
        droppedItemGO.transform.position = transform.position + Vector3.up * 0.5f; // Slightly above ground
        
        // Add DirectHarvestable component for dropped items
        var directHarvestable = droppedItemGO.AddComponent<DirectHarvestable>();
        directHarvestable.SetItemId(itemId);
        directHarvestable.SetDropCount(count);
        
        // Add collider for interaction detection
        droppedItemGO.AddComponent<SphereCollider>();
        
        // Add a simple visual representation (can be enhanced later)
        var visualGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualGO.transform.SetParent(droppedItemGO.transform);
        visualGO.transform.localPosition = Vector3.zero;
        visualGO.transform.localScale = Vector3.one * 0.3f;
        
        Debug.Log($"[{gameObject.name}] Created dropped item: {count}x item {itemId} at {droppedItemGO.transform.position}");
    }

    protected virtual bool HasRequiredTool()
    {
        if (!_requiresTool) return true;
        
        // Check if player has the required tool equipped
        // This depends on the player's equipment system
        var player = Player.Instance;
        if (player == null) return false;
        
        // For now, just return true - will be implemented when tool checking is added
        return true;
    }

    // Unity lifecycle
    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Item);
        
        // Subscribe to any necessary events
        SubscribeToEvents();
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    protected virtual void SubscribeToEvents()
    {
        // Override in subclasses if needed
    }

    protected virtual void UnsubscribeFromEvents()
    {
        // Override in subclasses if needed
    }
} 