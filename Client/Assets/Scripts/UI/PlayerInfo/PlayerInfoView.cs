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
    
    // 饥饿和理智UI组件
    private Slider slider_hunger;
    private TextMeshProUGUI txt_hunger_num;
    private Slider slider_sanity;
    private TextMeshProUGUI txt_sanity_num;

    protected override void Start()
    {
        base.Start();
        InitializeView();
        SubscribeEvents();
    }

    private void InitializeView()
    {
        // 查找UI组件
        slider_health = transform.Find("slider_health")?.GetComponent<Slider>();
        txt_num = transform.Find("slider_health/txt_num")?.GetComponent<TextMeshProUGUI>();
        
        // 查找饥饿和理智UI组件
        slider_hunger = transform.Find("slider_hunger")?.GetComponent<Slider>();
        txt_hunger_num = transform.Find("slider_hunger/txt_num")?.GetComponent<TextMeshProUGUI>();
        slider_sanity = transform.Find("slider_sanity")?.GetComponent<Slider>();
        txt_sanity_num = transform.Find("slider_sanity/txt_num")?.GetComponent<TextMeshProUGUI>();
        
        if (slider_health == null)
        {
            Debug.LogError("[PlayerInfoView] slider_health component not found");
        }
        
        if (txt_num == null)
        {
            Debug.LogError("[PlayerInfoView] txt_num component not found");
        }
        
        if (slider_hunger == null)
        {
            Debug.LogError("[PlayerInfoView] slider_hunger component not found");
        }
        
        if (txt_hunger_num == null)
        {
            Debug.LogError("[PlayerInfoView] txt_hunger_num component not found");
        }
        
        if (slider_sanity == null)
        {
            Debug.LogError("[PlayerInfoView] slider_sanity component not found");
        }
        
        if (txt_sanity_num == null)
        {
            Debug.LogError("[PlayerInfoView] txt_sanity_num component not found");
        }
        
        // 初始化显示
        UpdateHealthDisplay();
        UpdateHungerDisplay();
        UpdateSanityDisplay();
    }

    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<PlayerHealthChangeEvent>(OnPlayerHealthChanged);
        EventManager.Instance.Subscribe<PlayerHungerChangeEvent>(OnPlayerHungerChanged);
        EventManager.Instance.Subscribe<PlayerSanityChangeEvent>(OnPlayerSanityChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<PlayerHealthChangeEvent>(OnPlayerHealthChanged);
        EventManager.Instance.Unsubscribe<PlayerHungerChangeEvent>(OnPlayerHungerChanged);
        EventManager.Instance.Unsubscribe<PlayerSanityChangeEvent>(OnPlayerSanityChanged);
    }

    /// <summary>
    /// 处理玩家血量变化事件
    /// </summary>
    private void OnPlayerHealthChanged(PlayerHealthChangeEvent eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("[PlayerInfoView] Invalid health change event data received");
            return;
        }

        // 从Player实例获取最大血量
        if (Player.Instance != null)
        {
            UpdateHealthDisplay(eventData.CurrentHealth, Player.Instance.MaxHealth);
        }
    }

    /// <summary>
    /// 处理玩家饥饿值变化事件
    /// </summary>
    private void OnPlayerHungerChanged(PlayerHungerChangeEvent eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("[PlayerInfoView] Invalid hunger change event data received");
            return;
        }

        // 从Player实例获取最大饥饿值
        if (Player.Instance != null)
        {
            UpdateHungerDisplay(eventData.CurrentHunger, Player.Instance.MaxHunger);
        }
    }

    /// <summary>
    /// 处理玩家理智值变化事件
    /// </summary>
    private void OnPlayerSanityChanged(PlayerSanityChangeEvent eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("[PlayerInfoView] Invalid sanity change event data received");
            return;
        }

        // 从Player实例获取最大理智值
        if (Player.Instance != null)
        {
            UpdateSanityDisplay(eventData.CurrentSanity, Player.Instance.MaxSanity);
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

    /// <summary>
    /// 更新饥饿值显示 - 从Player实例获取当前饥饿值
    /// </summary>
    private void UpdateHungerDisplay()
    {
        if (Player.Instance == null) return;
        
        float currentHunger = Player.Instance.CurrentHunger;
        float maxHunger = Player.Instance.MaxHunger;
        
        UpdateHungerDisplay(currentHunger, maxHunger);
    }

    /// <summary>
    /// 更新饥饿值显示 - 使用指定的饥饿值
    /// </summary>
    private void UpdateHungerDisplay(float currentHunger, float maxHunger)
    {
        if (slider_hunger != null)
        {
            slider_hunger.value = maxHunger > 0 ? currentHunger / maxHunger : 0f;
        }

        if (txt_hunger_num != null)
        {
            int displayCurrent = Mathf.CeilToInt(currentHunger);
            int displayMax = Mathf.CeilToInt(maxHunger);
            txt_hunger_num.text = $"{displayCurrent}/{displayMax}";
        }
    }

    /// <summary>
    /// 更新理智值显示 - 从Player实例获取当前理智值
    /// </summary>
    private void UpdateSanityDisplay()
    {
        if (Player.Instance == null) return;
        
        float currentSanity = Player.Instance.CurrentSanity;
        float maxSanity = Player.Instance.MaxSanity;
        
        UpdateSanityDisplay(currentSanity, maxSanity);
    }

    /// <summary>
    /// 更新理智值显示 - 使用指定的理智值
    /// </summary>
    private void UpdateSanityDisplay(float currentSanity, float maxSanity)
    {
        if (slider_sanity != null)
        {
            slider_sanity.value = maxSanity > 0 ? currentSanity / maxSanity : 0f;
        }

        if (txt_sanity_num != null)
        {
            int displayCurrent = Mathf.CeilToInt(currentSanity);
            int displayMax = Mathf.CeilToInt(maxSanity);
            txt_sanity_num.text = $"{displayCurrent}/{displayMax}";
        }
    }
} 