using UnityEngine;

/// <summary>
/// Component that holds and updates a simple state for an ObjectBase.
/// </summary>
public class ObjectState : MonoBehaviour
{
    [SerializeField] private StateBase _currentState;
    public bool IsWorking => _currentState != null && _currentState.IsActive;

    public void StartState(StateBase state)
    {
        if (state == null) return;
        EndState();
        _currentState = state;
        _currentState.enabled = true;
        _currentState.EnterState();
    }

    public void EndState()
    {
        if (_currentState == null) return;
        _currentState.ExitState();
        _currentState.enabled = false;
        _currentState = null;
    }

    private void Update()
    {
        if (_currentState != null && _currentState.enabled)
        {
            _currentState.Tick();
        }
    }
} 