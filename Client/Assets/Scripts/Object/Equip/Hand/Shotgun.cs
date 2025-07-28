using UnityEngine;

/// <summary>
/// 散弹枪
/// </summary>
public class Shotgun : HandEquipBase
{
    [Header("Shotgun Settings")]
    [SerializeField] private float _damage = 25f;         // 每颗子弹的伤害
    [SerializeField] private float _shootRange = 15f;     // 射程
    [SerializeField] private int _pelletCount = 6;        // 子弹数量
    [SerializeField] private float _spread = 15f;         // 散布角度
    [SerializeField] private ParticleSystem _muzzleFlash; // 枪口特效

    public override void Use()
    {
        if (!CanUse || _owner == null) return;

        // 播放射击特效
        PlayAttackEffect();

        // 发射多颗子弹
        for (int i = 0; i < _pelletCount; i++)
        {
            // 计算散布方向
            Vector3 spreadDirection = GetAttackDirection();
            spreadDirection = Quaternion.Euler(
                Random.Range(-_spread, _spread),
                Random.Range(-_spread, _spread),
                0
            ) * spreadDirection;

            // 射击检测
            var hit = Physics.Raycast(
                GetAttackPoint(),
                spreadDirection,
                out RaycastHit hitInfo,
                _shootRange
            );

            if (hit)
            {
                var target = hitInfo.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    HandleHit(target, hitInfo.point);
                }
            }
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
        }
    }

    protected override void HandleHit(IDamageable target, Vector3 hitPoint)
    {
        if (target == null) return;

        var damageInfo = new DamageInfo
        {
            Damage = GetAttackBonus(),  // 每颗子弹的伤害单独计算
            Type = DamageType.Physical,
            HitPoint = hitPoint,
            Direction = GetAttackDirection(),
            Source = _owner
        };

        target.TakeDamage(damageInfo);
    }
} 