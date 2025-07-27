using UnityEngine;

/// <summary>
/// UZI冲锋枪
/// </summary>
public class Uzi : EquipBase
{
    [Header("Uzi Settings")]
    [SerializeField] private float _damage = 15f;         // 基础伤害
    [SerializeField] private float _shootRange = 20f;     // 射程
    [SerializeField] private ParticleSystem _muzzleFlash; // 枪口特效

    protected override void Awake()
    {
        base.Awake();
        _equipPart = EquipPart.Hand;  // 设置装备部位
    }

    public override void Use()
    {
        if (!CanUse || _owner == null) return;

        // 射击检测
        var hit = Physics.Raycast(
            transform.position,
            transform.forward,
            out RaycastHit hitInfo,
            _shootRange
        );

        // 播放枪口特效
        if (_muzzleFlash) _muzzleFlash.Play();

        if (hit)
        {
            var target = hitInfo.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                // 造成伤害
                var damageInfo = new DamageInfo
                {
                    Damage = _damage,
                    Type = DamageType.Physical,
                    HitPoint = hitInfo.point,
                    Direction = transform.forward,
                    Source = _owner
                };
                target.TakeDamage(damageInfo);
            }
        }

        base.Use();
    }

    public override float GetAttackBonus()
    {
        return _damage;
    }
} 