using UnityEngine;
using System;

/// <summary>
/// 建筑物组件 - 管理建筑物的数据和行为
/// 挂载到建筑物预制体上，用于管理唯一标识、等级、交互状态等
/// </summary>
public class Building : DamageableObject
{
    private int _itemId = -1;               // 对应的道具ID
    private Vector2 _mapPosition;           // 地图位置
    private int _level = 1;                 // 建筑物等级
    private bool _canBuildingInteract = true; // 建筑物特有的交互状态
    private float _constructTime;           // 建造时间
    private bool _isConstructed = true;     // 是否建造完成
    
    private float _maxHealthValue = 100f;   // 最大生命值

    // 实现DamageableObject的抽象属性
    public override float MaxHealth => _maxHealthValue;
    public override float Defense => 0f;
    public override bool CanInteract => _canBuildingInteract && CurrentHealth > 0;
    public override float GetInteractionRange() => 2f;

    // 公共属性
    public int ItemId => _itemId;
    public Vector2 MapPosition => _mapPosition;
    public int Level => _level;
    public bool CanBuildingInteract => _canBuildingInteract; // 重命名避免与抽象属性冲突
    public float ConstructTime => _constructTime;
    public bool IsConstructed => _isConstructed;
    
    // 事件
    public event Action<Building> OnDemolished;
    public event Action<Building, int> OnLevelChanged;
    public event Action<Building, bool> OnInteractStateChanged;

    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Building);
    }
    
    public void Initialize(int itemId, Vector2 mapPos, int uid = 0)
    {
        _itemId = itemId;
        _mapPosition = mapPos;
        if (uid > 0)
        {
            SetUid(uid);
        }
        else if (Uid == 0)
        {
            SetUid(ResourceUtils.GenerateUid());
        }
        _constructTime = Time.time;
        
        LoadBuildingConfig();
        UpdateGameObjectName();
    }
    
    private void LoadBuildingConfig()
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        if (reader != null)
        {
            _maxHealthValue = reader.GetValue<float>(_itemId, "MaxHealth", 100f);
        }
    }
    
    private void UpdateGameObjectName()
    {
        gameObject.name = $"{GetPrefabName()}_{Uid}";
    }
    
    public string GetBuildingName()
    {
        return ResourceUtils.GetItemName(_itemId);
    }
    
    public string GetPrefabName()
    {
        string currentName = gameObject.name;
        if (currentName.EndsWith("(Clone)"))
        {
            currentName = currentName.Replace("(Clone)", "");
        }
        return currentName;
    }
    
    public bool UpgradeLevel()
    {
        int maxLevel = GetMaxLevel();
        if (_level >= maxLevel)
        {
            return false;
        }
        
        int oldLevel = _level;
        _level++;
        OnLevelChanged?.Invoke(this, oldLevel);
        return true;
    }
    
    public void SetInteractable(bool canInteract)
    {
        if (_canBuildingInteract != canInteract)
        {
            _canBuildingInteract = canInteract;
            OnInteractStateChanged?.Invoke(this, canInteract);
        }
    }
    
    public int GetMaxLevel()
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        return reader?.GetValue<int>(_itemId, "MaxLevel", 5) ?? 5;
    }
    
    protected override void OnDeath()
    {
        base.OnDeath();
        OnBuildingDestroyed();
    }
    
    private void OnBuildingDestroyed()
    {
        Demolish();
    }
    
    public void Demolish()
    {
        OnDemolished?.Invoke(this);
        MapModel.Instance.RemoveBuildingByUid(Uid);
        Destroy(gameObject);
    }

    public override void OnClick(Vector3 clickPosition)
    {
        if (!CanInteract) return;

        var player = Player.Instance;
        if (player != null)
        {
            // 计算玩家到建筑物的距离
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            float interactionRange = GetInteractionRange();

            // 如果玩家已经在交互范围内，直接触发交互
            if (distanceToPlayer <= interactionRange)
            {
                OnInteract(clickPosition);
            }
            else
            {
                // 计算寻路目标位置（在建筑物附近停下）
                Vector3 targetPosition = GetInteractionPosition(player.transform.position);
                
                // 让玩家寻路过去
                bool moveStarted = player.MoveToPlayerPosition(targetPosition);
                if (moveStarted)
                {
                    // 移动成功启动后，延迟触发交互事件
                    StartCoroutine(WaitForPlayerAndInteract(clickPosition));
                }
                else
                {
                    // 移动失败，直接触发交互
                    OnInteract(clickPosition);
                }
            }
        }
    }

    /// <summary>
    /// 计算合适的交互位置
    /// </summary>
    private Vector3 GetInteractionPosition(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
        float interactionRange = GetInteractionRange();
        
        // 在交互范围内，但保持一定距离避免重叠
        float stopDistance = Mathf.Max(0.5f, interactionRange - 0.5f);
        return transform.position + directionToPlayer * stopDistance;
    }

    /// <summary>
    /// 等待玩家到达并触发交互
    /// </summary>
    private System.Collections.IEnumerator WaitForPlayerAndInteract(Vector3 clickPosition)
    {
        var player = Player.Instance;
        if (player == null) yield break;

        float interactionRange = GetInteractionRange();
        float timeoutTime = 10f; // 最大等待时间
        float elapsedTime = 0f;

        // 等待玩家到达交互范围或超时
        while (elapsedTime < timeoutTime)
        {
            if (player == null || this == null) yield break;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            // 玩家到达交互范围内
            if (distance <= interactionRange)
            {
                OnInteract(clickPosition);
                yield break;
            }

            // 玩家停止移动且不在范围内（可能遇到障碍物）
            if (!player.IsMoving && distance > interactionRange)
            {
                // 尝试重新寻路到更近的位置
                Vector3 newTargetPosition = GetInteractionPosition(player.transform.position);
                player.MoveToPlayerPosition(newTargetPosition);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 交互逻辑 - 子类可重写实现自定义交互行为
    /// </summary>
    protected virtual void OnInteract(Vector3 clickPosition)
    {
        
    }
} 