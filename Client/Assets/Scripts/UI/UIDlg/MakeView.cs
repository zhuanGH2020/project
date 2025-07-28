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
        
        var reader = LoadMakeConfig();
        if (reader == null) return;
        
        CreateMakeItems(uiList, reader);
    }
    
    private UIList GetUIList()
    {
        Transform listTransform = transform.Find("list_make") ?? FindChildWithUIList();
        return listTransform?.GetComponent<UIList>() ?? listTransform?.GetComponentInChildren<UIList>();
    }

    private ConfigReader LoadMakeConfig()
    {
        return ConfigManager.Instance.GetReader("Make");
    }
    
    private void CreateMakeItems(UIList uiList, ConfigReader reader)
    {
        foreach (var id in reader.GetAllKeysOfType<int>())
        {
            GameObject item = uiList.AddListItem();
            if (item == null) continue;
            
            string name = reader.GetValue<string>(id, "Name", "");
            
            var txtName = item.transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
            if (txtName != null)
            {
                txtName.text = name;
            }
            
            var toggle = item.GetComponent<Toggle>();
            if (toggle != null)
            {
                string itemName = name;
                toggle.onValueChanged.AddListener((isOn) => OnItemToggle(itemName, isOn));
            }
        }
    }
    
    private void OnItemToggle(string name, bool isOn)
    {
        Debug.Log($"Toggle item: {name}, isOn: {isOn}");
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