using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BaseView使用示例 - 简单示例展示用法
/// 这个文件仅作为参考，实际项目中可以删除
/// </summary>

#region 基础示例

// 示例1：最简单的用法
public class SimpleItemView : BaseView
{
    private void Start()
    {
        // 一行代码设置物品图标
        LoadAndSetItemIcon("img_icon", 1000);
        
        // 一行代码设置背景图片
        LoadAndSetSprite("img_background", "UI/item_background");
    }
    
    // 不需要OnDestroy，BaseView自动释放资源
}

// 示例2：需要额外清理的View
public class AdvancedItemView : BaseView
{
    private Coroutine _updateCoroutine;
    
    private void Start()
    {
        LoadAndSetItemIcon("img_icon", 1000);
        _updateCoroutine = StartCoroutine(UpdateLoop());
    }
    
    private System.Collections.IEnumerator UpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
        }
    }
    
    protected override void OnViewDestroy()
    {
        // 停止协程等额外清理工作
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        // 注意：资源会自动释放，无需手动处理
    }
}

#endregion 