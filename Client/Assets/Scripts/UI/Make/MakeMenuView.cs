using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 制作菜单视图 - 显示具体制作类型的详细内容
/// 响应制作类型选择事件，动态加载和显示对应的制作配方界面
/// </summary>
public class MakeMenuView : BaseView
{
    private TextMeshProUGUI txt_title;
    
    private int _currentTypeId = -1; // 当前选中的制作类型ID
    private const string TITLE_PREFIX = "制作 - "; // 标题前缀常量，避免字符串重复分配

    void Start()
    {
        InitializeView();
        SubscribeEvents();
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // 初始化视图组件，提供自动查找和默认状态设置
    private void InitializeView()
    {
        // 自动查找UI组件（如果未在Inspector中指定）
        if (txt_title == null)
            txt_title = transform.Find("txt_title")?.GetComponent<TextMeshProUGUI>();

        // 初始状态隐藏菜单，避免未初始化时的显示问题
        SetMenuVisible(false);
    }

    // 订阅制作系统相关事件
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<MakeMenuOpenEvent>(OnMakeMenuOpen);
        EventManager.Instance.Subscribe<MakeMenuCloseEvent>(OnMakeMenuClose);
        EventManager.Instance.Subscribe<MakeTypeSelectedEvent>(OnMakeTypeSelected);
        EventManager.Instance.Subscribe<ClickOutsideUIEvent>(OnClickOutsideUI);
    }

    // 取消订阅事件，防止内存泄漏
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<MakeMenuOpenEvent>(OnMakeMenuOpen);
        EventManager.Instance.Unsubscribe<MakeMenuCloseEvent>(OnMakeMenuClose);
        EventManager.Instance.Unsubscribe<MakeTypeSelectedEvent>(OnMakeTypeSelected);
        EventManager.Instance.Unsubscribe<ClickOutsideUIEvent>(OnClickOutsideUI);
    }

    // 处理制作菜单打开事件
    private void OnMakeMenuOpen(MakeMenuOpenEvent eventData)
    {
        if (eventData == null || eventData.TypeId <= 0)
        {
            return;
        }
        
        _currentTypeId = eventData.TypeId;
        ShowMakeMenu(eventData.TypeId);
    }

    // 处理制作菜单关闭事件
    private void OnMakeMenuClose(MakeMenuCloseEvent eventData)
    {
        CloseMakeMenu();
    }

    // 处理制作类型选择事件，更新菜单标题
    private void OnMakeTypeSelected(MakeTypeSelectedEvent eventData)
    {
        if (eventData != null && !string.IsNullOrEmpty(eventData.TypeName))
        {
            UpdateMenuTitle(eventData.TypeName);
        }else{
            Debug.LogWarning("Invalid OnMakeTypeSelected data");
        }
    }

    // 处理点击非UI区域事件
    private void OnClickOutsideUI(ClickOutsideUIEvent eventData)
    {
        if (gameObject.activeInHierarchy)
        {
            CloseMakeMenu();
        }
    }

    /// <summary>
    /// 显示制作菜单的完整流程
    /// 包括数据验证、界面显示、内容加载和状态更新
    /// </summary>
    private void ShowMakeMenu(int typeId)
    {
        var makeTypeData = MakeModel.Instance.GetMakeTypeData(typeId);
        if (makeTypeData == null)
        {
            return;
        }

        SetMenuVisible(true);
        UpdateMenuTitle(makeTypeData.typeName);
        LoadMenuContent(typeId);
    }

    // 更新菜单标题，使用常量前缀避免字符串分配
    private void UpdateMenuTitle(string typeName)
    {
        if (txt_title != null && !string.IsNullOrEmpty(typeName))
        {
            txt_title.text = TITLE_PREFIX + typeName;
        }
    }

    /// <summary>
    /// 根据制作类型ID加载对应的菜单内容
    /// 清理现有内容后动态创建新的制作配方UI元素
    /// </summary>
    private void LoadMenuContent(int typeId)
    {
        UIList uiList = GetUIList();
        if (uiList == null)
        {
            return;
        }
        
        uiList.RemoveAll();
        
        // 根据typeId创建制作菜单项
        CreateMakeMenuItems(uiList, typeId);
    }
    
    // 获取UIList组件，查找名为list_type的UIList
    private UIList GetUIList()
    {
        Transform listTransform = transform.Find("list_type") ?? FindChildWithUIList();
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
    /// 根据制作类型ID创建制作菜单项
    /// 从MakeMenu.csv配置表中读取对应类型的制作配方数据
    /// </summary>
    private void CreateMakeMenuItems(UIList uiList, int typeId)
    {
        // 获取MakeMenu配置读取器
        var reader = ConfigManager.Instance.GetReader("MakeMenu");
        if (reader == null)
        {
            return;
        }
        
        // 遍历所有配置项，筛选对应类型的制作配方
        foreach (var key in reader.GetAllKeysOfType<int>())
        {
            int configTypeId = reader.GetValue<int>(key, "Type", 0);
            if (configTypeId == typeId)
            {
                GameObject item = uiList.AddListItem();
                if (item != null)
                {
                    SetupMakeMenuItem(item, reader, key);
                }
            }
        }
    }
    
    // 设置制作菜单项的UI内容和交互
    private void SetupMakeMenuItem(GameObject item, ConfigReader reader, object key)
    {
        // 获取配置数据
        string itemName = reader.GetValue<string>(key, "Name", "Unknown");
        string description = reader.GetValue<string>(key, "Description", "");
        string imagePath = reader.GetValue<string>(key, "Image", "");
        
        // 设置物品名称
        var txtName = item.transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
        if (txtName != null)
        {
            txtName.text = itemName;
        }
        
        // 设置描述
        var txtDescription = item.transform.Find("txt_description")?.GetComponent<TextMeshProUGUI>();
        if (txtDescription != null)
        {
            txtDescription.text = description;
        }
        
        // 设置物品图标
        var imgIcon = item.transform.Find("img_icon")?.GetComponent<Image>();
        if (imgIcon != null && !string.IsNullOrEmpty(imagePath))
        {
            // TODO: 可以通过ResourceManager加载图片资源
            // var sprite = ResourceManager.Instance.LoadSprite(imagePath);
            // if (sprite != null) imgIcon.sprite = sprite;
        }
        
        // 设置材料需求显示
        SetupMaterialRequirements(item, reader, key);
        
        // 设置按钮交互
        var button = item.GetComponent<Button>();
        if (button != null)
        {
            int itemId = reader.GetValue<int>(key, "Id", 0);
            button.onClick.AddListener(() => OnMakeItemClick(itemId, itemName));
        }
        
        // 设置悬停事件处理
        SetupHoverEvents(item, reader, key);
    }
    
    // 设置材料需求显示
    private void SetupMaterialRequirements(GameObject item, ConfigReader reader, object key)
    {
        // 获取材料需求数据
        string material1 = reader.GetValue<string>(key, "Material1", "");
        string material2 = reader.GetValue<string>(key, "Material2", "");
        string material3 = reader.GetValue<string>(key, "Material3", "");
        
        // 设置材料1
        SetupMaterialDisplay(item, "material1", material1);
        // 设置材料2
        SetupMaterialDisplay(item, "material2", material2);
        // 设置材料3
        SetupMaterialDisplay(item, "material3", material3);
    }
    
    /// <summary>
    /// 设置单个材料显示
    /// 材料格式: "物品ID;数量" 例如: "1000;3"
    /// </summary>
    private void SetupMaterialDisplay(GameObject item, string materialSlotName, string materialData)
    {
        Transform materialSlot = item.transform.Find(materialSlotName);
        if (materialSlot == null) return;
        
        if (string.IsNullOrEmpty(materialData))
        {
            materialSlot.gameObject.SetActive(false);
            return;
        }
        
        // 解析材料数据: "物品ID;数量"
        string[] parts = materialData.Split(';');
        if (parts.Length != 2) return;
        
        if (int.TryParse(parts[0], out int materialId) && int.TryParse(parts[1], out int quantity))
        {
            materialSlot.gameObject.SetActive(true);
            
            // 设置材料数量文本
            var txtQuantity = materialSlot.Find("txt_quantity")?.GetComponent<TextMeshProUGUI>();
            if (txtQuantity != null)
            {
                txtQuantity.text = quantity.ToString();
            }
            
            // TODO: 可以根据materialId设置材料图标和名称
            // 可以通过Item配置表获取材料的详细信息
        }
    }
    
    /// <summary>
    /// 设置物品悬停事件处理
    /// 添加鼠标悬停监听，悬停时打开制作详情视图，离开时关闭详情视图
    /// </summary>
    private void SetupHoverEvents(GameObject item, ConfigReader reader, object key)
    {
        int itemId = reader.GetValue<int>(key, "Id", 0);
        
        // 添加EventTrigger组件处理悬停事件
        var eventTrigger = item.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = item.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 添加鼠标进入事件
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => {
            // 获取item的UI位置
            Vector2 itemPosition = GetItemUIPosition(item);
            OnItemHover(itemId, itemPosition);
        });
        eventTrigger.triggers.Add(pointerEnter);
        
        // 添加鼠标离开事件
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => {
            OnItemHoverExit();
        });
        eventTrigger.triggers.Add(pointerExit);
    }
    
    /// <summary>
    /// 获取item的UI位置
    /// 直接返回item的anchored position
    /// </summary>
    private Vector2 GetItemUIPosition(GameObject item)
    {
        RectTransform rectTransform = item.transform as RectTransform;
        if (rectTransform == null) return Vector2.zero;
        
        // 直接返回UI坐标，无需转换
        return rectTransform.anchoredPosition;
    }

    // 处理物品悬停事件
    private void OnItemHover(int itemId, Vector2 itemPosition)
    {
        EventManager.Instance.Publish(new MakeDetailOpenEvent(itemId, itemPosition));
    }

    // 处理物品悬停离开事件
    private void OnItemHoverExit()
    {
        EventManager.Instance.Publish(new MakeDetailCloseEvent());
    }

    // 处理制作物品点击事件
    private void OnMakeItemClick(int itemId, string itemName)
    {
        Debug.Log($"点击制作物品: {itemName} (ID: {itemId})");
        
        // TODO: 可以触发制作事件或显示制作确认界面
        // EventManager.Instance.Publish(new MakeItemEvent(itemId));
    }

    // 设置菜单的可见性
    private void SetMenuVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 关闭制作菜单并清理状态
    /// 重置选择状态，隐藏界面，通知Model清除选择
    /// </summary>
    public void CloseMakeMenu()
    {
        _currentTypeId = -1;
        SetMenuVisible(false);
        MakeModel.Instance.ClearSelection();
    }
}
