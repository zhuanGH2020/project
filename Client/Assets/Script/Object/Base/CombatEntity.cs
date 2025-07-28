using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 战斗实体基类
/// </summary>
public abstract class CombatEntity : DamageableObject, IAttacker
{
    [Header("Combat Settings")]
    [SerializeField] protected float _baseAttack = 10f;
    [SerializeField] protected float _attackCooldown = 1f;

    [Header("Equipment Points")]
    [SerializeField] protected Transform _handPoint;           // 手部挂载点
    [SerializeField] protected SkinnedMeshRenderer _headMesh; // 头部渲染器
    [SerializeField] protected SkinnedMeshRenderer _bodyMesh; // 身体渲染器

    [Header("Equipment")]
    [SerializeField] protected List<EquipBase> _equips = new List<EquipBase>();  // 装备列表

    protected CooldownTimer _attackTimer;

    public float BaseAttack => _baseAttack;
    public bool CanAttack => _attackTimer.IsReady;
    public Transform HandPoint => _handPoint;            // 提供手部节点访问
    public SkinnedMeshRenderer HeadMesh => _headMesh;   // 提供头部渲染器访问
    public SkinnedMeshRenderer BodyMesh => _bodyMesh;   // 提供身体渲染器访问

    // 计算总攻击力
    public virtual float TotalAttack
    {
        get
        {
            float total = BaseAttack;
            foreach (var equip in _equips)
            {
                total += equip.GetAttackBonus();
            }
            return total;
        }
    }

    // 计算总防御力
    public override float Defense
    {
        get
        {
            float total = base.Defense;
            foreach (var equip in _equips)
            {
                total += equip.GetDefenseBonus();
            }
            return total;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _attackTimer = new CooldownTimer(_attackCooldown);
    }

    protected virtual void Update()
    {
        _attackTimer.Update();
    }

    protected virtual void OnValidate()
    {
        // 检查装备列表中是否有重复部位的装备
        var duplicateParts = _equips
            .GroupBy(e => e.EquipPart)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateParts.Any())
        {
            Debug.LogError($"CombatEntity has multiple equipment in parts: {string.Join(", ", duplicateParts)}");
            // 移除重复部位的装备，只保留每个部位的第一个装备
            _equips = _equips
                .GroupBy(e => e.EquipPart)
                .Select(g => g.First())
                .ToList();
        }
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    public virtual void PerformAttack(IDamageable target)
    {
        if (!CanAttack || target == null) return;

        // 创建伤害信息
        var damageInfo = new DamageInfo
        {
            Damage = TotalAttack,  // 使用总攻击力
            Type = DamageType.Physical,
            HitPoint = transform.position + transform.forward,
            Direction = transform.forward,
            Source = this
        };

        // 造成伤害
        target.TakeDamage(damageInfo);

        // 开始冷却
        _attackTimer.StartCooldown();
    }

    /// <summary>
    /// 装备物品
    /// </summary>
    public virtual void Equip(EquipBase equip)
    {
        if (equip == null) return;

        // 卸下同部位的装备
        UnequipByPart(equip.EquipPart);

        // 添加新装备
        _equips.Add(equip);
        equip.transform.SetParent(transform);  // 装备直接挂到实体下
        equip.transform.localPosition = Vector3.zero;
        equip.transform.localRotation = Quaternion.identity;
        equip.OnEquip(this);
    }

    /// <summary>
    /// 卸下指定部位的装备
    /// </summary>
    protected virtual void UnequipByPart(EquipPart part)
    {
        var equip = GetEquipByPart(part);
        if (equip != null)
        {
            equip.OnUnequip();
            _equips.Remove(equip);
            Destroy(equip);
        }
    }

    /// <summary>
    /// 获取指定部位的装备
    /// </summary>
    protected virtual EquipBase GetEquipByPart(EquipPart part)
    {
        return _equips.FirstOrDefault(e => e.EquipPart == part);
    }

    /// <summary>
    /// 使用手持装备
    /// </summary>
    protected virtual void UseHandEquip()
    {
        var handEquip = GetEquipByPart(EquipPart.Hand);
        if (handEquip != null && handEquip.CanUse)
        {
            handEquip.Use();
        }
    }
} 