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
        }
        else
        {
            ViewUtils.QuickSetItemUI(item, 0, 0); // 空槽位
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
