using UnityEngine;

/// <summary>
/// 火把武器
/// </summary>
public class Torch : HandEquipBase
{
    [Header("火把设置")]
    [SerializeField] private ParticleSystem _flameEffect;  // 火焰特效
    [SerializeField] private Light _light;                 // 光源组件

    private bool _isLit = false;  // 是否点燃

    protected override void ApplyEquipEffect()
    {
        base.ApplyEquipEffect();
        SetLitState(false);
    }

    public override void Use()
    {
        if (!CanUse || _owner == null) return;

        // 第一次使用点燃火把
        if (!_isLit)
        {
            SetLitState(true);
            base.Use();
            return;
        }

        // 已点燃状态下可以造成火焰伤害
        var hit = Physics.Raycast(
            GetAttackPoint(),
            GetAttackDirection(),
            out RaycastHit hitInfo,
            _range // 使用基类的范围
        );

        if (hit)
        {
            var target = hitInfo.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                HandleHit(target, hitInfo.point);
            }
        }

        base.Use();
    }

    private void SetLitState(bool lit)
    {
        _isLit = lit;
        if (_flameEffect) _flameEffect.gameObject.SetActive(lit);
        if (_light) _light.enabled = lit;
    }

    public override float GetAttackBonus()
    {
        return _isLit ? _damage : 0f; // 只有点燃时才有攻击力
    }

    protected override void HandleHit(IDamageable target, Vector3 hitPoint)
    {
        if (target == null || !_isLit) return;

        var damageInfo = new DamageInfo
        {
            Damage = _damage, // 使用基类的伤害值
            Type = DamageType.Fire,  // 火焰伤害
            HitPoint = hitPoint,
            Direction = GetAttackDirection(),
            Source = _owner
        };

        target.TakeDamage(damageInfo);
    }

    protected override void OnDurabilityDepleted()
    {
        base.OnDurabilityDepleted();
        SetLitState(false);
        // 火把损坏时的处理 - 移除组件而不是销毁GameObject
        Destroy(this);
    }
} 