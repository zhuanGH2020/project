using UnityEngine;

/// <summary>
/// 手部装备基类
/// </summary>
public abstract class HandEquipBase : EquipBase
{
    [Header("轨迹设置")]
    [SerializeField] protected Material _trailMaterial; // 轨迹材质（可选）
    
    protected GameObject _modelPrefab;
    protected Transform _model;  // 装备模型

    protected override void ApplyEquipEffect()
    {
        base.ApplyEquipEffect();

        // 获取装备配置
        var equipConfig = EquipManager.Instance.GetEquip(_configId);
        if (equipConfig == null) return;

        // 获取模型预制体路径
        string modelPath = equipConfig.Csv.GetValue<string>(_configId, "ModelPath");
        if (string.IsNullOrEmpty(modelPath)) return;
        
        // 加载并实例化模型
        _modelPrefab = ResourceManager.Instance.Load<GameObject>(modelPath);
        if (_modelPrefab != null)
        {
            _model = Instantiate(_modelPrefab, transform).transform;
            
            // 获取手部挂载点
            var combatEntity = _owner as CombatEntity;
            if (combatEntity != null && combatEntity.HandPoint != null)
            {
                // 将模型挂到手部节点下
                _model.SetParent(combatEntity.HandPoint);
                _model.localPosition = Vector3.zero;
                _model.localRotation = Quaternion.identity;
            }
        }
    }

    protected override void RemoveEquipEffect()
    {
        base.RemoveEquipEffect();
        
        // 清理模型
        if (_model != null)
        {
            Destroy(_model.gameObject);
            _model = null;
        }
        if (_modelPrefab != null)
        {
            ResourceManager.Instance.Release(_modelPrefab);
            _modelPrefab = null;
        }
    }

    /// <summary>
    /// 获取攻击点位置
    /// </summary>
    protected virtual Vector3 GetAttackPoint()
    {
        return _model != null ? _model.position : transform.position;
    }

    /// <summary>
    /// 获取攻击方向
    /// </summary>
    protected virtual Vector3 GetAttackDirection()
    {
        return _model != null ? _model.forward : transform.forward;
    }

    /// <summary>
    /// 播放攻击特效
    /// </summary>
    protected virtual void PlayAttackEffect()
    {
        // 由子类实现具体特效
    }

    /// <summary>
    /// Show bullet trail effect
    /// </summary>
    /// <param name="startPoint">Trail start point</param>
    /// <param name="endPoint">Trail end point</param>
    protected virtual void ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)
    {
        BulletTrail.CreateTrail(startPoint, endPoint, _trailMaterial);
    }

    /// <summary>
    /// 处理攻击命中
    /// </summary>
    protected virtual void HandleHit(IDamageable target, Vector3 hitPoint)
    {
        if (target == null) return;

        var damageInfo = new DamageInfo
        {
            Damage = GetAttackBonus(),
            Type = DamageType.Physical,
            HitPoint = hitPoint,
            Direction = GetAttackDirection(),
            Source = _owner
        };

        target.TakeDamage(damageInfo);
    }
} 