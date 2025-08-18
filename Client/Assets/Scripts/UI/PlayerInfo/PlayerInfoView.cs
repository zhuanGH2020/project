using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 玩家信息视图 - 显示玩家血量等核心状态信息
/// </summary>
public class PlayerInfoView : BaseView
{
    private Slider slider_health;
    private TextMeshProUGUI txt_num;

    void Start()
    {
        InitializeView();
        SubscribeEvents();
    }

    private void InitializeView()
    {
        // 查找UI组件
        slider_health = transform.Find("slider_health")?.GetComponent<Slider>();
        txt_num = transform.Find("slider_health/txt_num")?.GetComponent<TextMeshProUGUI>();
        
        if (slider_health == null)
        {
            Debug.LogWarning("[PlayerInfoView] slider_health component not found");
        }
        
        if (txt_num == null)
        {
            Debug.LogWarning("[PlayerInfoView] txt_num component not found");
        }
        
        // 初始化血量显示
        UpdateHealthDisplay();
    }

    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<PlayerHealthChangeEvent>(OnPlayerHealthChanged);
    }

    protected override void OnViewDestroy()
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<PlayerHealthChangeEvent>(OnPlayerHealthChanged);
    }

    /// <summary>
    /// 处理玩家血量变化事件
    /// </summary>
    private void OnPlayerHealthChanged(PlayerHealthChangeEvent eventData)
    {
        if (eventData == null)
        {
            Debug.LogWarning("[PlayerInfoView] Invalid health change event data received");
            return;
        }

        // 从Player实例获取最大血量
        if (Player.Instance != null)
        {
            UpdateHealthDisplay(eventData.CurrentHealth, Player.Instance.MaxHealth);
        }
    }

    /// <summary>
    /// 更新血量显示 - 从Player实例获取当前血量
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (Player.Instance == null) return;
        
        float currentHealth = Player.Instance.CurrentHealth;
        float maxHealth = Player.Instance.MaxHealth;
        
        UpdateHealthDisplay(currentHealth, maxHealth);
    }

    /// <summary>
    /// 更新血量显示 - 使用指定的血量值
    /// </summary>
    private void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        if (slider_health != null)
        {
            slider_health.value = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }

        if (txt_num != null)
        {
            int displayCurrent = Mathf.CeilToInt(currentHealth);
            int displayMax = Mathf.CeilToInt(maxHealth);
            txt_num.text = $"{displayCurrent}/{displayMax}";
        }
    }


} 