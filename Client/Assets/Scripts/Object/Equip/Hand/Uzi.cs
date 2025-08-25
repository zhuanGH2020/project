using UnityEngine;

/// <summary>
/// UZI冲锋枪
/// </summary>
public class Uzi : HandEquipBase
{
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

        // 设置LayerMask，排除Equip Layer，避免射线被自己的碰撞体阻挡
        int equipLayer = LayerMask.NameToLayer("Equip");
        int layerMask = ~(1 << equipLayer); // 排除Equip Layer
        
        // 射击检测
        var hit = Physics.Raycast(
            shootPoint,
            shootDir,
            out RaycastHit hitInfo,
            _range, // 使用基类的射程
            layerMask // 使用LayerMask排除Equip Layer
        );

        // 计算轨迹线终点
        Vector3 trailEndPoint = hit ? hitInfo.point : shootPoint + shootDir * _range;
        
        // 显示子弹轨迹线
        ShowBulletTrail(shootPoint, trailEndPoint);

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
} 