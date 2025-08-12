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
    private Image _buildingImage; // 待放置建筑物的图标 (img_put)
    
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Camera _worldCamera;
    private GameObject _currentHoveredObject; // 当前悬停的对象
    private int _currentPendingBuildingId = -1; // 当前待放置的建筑物ID
    private bool _inBuildingPlacementMode = false; // TouchView自己管理建筑放置状态
    
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
        
        // 如果有待放置的建筑物，让建筑物图标跟随鼠标移动
        if (_buildingImage != null && _buildingImage.gameObject.activeSelf)
        {
            UpdateBuildingPosition();
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
        _buildingImage = transform.Find("img_put")?.GetComponent<Image>();
        
        // 初始状态隐藏组件
        if (_touchText != null)
        {
            _touchText.gameObject.SetActive(false);
        }
        if (_itemImage != null)
        {
            _itemImage.gameObject.SetActive(false);
        }
        if (_buildingImage != null)
        {
            _buildingImage.gameObject.SetActive(false);
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
        EventManager.Instance.Subscribe<BuildingPendingPlaceEvent>(OnBuildingPendingPlace);
        
        // 订阅InputManager的高优先级输入事件（建筑放置等）
        InputManager.Instance.OnLeftClickHighPriority += OnLeftClick;
        InputManager.Instance.OnRightClick += OnRightClick;
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<MouseHoverEvent>(OnMouseHover);
        EventManager.Instance.Unsubscribe<MouseHoverExitEvent>(OnMouseHoverExit);
        EventManager.Instance.Unsubscribe<PackageItemSelectedEvent>(OnPackageItemSelected);
        EventManager.Instance.Unsubscribe<BuildingPendingPlaceEvent>(OnBuildingPendingPlace);
        
        // 取消订阅InputManager的高优先级输入事件
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLeftClickHighPriority -= OnLeftClick;
            InputManager.Instance.OnRightClick -= OnRightClick;
        }
    }
    
    /// <summary>
    /// 处理鼠标悬停事件
    /// </summary>
    private void OnMouseHover(MouseHoverEvent e)
    {
        // 如果在建筑放置模式，不显示悬停提示文本
        if (_inBuildingPlacementMode && _currentPendingBuildingId > 0)
        {
            return;
        }
        
        string touchText = null;
        
        // 检查悬停的对象类型
        CombatEntity combatEntity = e.HoveredObject.GetComponent<Monster>();
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
        // 如果在建筑放置模式，不处理悬停退出事件
        if (_inBuildingPlacementMode && _currentPendingBuildingId > 0)
        {
            return;
        }
        
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
    

    
    /// <summary>
    /// 显示待放置建筑物图标
    /// </summary>
    private void ShowPendingBuildingIcon(int buildingId)
    {
        if (_buildingImage == null) return;
        
        // 获取建筑物配置信息
        var itemConfig = ItemManager.Instance.GetItem(buildingId);
        string iconPath = itemConfig?.Csv.GetValue<string>(buildingId, "IconPath", "") ?? "";
        
        // 加载并设置图标
        LoadAndSetSprite(_buildingImage, iconPath, false);
        _buildingImage.gameObject.SetActive(true);
        
        Debug.Log($"显示待放置建筑物图标: {buildingId}");
    }
    
    /// <summary>
    /// 隐藏待放置建筑物图标
    /// </summary>
    private void HidePendingBuildingIcon()
    {
        if (_buildingImage != null)
        {
            _buildingImage.gameObject.SetActive(false);
        }
        _currentPendingBuildingId = -1;
    }
    
    /// <summary>
    /// 更新待放置建筑物图标位置，跟随鼠标
    /// </summary>
    private void UpdateBuildingPosition()
    {
        if (_canvas == null || _buildingImage == null) return;
        
        // 获取鼠标屏幕坐标
        Vector3 mousePosition = Input.mousePosition;
        
        // 转换屏幕坐标到Canvas本地坐标
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            mousePosition,
            _canvas.worldCamera,
            out Vector2 localPoint))
        {
            // 设置建筑物图标位置到鼠标位置，稍作偏移避免遮挡鼠标
            localPoint.x += 20f; // 向右偏移20像素
            localPoint.y += 20f; // 向上偏移20像素
            
            (_buildingImage.transform as RectTransform).localPosition = localPoint;
        }
    }
    
    /// <summary>
    /// 处理建筑物待放置事件
    /// </summary>
    private void OnBuildingPendingPlace(BuildingPendingPlaceEvent e)
    {

        _currentPendingBuildingId = e.BuildingId;
        _inBuildingPlacementMode = true;
        
        // 发布建筑放置模式状态变化事件
        EventManager.Instance.Publish(new BuildingPlacementModeEvent(true, e.BuildingId));

        
        ShowPendingBuildingIcon(e.BuildingId);
    }
    
    /// <summary>
    /// 处理左键点击（来自InputManager）
    /// </summary>
    private bool OnLeftClick(Vector3 worldPosition)
    {
        // 如果在建筑放置模式，处理建筑放置
        if (_inBuildingPlacementMode && _currentPendingBuildingId > 0)
        {
            HandleBuildingPlacement(worldPosition);
            return true; // 消费事件，阻止后续处理（如Player移动）
        }
        
        // 如果没有处理特殊逻辑，返回false让其他系统继续处理
        return false;
    }
    
    /// <summary>
    /// 处理右键点击（来自InputManager）
    /// </summary>
    private void OnRightClick()
    {
        // 如果在建筑放置模式，取消建筑放置
        if (_inBuildingPlacementMode && _currentPendingBuildingId > 0)
        {
            // 退出建筑放置模式并隐藏图标
            ExitBuildingPlacementMode();
        }
        // 可以在这里添加其他右键点击处理逻辑
    }
    
    /// <summary>
    /// 处理建筑物放置
    /// </summary>
    private void HandleBuildingPlacement(Vector3 worldPosition)
    {
        // 生成新的建筑物UID
        int newBuildingUID = ResourceUtils.GenerateUID();
        
        // 在地图上放置建筑物，传递buildingUID参数
        bool placed = MapModel.Instance.AddMapData(_currentPendingBuildingId, worldPosition.x, worldPosition.z, newBuildingUID);
        
        if (placed)
        {
            // 从MakeModel中移除已放置的建筑
            MakeModel.Instance.RemoveBuilding(_currentPendingBuildingId);
            // 退出建筑放置模式并隐藏图标
            ExitBuildingPlacementMode();
            
            Debug.Log($"建筑物放置成功: {_currentPendingBuildingId} at ({worldPosition.x}, {worldPosition.z}), UID: {newBuildingUID}");
        }
        else
        {
            Debug.LogWarning($"建筑物放置失败: {_currentPendingBuildingId}");
        }
    }
    
    /// <summary>
    /// 退出建筑放置模式
    /// </summary>
    private void ExitBuildingPlacementMode()
    {
        _inBuildingPlacementMode = false;
        
        // 发布建筑放置模式状态变化事件
        EventManager.Instance.Publish(new BuildingPlacementModeEvent(false));
        
        HidePendingBuildingIcon();
    }
}
