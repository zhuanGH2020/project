using UnityEngine;
using UnityEngine.AI;
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

    [Header("移动设置")]
    [SerializeField] protected float _moveSpeed = 5f;

    [Header("装备挂载点")]
    [SerializeField] protected Transform _handPoint;           // 手部挂载点
    [SerializeField] protected SkinnedMeshRenderer _headMesh; // 头部渲染器
    [SerializeField] protected SkinnedMeshRenderer _bodyMesh; // 身体渲染器

    [Header("装备")]
    [SerializeField] protected List<EquipBase> _equips = new List<EquipBase>();  // 装备列表

    // 对话系统相关
    protected float _dialogRange;            // 对话范围
    protected int[] _availableDialogIds;     // 可用对话ID列表
    protected float _lastDialogTime;         // 上次对话时间
    protected float _dialogCooldown = 5f;    // 对话冷却时间（秒）
    protected int _currentDialogId = -1;     // 当前对话框ID

    protected CooldownTimer _attackTimer;
    protected NavMeshAgent _navMeshAgent;

    public float BaseAttack => _baseAttack;
    public bool CanAttack => _attackTimer.IsReady;
    public Transform HandPoint => _handPoint;            // 提供手部节点访问
    public SkinnedMeshRenderer HeadMesh => _headMesh;   // 提供头部渲染器访问
    public SkinnedMeshRenderer BodyMesh => _bodyMesh;   // 提供身体渲染器访问

    // 移动相关属性
    public float MoveSpeed => _moveSpeed;
    public bool IsMoving => _navMeshAgent != null && _navMeshAgent.hasPath && _navMeshAgent.remainingDistance > 0.1f;
    public bool HasNavMeshAgent => _navMeshAgent != null;
    public Vector3 Destination => _navMeshAgent != null && _navMeshAgent.hasPath ? _navMeshAgent.destination : transform.position;

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
        
        // 初始化NavMeshAgent
        InitializeNavMeshAgent();
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

    /// <summary>
    /// 初始化NavMeshAgent组件
    /// </summary>
    protected virtual void InitializeNavMeshAgent()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent != null)
        {
            _navMeshAgent.speed = _moveSpeed;
            _navMeshAgent.stoppingDistance = 0.5f;
            _navMeshAgent.angularSpeed = 120f;
            _navMeshAgent.acceleration = 8f;
        }
    }

    #region 移动接口

    /// <summary>
    /// 移动到指定位置 - 统一移动接口
    /// </summary>
    public virtual bool MoveToPosition(Vector3 targetPosition)
    {
        if (_navMeshAgent == null)
        {
            Debug.LogWarning($"[{name}] NavMeshAgent component is missing!");
            return false;
        }

        if (!_navMeshAgent.enabled)
        {
            Debug.LogWarning($"[{name}] NavMeshAgent is disabled!");
            return false;
        }

        // 检查目标位置是否在NavMesh上
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
            return true;
        }
        else
        {
            Debug.LogWarning($"[{name}] Target position is not on NavMesh: {targetPosition}");
            return false;
        }
    }

    /// <summary>
    /// 移动到指定目标 - 重载方法
    /// </summary>
    public virtual bool MoveToTarget(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning($"[{name}] Target transform is null!");
            return false;
        }

        return MoveToPosition(target.position);
    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public virtual void StopMovement()
    {
        if (_navMeshAgent != null && _navMeshAgent.enabled)
        {
            _navMeshAgent.ResetPath();
        }
    }

    /// <summary>
    /// 设置移动速度
    /// </summary>
    public virtual void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
        if (_navMeshAgent != null)
        {
            _navMeshAgent.speed = speed;
        }
    }

    /// <summary>
    /// 获取到目标的剩余距离
    /// </summary>
    public virtual float GetRemainingDistance()
    {
        if (_navMeshAgent != null && _navMeshAgent.hasPath)
        {
            return _navMeshAgent.remainingDistance;
        }
        return 0f;
    }

    /// <summary>
    /// 检查是否能到达指定位置
    /// </summary>
    public virtual bool CanReachPosition(Vector3 targetPosition)
    {
        if (_navMeshAgent == null) return false;

        NavMeshPath path = new NavMeshPath();
        if (_navMeshAgent.CalculatePath(targetPosition, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    /// <summary>
    /// 暂停/恢复NavMeshAgent
    /// </summary>
    public virtual void SetNavMeshEnabled(bool enabled)
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = enabled;
        }
    }

    #endregion

    #region 对话系统

    /// <summary>
    /// 显示实体消息（在头顶显示对话框）
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="duration">显示时长（秒），默认2秒</param>
    protected virtual void ShowEntityMessage(string message, float duration = 2f)
    {
        if (DialogManager.Instance == null) return;
        
        // 销毁之前的消息对话框（如果存在）
        if (_currentDialogId != -1)
        {
            DialogManager.Instance.DestroyDialog(_currentDialogId);
            _currentDialogId = -1;
        }
        
        Vector3 dialogOffset = new Vector3(0, 2.5f, 0); // 在实体头顶显示
        _currentDialogId = DialogManager.Instance.CreateDialog(
            transform,
            message,
            dialogOffset,
            duration
        );
    }

    /// <summary>
    /// 根据对话ID显示随机消息
    /// </summary>
    /// <param name="dialogIds">对话ID数组</param>
    /// <param name="duration">显示时长（秒），默认2秒</param>
    protected virtual void ShowRandomDialogMessage(int[] dialogIds, float duration = 2f)
    {
        if (dialogIds == null || dialogIds.Length == 0)
        {
            Debug.LogWarning($"[{GetType().Name}] 没有可用的对话ID");
            return;
        }

        // 销毁之前的消息对话框（如果存在）
        if (_currentDialogId != -1)
        {
            DialogManager.Instance.DestroyDialog(_currentDialogId);
            _currentDialogId = -1;
        }

        // 创建随机对话
        Vector3 dialogOffset = new Vector3(0, 2.5f, 0); // 在实体头顶显示
        _currentDialogId = DialogManager.Instance.CreateRandomDialog(
            transform,
            dialogIds,
            dialogOffset,
            duration
        );
    }

    /// <summary>
    /// 触发随机对话（供子类重写）
    /// </summary>
    protected virtual void TriggerRandomDialog()
    {
        if (_availableDialogIds == null || _availableDialogIds.Length == 0)
        {
            Debug.LogWarning($"[{GetType().Name}] {gameObject.name} 没有可用的对话ID");
            return;
        }

        ShowRandomDialogMessage(_availableDialogIds, 3f);
        _lastDialogTime = Time.time;
    }

    /// <summary>
    /// 检查对话冷却时间
    /// </summary>
    protected virtual bool CanTriggerDialog()
    {
        return Time.time - _lastDialogTime >= _dialogCooldown;
    }

    /// <summary>
    /// 清理对话框
    /// </summary>
    protected virtual void ClearDialog()
    {
        if (_currentDialogId != -1 && DialogManager.Instance != null)
        {
            DialogManager.Instance.DestroyDialog(_currentDialogId);
            _currentDialogId = -1;
        }
    }

    #endregion
} 