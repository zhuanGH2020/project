using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Harvestable object interface - defines objects that can be harvested for items
/// </summary>
public interface IHarvestable
{
    /// <summary>
    /// Whether this object can currently be harvested
    /// </summary>
    bool CanHarvest { get; }

    /// <summary>
    /// Get harvest information including drops and effects
    /// </summary>
    HarvestInfo GetHarvestInfo();

    /// <summary>
    /// Execute harvest action
    /// </summary>
    /// <param name="harvester">The entity performing the harvest</param>
    void OnHarvest(IAttacker harvester);
}

/// <summary>
/// Click interaction interface - defines objects that can be clicked to interact
/// </summary>
public interface IClickable
{
    /// <summary>
    /// Whether this object can currently be interacted with
    /// </summary>
    bool CanInteract { get; }

    /// <summary>
    /// Handle click interaction
    /// </summary>
    /// <param name="clickPosition">World position where clicked</param>
    void OnClick(Vector3 clickPosition);

    /// <summary>
    /// Get interaction range for this object
    /// </summary>
    float GetInteractionRange();
} 