using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 烹饪槽位组件 - 处理单个槽位的交互
/// 需要挂载到每个烹饪槽位的GameObject上
/// </summary>
public class CookingSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Settings")]
    [SerializeField] private int _slotIndex;  // 槽位索引

    public int SlotIndex => _slotIndex;

    /// <summary>
    /// 处理拖拽放置事件
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // 检查是否有选中的物品
        var selectedItem = PackageModel.Instance.SelectedItem;
        if (selectedItem == null)
        {
            Debug.LogWarning("[CookingSlot] 没有选中的物品");
            return;
        }

        // 尝试将物品放入槽位
        bool success = CookingModel.Instance.PlaceItemToSlot(_slotIndex, selectedItem.itemId, 1);
        if (success)
        {
            // 从选中物品中减去1个
            selectedItem.count -= 1;
            
            // 如果还有剩余，放回背包；如果没有了，清除选中状态
            if (selectedItem.count > 0)
            {
                PackageModel.Instance.UnselectItem();  // 将剩余物品放回背包
            }
            else
            {
                // 物品全部用完，清除选中状态（不放回背包）
                PackageModel.Instance.ClearSelectedItem();
            }
        }
        else
        {
            PackageModel.Instance.UnselectItem();  // 放置失败，取消选中
        }
    }

    /// <summary>
    /// 处理点击事件（移除槽位中的物品）
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
{
    if (eventData.button == PointerEventData.InputButton.Left)
    {
        // 左键点击时，尝试放置选中的物品
        var selectedItem = PackageModel.Instance.SelectedItem;
        if (selectedItem != null)
        {
            // 尝试将物品放入槽位
            bool success = CookingModel.Instance.PlaceItemToSlot(_slotIndex, selectedItem.itemId, 1);
            if (success)
            {
                // 从选中物品中减去1个
                selectedItem.count -= 1;
                
                // 如果还有剩余，放回背包；如果没有了，清除选中状态
                if (selectedItem.count > 0)
                {
                    PackageModel.Instance.UnselectItem();  // 将剩余物品放回背包
                }
                else
                {
                    // 物品全部用完，清除选中状态（不放回背包）
                    PackageModel.Instance.ClearSelectedItem();
                }
            }
            else
            {
                PackageModel.Instance.UnselectItem();  // 放置失败，取消选中
            }
        }
    }
    else if (eventData.button == PointerEventData.InputButton.Right)
    {
        // 右键点击移除物品
        CookingModel.Instance.RemoveItemFromSlot(_slotIndex);
    }
}

}