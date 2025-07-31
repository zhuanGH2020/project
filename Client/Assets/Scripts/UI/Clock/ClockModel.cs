using UnityEngine;

/// <summary>
/// 时钟模型 - 管理游戏时间循环和环境光照
/// </summary>
public class ClockModel
{
    // 单例实现
    private static ClockModel _instance;
    public static ClockModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ClockModel();
            }
            return _instance;
        }
    }

    // 私有字段
    private float _dayDuration = 60f; // 一天持续时间（秒）
    private int _maxDays = 60; // 最大天数
    private float _dayAmbientIntensity = 1.0f; // 白天环境光强度
    private float _duskAmbientIntensity = 0.6f; // 黄昏环境光强度  
    private float _nightAmbientIntensity = 0.3f; // 夜晚环境光强度
    private float _dayProgress; // 当天进度 (0.0-1.0)
    private int _clockDay = 1; // 当前天数 (1-60)
    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;
    private float _targetAmbientIntensity;

    // 公共属性
    public int ClockDay => _clockDay;
    public float DayProgress => _dayProgress;
    public TimeOfDay CurrentTimeOfDay => _currentTimeOfDay;

    // 私有构造函数
    private ClockModel()
    {
        InitializeTime();
    }

    // 公共方法
    /// <summary>
    /// 更新时间系统 - 需要外部定期调用
    /// </summary>
    public void UpdateTime()
    {
        UpdateTimeProgress();
        UpdateTimeOfDay();
        UpdateAmbientLight();
    }

    // 私有方法
    private void InitializeTime()
    {
        _dayProgress = 0f;
        _currentTimeOfDay = TimeOfDay.Day;
        _targetAmbientIntensity = _dayAmbientIntensity;
        RenderSettings.ambientIntensity = _targetAmbientIntensity;
    }

    private void UpdateTimeProgress()
    {
        _dayProgress += Time.deltaTime / _dayDuration;
        
        if (_dayProgress >= 1f)
        {
            _dayProgress = 0f;
            _clockDay = Mathf.Min(_clockDay + 1, _maxDays);
        }
    }

    private void UpdateTimeOfDay()
    {
        TimeOfDay newTimeOfDay = GetTimeOfDayFromProgress(_dayProgress);
        
        if (newTimeOfDay != _currentTimeOfDay)
        {
            TimeOfDay previousTime = _currentTimeOfDay;
            _currentTimeOfDay = newTimeOfDay;
            
            UpdateTargetAmbientIntensity();
            
            // 发布时间段切换事件
            EventManager.Instance.Publish(new TimeOfDayChangeEvent(previousTime, _currentTimeOfDay));
        }
    }

    private TimeOfDay GetTimeOfDayFromProgress(float progress)
    {
        if (progress < 0.5f) return TimeOfDay.Day;       // 白天 (0.0-0.5)
        if (progress < 0.75f) return TimeOfDay.Dusk;     // 黄昏 (0.5-0.75)
        return TimeOfDay.Night;                          // 夜晚 (0.75-1.0)
    }

    private void UpdateTargetAmbientIntensity()
    {
        switch (_currentTimeOfDay)
        {
            case TimeOfDay.Day:
                _targetAmbientIntensity = _dayAmbientIntensity;
                break;
            case TimeOfDay.Dusk:
                _targetAmbientIntensity = _duskAmbientIntensity;
                break;
            case TimeOfDay.Night:
                _targetAmbientIntensity = _nightAmbientIntensity;
                break;
        }
    }

    private void UpdateAmbientLight()
    {
        RenderSettings.ambientIntensity = Mathf.Lerp(
            RenderSettings.ambientIntensity, 
            _targetAmbientIntensity, 
            Time.deltaTime * 2f
        );
    }
}
