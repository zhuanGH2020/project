using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MakeView : MonoBehaviour
{
    void Start()
    {
        InitializeMakeList();
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
            toggle.onValueChanged.AddListener((isOn) => OnItemToggle(typeId, typeName, isOn));
        }
        
        // 设置Button交互（如果有的话）
        var button = item.GetComponent<Button>();
        if (button != null)
        {
            int typeId = makeType.typeId;
            button.onClick.AddListener(() => OnItemClick(typeId));
        }
    }
    
    private void OnItemToggle(int typeId, string typeName, bool isOn)
    {
        if (isOn)
        {
            MakeModel.Instance.SelectMakeType(typeId);
        }
    }
    
    private void OnItemClick(int typeId)
    {
        MakeModel.Instance.SelectMakeType(typeId);
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