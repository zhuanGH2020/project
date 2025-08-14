using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 菜单视图 - 游戏结束时的重开界面
/// 显示死亡天数信息，提供重开游戏功能
/// </summary>
public class MenuView : BaseView
{
    /// <summary>
    /// UI预制体路径 - 供UIManager使用
    /// </summary>
    public static string PrefabPath => "Prefabs/UI/pbui_menu";
    
    private TextMeshProUGUI _txtTitle;
    private Button _btnRevert;
    
    void Start()
    {
        InitializeUI();
        UpdateDeathMessage();
    }
    
    void OnDestroy()
    {
        // 移除按钮监听器
        _btnRevert?.onClick.RemoveListener(OnRevertButtonClick);
    }
    
    /// <summary>
    /// 初始化UI组件和事件监听
    /// </summary>
    private void InitializeUI()
    {
        _txtTitle = transform.Find("txt_title")?.GetComponent<TextMeshProUGUI>();
        _btnRevert = transform.Find("btn_revert")?.GetComponent<Button>();
        
        _btnRevert?.onClick.AddListener(OnRevertButtonClick);
    }
    
    /// <summary>
    /// 更新死亡信息显示
    /// 从ClockModel获取当前天数并显示"你死于第n天"
    /// </summary>
    private void UpdateDeathMessage()
    {
        if (_txtTitle != null)
        {
            int currentDay = ClockModel.Instance.ClockDay;
            _txtTitle.text = $"你死于第{currentDay}天";
        }
    }
    
    /// <summary>
    /// 重开按钮点击事件处理
    /// 删除当前存档并重置游戏状态，成功后关闭UI
    /// </summary>
    private void OnRevertButtonClick()
    {
        bool deleteSuccess = DebugModel.Instance.DeleteCurrentSaveAndReset();
        if (deleteSuccess)
        {
            // 恢复时间系统
            DebugModel.Instance.SetTimeEnabled(true);
            
            CloseMenu();
        }
        else
        {
            Debug.LogError("[MenuView] Failed to reset game");
        }
    }
    
    /// <summary>
    /// 关闭菜单UI
    /// 通过UIManager隐藏并销毁UI，释放内存
    /// </summary>
    private void CloseMenu()
    {
        UIManager.Instance.Destroy<MenuView>();
    }
} 