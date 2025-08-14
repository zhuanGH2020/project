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
    private float _dayDuration = GameSettings.ClockDayDuration;
    private int _maxDays = GameSettings.ClockMaxDays;
    private float _dayProgress;
    private int _clockDay = 1;
    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;
    
    private readonly float _dayEndRatio;
    private readonly float _duskEndRatio;
    
    private bool _isTimePaused = false;
    
    private bool _isRotating = false;
    private Vector3 _startRotation;
    private Vector3 _targetRotation;
    private float _rotationTimer = 0f;
    private float _rotationDuration = 0f;
    
    // 环境光照强度过渡相关字段
    private bool _isAmbientTransitioning = false;
    private float _startAmbientIntensity;
    private float _targetAmbientIntensity;
    private float _ambientTimer = 0f;
    private float _ambientDuration = 0f;

    // 公共属性
    public int ClockDay => _clockDay;
    public float DayProgress => _dayProgress;
    public TimeOfDay CurrentTimeOfDay => _currentTimeOfDay;
    public bool IsTimePaused => _isTimePaused;

    private ClockModel()
    {
        _dayEndRatio = GameSettings.ClockDayTimeRatio;
        _duskEndRatio = GameSettings.ClockDayTimeRatio + GameSettings.ClockDuskTimeRatio;
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
        UpdateMainLightRotation();
        UpdateAmbientIntensity();
    }

    // 私有方法
    private void InitializeTime()
    {
        _dayProgress = 0f;
        _currentTimeOfDay = TimeOfDay.Day;
        
        // 设置主光源初始旋转
        Light mainLight = RenderSettings.sun;
        if (mainLight != null)
        {
            mainLight.transform.rotation = Quaternion.Euler(GameSettings.ClockDayMainLightRotation);
        }
    }

    private void UpdateTimeProgress()
    {
        _dayProgress += Time.deltaTime / _dayDuration;
        
        if (_dayProgress >= 1f)
        {
            _dayProgress = 0f;
            int previousDay = _clockDay;
            _clockDay = Mathf.Min(_clockDay + 1, _maxDays);
            
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
            
            UpdateMainLight(previousTime);
            EventManager.Instance.Publish(new TimeOfDayChangeEvent(previousTime, _currentTimeOfDay));
        }
    }

    private TimeOfDay GetTimeOfDayFromProgress(float progress)
    {
        if (progress < _dayEndRatio) return TimeOfDay.Day;
        if (progress < _duskEndRatio) return TimeOfDay.Dusk;
        return TimeOfDay.Night;
    }

    private void UpdateMainLight(TimeOfDay previousTime)
    {
        Light mainLight = RenderSettings.sun;
        if (mainLight != null)
        {
            switch (_currentTimeOfDay)
            {
                case TimeOfDay.Day:
                    mainLight.color = GameSettings.ClockDayMainLightColor;
                    break;
                case TimeOfDay.Dusk:
                    mainLight.color = GameSettings.ClockDuskMainLightColor;
                    break;
                case TimeOfDay.Night:
                    mainLight.color = GameSettings.ClockNightMainLightColor;
                    break;
            }
            Vector3 targetRotation = Vector3.zero;
            float rotationDuration = 0f;

            switch (_currentTimeOfDay)
            {
                case TimeOfDay.Day:
                    targetRotation = GameSettings.ClockDayMainLightRotation;
                    rotationDuration = previousTime == TimeOfDay.Night ? GameSettings.ClockNightToDayRotationTime : 0f;
                    break;
                case TimeOfDay.Dusk:
                    targetRotation = GameSettings.ClockDuskMainLightRotation;
                    rotationDuration = previousTime == TimeOfDay.Day ? GameSettings.ClockDayToDuskRotationTime : 0f;
                    break;
                case TimeOfDay.Night:
                    targetRotation = GameSettings.ClockNightMainLightRotation;
                    rotationDuration = previousTime == TimeOfDay.Dusk ? GameSettings.ClockDuskToNightRotationTime : 0f;
                    break;
            }

            if (rotationDuration > 0f)
            {
                StartMainLightRotation(targetRotation, rotationDuration);
            }
            else if (!_isRotating)
            {
                mainLight.transform.rotation = Quaternion.Euler(targetRotation);
            }
        }
        
        // 环境光照强度过渡控制
        float targetAmbientIntensity = 2f; // 默认强度
        float ambientDuration = 0f;
        
        switch (_currentTimeOfDay)
        {
            case TimeOfDay.Night:
                targetAmbientIntensity = 0f;
                if (previousTime == TimeOfDay.Dusk)
                {
                    ambientDuration = GameSettings.ClockDuskToNightRotationTime;
                }
                break;
            case TimeOfDay.Day:
                targetAmbientIntensity = 2f;
                if (previousTime == TimeOfDay.Night)
                {
                    ambientDuration = GameSettings.ClockNightToDayRotationTime;
                }
                break;
        }
        
        if (ambientDuration > 0f)
        {
            StartAmbientIntensityTransition(targetAmbientIntensity, ambientDuration);
        }
        else if (!_isAmbientTransitioning)
        {
            RenderSettings.ambientIntensity = targetAmbientIntensity;
        }
    }

    /// <summary>
    /// 更新主光源旋转
    /// </summary>
    private void UpdateMainLightRotation()
    {
        if (!_isRotating) return;

        Light mainLight = RenderSettings.sun;
        if (mainLight == null) return;

        _rotationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_rotationTimer / _rotationDuration);

        Vector3 currentRotation = Vector3.Lerp(_startRotation, _targetRotation, progress);
        mainLight.transform.rotation = Quaternion.Euler(currentRotation);

        if (progress >= 1f)
        {
            _isRotating = false;
        }
    }

    /// <summary>
    /// 启动主光源旋转
    /// </summary>
    private void StartMainLightRotation(Vector3 targetRotation, float duration)
    {
        Light mainLight = RenderSettings.sun;
        if (mainLight == null) return;

        _startRotation = mainLight.transform.rotation.eulerAngles;
        _targetRotation = targetRotation;
        _rotationTimer = 0f;
        _rotationDuration = duration;
        _isRotating = true;
    }

    /// <summary>
    /// 更新环境光照强度过渡
    /// </summary>
    private void UpdateAmbientIntensity()
    {
        if (!_isAmbientTransitioning) return;

        _ambientTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_ambientTimer / _ambientDuration);

        float currentIntensity = Mathf.Lerp(_startAmbientIntensity, _targetAmbientIntensity, progress);
        RenderSettings.ambientIntensity = currentIntensity;

        if (progress >= 1f)
        {
            _isAmbientTransitioning = false;
        }
    }

    /// <summary>
    /// 启动环境光照强度过渡
    /// </summary>
    private void StartAmbientIntensityTransition(float targetIntensity, float duration)
    {
        _startAmbientIntensity = RenderSettings.ambientIntensity;
        _targetAmbientIntensity = targetIntensity;
        _ambientTimer = 0f;
        _ambientDuration = duration;
        _isAmbientTransitioning = true;
    }
    
    public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)
    {
        _clockDay = Mathf.Clamp(day, 1, _maxDays);
        _dayProgress = Mathf.Clamp01(progress);
        _currentTimeOfDay = timeOfDay;
        UpdateMainLight(_currentTimeOfDay);
    }

    /// <summary>
    /// 暂停时间系统
    /// </summary>
    public void PauseTime()
    {
        if (_isTimePaused) return;

        _isTimePaused = true;
    }

    /// <summary>
    /// 恢复时间系统
    /// </summary>
    public void ResumeTime()
    {
        if (!_isTimePaused) return;

        _isTimePaused = false;
        UpdateMainLight(_currentTimeOfDay);
    }
}
