using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View基类 - 统一管理资源生命周期
/// 所有View继承此类，自动获得资源管理功能
/// </summary>
public abstract class BaseView : MonoBehaviour
{
    private List<Object> _resources = new List<Object>();
    
    #region 图片加载
    
    /// <summary>
    /// 加载并设置图片到Image组件
    /// </summary>
    /// <param name="image">目标Image组件</param>
    /// <param name="imagePath">图片路径（相对于Resources目录）</param>
    /// <param name="isAtlas">是否为图集资源，true=直接加载Sprite，false=从Texture2D创建Sprite</param>
    /// <returns>是否成功设置</returns>
    protected bool LoadAndSetSprite(Image image, string imagePath, bool isAtlas = true)
    {
        if (image == null || string.IsNullOrEmpty(imagePath)) return false;
        
        // 移除扩展名，Unity Resources.Load不需要扩展名
        string pathWithoutExtension = System.IO.Path.ChangeExtension(imagePath, null);
        
        if (isAtlas)
        {
            // 图集模式：直接加载Sprite
            var sprite = LoadResource<Sprite>(pathWithoutExtension);
            if (sprite != null)
            {
                image.sprite = sprite;
                return true;
            }
        }
        else
        {
            // 纹理模式：从Texture2D创建Sprite
            var texture = LoadResource<Texture2D>(pathWithoutExtension);
            if (texture != null)
            {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 通过路径查找Image组件并设置图片
    /// </summary>
    /// <param name="imagePath">Image组件路径（相对于当前Transform）</param>
    /// <param name="spritePath">图片路径（相对于Resources目录）</param>
    /// <param name="isAtlas">是否为图集资源，true=直接加载Sprite，false=从Texture2D创建Sprite</param>
    /// <returns>是否成功设置</returns>
    protected bool LoadAndSetSprite(string imagePath, string spritePath, bool isAtlas = true)
    {
        var image = transform.Find(imagePath)?.GetComponent<Image>();
        return LoadAndSetSprite(image, spritePath, isAtlas);
    }
    
    /// <summary>
    /// 从配置加载并设置物品图标
    /// </summary>
    /// <param name="imagePath">Image组件路径（相对于当前Transform）</param>
    /// <param name="itemId">物品ID</param>
    /// <param name="isAtlas">是否为图集资源，true=直接加载Sprite，false=从Texture2D创建Sprite</param>
    /// <returns>是否成功设置</returns>
    protected bool LoadAndSetItemIcon(string imagePath, int itemId, bool isAtlas = false)
    {
        var image = transform.Find(imagePath)?.GetComponent<Image>();
        if (image == null) return false;
        
        // 获取物品图标路径
        string iconPath = GetItemIconPath(itemId);
        return LoadAndSetSprite(image, iconPath, isAtlas);
    }
    
    #endregion
    
    #region 资源加载
    
    /// <summary>
    /// 加载资源并自动管理生命周期
    /// </summary>
    protected T LoadResource<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        var resource = ResourceManager.Instance.Load<T>(path);
        if (resource != null)
        {
            _resources.Add(resource);
        }
        return resource;
    }
    
    /// <summary>
    /// 获取物品图标路径
    /// </summary>
    private string GetItemIconPath(int itemId)
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        return reader?.GetValue<string>(itemId, "IconPath", "") ?? "";
    }
    
    #endregion
    
    #region 资源管理
    
    /// <summary>
    /// 手动释放特定资源
    /// </summary>
    protected void ReleaseResource(Object resource)
    {
        if (resource != null && _resources.Remove(resource))
        {
            ResourceManager.Instance.Release(resource);
        }
    }
    
    /// <summary>
    /// 获取已加载资源数量
    /// </summary>
    protected int LoadedResourceCount => _resources.Count;
    
    #endregion
    
    #region 生命周期管理
    
    private void OnDestroy()
    {
        // 自动释放所有资源
        foreach (var resource in _resources)
        {
            if (resource != null)
            {
                ResourceManager.Instance.Release(resource);
            }
        }
        _resources.Clear();
        
        // 调用子类的清理方法
        OnViewDestroy();
    }
    
    /// <summary>
    /// 子类重写此方法进行额外的清理工作
    /// 注意：不要在此方法中释放资源，资源会自动释放
    /// </summary>
    protected virtual void OnViewDestroy() { }
    
    #endregion
} 