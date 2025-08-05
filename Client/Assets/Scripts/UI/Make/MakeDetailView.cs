using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 制作详情视图 - 显示制作配方详细信息和材料需求
/// 响应MakeDetailOpenEvent事件，显示具体物品的制作详情界面
/// 提供材料检查和制作功能，支持材料不足时的提示
/// </summary>
public class MakeDetailView : BaseView
{
    private TextMeshProUGUI txt_name;
    private TextMeshProUGUI txt_desc;
    private Button btn_make;
    
    private int _currentItemId = -1;
    private List<MaterialRequirement> _materialRequirements = new List<MaterialRequirement>();
    private Coroutine _closeDelayCoroutine;
    
    void Start()
    {
        InitializeView();
        SubscribeEvents();
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // 初始化视图组件
    private void InitializeView()
    {
        // 查找UI组件
        txt_name = transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
        txt_desc = transform.Find("txt_desc")?.GetComponent<TextMeshProUGUI>();
        btn_make = transform.Find("btn_make")?.GetComponent<Button>();
        
        // 设置按钮交互
        if (btn_make != null)
        {
            btn_make.onClick.AddListener(OnMakeClick);
        }
        
        
        // 设置鼠标离开事件处理
        SetupSelfHoverEvents();
        
        // 初始状态隐藏视图
        SetViewVisible(false);
    }

    // 订阅事件
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<MakeDetailOpenEvent>(OnMakeDetailOpen);
        EventManager.Instance.Subscribe<MakeDetailCloseEvent>(OnMakeDetailClose);
        EventManager.Instance.Subscribe<ClickOutsideUIEvent>(OnClickOutsideUI);
    }

    // 取消订阅事件
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<MakeDetailOpenEvent>(OnMakeDetailOpen);
        EventManager.Instance.Unsubscribe<MakeDetailCloseEvent>(OnMakeDetailClose);
        EventManager.Instance.Unsubscribe<ClickOutsideUIEvent>(OnClickOutsideUI);
    }

    // 处理制作详情打开事件
    private void OnMakeDetailOpen(MakeDetailOpenEvent eventData)
    {
        if (eventData == null || eventData.ItemId <= 0)
        {
            return;
        }
        
        // 检查是否是同一个物品的重复点击，且当前视图已显示
        if (gameObject.activeInHierarchy && _currentItemId == eventData.ItemId)
        {
            // 检查材料是否充足，如果充足则直接制作
            if (CheckMaterialsAvailable())
            {
                OnMakeClick(); // 直接执行制作
                return;
            }
            else
            {
                // 材料不足时显示提示
                EventManager.Instance.Publish(new NoticeEvent("材料不足"));
                return;
            }
        }
        
        ShowMakeDetail(eventData.ItemId, eventData.UIPosition);
    }

    // 处理制作详情关闭事件
    private void OnMakeDetailClose(MakeDetailCloseEvent eventData)
    {
        // 延迟关闭，给用户时间移动鼠标到MakeDetailView
        StartCloseDelay();
    }

    /// <summary>
    /// 设置MakeDetailView自身的悬停事件处理
    /// 当鼠标进入时取消延迟关闭，离开时开始延迟关闭
    /// </summary>
    private void SetupSelfHoverEvents()
    {
        // 添加EventTrigger组件处理悬停事件
        var eventTrigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 添加鼠标进入事件
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => {
            CancelCloseDelay();
        });
        eventTrigger.triggers.Add(pointerEnter);
        
        // 添加鼠标离开事件
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => {
            StartCloseDelay();
        });
        eventTrigger.triggers.Add(pointerExit);
    }

    /// <summary>
    /// 开始延迟关闭
    /// </summary>
    private void StartCloseDelay()
    {
        CancelCloseDelay();
        _closeDelayCoroutine = StartCoroutine(CloseDelayCoroutine());
    }

    /// <summary>
    /// 取消延迟关闭
    /// </summary>
    private void CancelCloseDelay()
    {
        if (_closeDelayCoroutine != null)
        {
            StopCoroutine(_closeDelayCoroutine);
            _closeDelayCoroutine = null;
        }
    }

    /// <summary>
    /// 延迟关闭协程
    /// </summary>
    private System.Collections.IEnumerator CloseDelayCoroutine()
    {
        yield return new WaitForSeconds(0.2f); // 200ms延迟
        CloseView();
    }

    // 处理点击非UI区域事件
    private void OnClickOutsideUI(ClickOutsideUIEvent eventData)
    {
        if (gameObject.activeInHierarchy)
        {
            CloseView();
        }
    }

    /// <summary>
    /// 显示制作详情
    /// 从MakeMenu配置表加载物品制作信息并更新UI显示
    /// 使用ViewUtils统一设置物品UI显示
    /// </summary>
    private void ShowMakeDetail(int itemId, Vector2 itemUIPosition)
    {
        var reader = ConfigManager.Instance.GetReader("MakeMenu");
        if (reader == null || !reader.HasKey(itemId))
        {
            return;
        }
        
        CancelCloseDelay(); // 取消任何正在进行的延迟关闭
        _currentItemId = itemId;
        
        // 获取制作配方数据
        string itemName = reader.GetValue<string>(itemId, "Name", "Unknown");
        int productId = itemId;
        
        // 通过产出物品ID从Item.csv获取物品详细信息
        string description = "";
        var itemReader = ConfigManager.Instance.GetReader("Item");
        if (itemReader != null && productId > 0)
        {
            description = itemReader.GetValue<string>(productId, "Description", "");
        }
        
        // 更新UI显示
        UpdateItemInfo(itemName, description);
        LoadMaterialRequirements(reader, itemId);
        
        // 设置UI位置
        SetViewPosition(itemUIPosition);
        
        SetViewVisible(true);
    }

    // 更新物品信息显示
    private void UpdateItemInfo(string itemName, string description)
    {
        if (txt_name != null)
        {
            txt_name.text = itemName;
        }
        
        if (txt_desc != null)
        {
            txt_desc.text = description;
        }
    }

    /// <summary>
    /// 加载材料需求并显示在list_mat中
    /// 解析Material1,Material2,Material3字段的材料数据
    /// </summary>
    private void LoadMaterialRequirements(ConfigReader reader, int itemId)
    {
        _materialRequirements.Clear();
        
        // 获取材料数据
        string material1 = reader.GetValue<string>(itemId, "Material1", "");
        string material2 = reader.GetValue<string>(itemId, "Material2", "");
        string material3 = reader.GetValue<string>(itemId, "Material3", "");
        
        // 解析材料需求
        ParseMaterialRequirement(material1);
        ParseMaterialRequirement(material2);
        ParseMaterialRequirement(material3);
        
        // 更新材料列表显示
        UpdateMaterialList();
    }

    /// <summary>
    /// 解析单个材料需求
    /// 材料格式: "物品ID;数量" 例如: "1000;3"
    /// </summary>
    private void ParseMaterialRequirement(string materialData)
    {
        if (string.IsNullOrEmpty(materialData)) return;
        
        string[] parts = materialData.Split(';');
        if (parts.Length != 2) return;
        
        if (int.TryParse(parts[0], out int materialId) && int.TryParse(parts[1], out int quantity))
        {
            _materialRequirements.Add(new MaterialRequirement
            {
                ItemId = materialId,
                RequiredQuantity = quantity
            });
        }
    }

    // 更新材料列表显示
    private void UpdateMaterialList()
    {
        UIList uiList = GetMaterialUIList();
        if (uiList == null)
        {
            Debug.LogWarning("Material UIList not found");
            return;
        }
        
        uiList.RemoveAll();
        
        foreach (var material in _materialRequirements)
        {
            GameObject item = uiList.AddListItem();
            if (item != null)
            {
                SetupMaterialItem(item, material);
            }
        }
    }

    // 获取材料列表UIList组件
    private UIList GetMaterialUIList()
    {
        Transform listTransform = transform.Find("list_mat") ?? FindChildWithUIList();
        return listTransform?.GetComponent<UIList>() ?? listTransform?.GetComponentInChildren<UIList>();
    }

    // 查找包含UIList组件的子对象
    private Transform FindChildWithUIList()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<UIList>() != null) return child;
            
            var childUIList = child.GetComponentInChildren<UIList>();
            if (childUIList != null) return childUIList.transform;
        }
        return null;
    }

    /// <summary>
    /// 设置材料列表项UI显示
    /// 使用ViewUtils统一设置材料项UI，显示材料名称、图标和需求数量
    /// </summary>
    private void SetupMaterialItem(GameObject item, MaterialRequirement material)
    {
        // 使用ViewUtils统一设置材料UI（图标、名称、数量）
        ViewUtils.QuickSetItemUI(item, material.ItemId, material.RequiredQuantity);
        
        // 可以在这里添加额外的UI处理，比如材料不足时的颜色变化
        int currentQuantity = PackageModel.Instance.GetItemCount(material.ItemId);
        if (currentQuantity < material.RequiredQuantity)
        {
            // 材料不足时可以设置文本颜色为红色等提示
            var txtCount = item.transform.Find("txt_count")?.GetComponent<TextMeshProUGUI>();
            if (txtCount != null)
            {
                txtCount.color = Color.red;
            }
        }
    }

    // 处理制作按钮点击
    private void OnMakeClick()
    {
        if (_currentItemId <= 0)
        {
            Debug.LogWarning("No item selected for making");
            return;
        }
        
        // 检查材料是否足够
        if (!CheckMaterialsAvailable())
        {
            EventManager.Instance.Publish(new NoticeEvent("材料不足"));
            return;
        }
        
        // 执行制作
        ExecuteMaking();
    }

    /// <summary>
    /// 检查所有材料是否足够
    /// </summary>
    private bool CheckMaterialsAvailable()
    {
        foreach (var material in _materialRequirements)
        {
            if (!PackageModel.Instance.HasEnoughItem(material.ItemId, material.RequiredQuantity))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 执行制作逻辑
    /// 消耗材料并获得产出物品
    /// </summary>
    private void ExecuteMaking()
    {
        var reader = ConfigManager.Instance.GetReader("MakeMenu");
        if (reader == null)
        {
            Debug.LogError("MakeMenu config reader not found");
            return;
        }
        
        // 获取产出物品ID（ProductId列已删除，使用制作配方ID）
        int productId = _currentItemId;
        if (productId <= 0)
        {
            Debug.LogError($"Invalid item ID for making: {_currentItemId}");
            return;
        }
        
        // 消耗材料
        foreach (var material in _materialRequirements)
        {
            PackageModel.Instance.RemoveItem(material.ItemId, material.RequiredQuantity);
        }
        
        // 获取物品配置信息
        var itemConfig = ItemManager.Instance.GetItem(productId);
        string itemName = itemConfig?.Csv.GetValue<string>(productId, "Name", "物品") ?? "物品";
        
        // 根据物品类型决定处理方式
        if (itemConfig != null && itemConfig.IsBuilding())
        {
            // 建筑物：添加到MakeModel的待放置建筑列表，并触发显示在TouchView上
            MakeModel.Instance.AddBuilding(productId);
            
            // 发布建筑物待放置事件，让TouchView显示建筑物图标
            EventManager.Instance.Publish(new BuildingPendingPlaceEvent(productId));

            Debug.Log($"建筑物制作完成，等待放置: {itemName} (ID: {productId})");
        }
        else
        {
            // 非建筑物：正常添加到背包
            PackageModel.Instance.AddItem(productId, 1);
            Debug.Log($"物品添加到背包: {itemName} (ID: {productId})");
        }
        
        // 发送制作成功通知
        EventManager.Instance.Publish(new NoticeEvent($"制作成功：{itemName}"));
        
        // 刷新材料列表显示
        UpdateMaterialList();
        
        // 检查是否需要关闭界面
        bool shouldClose = reader.GetValue<bool>(_currentItemId, "isClose", true);
        if (shouldClose)
        {
            // 关闭制作详情界面
            CloseView();
            
            // 关闭制作菜单界面
            EventManager.Instance.Publish(new MakeMenuCloseEvent());
        }
        
        Debug.Log($"Successfully made item: {itemName} (ID: {productId})");
    }

    // 设置视图可见性
    private void SetViewVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 设置视图位置
    /// 直接使用UI坐标设置位置，无需转换
    /// </summary>
    private void SetViewPosition(Vector2 itemUIPosition)
    {
        if (!(transform is RectTransform rectTransform)) return;
        
        Vector2 offset = new Vector2(-590, 210);
        
        // 直接设置位置，无需任何坐标转换
        rectTransform.anchoredPosition = itemUIPosition + offset;
    }
    // 显示视图
    public void ShowView()
    {
        SetViewVisible(true);
    }

    // 关闭视图
    public void CloseView()
    {
        CancelCloseDelay();
        _currentItemId = -1;
        _materialRequirements.Clear();
        SetViewVisible(false);
    }
}

/// <summary>
/// 材料需求数据结构
/// </summary>
[System.Serializable]
public class MaterialRequirement
{
    public int ItemId;
    public int RequiredQuantity;
}
