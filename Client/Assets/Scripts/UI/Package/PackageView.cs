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
        
        var allItems = PackageModel.Instance.GetAllItems();
        
        foreach (var packageItem in allItems)
        {
            GameObject item = uiList.AddListItem();
            if (item == null) continue;

            SetupItemUI(item, packageItem);
        }
    }

    private void SetupItemUI(GameObject item, PackageItem packageItem)
    {
        // 获取道具配置信息
        var itemConfig = ItemManager.Instance.GetItem(packageItem.itemId);
        string itemName = itemConfig?.Csv.GetValue<string>(packageItem.itemId, "Name", $"Item_{packageItem.itemId}") ?? $"Item_{packageItem.itemId}";

        // 设置道具名称
        var txtName = item.transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
        if (txtName != null)
        {
            txtName.text = itemName;
        }

        // 设置道具数量
        var txtCount = item.transform.Find("txt_count")?.GetComponent<TextMeshProUGUI>();
        if (txtCount != null)
        {
            txtCount.text = packageItem.count.ToString();
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
