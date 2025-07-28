using UnityEngine;

/// <summary>
/// 冷却计时器
/// </summary>
public class CooldownTimer
{
    /// <summary>
    /// 冷却时间
    /// </summary>
    public float CooldownTime { get; private set; }

    /// <summary>
    /// 剩余冷却时间
    /// </summary>
    public float RemainingTime { get; private set; }

    /// <summary>
    /// 是否已冷却完成
    /// </summary>
    public bool IsReady => RemainingTime <= 0;

    public CooldownTimer(float cooldownTime)
    {
        CooldownTime = cooldownTime;
        RemainingTime = 0;
    }

    /// <summary>
    /// 开始冷却
    /// </summary>
    public void StartCooldown()
    {
        RemainingTime = CooldownTime;
    }

    /// <summary>
    /// 更新冷却时间
    /// </summary>
    public void Update()
    {
        if (RemainingTime > 0)
        {
            RemainingTime -= Time.deltaTime;
        }
    }
} 