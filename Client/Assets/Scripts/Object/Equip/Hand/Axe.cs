using UnityEngine;

/// <summary>
/// 斧头武器
/// </summary>
public class Axe : HandEquipBase
{
    [Header("斧头设置")]
    [SerializeField] private ParticleSystem _hitEffect;   // 命中特效

    public override void Use()
    {
        if (!CanUse || _owner == null) return;

        // 设置LayerMask，排除Equip Layer，避免射线被自己的碰撞体阻挡
        int equipLayer = LayerMask.NameToLayer("Equip");
        int layerMask = ~(1 << equipLayer); // 排除Equip Layer

        // 射击检测
        var hit = Physics.Raycast(
            GetAttackPoint(),
            GetAttackDirection(),
            out RaycastHit hitInfo,
            _range,
            layerMask // 使用LayerMask排除Equip Layer
        );

        if (hit)
        {
            var target = hitInfo.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                HandleHit(target, hitInfo.point);
                PlayAttackEffect();
            }
        }

        base.Use();
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
        // 斧头损坏时的处理 - 移除组件而不是销毁GameObject
        Destroy(this);
    }
} 