using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI层级枚举 - 定义UI显示层次
/// </summary>
public enum UILayer
{
    Background = 0,  // 背景层
    Normal = 100,    // 普通UI层
    Popup = 200,     // 弹窗层
    System = 400     // 系统层
}

/// <summary>
/// UI管理器 - 统一管理所有UI界面的显示隐藏和生命周期
/// 与BaseView架构集成，提供简洁的UI管理能力
/// </summary>
public class UIManager
{
    private static UIManager _instance;
    public static UIManager Instance => _instance ??= new UIManager();
    
    private Dictionary<Type, BaseView> _views = new Dictionary<Type, BaseView>();
    private Canvas _uiCanvas;
    private CanvasGroup _canvasGroup;
    
    private UIManager()
    {
        Initialize();
    }
    
    /// <summary>
    /// 初始化UI管理器
    /// </summary>
    private void Initialize()
    {
        var uiCanvasObj = GameObject.Find("UICanvas");
        _uiCanvas = uiCanvasObj?.GetComponent<Canvas>();
        
        // 确保UICanvas有CanvasGroup用于全局控制
        _canvasGroup = _uiCanvas.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = _uiCanvas.gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    #region UI生命周期管理
    
    /// <summary>
    /// 显示UI界面
    /// </summary>
    /// <typeparam name="T">UI界面类型，必须继承BaseView</typeparam>
    /// <param name="layer">UI显示层级</param>
    /// <returns>是否成功显示</returns>
    public bool Show<T>(UILayer layer = UILayer.Normal) where T : BaseView
    {
        Type viewType = typeof(T);
        
        // 检查是否已显示
        if (_views.TryGetValue(viewType, out BaseView existingView) && 
            existingView != null && existingView.gameObject.activeInHierarchy)
        {
            return true;
        }
        
        // 获取或创建View实例
        BaseView view = GetOrCreateView<T>();
        if (view == null) return false;
        
        // 存储View引用
        _views[viewType] = view;
        
        // 设置层级
        SetViewLayer(view, layer);
        
        // 显示界面
        view.gameObject.SetActive(true);
        
        // 发布事件
        EventManager.Instance.Publish(new UIShowEvent { ViewType = viewType });
        
        return true;
    }
    
    /// <summary>
    /// 隐藏UI界面
    /// </summary>
    public bool Hide<T>() where T : BaseView
    {
        Type viewType = typeof(T);
        
        if (!_views.TryGetValue(viewType, out BaseView view) || 
            view == null || !view.gameObject.activeInHierarchy)
        {
            return false;
        }
        
        // 隐藏界面
        view.gameObject.SetActive(false);
        
        // 发布事件
        EventManager.Instance.Publish(new UIHideEvent { ViewType = viewType });
        
        return true;
    }
    
    /// <summary>
    /// 销毁UI界面
    /// </summary>
    public bool Destroy<T>() where T : BaseView
    {
        Type viewType = typeof(T);
        
        if (_views.TryGetValue(viewType, out BaseView view) && view != null)
        {
            GameObject.Destroy(view.gameObject);
            _views.Remove(viewType);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 隐藏所有UI界面 - 通过UICanvas的CanvasGroup控制整个UI系统
    /// </summary>
    public void HideAll()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// 显示所有UI界面 - 恢复整个UI系统的显示和交互
    /// </summary>
    public void ShowAll()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// 检查UI界面是否可见
    /// </summary>
    public bool IsVisible<T>() where T : BaseView
    {
        Type viewType = typeof(T);
        return _views.TryGetValue(viewType, out BaseView view) && 
               view != null && view.gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// 获取UI界面实例
    /// </summary>
    public T Get<T>() where T : BaseView
    {
        Type viewType = typeof(T);
        _views.TryGetValue(viewType, out BaseView view);
        return view as T;
    }
    
    #endregion
    
    #region View实例管理
    
    /// <summary>
    /// 获取或创建View实例
    /// </summary>
    private BaseView GetOrCreateView<T>() where T : BaseView
    {
        Type viewType = typeof(T);
        
        // 检查缓存
        if (_views.TryGetValue(viewType, out BaseView cachedView) && cachedView != null)
        {
            return cachedView;
        }
        
        // 从场景查找
        BaseView sceneView = GameObject.FindObjectOfType<T>();
        if (sceneView != null)
        {
            return sceneView;
        }
        
        // 获取预制体路径 - 支持自定义路径
        string prefabPath = GetViewPrefabPath(viewType);
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            GameObject instance = GameObject.Instantiate(prefab, _uiCanvas.transform);
            return instance.GetComponent<T>();
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取View的预制体路径 - 支持自定义路径
    /// </summary>
    private string GetViewPrefabPath(Type viewType)
    {
        // 检查View类是否有自定义PrefabPath属性
        var prefabPathProperty = viewType.GetProperty("PrefabPath", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
        if (prefabPathProperty != null && prefabPathProperty.PropertyType == typeof(string))
        {
            string customPath = (string)prefabPathProperty.GetValue(null);
            if (!string.IsNullOrEmpty(customPath))
            {
                return customPath;
            }
        }
        
        // 使用默认路径
        return $"Prefabs/UI/{viewType.Name}";
    }
    
    /// <summary>
    /// 设置View显示层级
    /// </summary>
    private void SetViewLayer(BaseView view, UILayer layer)
    {
        Canvas canvas = view.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)layer;
        }
    }
    
    #endregion
    
    #region GameMain集成
    
    /// <summary>
    /// 更新方法 - 供GameMain调用
    /// </summary>
    public void Update()
    {
        // 处理ESC键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventManager.Instance.Publish(new BackKeyEvent());
        }
    }
    
    /// <summary>
    /// 清理方法 - 供GameMain调用
    /// </summary>
    public void Cleanup()
    {
        // 销毁所有UI
        foreach (var view in _views.Values)
        {
            if (view != null)
            {
                GameObject.Destroy(view.gameObject);
            }
        }
        _views.Clear();
    }
    
    #endregion
}

#region 事件定义

/// <summary>
/// UI显示事件
/// </summary>
public class UIShowEvent : IEvent
{
    public Type ViewType { get; set; }
}

/// <summary>
/// UI隐藏事件
/// </summary>
public class UIHideEvent : IEvent
{
    public Type ViewType { get; set; }
}

/// <summary>
/// 返回键事件
/// </summary>
public class BackKeyEvent : IEvent
{
}

#endregion 