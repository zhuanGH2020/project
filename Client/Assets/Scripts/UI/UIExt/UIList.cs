// Last modified: 2024-12-19 14:30:15
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[DisallowMultipleComponent]
[AddComponentMenu("UI/UIList")]
[ExecuteInEditMode]
public class UIList : MonoBehaviour
{
    public GameObject TemplateItem;
    
    public enum Arrangement
    {
        Horizontal,
        Vertical
    }
    
    [Header("Arrangement Settings")]
    public Arrangement arrangement = Arrangement.Horizontal;
    public int SpaceWidth = 0;
    public int SpaceHeight = 0;
    
    [HideInInspector]
    public List<RectTransform> listItem = new List<RectTransform>();
    
    private RectTransform rectTrans;
    
    void Awake()
    {
        rectTrans = this.transform as RectTransform;
        if (rectTrans != null)
        {
            rectTrans.pivot = new Vector2(0, 1);
        }
        if (TemplateItem != null)
        {
            TemplateItem.SetActive(false);
        }
        
    }
    
    public GameObject AddListItem()
    {
        if (TemplateItem == null)
        {
            Debug.LogError("TemplateItem is null!");
            return null;
        }
        if(TemplateItem.activeSelf)
        {
            TemplateItem.SetActive(false);
        }
        
        GameObject newItem = Instantiate(TemplateItem, transform);
        newItem.SetActive(true);
        newItem.transform.SetParent(transform, false);
        newItem.name = TemplateItem.name + "_" + listItem.Count;
        var rect = newItem.transform as RectTransform;
        LayoutRebuilder.MarkLayoutForRebuild(rect);
        RectTransform rectItem = newItem.transform as RectTransform;
        listItem.Add(rectItem);
        
        Reposition();
        return newItem;
    }
    
    public void RemoveAll()
    {
        for (int i = listItem.Count - 1; i >= 0; i--)
        {
            if (listItem[i] != null)
            {
                DestroyImmediate(listItem[i].gameObject);
            }
        }
        listItem.Clear();
    }
    
    public int GetItemCount()
    {
        return listItem.Count;
    }
    
    public void Reposition()
    {
        if (rectTrans == null || listItem == null || listItem.Count == 0)
            return;
            
        Vector3 currentPos = Vector3.zero;
        
        for (int i = 0; i < listItem.Count; i++)
        {
            if (listItem[i] == null) continue;
            
            listItem[i].anchorMin = listItem[i].anchorMax = new Vector2(0, 1);
            listItem[i].pivot = new Vector2(0, 1);
            
            listItem[i].localPosition = currentPos;
            
            if (arrangement == Arrangement.Horizontal)
            {
                currentPos.x += listItem[i].rect.width + SpaceWidth;
            }
            else if (arrangement == Arrangement.Vertical)
            {
                currentPos.y -= listItem[i].rect.height + SpaceHeight;
            }
        }
    }
}