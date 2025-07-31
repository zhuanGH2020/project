using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 制作菜单视图 - 显示具体制作类型的详细内容
/// 响应制作类型选择事件，动态加载和显示对应的制作配方界面
/// 需要挂载到GameObject上使用
/// </summary>
public class MakeMenuView : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI txt_title; // 菜单标题文本组件
    [SerializeField] private GameObject content_panel;   // 动态内容容器面板
    
    private int _currentTypeId = -1; // 当前选中的制作类型ID
    private const string TITLE_PREFIX = "制作 - "; // 标题前缀常量，避免字符串重复分配

    void Start()
    {
        InitializeView();
        SubscribeEvents();
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // 初始化视图组件，提供自动查找和默认状态设置
    private void InitializeView()
    {
        // 自动查找UI组件（如果未在Inspector中指定）
        if (txt_title == null)
            txt_title = transform.Find("txt_title")?.GetComponent<TextMeshProUGUI>();
        
        if (content_panel == null)
            content_panel = transform.Find("content_panel")?.gameObject;

        // 初始状态隐藏菜单，避免未初始化时的显示问题
        SetMenuVisible(false);
    }

    // 订阅制作系统相关事件
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<MakeMenuOpenEvent>(OnMakeMenuOpen);
        EventManager.Instance.Subscribe<MakeTypeSelectedEvent>(OnMakeTypeSelected);
    }

    // 取消订阅事件，防止内存泄漏
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<MakeMenuOpenEvent>(OnMakeMenuOpen);
        EventManager.Instance.Unsubscribe<MakeTypeSelectedEvent>(OnMakeTypeSelected);
    }

    // 处理制作菜单打开事件
    private void OnMakeMenuOpen(MakeMenuOpenEvent eventData)
    {
        if (eventData == null || eventData.TypeId <= 0)
        {
            Debug.LogWarning("Invalid MakeMenuOpenEvent data");
            return;
        }
        
        _currentTypeId = eventData.TypeId;
        ShowMakeMenu(eventData.TypeId);
    }

    // 处理制作类型选择事件，更新菜单标题
    private void OnMakeTypeSelected(MakeTypeSelectedEvent eventData)
    {
        if (eventData != null && !string.IsNullOrEmpty(eventData.TypeName))
        {
            UpdateMenuTitle(eventData.TypeName);
        }
    }

    /// <summary>
    /// 显示制作菜单的完整流程
    /// 包括数据验证、界面显示、内容加载和状态更新
    /// </summary>
    private void ShowMakeMenu(int typeId)
    {
        var makeTypeData = MakeModel.Instance.GetMakeTypeData(typeId);
        if (makeTypeData == null)
        {
            Debug.LogWarning($"未找到制作类型数据: TypeId={typeId}");
            return;
        }

        SetMenuVisible(true);
        UpdateMenuTitle(makeTypeData.typeName);
        LoadMenuContent(typeId);
        
        Debug.Log($"打开制作菜单: {makeTypeData.typeName} (ID: {typeId})");
    }

    // 更新菜单标题，使用常量前缀避免字符串分配
    private void UpdateMenuTitle(string typeName)
    {
        if (txt_title != null && !string.IsNullOrEmpty(typeName))
        {
            txt_title.text = TITLE_PREFIX + typeName;
        }
    }

    /// <summary>
    /// 根据制作类型ID加载对应的菜单内容
    /// 清理现有内容后动态创建新的制作配方UI元素
    /// </summary>
    private void LoadMenuContent(int typeId)
    {
        if (content_panel == null) 
        {
            Debug.LogWarning("Content panel未设置，无法加载菜单内容");
            return;
        }
        
        // 清理现有内容
        ClearMenuContent();
        
        // TODO: 根据typeId从配置表加载具体制作项
        // 例如：显示该类型下的所有制作配方、材料需求等
        Debug.Log($"加载制作类型 {typeId} 的内容");
    }

    // 清理菜单内容面板中的所有子对象
    private void ClearMenuContent()
    {
        if (content_panel == null) return;
        
        for (int i = content_panel.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(content_panel.transform.GetChild(i).gameObject);
        }
    }

    // 设置菜单和内容面板的可见性
    private void SetMenuVisible(bool visible)
    {
        gameObject.SetActive(visible);
        
        if (content_panel != null)
        {
            content_panel.SetActive(visible);
        }
    }

    /// <summary>
    /// 关闭制作菜单并清理状态
    /// 重置选择状态，隐藏界面，通知Model清除选择
    /// </summary>
    public void CloseMakeMenu()
    {
        _currentTypeId = -1;
        SetMenuVisible(false);
        MakeModel.Instance.ClearSelection();
        
        Debug.Log("关闭制作菜单");
    }
}
