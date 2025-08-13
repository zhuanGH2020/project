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
    private float _dayDuration = GameSettings.ClockDayDuration; // 一天持续时间（秒）
    private int _maxDays = GameSettings.ClockMaxDays; // 最大天数
    private float _dayAmbientIntensity = GameSettings.ClockDayAmbientIntensity; // 白天环境光强度
    private float _duskAmbientIntensity = GameSettings.ClockDuskAmbientIntensity; // 黄昏环境光强度  
    private float _nightAmbientIntensity = GameSettings.ClockNightAmbientIntensity; // 夜晚环境光强度
    private float _dayProgress; // 当天进度 (0.0-1.0)
    private int _clockDay = 1; // 当前天数 (1-60)
    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;
    private float _targetAmbientIntensity;
    
    // 时间控制
    private bool _isTimePaused = false; // 时间是否暂停

    // 公共属性
    public int ClockDay => _clockDay;
    public float DayProgress => _dayProgress;
    public TimeOfDay CurrentTimeOfDay => _currentTimeOfDay;
    public bool IsTimePaused => _isTimePaused;

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
        if (!_isTimePaused)
        {
            UpdateTimeProgress();
            UpdateTimeOfDay();
        }
        UpdateAmbientLight();
    }

    // 私有方法
    private void InitializeTime()
    {
        _dayProgress = 0f;
        _currentTimeOfDay = TimeOfDay.Day;
        _targetAmbientIntensity = _dayAmbientIntensity;
        
        // 确保环境光模式正确设置
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientIntensity = _targetAmbientIntensity;
        RenderSettings.ambientLight = Color.white;
        
        Debug.Log($"[ClockModel] Initialize Time - Mode: {RenderSettings.ambientMode}, Current: {RenderSettings.ambientIntensity}, Target: {_targetAmbientIntensity}, Day Intensity: {_dayAmbientIntensity}");
    }

    private void UpdateTimeProgress()
    {
        _dayProgress += Time.deltaTime / _dayDuration;
        
        if (_dayProgress >= 1f)
        {
            _dayProgress = 0f;
            int previousDay = _clockDay;
            _clockDay = Mathf.Min(_clockDay + 1, _maxDays);
            
            // 发布天数变化事件
            if (_clockDay != previousDay)
            {
                EventManager.Instance.Publish(new DayChangeEvent(previousDay, _clockDay));
            }
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
        if (progress < GameSettings.ClockDayTimeRatio) return TimeOfDay.Day;       // 白天 (0.0-0.5)
        if (progress < GameSettings.ClockDuskTimeRatio) return TimeOfDay.Dusk;     // 黄昏 (0.5-0.75)
        return TimeOfDay.Night;                          // 夜晚 (0.75-1.0)
    }

    private void UpdateTargetAmbientIntensity()
    {
        switch (_currentTimeOfDay)
        {
            case TimeOfDay.Day:
                _targetAmbientIntensity = _dayAmbientIntensity;
                RenderSettings.ambientLight = Color.white; // 白天：亮白色
                break;
            case TimeOfDay.Dusk:
                _targetAmbientIntensity = _duskAmbientIntensity;
                RenderSettings.ambientLight = new Color(1f, 0.7f, 0.4f); // 黄昏：橙黄色
                break;
            case TimeOfDay.Night:
                _targetAmbientIntensity = _nightAmbientIntensity;
                RenderSettings.ambientLight = new Color(0.2f, 0.3f, 0.6f); // 夜晚：深蓝色
                break;
        }
        
        // 同时调整主光源强度，让夜晚更暗
        Light mainLight = RenderSettings.sun;
        if (mainLight != null)
        {
            switch (_currentTimeOfDay)
            {
                case TimeOfDay.Day:
                    mainLight.intensity = 1.0f; // 白天：最亮
                    mainLight.color = Color.white;
                    break;
                case TimeOfDay.Dusk:
                    mainLight.intensity = 0.5f; // 黄昏：中等
                    mainLight.color = new Color(1f, 0.8f, 0.6f);
                    break;
                case TimeOfDay.Night:
                    mainLight.intensity = 0.1f; // 夜晚：很暗
                    mainLight.color = new Color(0.6f, 0.7f, 1f);
                    break;
            }
        }
    }

    private void UpdateAmbientLight()
    {
        float oldIntensity = RenderSettings.ambientIntensity;
        RenderSettings.ambientIntensity = Mathf.Lerp(
            RenderSettings.ambientIntensity, 
            _targetAmbientIntensity, 
            Time.deltaTime * 2f
        );
        
        // 只在强度变化较大时打印调试信息
        if (Mathf.Abs(RenderSettings.ambientIntensity - oldIntensity) > 0.01f)
        {
            Debug.Log($"[ClockModel] Ambient Light Update - From: {oldIntensity:F2} To: {RenderSettings.ambientIntensity:F2} Target: {_targetAmbientIntensity:F2} TimeOfDay: {_currentTimeOfDay} Color: {RenderSettings.ambientLight}");
        }
    }
    
    // 设置游戏时间 - 用于加载存档
    /// <param name="day">天数</param>
    /// <param name="progress">当天进度</param>
    /// <param name="timeOfDay">时间段</param>
    public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)
    {
        _clockDay = Mathf.Clamp(day, 1, _maxDays);
        _dayProgress = Mathf.Clamp01(progress);
        _currentTimeOfDay = timeOfDay;
        UpdateTargetAmbientIntensity();
        RenderSettings.ambientIntensity = _targetAmbientIntensity;
    }

    /// <summary>
    /// 暂停时间系统
    /// 时间记录暂停，环境光设为白天状态
    /// </summary>
    public void PauseTime()
    {
        if (_isTimePaused) return;

        _isTimePaused = true;

        // 强制设置为白天环境光
        _targetAmbientIntensity = _dayAmbientIntensity;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientIntensity = _dayAmbientIntensity;
        RenderSettings.ambientLight = Color.white;
        
        // 同时设置主光源为白天状态
        Light mainLight = RenderSettings.sun;
        if (mainLight != null)
        {
            mainLight.intensity = 1.0f;
            mainLight.color = Color.white;
        }
        
        Debug.Log($"[ClockModel] Time paused - Ambient Intensity: {RenderSettings.ambientIntensity}, Mode: {RenderSettings.ambientMode}");
    }

    /// <summary>
    /// 恢复时间系统
    /// 恢复计时和根据当前时间段恢复环境光
    /// </summary>
    public void ResumeTime()
    {
        if (!_isTimePaused) return;

        _isTimePaused = false;

        // 根据当前时间段设置正确的环境光目标强度
        // UpdateAmbientLight()会平滑过渡到目标强度
        UpdateTargetAmbientIntensity();
        
        Debug.Log($"[ClockModel] Time resumed - Current TimeOfDay: {_currentTimeOfDay}, Target Intensity: {_targetAmbientIntensity}, Current Intensity: {RenderSettings.ambientIntensity}");
    }
}
