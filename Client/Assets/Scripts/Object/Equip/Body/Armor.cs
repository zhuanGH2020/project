using UnityEngine;

/// <summary>
/// 护甲装备
/// </summary>
public class Armor : BodyEquipBase
{
    [Header("Armor Settings")]
    [SerializeField] private float _defense = 5f;  // 防御值

    protected override void Awake()
    {
        base.Awake();
        _equipPart = EquipPart.Body;  // 设置装备部位
    }

    public override void Use()
    {
        // 护甲是被动装备，不需要主动使用
    }

    public override float GetDefenseBonus()
    {
        return _defense;
    }

    protected override void OnDurabilityDepleted()
    {
        base.OnDurabilityDepleted();
        // 护甲损坏时的处理
        Destroy(gameObject);
    }
} 