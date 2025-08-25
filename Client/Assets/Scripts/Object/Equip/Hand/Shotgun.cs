using UnityEngine;

/// <summary>
/// 散弹枪
/// </summary>
public class Shotgun : HandEquipBase
{
    [Header("散弹枪设置")]
    [SerializeField] private int _pelletCount = 6;        // 子弹数量
    [SerializeField] private float _spread = 15f;         // 散布角度

    public override void Use()
    {
        if (!CanUse || _owner == null) return;

        // 设置LayerMask，排除Equip Layer，避免射线被自己的碰撞体阻挡
        int equipLayer = LayerMask.NameToLayer("Equip");
        int layerMask = ~(1 << equipLayer); // 排除Equip Layer

        // 发射多颗子弹
        Vector3 shootPoint = GetAttackPoint();
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
                shootPoint,
                spreadDirection,
                out RaycastHit hitInfo,
                _range, // 使用基类的射程
                layerMask // 使用LayerMask排除Equip Layer
            );

            // 计算轨迹线终点并显示
            Vector3 trailEndPoint = hit ? hitInfo.point : shootPoint + spreadDirection * _range;
            ShowBulletTrail(shootPoint, trailEndPoint);

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
} 