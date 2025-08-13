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
    
    /// <summary>
    /// 从PNG图片文件创建Sprite
    /// </summary>
    public static Sprite LoadSpriteFromTexture(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        var texture = LoadResource<Texture2D>(path);
        if (texture != null)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }
    
    #endregion
    
    #region 图片设置工具
    
    /// <summary>
    /// 加载并设置图片到Image组件
    /// </summary>
    /// <param name="image">目标Image组件</param>
    /// <param name="imagePath">图片路径（相对于Resources目录）</param>
    /// <param name="isAtlas">是否为图集资源，true=直接加载Sprite，false=从Texture2D创建Sprite</param>
    /// <param name="cache">资源缓存列表，用于后续释放</param>
    /// <returns>是否成功设置</returns>
    public static bool LoadAndSetSprite(Image image, string imagePath, bool isAtlas = true, List<Object> cache = null)
    {
        if (image == null || string.IsNullOrEmpty(imagePath)) return false;
        
        // 移除扩展名，Unity Resources.Load不需要扩展名
        string pathWithoutExtension = System.IO.Path.ChangeExtension(imagePath, null);
            
        if (isAtlas)
        {
            // 图集模式：直接加载Sprite
            var sprite = LoadSprite(pathWithoutExtension);
            if (sprite != null)
            {
                image.sprite = sprite;
                cache?.Add(sprite);
                return true;
            }
        }
        else
        {
            // 纹理模式：从Texture2D创建Sprite
            var sprite = LoadSpriteFromTexture(pathWithoutExtension);
            if (sprite != null)
            {
                image.sprite = sprite;
                cache?.Add(sprite);
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 从配置加载并设置物品图标
    /// </summary>
    /// <param name="image">目标Image组件</param>
    /// <param name="itemId">物品ID</param>
    /// <param name="isAtlas">是否为图集资源，true=直接加载Sprite，false=从Texture2D创建Sprite</param>
    /// <param name="cache">资源缓存列表，用于后续释放</param>
    /// <returns>是否成功设置</returns>
    public static bool LoadAndSetItemIcon(Image image, int itemId, bool isAtlas = false, List<Object> cache = null)
    {
        if (image == null) return false;
        
        string iconPath = GetItemIconPath(itemId);
        return LoadAndSetSprite(image, iconPath, isAtlas, cache);
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
        string iconPath = reader?.GetValue<string>(itemId, "IconPath", "") ?? "";
        
        // Unity Resources.Load不需要文件扩展名，移除扩展名
        if (!string.IsNullOrEmpty(iconPath))
        {
            iconPath = System.IO.Path.ChangeExtension(iconPath, null);
        }
        
        return iconPath;
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
    
    #region Uid生成工具
    
    /// <summary>
    /// 生成带随机数的Uid - 在同一时刻创建多个对象时确保唯一性
    /// </summary>
    /// <returns>时间戳+随机数组合Uid</returns>
    public static int GenerateUid()
    {
        long timestamp = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        int random = UnityEngine.Random.Range(1000, 9999);
        return (int)((timestamp + random) % int.MaxValue);
    }
    
    #endregion
} 