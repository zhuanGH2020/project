using UnityEngine;

/// <summary>
/// UZI冲锋枪
/// </summary>
public class Uzi : HandEquipBase
{
    [Header("Uzi Settings")]
    [SerializeField] private float _damage = 15f;         // 基础伤害
    [SerializeField] private float _shootRange = 20f;     // 射程
    [SerializeField] private ParticleSystem _muzzleFlash; // 枪口特效

    protected override void ApplyEquipEffect()
    {
        Debug.Log($"[Uzi] ApplyEquipEffect - ConfigID: {_configId}");
        base.ApplyEquipEffect();
    }

    public override void Use()
    {
        if (!CanUse)
        {
            Debug.Log("[Uzi] Cannot use - CD or durability");
            return;
        }
        
        if (_owner == null)
        {
            Debug.Log("[Uzi] Cannot use - No owner");
            return;
        }

        Vector3 shootPoint = GetAttackPoint();
        Vector3 shootDir = GetAttackDirection();
        Debug.Log($"[Uzi] Shooting from {shootPoint} in direction {shootDir}");

        // 射击检测
        var hit = Physics.Raycast(
            shootPoint,
            shootDir,
            out RaycastHit hitInfo,
            _shootRange
        );

        // 播放射击特效
        PlayAttackEffect();

        if (hit)
        {
            Debug.Log($"[Uzi] Hit something at {hitInfo.point}");
            var target = hitInfo.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                Debug.Log($"[Uzi] Hit damageable target: {hitInfo.collider.name}");
                HandleHit(target, hitInfo.point);
            }
            else
            {
                Debug.Log($"[Uzi] Hit non-damageable object: {hitInfo.collider.name}");
            }
        }
        else
        {
            Debug.Log("[Uzi] No hit");
        }

        base.Use();
    }

    public override float GetAttackBonus()
    {
        return _damage;
    }

    protected override void PlayAttackEffect()
    {
        if (_muzzleFlash != null)
        {
            _muzzleFlash.Play();
            Debug.Log("[Uzi] Playing muzzle flash");
        }
        else
        {
            Debug.Log("[Uzi] No muzzle flash particle system");
        }
    }
} 