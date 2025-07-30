using UnityEngine;

/// <summary>
/// 护甲装备
/// </summary>
public class Armor : BodyEquipBase
{
    public override void Use()
    {
        // 护甲是被动装备，不需要主动使用
    }

    protected override void OnDurabilityDepleted()
    {
        base.OnDurabilityDepleted();
        // 护甲损坏时的处理
        Destroy(gameObject);
    }
} 