using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 烹饪界面视图 - 管理烹饪UI的显示和交互
/// 需要挂载到Canvas上的UI面板
/// </summary>
public class CookingView : BaseView, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private GameObject[] _cookingSlots;  // 四个烹饪槽位
    [SerializeField] private Button _cookButton;          // 烹饪按钮
    [SerializeField] private Button _closeButton;         // 关闭按钮
    [SerializeField] private GameObject _interactionHint; // 交互提示文本
    
    [Header("Animation Settings")]
    [SerializeField] private float _fadeAnimationDuration = 0.3f;  // 淡入淡出动画时长
    
    private CanvasGroup _canvasGroup;  // 用于控制透明度的组件
    private Coroutine _fadeCoroutine;  // 当前运行的动画协程

    private void Start()
    {
        InitializeUI();
        SubscribeEvents();
        
        // 初始状态：隐藏界面
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    /// <summary>
    /// 初始化UI组件
    /// </summary>
    private void InitializeUI()
    {
        // 获取或添加CanvasGroup组件
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 设置按钮事件
        if (_cookButton != null)
        {
            _cookButton.onClick.AddListener(OnCookButtonClicked);
            _cookButton.interactable = false;  // 初始状态不可点击
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        // 初始化槽位UI
        UpdateAllSlots();
        
        // 隐藏交互提示
        if (_interactionHint != null)
        {
            _interactionHint.SetActive(false);
        }
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<CookingUIOpenEvent>(OnCookingUIOpen);
        EventManager.Instance.Subscribe<CookingUICloseEvent>(OnCookingUIClose);
        EventManager.Instance.Subscribe<CookingSlotUpdateEvent>(OnCookingSlotUpdate);
        EventManager.Instance.Subscribe<CookingInteractionEvent>(OnCookingInteraction);
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<CookingUIOpenEvent>(OnCookingUIOpen);
        EventManager.Instance.Unsubscribe<CookingUICloseEvent>(OnCookingUIClose);
        EventManager.Instance.Unsubscribe<CookingSlotUpdateEvent>(OnCookingSlotUpdate);
        EventManager.Instance.Unsubscribe<CookingInteractionEvent>(OnCookingInteraction);
    }

    /// <summary>
    /// 处理烹饪界面打开事件
    /// </summary>
    private void OnCookingUIOpen(CookingUIOpenEvent e)
    {
        gameObject.SetActive(true);
        UpdateAllSlots();
        UpdateCookButton();
        
        // 根据锅的位置调整UI位置到屏幕右侧
        PositionUIRightOfPot(e.PotWorldPosition);
        
        // 播放淡入动画
        PlayFadeAnimation(true);
    }

    /// <summary>
    /// 处理烹饪界面关闭事件
    /// </summary>
    private void OnCookingUIClose(CookingUICloseEvent e)
    {
        // 播放淡出动画，动画完成后隐藏GameObject
        PlayFadeAnimation(false);
    }

    /// <summary>
    /// 处理烹饪槽位更新事件
    /// </summary>
    private void OnCookingSlotUpdate(CookingSlotUpdateEvent e)
    {
        UpdateSlot(e.SlotIndex);
        UpdateCookButton();
    }

    /// <summary>
    /// 处理烹饪交互事件
    /// </summary>
    private void OnCookingInteraction(CookingInteractionEvent e)
    {
        if (_interactionHint != null)
        {
            _interactionHint.SetActive(e.InRange && !CookingModel.Instance.IsUIOpen);
        }
    }

    /// <summary>
    /// 更新指定槽位的UI
    /// </summary>
    private void UpdateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _cookingSlots.Length)
            return;

        var slot = CookingModel.Instance.CookingSlots[slotIndex];
        var slotUI = _cookingSlots[slotIndex];
        
        // 使用ViewUtils更新槽位UI
        ViewUtils.QuickSetItemUI(slotUI, slot.itemId, slot.count);
    }

    /// <summary>
    /// 更新所有槽位UI
    /// </summary>
    private void UpdateAllSlots()
    {
        for (int i = 0; i < CookingModel.COOKING_SLOTS && i < _cookingSlots.Length; i++)
        {
            UpdateSlot(i);
        }
    }

    /// <summary>
    /// 更新烹饪按钮状态
    /// </summary>
    private void UpdateCookButton()
    {
        if (_cookButton != null)
        {
            _cookButton.interactable = CookingModel.Instance.AllSlotsFilled();
        }
    }

    /// <summary>
    /// 烹饪按钮点击事件
    /// </summary>
    private void OnCookButtonClicked()
    {
        CookingModel.Instance.Cook();
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void OnCloseButtonClicked()
    {
        CookingModel.Instance.CloseCookingUI();
    }

    /// <summary>
    /// 处理拖拽放置事件
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // 检查是否有选中的物品
        var selectedItem = PackageModel.Instance.SelectedItem;
        if (selectedItem == null)
        {
            Debug.LogWarning("[CookingView] 没有选中的物品");
            return;
        }

        // 查找最近的槽位
        int targetSlot = FindNearestSlot(eventData.position);
        if (targetSlot == -1)
        {
            Debug.LogWarning("[CookingView] 未找到有效的槽位");
            PackageModel.Instance.UnselectItem();  // 取消选中
            return;
        }

        // 尝试将物品放入槽位
        bool success = CookingModel.Instance.PlaceItemToSlot(targetSlot, selectedItem.itemId, 1);
        if (success)
        {
            // 从选中物品中减去1个
            selectedItem.count -= 1;
            
            // 如果还有剩余，放回背包；如果没有了，发送物品用完事件
            if (selectedItem.count > 0)
            {
                PackageModel.Instance.UnselectItem();  // 将剩余物品放回背包
            }
            else
            {
                // 物品全部用完，清除选中状态（不放回背包）
                PackageModel.Instance.ClearSelectedItem();
            }
        }
        else
        {
            PackageModel.Instance.UnselectItem();  // 放置失败，取消选中
        }
    }

    /// <summary>
    /// 将UI定位到锅的屏幕右侧
    /// </summary>
    private void PositionUIRightOfPot(Vector3 potWorldPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 将锅的世界坐标转换为屏幕坐标
        Vector3 potScreenPos = mainCamera.WorldToScreenPoint(potWorldPosition);
        
        // 获取Canvas信息用于坐标转换
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // 将屏幕坐标转换为Canvas内的局部坐标
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            potScreenPos,
            canvas.worldCamera,
            out localPoint);

        // 获取UI面板的宽度
        float panelWidth = rectTransform.rect.width;
        
        // 设置UI位置到锅的右侧（添加一些偏移）
        float offsetX = panelWidth * 0.5f + 50f; // UI宽度的一半 + 额外间距
        Vector2 targetPosition = new Vector2(localPoint.x + offsetX, localPoint.y - 50f);
        
        // 确保UI不会超出屏幕边界
        float canvasWidth = (canvas.transform as RectTransform).rect.width;
        float canvasHeight = (canvas.transform as RectTransform).rect.height;
        
        // 检查右边界
        if (targetPosition.x + panelWidth * 0.5f > canvasWidth * 0.5f)
        {
            // 如果右侧放不下，放到左侧
            targetPosition.x = localPoint.x - offsetX;
        }
        
        // 检查上下边界
        float panelHeight = rectTransform.rect.height;
        targetPosition.y = Mathf.Clamp(targetPosition.y, 
            -canvasHeight * 0.5f + panelHeight * 0.5f, 
            canvasHeight * 0.5f - panelHeight * 0.5f);
        
        rectTransform.localPosition = targetPosition;
    }

    /// <summary>
    /// 查找最近的槽位
    /// </summary>
    private int FindNearestSlot(Vector2 screenPosition)
    {
        float minDistance = float.MaxValue;
        int nearestSlot = -1;

        for (int i = 0; i < _cookingSlots.Length; i++)
        {
            var slotTransform = _cookingSlots[i].transform as RectTransform;
            if (slotTransform == null) continue;

            Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(null, slotTransform.position);
            
            float distance = Vector2.Distance(screenPosition, slotScreenPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestSlot = i;
            }
        }

        // 只有距离足够近才算有效
        return minDistance < 100f ? nearestSlot : -1;
    }

    /// <summary>
    /// 播放淡入淡出动画
    /// </summary>
    /// <param name="fadeIn">true为淡入，false为淡出</param>
    private void PlayFadeAnimation(bool fadeIn)
    {
        // 停止当前运行的动画
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        // 启动新的动画
        _fadeCoroutine = StartCoroutine(FadeAnimation(fadeIn));
    }

    /// <summary>
    /// 淡入淡出动画协程
    /// </summary>
    /// <param name="fadeIn">true为淡入，false为淡出</param>
    private IEnumerator FadeAnimation(bool fadeIn)
    {
        if (_canvasGroup == null) yield break;

        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        // 设置初始透明度
        _canvasGroup.alpha = startAlpha;
        
        // 淡入时启用交互，淡出时禁用交互
        _canvasGroup.interactable = fadeIn;
        _canvasGroup.blocksRaycasts = fadeIn;

        // 执行动画
        while (elapsedTime < _fadeAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _fadeAnimationDuration;
            
            // 使用平滑插值
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            _canvasGroup.alpha = currentAlpha;
            
            yield return null;
        }

        // 确保最终透明度正确
        _canvasGroup.alpha = targetAlpha;
        
        // 如果是淡出动画，动画完成后隐藏GameObject
        if (!fadeIn)
        {
            gameObject.SetActive(false);
        }

        _fadeCoroutine = null;
    }
}