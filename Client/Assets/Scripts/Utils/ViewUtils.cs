using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI视图工具类 - 提供统一的UI设置和处理方法
/// 无状态设计，所有方法为静态方法
/// </summary>
public static class ViewUtils
{
    /// <summary>
    /// 快速设置道具UI - 统一处理道具相关的UI组件设置
    /// </summary>
    /// <param name="uiRoot">UI根节点，包含img_icon、txt_name、txt_count等子对象</param>
    /// <param name="itemId">道具ID，<=0表示空槽位</param>
    /// <param name="count">道具数量</param>
    /// <returns>是否成功设置UI</returns>
    public static bool QuickSetItemUI(GameObject uiRoot, int itemId, int count)
    {
        if (uiRoot == null) return false;
        
        // 获取UI组件
        var imgIcon = uiRoot.transform.Find("img_icon")?.GetComponent<Image>();
        var txtName = uiRoot.transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
        var txtCount = uiRoot.transform.Find("txt_count")?.GetComponent<TextMeshProUGUI>();
        
        // 如果道具ID不存在，隐藏所有UI元素
        if (itemId <= 0)
        {
            if (imgIcon != null) imgIcon.gameObject.SetActive(false);
            if (txtName != null) txtName.gameObject.SetActive(false);
            if (txtCount != null) txtCount.gameObject.SetActive(false);
            return true;
        }
        
        // 道具ID存在，显示图标和名称
        if (imgIcon != null) imgIcon.gameObject.SetActive(true);
        if (txtName != null) txtName.gameObject.SetActive(true);
        
        // 获取道具配置信息
        var itemConfig = ItemManager.Instance.GetItem(itemId);
        string itemName = itemConfig?.Csv.GetValue<string>(itemId, "Name", $"Item_{itemId}") ?? $"Item_{itemId}";
        
        // 设置道具图标
        if (imgIcon != null)
        {
            string iconPath = itemConfig?.Csv.GetValue<string>(itemId, "IconPath", "") ?? "";
            if (!string.IsNullOrEmpty(iconPath))
            {
                // 移除扩展名，Unity Resources.Load不需要扩展名
                string pathWithoutExtension = System.IO.Path.ChangeExtension(iconPath, null);
                var texture = Resources.Load<Texture2D>(pathWithoutExtension);
                if (texture != null)
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    imgIcon.sprite = sprite;
                }
            }
        }
        
        // 设置道具名称
        if (txtName != null)
        {
            txtName.text = itemName;
        }
        
        // 判断数量是否显示
        if (count <= 0)
        {
            // 数量不存在，隐藏数量组件
            if (txtCount != null) txtCount.gameObject.SetActive(false);
        }
        else
        {
            // 数量存在，显示并设置数量
            if (txtCount != null)
            {
                txtCount.gameObject.SetActive(true);
                txtCount.text = count.ToString();
            }
        }
        
        return true;
    }
} 