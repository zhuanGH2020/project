using UnityEngine;

/// <summary>
/// 斧头武器
/// </summary>
public class Axe : HandEquipBase
{
    [Header("Axe Settings")]
    [SerializeField] private float _damage = 20f;         // 基础伤害
    [SerializeField] private float _treeDamageBonus = 2f; // 对树木的伤害加成
    [SerializeField] private float _attackRange = 2f;     // 攻击范围
    [SerializeField] private ParticleSystem _hitEffect;   // 命中特效

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
            GetAttackPoint(),
            GetAttackDirection(),
            out RaycastHit hitInfo,
            _attackRange
        );

        if (hit)
        {
            var target = hitInfo.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                // 计算伤害
                if (target is Tree)
                {
                    _damage *= _treeDamageBonus;
                }

                HandleHit(target, hitInfo.point);
                PlayAttackEffect();
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
        if (_hitEffect != null)
        {
            _hitEffect.Play();
        }
    }

    protected override void OnDurabilityDepleted()
    {
        base.OnDurabilityDepleted();
        // 斧头损坏时的处理
        Destroy(gameObject);
    }
} 