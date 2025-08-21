using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View基类 - 直接调用各种Manager，简洁高效
/// 所有View继承此类，获得统一的基础功能
/// </summary>
public abstract class BaseView : MonoBehaviour
{
    #region 便捷方法 - 直接调用Manager
    
    /// <summary>
    /// 加载资源
    /// </summary>
    protected T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        return ResourceManager.Instance.Load<T>(path);
    }
    
    /// <summary>
    /// 获取配置读取器
    /// </summary>
    protected ConfigReader GetConfig(string configName)
    {
        return ConfigManager.Instance.GetReader(configName);
    }
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    protected void SubscribeEvent<T>(System.Action<T> handler) where T : IEvent
    {
        EventManager.Instance.Subscribe<T>(handler);
    }
    
    /// <summary>
    /// 发布事件
    /// </summary>
    protected void PublishEvent<T>(T eventData) where T : IEvent
    {
        EventManager.Instance.Publish(eventData);
    }
    
    /// <summary>
    /// 取消订阅事件（需要手动调用）
    /// </summary>
    protected void UnsubscribeEvent<T>(System.Action<T> handler) where T : IEvent
    {
        EventManager.Instance.Unsubscribe<T>(handler);
    }
    
    #endregion
    
    #region UI便捷方法
    
    /// <summary>
    /// 根据名称查找子对象
    /// </summary>
    protected GameObject FindChildByName(string childName)
    {
        Transform childTransform = transform.Find(childName);
        return childTransform?.gameObject;
    }
    
    /// <summary>
    /// 根据名称查找子对象的组件
    /// </summary>
    protected T FindChildComponent<T>(string childName) where T : Component
    {
        GameObject childObj = FindChildByName(childName);
        return childObj?.GetComponent<T>();
    }
    
    /// <summary>
    /// 显示这个View
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏这个View
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 加载并设置图片到Image组件
    /// </summary>
    /// <param name="image">目标Image组件</param>
    /// <param name="imagePath">图片路径（相对于Resources目录）</param>
    /// <param name="isAtlas">是否为图集资源，true=直接加载Sprite，false=从Texture2D创建Sprite</param>
    /// <returns>是否成功设置</returns>
    protected bool LoadAndSetSprite(Image image, string imagePath, bool isAtlas = true)
    {
        return ResourceUtils.LoadAndSetSprite(image, imagePath, isAtlas);
    }
    
    #endregion
    
    #region Unity生命周期
    
    protected virtual void Awake()
    {
        // 子类可重写
    }
    
    protected virtual void Start()
    {
        // 子类可重写
    }
    
    protected virtual void OnDestroy()
    {
        // 子类可重写，用于清理事件订阅等
    }
    
    #endregion
} 