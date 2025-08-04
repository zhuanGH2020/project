using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 悬停提示视图 - 处理鼠标悬停事件并显示提示文本
/// 需要挂载到UI Canvas上，显示时设置一次位置，不跟随鼠标移动
/// </summary>
public class TouchView : BaseView
{
    private TextMeshProUGUI _touchText;
    
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Camera _worldCamera;
    private GameObject _currentHoveredObject; // 当前悬停的对象
    
    void Start()
    {
        InitializeComponents();
        SubscribeEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeEvents();
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        _rectTransform = transform as RectTransform;
        _canvas = GetComponentInParent<Canvas>();
        _worldCamera = Camera.main;

        _touchText = transform.Find("txt_touch")?.GetComponent<TextMeshProUGUI>();
        
        // 初始状态隐藏
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 订阅悬停事件
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<MouseHoverEvent>(OnMouseHover);
        EventManager.Instance.Subscribe<MouseHoverExitEvent>(OnMouseHoverExit);
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<MouseHoverEvent>(OnMouseHover);
        EventManager.Instance.Unsubscribe<MouseHoverExitEvent>(OnMouseHoverExit);
    }
    
    /// <summary>
    /// 处理鼠标悬停事件
    /// </summary>
    private void OnMouseHover(MouseHoverEvent e)
    {
        string touchText = null;
        
        // 检查悬停的对象类型
        Monster monster = e.HoveredObject.GetComponent<Monster>();
        if (monster != null)
        {
            touchText = "攻击";
        }
        else
        {
            BerryBush berryBush = e.HoveredObject.GetComponent<BerryBush>();
            if (berryBush != null)
            {
                touchText = "采集";
            }
        }
        
        // 如果找到可交互对象
        if (touchText != null)
        {
            // 只有当悬停对象发生变化时才重新设置位置
            if (_currentHoveredObject != e.HoveredObject)
            {
                _currentHoveredObject = e.HoveredObject;
                ShowTouch(touchText, e.HoverPosition);
            }
        }
        else
        {
            // 悬停到非交互对象，隐藏提示
            if (_currentHoveredObject != null)
            {
                _currentHoveredObject = null;
                HideTouch();
            }
        }
    }
    
    /// <summary>
    /// 处理鼠标离开悬停事件
    /// </summary>
    private void OnMouseHoverExit(MouseHoverExitEvent e)
    {
        _currentHoveredObject = null;
        HideTouch();
    }
    
    /// <summary>
    /// 显示悬停提示，位置只设置一次
    /// </summary>
    private void ShowTouch(string text, Vector3 worldPosition)
    {
        // 设置文本
        if (_touchText != null)
            _touchText.text = text;
        
        // 显示面板
        gameObject.SetActive(true);
        
        // 设置位置到世界坐标点（只设置一次）
        SetPositionToWorldPoint(worldPosition);
    }
    
    /// <summary>
    /// 隐藏悬停提示
    /// </summary>
    private void HideTouch()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 设置UI位置到世界坐标点
    /// </summary>
    private void SetPositionToWorldPoint(Vector3 worldPosition)
    {
        if (_canvas == null || _rectTransform == null || _worldCamera == null) return;
        
        // 将世界坐标转换为屏幕坐标
        Vector3 screenPosition = _worldCamera.WorldToScreenPoint(worldPosition);
        
        // 添加偏移，避免遮挡鼠标点击的对象
        screenPosition.y += 50f; // 向上偏移50像素
        
        // 转换屏幕坐标到Canvas本地坐标
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out Vector2 localPoint))
        {
            _rectTransform.localPosition = localPoint;
        }
        
        // 确保UI在屏幕范围内
        ClampToScreen();
    }
    
    /// <summary>
    /// 限制UI在屏幕范围内
    /// </summary>
    private void ClampToScreen()
    {
        if (_canvas == null || _rectTransform == null) return;
        
        RectTransform canvasRect = _canvas.transform as RectTransform;
        Vector3 pos = _rectTransform.localPosition;
        
        Vector2 size = _rectTransform.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;
        
        // 限制X轴
        pos.x = Mathf.Clamp(pos.x, -canvasSize.x / 2 + size.x / 2, canvasSize.x / 2 - size.x / 2);
        
        // 限制Y轴
        pos.y = Mathf.Clamp(pos.y, -canvasSize.y / 2 + size.y / 2, canvasSize.y / 2 - size.y / 2);
        
        _rectTransform.localPosition = pos;
    }
}
