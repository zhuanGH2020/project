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
/// 专注于UI管理，View直接调用各种Manager更加简洁高效
/// </summary>
public class UIManager
{
    private static UIManager _instance;
    public static UIManager Instance => _instance ??= new UIManager();
    
    // UI管理核心数据
    private Dictionary<Type, BaseView> _views = new Dictionary<Type, BaseView>();
    private Canvas _uiCanvas;
    private CanvasGroup _canvasGroup;
    
    private UIManager()
    {
        InitializeUICanvas();
    }
    
    #region UI生命周期管理
    
    /// <summary>
    /// 显示UI界面
    /// </summary>
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
        
        // 发布UI显示事件（一对多通知，适合用事件）
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
        
        // 发布UI隐藏事件（一对多通知，适合用事件）
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
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
    
    /// <summary>
    /// 显示所有UI界面 - 通过UICanvas的CanvasGroup控制整个UI系统
    /// </summary>
    public void ShowAll()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
    
    /// <summary>
    /// 更新方法 - 由GameMain调用
    /// </summary>
    public void Update()
    {
        // 简单更新，无复杂逻辑
    }
    
    #endregion
    
    #region UI层级和组件管理
    
    /// <summary>
    /// 初始化UI Canvas
    /// </summary>
    private void InitializeUICanvas()
    {
        // 查找或创建UI Canvas
        GameObject canvasObj = GameObject.Find("UICanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("UICanvas");
            _uiCanvas = canvasObj.AddComponent<Canvas>();
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _uiCanvas.sortingOrder = 0;
        }
        else
        {
            _uiCanvas = canvasObj.GetComponent<Canvas>();
        }
        
        // 添加CanvasGroup用于全局UI控制
        _canvasGroup = _uiCanvas.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = _uiCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        GameObject.DontDestroyOnLoad(_uiCanvas.gameObject);
    }
    
    /// <summary>
    /// 获取或创建View实例
    /// </summary>
    private BaseView GetOrCreateView<T>() where T : BaseView
    {
        Type viewType = typeof(T);
        
        // 尝试从已有View中获取
        if (_views.TryGetValue(viewType, out BaseView existingView) && existingView != null)
        {
            return existingView;
        }
        
        // 尝试从Prefab加载
        string prefabPath = $"Prefabs/UI/{viewType.Name}";
        GameObject prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] 找不到UI预制体: {prefabPath}");
            return null;
        }
        
        // 实例化并获取组件
        GameObject viewObj = GameObject.Instantiate(prefab, _uiCanvas.transform);
        BaseView view = viewObj.GetComponent<T>();
        
        if (view == null)
        {
            Debug.LogError($"[UIManager] 预制体上没有找到{viewType.Name}组件");
            GameObject.Destroy(viewObj);
            return null;
        }
        
        return view;
    }
    
    /// <summary>
    /// 设置View层级
    /// </summary>
    private void SetViewLayer(BaseView view, UILayer layer)
    {
        if (view != null)
        {
            view.transform.SetAsLastSibling(); // 设置为最后，确保显示在最前面
            
            // 可以根据UILayer设置具体的Canvas层级
            Canvas viewCanvas = view.GetComponent<Canvas>();
            if (viewCanvas != null)
            {
                viewCanvas.sortingOrder = (int)layer;
            }
        }
    }
    
    #endregion
}

#region UI事件定义

/// <summary>
/// UI显示事件 - 一对多通知，适合用事件系统
/// </summary>
public class UIShowEvent : IEvent
{
    public Type ViewType { get; set; }
}

/// <summary>
/// UI隐藏事件 - 一对多通知，适合用事件系统
/// </summary>
public class UIHideEvent : IEvent
{
    public Type ViewType { get; set; }
}

#endregion 