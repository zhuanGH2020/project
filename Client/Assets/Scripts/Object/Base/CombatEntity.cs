using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 战斗实体基类
/// </summary>
public abstract class CombatEntity : DamageableObject, IAttacker
{
    [Header("战斗设置")]
    [SerializeField] protected float _baseAttack = 10f;
    [SerializeField] protected float _attackCooldown = 1f;

    [Header("装备挂载点")]
    [SerializeField] protected Transform _handPoint;           // 手部挂载点
    [SerializeField] protected SkinnedMeshRenderer _headMesh; // 头部渲染器
    [SerializeField] protected SkinnedMeshRenderer _bodyMesh; // 身体渲染器

    [Header("装备")]
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
        // 默认把战斗实体归类为Monster，子类如Player可在Awake里覆盖
        SetObjectType(ObjectType.Monster);
        _attackTimer = new CooldownTimer(_attackCooldown);
    }

    protected virtual void Update()
    {
        _attackTimer.Update();
    }

    protected virtual void OnValidate()
    {
        // 检查装备列表中是否有重复部位的装备
        Dictionary<EquipPart, int> partCounts = new Dictionary<EquipPart, int>();
        List<EquipPart> duplicateParts = new List<EquipPart>();
        
        foreach (var equip in _equips)
        {
            if (equip == null) continue;
            if (partCounts.ContainsKey(equip.EquipPart))
            {
                partCounts[equip.EquipPart]++;
            }
            else
            {
                partCounts[equip.EquipPart] = 1;
            }
        }
        
        foreach (var kvp in partCounts)
        {
            if (kvp.Value > 1)
            {
                duplicateParts.Add(kvp.Key);
            }
        }

        if (duplicateParts.Count > 0)
        {
            string duplicatePartsStr = string.Join(", ", duplicateParts);
            Debug.LogError($"CombatEntity has multiple equipment in parts: {duplicatePartsStr}");
            
            List<EquipBase> newEquips = new List<EquipBase>();
            HashSet<EquipPart> addedParts = new HashSet<EquipPart>();
            
            foreach (var equip in _equips)
            {
                if (equip == null) continue;
                if (!addedParts.Contains(equip.EquipPart))
                {
                    newEquips.Add(equip);
                    addedParts.Add(equip.EquipPart);
                }
            }
            
            _equips = newEquips;
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
    /// 通过装备ID装备物品
    /// </summary>
    public virtual void Equip(int equipId)
    {
        // 获取装备配置
        var equipConfig = EquipManager.Instance.GetEquip(equipId);
        if (equipConfig == null)
        {
            Debug.LogError($"[CombatEntity] Equipment config not found: {equipId}");
            return;
        }

        // 根据配置类型创建对应的装备组件
        EquipBase equip = null;
        EquipType equipType = equipConfig.Csv.GetValue<EquipType>(equipId, "EquipType");
        
        switch (equipType)
        {
            case EquipType.Axe:
                equip = gameObject.AddComponent<Axe>();
                break;
            case EquipType.Uzi:
                equip = gameObject.AddComponent<Uzi>();
                break;  
            case EquipType.Shotgun:
                equip = gameObject.AddComponent<Shotgun>();
                break;
            case EquipType.Torch:
                equip = gameObject.AddComponent<Torch>();
                break;
            case EquipType.Armor:
                equip = gameObject.AddComponent<Armor>();
                break;
            case EquipType.Helmet:
                // 可以添加头盔类，暂时用Armor代替
                equip = gameObject.AddComponent<Armor>();
                break;
            default:
                Debug.LogError($"[CombatEntity] Unknown equipment type: {equipType}");
                return;
        }

        if (equip != null)
        {
            // 初始化装备配置
            equip.Init(equipId);
            
            // 调用原有装备方法
            EquipInternal(equip);
        }
    }

    /// <summary>
    /// 内部装备处理方法
    /// </summary>
    protected virtual void EquipInternal(EquipBase equip)
    {
        if (equip == null) return;

        // 卸下同部位的装备
        UnequipByPart(equip.EquipPart);

        // 添加新装备
        _equips.Add(equip);
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
        }
    }

    /// <summary>
    /// 获取指定部位的装备
    /// </summary>
    protected virtual EquipBase GetEquipByPart(EquipPart part)
    {
        foreach (var equip in _equips)
        {
            if (equip.EquipPart == part)
            {
                return equip;
            }
        }
        return null;
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

    /// <summary>
    /// 获取已装备道具的配置ID列表 - 供存档系统使用
    /// </summary>
    public virtual List<int> GetEquippedItemIds()
    {
        List<int> equipIds = new List<int>();
        foreach (var equip in _equips)
        {
            // 使用反射获取私有字段_configId，或者通过公共属性获取
            if (equip != null)
            {
                // 假设EquipBase有ConfigId属性，如果没有则需要添加
                var configId = GetEquipConfigId(equip);
                if (configId > 0)
                {
                    equipIds.Add(configId);
                }
            }
        }
        return equipIds;
    }
    
    /// <summary>
    /// 获取装备的配置ID - 辅助方法
    /// </summary>
    private int GetEquipConfigId(EquipBase equip)
    {
        // 使用反射获取私有字段_configId
        var field = typeof(EquipBase).GetField("_configId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (int)field.GetValue(equip);
        }
        return 0;
    }
} 