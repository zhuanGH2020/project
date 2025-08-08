using UnityEngine;

public class StateIdle : StateBase { }

public class StateMove : StateBase
{
    [SerializeField] private float _speed = 2f;
    public override void Tick()
    {
        base.Tick();
        // Simple demo motion around current forward
        transform.position += transform.forward * _speed * Time.deltaTime;
    }
}

public class StateAttack : StateBase { }

public class StateDead : StateBase
{
    public override void EnterState()
    {
        base.EnterState();
        // Disable colliders/renderers gradually if needed in real implementation
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }
} 