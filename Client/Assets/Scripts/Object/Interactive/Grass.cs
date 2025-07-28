using UnityEngine;

/// <summary>
/// 可拔起的草
/// </summary>
public class Grass : DamageableObject
{
    [Header("Grass Settings")]
    [SerializeField] private ParticleSystem _pullEffect;  // 拔起特效

    protected override void Awake()
    {
        base.Awake();
        // 设置为0生命值，表示不可破坏
        _maxHealth = 0;
        _currentHealth = 0;
    }

    /// <summary>
    /// 拔起草
    /// </summary>
    public void Pull()
    {
        // 播放特效
        if (_pullEffect)
        {
            _pullEffect.Play();
        }

        // 这里可以添加给玩家物品的逻辑

        // 销毁草
        Destroy(gameObject, _pullEffect ? _pullEffect.main.duration : 0f);
    }

    public override float TakeDamage(DamageInfo damageInfo)
    {
        // 不可被伤害
        return 0;
    }
} 