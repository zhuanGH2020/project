using UnityEngine;

/// <summary>
/// 科技台组件 - 处理玩家与科技台的交互，支持多级升级和发光功能
/// 需要挂载到科技台的GameObject上
/// 
/// 组件要求：
/// - GameObject必须有Collider组件（BoxCollider、SphereCollider等）用于点击检测
/// - 推荐添加MeshRenderer和MeshFilter显示3D模型
/// - 手动添加Light组件（Point Light），用于3级科技台夜晚发光
/// </summary>
public class TechTable : Building
{
    [Header("科技台设置")]
    [SerializeField] private float _interactionRange = 3f;  // 交互范围
    
    [Header("发光设置（3级时启用）")]
    [SerializeField] private Light _light;  // 光源组件，需要手动指定
    
    // 私有字段
    private bool _isLightInitialized = false;  // 光源是否已初始化

    protected override void Awake()
    {
        base.Awake();
        // 确保对象类型正确设置为Building（继承自Building已设置，这里可选）
        SetObjectType(ObjectType.Building);
        
        // 订阅事件
        EventManager.Instance.Subscribe<TimeOfDayChangeEvent>(OnTimeOfDayChanged);
        EventManager.Instance.Subscribe<TechTableLevelUpEvent>(OnTechTableLevelUp);
    }

    /// <summary>
    /// 取消订阅所有事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<TimeOfDayChangeEvent>(OnTimeOfDayChanged);
        EventManager.Instance.Unsubscribe<TechTableLevelUpEvent>(OnTechTableLevelUp);
    }

    /// <summary>
    /// 重写建筑销毁方法 - 处理事件取消订阅
    /// </summary>
    public override void Demolish()
    {
        UnsubscribeEvents();
        base.Demolish();
    }

    /// <summary>
    /// 重写死亡处理方法 - 处理事件取消订阅
    /// </summary>
    protected override void OnDeathCustom()
    {
        UnsubscribeEvents();
        base.OnDeathCustom();
    }

    /// <summary>
    /// 重写交互逻辑 - 根据背包选中状态处理交互
    /// 此方法由InteractionManager在玩家到达后调用
    /// </summary>
    public override void OnInteract(Vector3 clickPosition)
    {
        base.OnInteract(clickPosition);

        // 检查背包是否有选中的物品
        if (PackageModel.Instance.HasSelectedItem())
        {
            TryAddSelectedItemAsMaterial();
        }
        else
        {
            ShowUpgradeRequirements();
        }
    }

    /// <summary>
    /// 尝试将选中的物品添加为升级材料
    /// </summary>
    private void TryAddSelectedItemAsMaterial()
    {
        var selectedItem = PackageModel.Instance.SelectedItem;
        if (selectedItem == null) return;

        // 尝试添加选中的材料，返回实际消耗的数量
        int consumedCount = TechTableModel.Instance.TryAddSelectedMaterial(selectedItem);
        
        if (consumedCount > 0)
        {
            // 计算剩余数量
            int remainingCount = selectedItem.count - consumedCount;
            
            if (remainingCount <= 0)
            {
                // 物品完全消耗，清除选中状态
                PackageModel.Instance.ClearSelectedItem();
            }
            else
            {
                // 还有剩余，创建新的物品实例并减少数量，然后放回背包
                selectedItem.count = remainingCount;
                PackageModel.Instance.ClearSelectedItem(); // 先清除选中
                PackageModel.Instance.AddItem(selectedItem.itemId, remainingCount); // 将剩余部分放回背包
            }
        }
    }

    /// <summary>
    /// 显示升级需求
    /// </summary>
    private void ShowUpgradeRequirements()
    {
        TechTableModel.Instance.ShowRemainingMaterials();
    }

    /// <summary>
    /// 玩家进入科技台交互范围时调用
    /// </summary>
    public override void OnEnterInteractionRange()
    {
        base.OnEnterInteractionRange();
        
        // 发布进入交互范围事件，用于显示UI提示
        EventManager.Instance.Publish(new TechTableInteractionEvent(true, this));
    }

    /// <summary>
    /// 玩家离开科技台交互范围时调用
    /// </summary>
    public override void OnLeaveInteractionRange()
    {
        base.OnLeaveInteractionRange();
        
        // 发布离开交互范围事件，用于隐藏UI提示
        EventManager.Instance.Publish(new TechTableInteractionEvent(false, this));
    }

    public override float GetInteractionRange()
    {
        return _interactionRange;
    }

    /// <summary>
    /// 时间变化事件处理 - 控制夜晚发光
    /// </summary>
    private void OnTimeOfDayChanged(TimeOfDayChangeEvent eventData)
    {
        UpdateLightState();
    }

    /// <summary>
    /// 科技台等级提升事件处理 - 初始化发光功能
    /// </summary>
    private void OnTechTableLevelUp(TechTableLevelUpEvent eventData)
    {
        Debug.Log($"[TechTable] 等级提升事件 - 从{eventData.OldLevel}级升级到{eventData.NewLevel}级");
        
        // 更新光源状态（任何等级变化都需要检查）
        UpdateLightState();
    }

    /// <summary>
    /// 初始化光源组件
    /// </summary>
    private void InitializeLightComponent()
    {
        if (_isLightInitialized) 
        {
            Debug.Log($"[TechTable] 光源已经初始化，跳过");
            return;
        }

        Debug.Log($"[TechTable] 开始初始化光源组件");

        // 如果没有手动指定Light组件，尝试查找
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light != null)
            {
                Debug.Log($"[TechTable] 找到现有Light组件，初始状态：{_light.enabled}");
                // 确保现有组件默认关闭
                _light.enabled = false;
            }
        }

        // 如果仍然没有找到，创建一个新的
        if (_light == null)
        {
            Debug.Log($"[TechTable] 创建新的Light组件");
            GameObject lightObj = new GameObject("TechTableLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.up * 0.5f; // 在科技台上方0.5米
            
            _light = lightObj.AddComponent<Light>();
        }

        // 配置光源属性
        _light.type = LightType.Point;
        _light.color = GameSettings.TechTableLightColor;
        _light.intensity = GameSettings.TechTableLightIntensity;
        _light.range = GameSettings.TechTableLightRange;
        _light.shadows = LightShadows.Soft;
        
        // 默认关闭光源
        _light.enabled = false;
        
        _isLightInitialized = true;
        Debug.Log($"[TechTable] 光源初始化完成，默认状态：关闭");
    }

    /// <summary>
    /// 更新光源状态 - 根据时间和等级控制发光
    /// </summary>
    private void UpdateLightState()
    {
        int currentLevel = TechTableModel.Instance.CurrentLevel;
        bool canEmitLight = TechTableModel.Instance.CanEmitLight;
        
        Debug.Log($"[TechTable] UpdateLightState - 当前等级: {currentLevel}, 可发光: {canEmitLight}");
        
        // 只有3级及以上才能发光
        if (!canEmitLight)
        {
            if (_light != null)
            {
                if (_light.enabled)
                {
                    Debug.Log($"[TechTable] 等级不足({currentLevel}级)，关闭光源");
                    _light.enabled = false;
                }
            }
            return;
        }

        // 确保光源已初始化（只有3级时才初始化）
        if (!_isLightInitialized)
        {
            InitializeLightComponent();
        }

        if (_light == null) 
        {
            Debug.LogError($"[TechTable] 光源组件为null，无法控制发光");
            return;
        }

        // 根据时间决定是否发光 - 只在黄昏和夜晚发光
        TimeOfDay currentTime = ClockModel.Instance.CurrentTimeOfDay;
        bool shouldEmitLight = (currentTime == TimeOfDay.Dusk || currentTime == TimeOfDay.Night);
        
        // 如果光源状态发生变化，打印调试信息
        if (_light.enabled != shouldEmitLight)
        {
            Debug.Log($"[TechTable] 光源状态改变: 等级{currentLevel}, 时间{currentTime}, 应该发光{shouldEmitLight}, 之前状态{_light.enabled}");
        }
        
        _light.enabled = shouldEmitLight;
    }

    /// <summary>
    /// 获取当前科技台的等级
    /// </summary>
    public int GetCurrentLevel()
    {
        return TechTableModel.Instance.CurrentLevel;
    }

    /// <summary>
    /// 获取等级描述文本
    /// </summary>
    public string GetLevelDescription()
    {
        return TechTableModel.Instance.GetLevelDescription();
    }

    /// <summary>
    /// 是否可以发光
    /// </summary>
    public bool CanEmitLight()
    {
        return TechTableModel.Instance.CanEmitLight;
    }

    private void Start()
    {
        // 验证必要组件
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"[TechTable] GameObject '{gameObject.name}' 缺少Collider组件，无法进行点击检测！");
        }

        // 调试：打印初始状态
        Debug.Log($"[TechTable] Start - 初始等级: {TechTableModel.Instance.CurrentLevel}, 可发光: {TechTableModel.Instance.CanEmitLight}, 当前时间: {ClockModel.Instance.CurrentTimeOfDay}");

        // 初始化光源状态
        UpdateLightState();
    }

    private void OnDrawGizmosSelected()
    {
        // 在Scene视图中显示交互范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
        
        // 如果可以发光，显示发光范围
        if (TechTableModel.Instance.CanEmitLight && _light != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, GameSettings.TechTableLightRange);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor专用：显示科技台状态信息
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // 在Game视图左上角显示科技台信息
        GUILayout.BeginArea(new Rect(10, 200, 350, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"科技台状态:");
        GUILayout.Label($"等级: {GetCurrentLevel()}");
        GUILayout.Label($"描述: {GetLevelDescription()}");
        GUILayout.Label($"可发光: {CanEmitLight()}");
        GUILayout.Label($"当前时间: {ClockModel.Instance.CurrentTimeOfDay}");
        
        if (_light != null)
        {
            GUILayout.Label($"光源状态: {(_light.enabled ? "开启" : "关闭")}");
            GUILayout.Label($"光源已初始化: {_isLightInitialized}");
        }
        else
        {
            GUILayout.Label($"光源组件: 未找到");
        }
        
        // 添加调试按钮
        if (GUILayout.Button("强制更新光源状态"))
        {
            UpdateLightState();
        }
        
        if (GUILayout.Button("重置科技台等级"))
        {
            TechTableModel.Instance.ResetUpgradeStatus();
            UpdateLightState();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
#endif
}