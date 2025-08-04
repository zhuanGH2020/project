using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 资源工具类 - 提供纯功能性的资源加载方法
/// 无状态设计，所有方法为静态方法
/// </summary>
public static class ResourceUtils
{
    #region 基础资源加载
    
    /// <summary>
    /// 加载资源
    /// </summary>
    public static T LoadResource<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path)) return null;
        return ResourceManager.Instance.Load<T>(path);
    }
    
    public static Sprite LoadSprite(string path) => LoadResource<Sprite>(path);
    public static GameObject LoadGameObject(string path) => LoadResource<GameObject>(path);
    public static AudioClip LoadAudioClip(string path) => LoadResource<AudioClip>(path);
    
    #endregion
    
    #region 图片设置工具
    
    /// <summary>
    /// 加载并设置图片到Image组件
    /// </summary>
    public static bool LoadAndSetSprite(Image image, string spritePath, List<Object> cache = null)
    {
        if (image == null || string.IsNullOrEmpty(spritePath)) return false;
            
        var sprite = LoadSprite(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            cache?.Add(sprite);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 从配置加载并设置物品图标
    /// </summary>
    public static bool LoadAndSetItemIcon(Image image, int itemId, List<Object> cache = null)
    {
        if (image == null) return false;
        
        string iconPath = GetItemIconPath(itemId);
        return LoadAndSetSprite(image, iconPath, cache);
    }
    
    #endregion
    
    #region 资源释放工具
    
    /// <summary>
    /// 释放资源列表
    /// </summary>
    public static void ReleaseResources(List<Object> resources)
    {
        if (resources == null) return;
        
        foreach (var resource in resources)
        {
            if (resource != null)
            {
                ResourceManager.Instance.Release(resource);
            }
        }
        resources.Clear();
    }
    
    /// <summary>
    /// 释放单个资源
    /// </summary>
    public static void ReleaseResource(Object resource)
    {
        if (resource != null)
        {
            ResourceManager.Instance.Release(resource);
        }
    }
    
    #endregion
    
    #region 配置工具
    
    /// <summary>
    /// 获取物品图标路径
    /// </summary>
    public static string GetItemIconPath(int itemId)
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        return reader?.GetValue<string>(itemId, "IconPath", "") ?? "";
    }
    
    /// <summary>
    /// 获取物品名称
    /// </summary>
    public static string GetItemName(int itemId)
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        return reader?.GetValue<string>(itemId, "Name", $"Item_{itemId}") ?? $"Item_{itemId}";
    }
    
    #endregion
} 