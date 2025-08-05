using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 悬停提示视图 - 处理鼠标悬停事件并显示提示文本
/// 需要挂载到UI Canvas上，显示跟随鼠标移动
/// </summary>
public class TouchView : BaseView
{
    private TextMeshProUGUI _touchText;
    private Image _itemImage; // 选中道具的图标
    
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
    
    void Update()
    {
        // 如果有选中的道具，让图标跟随鼠标移动
        if (_itemImage != null && _itemImage.gameObject.activeSelf)
        {
            UpdateSelectedItemPosition();
        }
        
        // 如果悬停文本显示，让文本跟随鼠标移动
        if (_touchText != null && _touchText.gameObject.activeSelf)
        {
            UpdateTouchTextPosition();
        }
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
        _itemImage = transform.Find("img_item")?.GetComponent<Image>();
        
        // 初始状态隐藏组件
        if (_touchText != null)
        {
            _touchText.gameObject.SetActive(false);
        }
        if (_itemImage != null)
        {
            _itemImage.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 订阅悬停事件
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<MouseHoverEvent>(OnMouseHover);
        EventManager.Instance.Subscribe<MouseHoverExitEvent>(OnMouseHoverExit);
        EventManager.Instance.Subscribe<PackageItemSelectedEvent>(OnPackageItemSelected);
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<MouseHoverEvent>(OnMouseHover);
        EventManager.Instance.Unsubscribe<MouseHoverExitEvent>(OnMouseHoverExit);
        EventManager.Instance.Unsubscribe<PackageItemSelectedEvent>(OnPackageItemSelected);
    }
    
    /// <summary>
    /// 处理鼠标悬停事件
    /// </summary>
    private void OnMouseHover(MouseHoverEvent e)
    {
        string touchText = null;
        
        // 检查悬停的对象类型
        CombatEntity combatEntity = e.HoveredObject.GetComponent<Monster>();
        if (combatEntity == null)
        {
            combatEntity = e.HoveredObject.GetComponent<MonsterAI_Enhanced>();
        }
        
        if (combatEntity != null)
        {
            int currentHp = Mathf.RoundToInt(combatEntity.CurrentHealth);
            int maxHp = Mathf.RoundToInt(combatEntity.MaxHealth);
            touchText = $"攻击\n{currentHp}/{maxHp}";
        }
        else
        {
            // 检查新的采集物系统
            HarvestableObject harvestable = e.HoveredObject.GetComponent<HarvestableObject>();
            if (harvestable != null && harvestable.CanInteract)
            {
                touchText = harvestable.GetActionDisplayName();
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
    /// 处理背包道具选中状态变化事件
    /// </summary>
    private void OnPackageItemSelected(PackageItemSelectedEvent e)
    {
        if (_itemImage == null) return;

        if (e.IsSelected && e.SelectedItem != null)
        {
            // 显示选中道具的图标
            ShowSelectedItemIcon(e.SelectedItem);
        }
        else
        {
            // 隐藏选中道具图标
            HideSelectedItemIcon();
        }
    }
    
    /// <summary>
    /// 显示选中道具图标
    /// </summary>
    private void ShowSelectedItemIcon(PackageItem selectedItem)
    {
        if (_itemImage == null) return;

        // 获取道具配置信息
        var itemConfig = ItemManager.Instance.GetItem(selectedItem.itemId);
        string iconPath = itemConfig?.Csv.GetValue<string>(selectedItem.itemId, "IconPath", "") ?? "";
        
        // 加载并设置图标
        LoadAndSetSprite(_itemImage, iconPath, false);
        _itemImage.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏选中道具图标
    /// </summary>
    private void HideSelectedItemIcon()
    {
        if (_itemImage != null)
        {
            _itemImage.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 更新选中道具图标位置，跟随鼠标
    /// </summary>
    private void UpdateSelectedItemPosition()
    {
        if (_canvas == null || _itemImage == null) return;
        
        // 获取鼠标屏幕坐标
        Vector3 mousePosition = Input.mousePosition;
        
        // 转换屏幕坐标到Canvas本地坐标
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            mousePosition,
            _canvas.worldCamera,
            out Vector2 localPoint))
        {
            // 设置道具图标位置到鼠标位置，稍作偏移避免遮挡鼠标
            localPoint.x += 20f; // 向右偏移20像素
            localPoint.y += 20f; // 向上偏移20像素
            
            (_itemImage.transform as RectTransform).localPosition = localPoint;
        }
    }
    
    /// <summary>
    /// 更新悬停文本位置，跟随鼠标
    /// </summary>
    private void UpdateTouchTextPosition()
    {
        if (_canvas == null || _touchText == null) return;
        
        RectTransform textRect = _touchText.transform as RectTransform;
        
        // 获取鼠标屏幕坐标
        Vector3 mousePosition = Input.mousePosition;
        
        // 转换屏幕坐标到Canvas本地坐标
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            mousePosition,
            _canvas.worldCamera,
            out Vector2 localPoint))
        {
            // 设置文本位置到鼠标位置，向上偏移避免遮挡鼠标
            localPoint.y += 50f; // 向上偏移50像素
            
            textRect.localPosition = localPoint;
        }
        
        // 确保UI在屏幕范围内
        ClampToScreen(textRect);
    }
    

    
    /// <summary>
    /// 显示悬停提示，位置跟随鼠标动态更新
    /// </summary>
    private void ShowTouch(string text, Vector3 worldPosition)
    {
        // 设置文本并显示
        if (_touchText != null)
        {
            _touchText.text = text;
            _touchText.gameObject.SetActive(true);
        }
        
        // 位置将在Update中动态跟随鼠标更新，不再需要这里设置
    }
    
    /// <summary>
    /// 隐藏悬停提示
    /// </summary>
    private void HideTouch()
    {
        if (_touchText != null)
        {
            _touchText.gameObject.SetActive(false);
        }
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
        ClampToScreen(_rectTransform);
    }
    
    /// <summary>
    /// 限制指定RectTransform在屏幕范围内
    /// </summary>
    private void ClampToScreen(RectTransform targetRect)
    {
        if (_canvas == null || targetRect == null) return;
        
        RectTransform canvasRect = _canvas.transform as RectTransform;
        Vector3 pos = targetRect.localPosition;
        
        Vector2 size = targetRect.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;
        
        // 限制X轴
        pos.x = Mathf.Clamp(pos.x, -canvasSize.x / 2 + size.x / 2, canvasSize.x / 2 - size.x / 2);
        
        // 限制Y轴
        pos.y = Mathf.Clamp(pos.y, -canvasSize.y / 2 + size.y / 2, canvasSize.y / 2 - size.y / 2);
        
        targetRect.localPosition = pos;
    }
}
