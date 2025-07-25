using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeView : MonoBehaviour
{
    void Start()
    {
        InitializeMakeList();
    }
    
    private void InitializeMakeList()
    {
        Transform listMakeTransform = transform.Find("list_make");
        if (listMakeTransform == null)
        {
            listMakeTransform = FindChildWithUIList();
            if (listMakeTransform == null)
                return;
        }
        
        UIList uiList = listMakeTransform.GetComponent<UIList>();
        if (uiList == null)
        {
            uiList = listMakeTransform.GetComponentInChildren<UIList>();
            if (uiList == null)
                return;
        }
        
        if (!ConfigManager.Instance.LoadConfig("Make", "Configs/make"))
            return;
        
        var reader = ConfigManager.Instance.GetReader("Make");
        if (reader == null)
            return;
        
        foreach (var id in reader.GetAllIds())
        {
            uiList.AddListItem();
        }
    }
    
    private Transform FindChildWithUIList()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            UIList uiList = child.GetComponent<UIList>();
            if (uiList != null)
            {
                return child;
            }
            
            uiList = child.GetComponentInChildren<UIList>();
            if (uiList != null)
            {
                return uiList.transform;
            }
        }
        return null;
    }
}
