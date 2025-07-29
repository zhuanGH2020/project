// Last modified_2: 2024-12-19 14:30:15
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 时钟模型 - 管理游戏时间循环和环境光照
public class ClockModel : MonoBehaviour
{
    private float _dayDuration = 60f; // 一天持续时间（秒）
    private int _maxDays = 60; // 最大天数

    private float _dayAmbientIntensity = 1.0f; // 白天环境光强度
    private float _duskAmbientIntensity = 0.6f; // 黄昏环境光强度  
    private float _nightAmbientIntensity = 0.3f; // 夜晚环境光强度

    // 时间状态
    private float _dayProgress; // 当天进度 (0.0-1.0)
    private int _clockDay = 1; // 当前天数 (1-60)
    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;
    private float _targetAmbientIntensity;

    // 当前天数
    public int ClockDay => _clockDay;

    // 当天进度 (0.0-1.0)
    public float DayProgress => _dayProgress;

    // 当前时间段
    public TimeOfDay CurrentTimeOfDay => _currentTimeOfDay;

    void Start()
    {
        InitializeTime();
    }

    void Update()
    {
        UpdateTimeProgress();
        UpdateTimeOfDay();
        UpdateAmbientLight();
    }

    // 初始化时间系统
    private void InitializeTime()
    {
        _dayProgress = 0f;
        _currentTimeOfDay = TimeOfDay.Day;
        _targetAmbientIntensity = _dayAmbientIntensity;
        RenderSettings.ambientIntensity = _targetAmbientIntensity;
    }

    // 更新时间进度
    private void UpdateTimeProgress()
    {
        _dayProgress += Time.deltaTime / _dayDuration;
        
        if (_dayProgress >= 1f)
        {
            _dayProgress = 0f;
            _clockDay = Mathf.Min(_clockDay + 1, _maxDays);
        }
    }

    // 更新时间段状态
    private void UpdateTimeOfDay()
    {
        TimeOfDay newTimeOfDay = GetTimeOfDayFromProgress(_dayProgress);
        
        if (newTimeOfDay != _currentTimeOfDay)
        {
            TimeOfDay previousTime = _currentTimeOfDay;
            _currentTimeOfDay = newTimeOfDay;
            
            // 更新目标光照强度
            UpdateTargetAmbientIntensity();
            
            // 发布时间段切换事件
            EventManager.Instance.Publish(new TimeOfDayChangeEvent(previousTime, _currentTimeOfDay));
        }
    }

    // 根据进度获取时间段
    private TimeOfDay GetTimeOfDayFromProgress(float progress)
    {
        if (progress < 0.5f) return TimeOfDay.Day;       // 白天 (0.0-0.5)
        if (progress < 0.75f) return TimeOfDay.Dusk;     // 黄昏 (0.5-0.75)
        return TimeOfDay.Night;                          // 夜晚 (0.75-1.0)
    }

    // 更新目标环境光强度
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

    // 平滑更新环境光照
    private void UpdateAmbientLight()
    {
        RenderSettings.ambientIntensity = Mathf.Lerp(
            RenderSettings.ambientIntensity, 
            _targetAmbientIntensity, 
            Time.deltaTime * 2f
        );
    }
}
