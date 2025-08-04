using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// PackageView - 背包UI视图

public class PackageView : BaseView
{
    private void Start()
    {
        InitializePackageList();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void InitializePackageList()
    {
        UIList uiList = GetUIList();
        if (uiList == null) return;

        CreatePackageItems(uiList);
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
        // 获取UI组件
        var imgIcon = item.transform.Find("img_icon")?.GetComponent<Image>();
        var txtName = item.transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
        var txtCount = item.transform.Find("txt_count")?.GetComponent<TextMeshProUGUI>();
        var button = item.GetComponent<Button>();

        // 设置按钮点击事件
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // 清除之前的监听器
            button.onClick.AddListener(() => OnSlotClicked(slotIndex));
        }

        // 如果道具不存在，隐藏所有UI元素
        if (packageItem == null)
        {
            if (imgIcon != null) imgIcon.gameObject.SetActive(false);
            if (txtName != null) txtName.gameObject.SetActive(false);
            if (txtCount != null) txtCount.gameObject.SetActive(false);
            return;
        }

        // 道具存在，显示并设置UI元素
        if (imgIcon != null) imgIcon.gameObject.SetActive(true);
        if (txtName != null) txtName.gameObject.SetActive(true);
        if (txtCount != null) txtCount.gameObject.SetActive(true);

        // 获取道具配置信息
        var itemConfig = ItemManager.Instance.GetItem(packageItem.itemId);
        string itemName = itemConfig?.Csv.GetValue<string>(packageItem.itemId, "Name", $"Item_{packageItem.itemId}") ?? $"Item_{packageItem.itemId}";

        // 设置道具图标
        if (imgIcon != null)
        {
            string iconPath = itemConfig?.Csv.GetValue<string>(packageItem.itemId, "IconPath", "") ?? "";
            LoadAndSetSprite(imgIcon, iconPath, false); // false表示从PNG纹理创建Sprite
        }

        // 设置道具名称
        if (txtName != null)
        {
            txtName.text = itemName;
        }

        // 设置道具数量
        if (txtCount != null)
        {
            txtCount.text = packageItem.count.ToString();
        }
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

    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<ItemChangeEvent>(OnItemChanged);
    }

    private void OnItemChanged(ItemChangeEvent eventData)
    {
        // 背包变化时刷新UI
        InitializePackageList();
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
