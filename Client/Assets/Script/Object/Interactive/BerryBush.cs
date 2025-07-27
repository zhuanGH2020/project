using UnityEngine;

/// <summary>
/// 可采集的浆果丛
/// </summary>
public class BerryBush : DamageableObject
{
    [Header("Berry Settings")]
    [SerializeField] private GameObject _berryVisual;     // 浆果视觉表现
    [SerializeField] private float _regrowTime = 300f;    // 重生时间(秒)
    [SerializeField] private int _maxBerryCount = 3;      // 最大浆果数量

    private int _currentBerryCount;
    private float _regrowTimer;
    private bool _isRegrowing;

    protected override void Awake()
    {
        base.Awake();
        // 设置为0生命值，表示不可破坏
        _maxHealth = 0;
        _currentHealth = 0;
        _currentBerryCount = _maxBerryCount;
        UpdateVisual();
    }

    private void Update()
    {
        if (_isRegrowing)
        {
            _regrowTimer += Time.deltaTime;
            if (_regrowTimer >= _regrowTime)
            {
                Regrow();
            }
        }
    }

    /// <summary>
    /// 采集浆果
    /// </summary>
    public void Harvest()
    {
        if (_currentBerryCount <= 0) return;

        _currentBerryCount--;
        UpdateVisual();

        // 如果采完了，开始重生计时
        if (_currentBerryCount <= 0)
        {
            StartRegrow();
        }

        // 这里可以添加给玩家物品的逻辑
    }

    private void StartRegrow()
    {
        _isRegrowing = true;
        _regrowTimer = 0f;
    }

    private void Regrow()
    {
        _isRegrowing = false;
        _currentBerryCount = _maxBerryCount;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_berryVisual)
        {
            _berryVisual.SetActive(_currentBerryCount > 0);
        }
    }

    public override float TakeDamage(DamageInfo damageInfo)
    {
        // 不可被伤害
        return 0;
    }
} 