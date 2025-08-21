using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 通知视图 - 显示临时通知信息
/// 接收NoticeEvent事件显示通知文本，3秒后自动隐藏
/// </summary>
public class NoticeView : BaseView
{
    private TextMeshProUGUI txt_content;
    
    private Coroutine _hideCoroutine;
    private const float HIDE_DELAY = 3.0f; // 3秒后隐藏

    protected override void Start()
    {
        base.Start();
        InitializeView();
        SubscribeEvents();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeEvents();
        
        // 清理协程
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
    }

    // 初始化视图组件
    private void InitializeView()
    {
        // 查找UI组件
        txt_content = transform.Find("txt_content")?.GetComponent<TextMeshProUGUI>();
        
        // 初始状态隐藏通知
        SetNoticeVisible(false);
    }

    // 订阅通知事件
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<NoticeEvent>(OnNoticeEvent);
    }

    // 取消订阅事件
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<NoticeEvent>(OnNoticeEvent);
    }

    // 处理通知事件
    private void OnNoticeEvent(NoticeEvent eventData)
    {
        if (eventData == null || string.IsNullOrEmpty(eventData.Message))
        {
            Debug.LogWarning("Invalid notice event data");
            return;
        }

        ShowNotice(eventData.Message);
    }

    /// <summary>
    /// 显示通知信息
    /// 更新文本内容，显示通知，并启动3秒后隐藏的协程
    /// </summary>
    private void ShowNotice(string message)
    {
        // 停止之前的隐藏协程
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }

        // 更新通知文本
        if (txt_content != null)
        {
            txt_content.text = message;
        }

        // 显示通知
        SetNoticeVisible(true);

        // 启动3秒后隐藏协程
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    // 3秒后隐藏的协程
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(HIDE_DELAY);
        
        SetNoticeVisible(false);
        _hideCoroutine = null;
    }

    // 设置通知的可见性
    private void SetNoticeVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 立即隐藏通知
    /// 停止延时隐藏协程并立即隐藏界面
    /// </summary>
    public void HideNotice()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        
        SetNoticeVisible(false);
    }
}
