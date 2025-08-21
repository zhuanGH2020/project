using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// PackageView - 背包UI视图

public class PackageView : BaseView
{
    [Header("装备槽位")]
    [SerializeField] private Transform cellHead;    // 头部装备槽
    [SerializeField] private Transform cellBody;    // 身体装备槽
    [SerializeField] private Transform cellHand;    // 手部装备槽
    
    protected override void Start()
    {
        base.Start();
        InitializePackageList();
        InitializeEquipSlots();
        SubscribeEvents();
        
        // 延迟一帧检查装备状态同步，确保Player已经完全初始化
        StartCoroutine(CheckEquipmentSync());
    }
    
    /// <summary>
    /// 检查装备状态同步
    /// </summary>
    private IEnumerator CheckEquipmentSync()
    {
        // 等待一帧确保Player完全初始化
        yield return null;
        
        // 检查装备状态一致性（用于调试）
        EquipManager.Instance.SyncPlayerEquipmentState();
        
        // 更新装备槽位显示
        UpdateAllEquipSlots();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeEvents();
    }

    private void InitializePackageList()
    {
        UIList uiList = GetUIList();
        if (uiList == null) return;

        CreatePackageItems(uiList);
    }
    
    /// <summary>
    /// 初始化装备槽位
    /// </summary>
    private void InitializeEquipSlots()
    {
        // 查找装备槽位节点
        if (cellHead == null) cellHead = transform.Find("cell_head");
        if (cellBody == null) cellBody = transform.Find("cell_body");
        if (cellHand == null) cellHand = transform.Find("cell_hand");
        
        // 更新装备槽位显示
        UpdateEquipSlot(EquipPart.Head, cellHead);
        UpdateEquipSlot(EquipPart.Body, cellBody);
        UpdateEquipSlot(EquipPart.Hand, cellHand);
    }

    private UIList GetUIList()
    {
        Transform listTransform = transform.Find("list_package") ?? FindChildWithUIList();
        return listTransform?.GetComponent<UIList>() ?? listTransform?.GetComponentInChildren<UIList>();
    }

    private void CreatePackageItems(UIList uiList)
    {
        uiList.RemoveAll();
        
        for (int i = 0; i < PackageModel.MAX_SLOTS; i++)
        {
            GameObject item = uiList.AddListItem();
            if (item == null) continue;

            // 根据格子索引获取对应的道具
            PackageItem packageItem = PackageModel.Instance.GetItemByIndex(i);
            SetupItemUI(item, packageItem, i);
        }
    }

    private void SetupItemUI(GameObject item, PackageItem packageItem, int slotIndex)
    {
        // 设置按钮点击事件
        var button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // 清除之前的监听器
            button.onClick.AddListener(() => OnSlotClicked(slotIndex));
        }

        // 使用ViewUtils统一设置道具UI
        if (packageItem != null)
        {
            ViewUtils.QuickSetItemUI(item, packageItem.itemId, packageItem.count);
            
            // 为装备类型物品添加右键点击事件
            var itemConfig = ItemManager.Instance.GetItem(packageItem.itemId);
            if (itemConfig != null && itemConfig.IsEquip())
            {
                // 添加右键点击事件监听器
                StartCoroutine(SetupRightClickForItem(item, slotIndex));
            }
        }
        else
        {
            ViewUtils.QuickSetItemUI(item, 0, 0); // 空槽位
        }
    }
    
    /// <summary>
    /// 为物品设置右键点击事件
    /// </summary>
    /// <param name="item">物品GameObject</param>
    /// <param name="slotIndex">格子索引</param>
    private IEnumerator SetupRightClickForItem(GameObject item, int slotIndex)
    {
        // 等待一帧确保UI完全初始化
        yield return null;
        
        // 添加EventTrigger组件来处理右键点击
        var eventTrigger = item.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = item.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 清除现有的事件
        eventTrigger.triggers.Clear();
        
        // 创建右键点击事件
        var rightClickEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        rightClickEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
        rightClickEntry.callback.AddListener((data) => {
            var pointerEventData = (UnityEngine.EventSystems.PointerEventData)data;
            if (pointerEventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
            {
                OnItemRightClicked(slotIndex);
            }
        });
        
        eventTrigger.triggers.Add(rightClickEntry);
    }

    private void OnSlotClicked(int slotIndex)
    {
        // 检查格子是否有道具
        PackageItem slotItem = PackageModel.Instance.GetItemByIndex(slotIndex);
        
        if (slotItem != null)
        {
            // 格子有道具，选中该道具
            bool success = PackageModel.Instance.SelectItemByIndex(slotIndex);
        }
        else
        {
            // 格子没有道具，尝试将选中的道具放到这个格子
            if (PackageModel.Instance.HasSelectedItem())
            {
                bool success = PackageModel.Instance.PlaceSelectedItemToSlot(slotIndex);
            }
        }
    }
    
    /// <summary>
    /// 处理物品右键点击事件
    /// </summary>
    /// <param name="slotIndex">格子索引</param>
    private void OnItemRightClicked(int slotIndex)
    {
        PackageItem slotItem = PackageModel.Instance.GetItemByIndex(slotIndex);
        if (slotItem == null) return;
        
        // 检查物品类型
        var itemConfig = ItemManager.Instance.GetItem(slotItem.itemId);
        if (itemConfig == null) return;
        
        if (itemConfig.IsEquip())
        {
            // 装备类型，尝试装备
            TryEquipItem(slotItem.itemId);
        }
        else
        {
            // 其他类型，保持原有逻辑（暂时不处理）
            Debug.Log($"[PackageView] 物品 {slotItem.itemId} 不是装备类型，右键功能待实现");
        }
    }
    
    /// <summary>
    /// 尝试装备物品
    /// </summary>
    /// <param name="itemId">物品ID</param>
    private void TryEquipItem(int itemId)
    {
        // 获取装备配置
        var equipReader = ConfigManager.Instance.GetReader("Equip");
        if (equipReader == null || !equipReader.HasKey(itemId))
        {
            Debug.LogWarning($"[PackageView] 无法获取装备 {itemId} 的配置");
            return;
        }
        
        // 根据装备部位尝试装备
        EquipPart equipPart = equipReader.GetValue<EquipPart>(itemId, "Type", EquipPart.None);
        bool equipSuccess = EquipManager.Instance.EquipItem(itemId, equipPart);
        
        if (equipSuccess)
        {
            // 装备成功，更新装备槽位显示
            UpdateEquipSlot(equipPart);
            Debug.Log($"[PackageView] 成功装备物品 {itemId} 到 {equipPart} 部位");
        }
        else
        {
            Debug.LogWarning($"[PackageView] 装备物品 {itemId} 到 {equipPart} 部位失败");
        }
    }
    
    /// <summary>
    /// 更新指定装备槽位的显示
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    private void UpdateEquipSlot(EquipPart equipPart)
    {
        Transform slotTransform = GetEquipSlotTransform(equipPart);
        if (slotTransform != null)
        {
            UpdateEquipSlot(equipPart, slotTransform);
        }
    }
    
    /// <summary>
    /// 更新装备槽位显示
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    /// <param name="slotTransform">槽位Transform</param>
    private void UpdateEquipSlot(EquipPart equipPart, Transform slotTransform)
    {
        if (slotTransform == null) return;
        
        // 直接获取该部位的装备ID
        int equipId = EquipManager.Instance.GetEquippedItemId(equipPart);
        
        // 设置装备槽位UI
        if (equipId > 0)
        {
            ViewUtils.QuickSetItemUI(slotTransform.gameObject, equipId, 1);
            
            // 设置装备槽位的点击事件（卸下装备）
            var button = slotTransform.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnEquipSlotClicked(equipPart));
            }
        }
        else
        {
            // 空槽位
            ViewUtils.QuickSetItemUI(slotTransform.gameObject, 0, 0);
            
            // 清空点击事件
            var button = slotTransform.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
    

    
    /// <summary>
    /// 装备槽位被点击（卸下装备）
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    private void OnEquipSlotClicked(EquipPart equipPart)
    {
        if (EquipManager.Instance.HasEquippedItem(equipPart))
        {
            bool unequipSuccess = EquipManager.Instance.UnequipItem(equipPart);
            if (unequipSuccess)
            {
                // 卸下成功，更新装备槽位显示
                UpdateEquipSlot(equipPart);
                Debug.Log($"[PackageView] 成功卸下 {equipPart} 部位的装备");
            }
        }
    }
    
    /// <summary>
    /// 根据装备部位获取对应的槽位Transform
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    /// <returns>槽位Transform</returns>
    private Transform GetEquipSlotTransform(EquipPart equipPart)
    {
        switch (equipPart)
        {
            case EquipPart.Head:
                return cellHead;
            case EquipPart.Body:
                return cellBody;
            case EquipPart.Hand:
                return cellHand;
            default:
                return null;
        }
    }

    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);
        EventManager.Instance.Subscribe<PackageRefreshEvent>(OnPackageRefresh);
        
        // 订阅装备变化事件
        EquipManager.Instance.OnEquipmentChanged += OnEquipmentChanged;
        
        // 订阅右键点击事件，用于取消选中物品和装备物品
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnRightClick += OnRightClick;
        }
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<ItemChangeEvent>(OnItemChanged);
        EventManager.Instance.Unsubscribe<PackageRefreshEvent>(OnPackageRefresh);
        
        // 取消订阅装备变化事件
        EquipManager.Instance.OnEquipmentChanged -= OnEquipmentChanged;
        
        // 取消订阅右键点击事件
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnRightClick -= OnRightClick;
        }
    }

    private void OnItemChanged(ItemChangeEvent eventData)
    {
        // 背包变化时刷新UI
        InitializePackageList();
    }
    
    private void OnPackageRefresh(PackageRefreshEvent eventData)
    {
        // 背包刷新时更新UI
        InitializePackageList();
        // 同时更新装备槽位
        UpdateAllEquipSlots();
        Debug.Log($"[PackageView] Package refreshed with {eventData.ItemCount} items, UI updated");
    }
    
    /// <summary>
    /// 处理装备变化事件
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    /// <param name="equipId">装备ID</param>
    /// <param name="isEquipped">是否装备</param>
    private void OnEquipmentChanged(EquipPart equipPart, int equipId, bool isEquipped)
    {
        // 更新对应装备槽位的显示
        UpdateEquipSlot(equipPart);
        
        if (isEquipped)
        {
            Debug.Log($"[PackageView] Equipment {equipId} equipped to {equipPart}");
        }
        else
        {
            Debug.Log($"[PackageView] Equipment {equipId} unequipped from {equipPart}");
        }
    }
    
    /// <summary>
    /// 更新所有装备槽位
    /// </summary>
    private void UpdateAllEquipSlots()
    {
        UpdateEquipSlot(EquipPart.Head);
        UpdateEquipSlot(EquipPart.Body);
        UpdateEquipSlot(EquipPart.Hand);
    }
    

    
    /// <summary>
    /// 处理右键点击事件 - 取消选中物品或装备物品
    /// </summary>
    private void OnRightClick()
    {
        // 检查是否有选中的物品
        if (PackageModel.Instance.HasSelectedItem())
        {
            // 取消选中物品
            PackageModel.Instance.UnselectItem();
        }
        else
        {
            // 没有选中物品时，检查鼠标是否在背包物品上
            // 这里可以通过射线检测或其他方式实现
            // 暂时保持原有逻辑
        }
    }

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
}
