using UnityEngine;

/// <summary>
/// 身体装备基类
/// </summary>
public abstract class BodyEquipBase : EquipBase
{
    protected Material _originalMaterial;  // 原始材质

    protected override void Awake()
    {
        base.Awake();
        _equipPart = EquipPart.Body;  // 设置装备部位
    }

    protected override void ApplyEquipEffect()
    {
        base.ApplyEquipEffect();

        // 获取装备配置
        var equipConfig = EquipManager.Instance.GetEquip(_configId);
        if (equipConfig == null) return;

        // 获取材质路径
        string materialPath = equipConfig.Csv.GetValue<string>(_configId, "MaterialPath");
        if (string.IsNullOrEmpty(materialPath)) return;
        /*
        // 加载材质
        var material = ResourceManager.Instance.Load<Material>(materialPath);
        if (material == null) return;

        // 获取身体渲染器
        var combatEntity = _owner as CombatEntity;
        if (combatEntity?.BodyMesh == null) return;

        // 保存原始材质并更换新材质
        _originalMaterial = combatEntity.BodyMesh.material;
        combatEntity.BodyMesh.material = material;
        */
    }

    protected override void RemoveEquipEffect()
    {
        base.RemoveEquipEffect();
        /*
        // 恢复原始材质
        var combatEntity = _owner as CombatEntity;
        if (combatEntity?.BodyMesh != null && _originalMaterial != null)
        {
            combatEntity.BodyMesh.material = _originalMaterial;
            _originalMaterial = null;
        }
        */
    }
} 