using UnityEngine;

/// <summary>
/// Base class for object states
/// </summary>
public abstract class StateBase : MonoBehaviour
{
    [SerializeField] protected bool _isActive;
    public bool IsActive => _isActive;

    public virtual void EnterState()
    {
        _isActive = true;
    }

    public virtual void ExitState()
    {
        _isActive = false;
    }

    public virtual void Tick() { }
} 