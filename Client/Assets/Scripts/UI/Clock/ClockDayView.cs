// Last modified_2: 2024-12-19 14:35:10
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 时钟日期视图 - 显示时间段和天数信息
public class ClockDayView : MonoBehaviour
{
    private TextMeshProUGUI txt_time; // 时间段文本
    private TextMeshProUGUI txt_day;  // 天数文本

    private ClockModel _clockModel; // 时钟模型引用

    void Start()
    {
        InitializeView();
        SubscribeEvents();
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void InitializeView()
    {
        // 自动查找UI组件（如果未在Inspector中指定）
        txt_time = transform.Find("txt_time")?.GetComponent<TextMeshProUGUI>();
        txt_day = transform.Find("txt_day")?.GetComponent<TextMeshProUGUI>();
        _clockModel = FindObjectOfType<ClockModel>();

        RefreshTimeDisplay();
        RefreshDayDisplay();
    }

    // 订阅事件
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<TimeOfDayChangeEvent>(OnTimeOfDayChanged);
    }

    // 取消订阅事件
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<TimeOfDayChangeEvent>(OnTimeOfDayChanged);
    }

    // 时间段变化事件处理
    private void OnTimeOfDayChanged(TimeOfDayChangeEvent eventData)
    {
        RefreshTimeDisplay();
        RefreshDayDisplay();
    }

    // 刷新时间段显示
    private void RefreshTimeDisplay()
    {
        if (txt_time == null || _clockModel == null) return;

        string timeText = GetTimeOfDayText(_clockModel.CurrentTimeOfDay);
        txt_time.text = timeText;
    }

    // 刷新天数显示
    private void RefreshDayDisplay()
    {
        if (txt_day == null || _clockModel == null) return;

        txt_day.text = _clockModel.ClockDay.ToString();
    }

    // 根据时间段枚举获取中文文本
    private string GetTimeOfDayText(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                return "白天";
            case TimeOfDay.Dusk:
                return "黄昏";
            case TimeOfDay.Night:
                return "夜晚";
            default:
                return "未知";
        }
    }

    void Update()
    {
        // 每帧更新天数显示（处理天数变化但没有时间段切换的情况）
        if (_clockModel != null)
        {
            RefreshDayDisplay();
        }
    }
}
