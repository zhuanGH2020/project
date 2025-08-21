using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MakeView : BaseView
{
    private Toggle _currentSelectedToggle;

    protected override void Start()
    {
        base.Start();
        InitializeMakeList();
        SubscribeEvents();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeEvents();
    }
    
    private void InitializeMakeList()
    {
        UIList uiList = GetUIList();
        if (uiList == null) return;
        
        CreateMakeItems(uiList);
    }
    
    private UIList GetUIList()
    {
        Transform listTransform = transform.Find("list_make") ?? FindChildWithUIList();
        return listTransform?.GetComponent<UIList>() ?? listTransform?.GetComponentInChildren<UIList>();
    }
    
    private void CreateMakeItems(UIList uiList)
    {
        // 从Model获取制作类型数据
        var makeTypes = MakeModel.Instance.MakeTypes;
        
        foreach (var makeType in makeTypes)
        {
            GameObject item = uiList.AddListItem();
            if (item == null) continue;
            
            SetupMakeItem(item, makeType);
        }
    }
    
    private void SetupMakeItem(GameObject item, MakeTypeData makeType)
    {
        // 设置制作类型名称
        var txtName = item.transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
        if (txtName != null)
        {
            txtName.text = makeType.typeName;
        }
        
        // 设置Toggle交互
        var toggle = item.GetComponent<Toggle>();
        if (toggle != null)
        {
            int typeId = makeType.typeId;
            string typeName = makeType.typeName;
            
            toggle.onValueChanged.AddListener((isOn) => OnItemToggle(typeId, typeName, isOn, toggle));
        }
    }
    
    private void OnItemToggle(int typeId, string typeName, bool isOn, Toggle toggle)
    {
        if (isOn)
        {
            // 记录当前选中的Toggle
            _currentSelectedToggle = toggle;
            // Toggle变为选中状态，选择新的制作类型
            MakeModel.Instance.SelectMakeType(typeId);
        }
        else
        {
            // Toggle变为未选中状态，检查是否是当前选中的类型
            if (MakeModel.Instance.SelectedTypeId == typeId)
            {
                // 点击已选中的item取消选中，关闭菜单
                _currentSelectedToggle = null;
                MakeModel.Instance.CloseMakeMenu();
            }
        }
    }

    // 订阅事件
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<MakeMenuCloseEvent>(OnMakeMenuClose);
    }

    // 取消订阅事件
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<MakeMenuCloseEvent>(OnMakeMenuClose);
    }

    // 处理制作菜单关闭事件
    private void OnMakeMenuClose(MakeMenuCloseEvent eventData)
    {
        // 取消当前选中Toggle的选中状态
        if (_currentSelectedToggle != null)
        {
            _currentSelectedToggle.SetIsOnWithoutNotify(false);
            _currentSelectedToggle = null;
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