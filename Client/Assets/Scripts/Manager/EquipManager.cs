using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装备管理器 - 统一管理所有装备相关逻辑
/// 作为装备系统的单一数据源和操作入口
/// </summary>
public class EquipManager
{
    private static EquipManager _instance;
    public static EquipManager Instance => _instance ??= new EquipManager();
    
    // 装备数据（单一数据源）- 直接存储装备ID
    private Dictionary<EquipPart, int> _equippedItems = new Dictionary<EquipPart, int>();
    
    private EquipManager() { }
    
    /// <summary>
    /// 装备物品到指定部位
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <param name="equipPart">装备部位</param>
    /// <returns>是否装备成功</returns>
    public bool EquipItem(int itemId, EquipPart equipPart)
    {
        // 检查物品是否为装备类型
        var itemConfig = ItemManager.Instance.GetItem(itemId);
        if (itemConfig == null || !itemConfig.IsEquip())
        {
            Debug.LogWarning($"[EquipManager] 物品 {itemId} 不是装备类型");
            return false;
        }
        
        // 检查装备部位是否匹配
        var equipReader = ConfigManager.Instance.GetReader("Equip");
        if (equipReader == null || !equipReader.HasKey(itemId))
        {
            Debug.LogWarning($"[EquipManager] 无法获取装备 {itemId} 的配置");
            return false;
        }

        EquipPart configEquipPart = equipReader.GetValue<EquipPart>(itemId, "Type", EquipPart.None);
        
        if (configEquipPart != equipPart)
        {
            Debug.LogWarning($"[EquipManager] 物品 {itemId} 不能装备到 {equipPart} 部位，配置部位为 {configEquipPart}");
            return false;
        }
        
        // 检查背包中是否有该物品
        if (!PackageModel.Instance.HasEnoughItem(itemId, 1))
        {
            Debug.LogWarning($"[EquipManager] 背包中没有物品 {itemId}");
            return false;
        }
        
        // 卸下同部位的装备
        UnequipItem(equipPart);
        
        // 从背包移除物品
        PackageModel.Instance.RemoveItem(itemId, 1);
        
        // 创建装备组件并装备到Player
        var player = Player.Instance;
        if (player != null)
        {
            bool equipSuccess = player.Equip(itemId);
            if (equipSuccess)
            {
                // 直接存储装备ID
                _equippedItems[equipPart] = itemId;
                
                // 使用事件系统发布装备变化事件
                EventManager.Instance.Publish(new EquipChangeEvent(equipPart, itemId, true));
                
                Debug.Log($"[EquipManager] 成功装备物品 {itemId} 到 {equipPart} 部位");
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 卸下指定部位的装备
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    /// <returns>是否卸下成功</returns>
    public bool UnequipItem(EquipPart equipPart)
    {
        if (!_equippedItems.ContainsKey(equipPart))
        {
            return false; // 该部位没有装备
        }
        
        int equipId = _equippedItems[equipPart];
        if (equipId <= 0) return false;
        
        // 从装备字典中移除
        _equippedItems.Remove(equipPart);
        
        // 从Player移除装备组件
        var player = Player.Instance;
        if (player != null)
        {
            var equipComponent = GetEquipComponentByPart(equipPart);
            if (equipComponent != null)
            {
                RemoveEquipComponent(player, equipComponent);
            }
        }

        // 将装备物品放回背包
        bool addSuccess = PackageModel.Instance.AddItem(equipId, 1);
        if (!addSuccess)
        {
            Debug.LogWarning($"[EquipManager] 背包已满，无法卸下装备 {equipId}");
        }
        
        // 使用事件系统发布装备变化事件
        EventManager.Instance.Publish(new EquipChangeEvent(equipPart, equipId, false));
        
        Debug.Log($"[EquipManager] 成功卸下 {equipPart} 部位的装备 {equipId}");
        return true;
    }
    

    
        /// <summary>
    /// 获取指定部位的装备ID
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    /// <returns>装备ID，如果没有装备则返回0</returns>
    public int GetEquippedItemId(EquipPart equipPart)
    {
        return _equippedItems.ContainsKey(equipPart) ? _equippedItems[equipPart] : 0;
    }
    
    /// <summary>
    /// 获取所有已装备物品的ID字典
    /// </summary>
    /// <returns>装备部位到装备ID的映射字典</returns>
    public Dictionary<EquipPart, int> GetAllEquippedItemIds()
    {
        return new Dictionary<EquipPart, int>(_equippedItems);
    }
    
    /// <summary>
    /// 检查指定部位是否有装备
    /// </summary>
    /// <param name="equipPart">装备部位</param>
    /// <returns>是否有装备</returns>
    public bool HasEquippedItem(EquipPart equipPart)
    {
        return _equippedItems.ContainsKey(equipPart) && _equippedItems[equipPart] > 0;
    }
    

    
    /// <summary>
    /// 从存档加载装备数据
    /// </summary>
    /// <param name="equippedItems">存档中的装备ID列表</param>
    public void LoadEquippedItemsFromSave(List<int> equippedItems)
    {
        _equippedItems.Clear();
        
        if (equippedItems == null || equippedItems.Count == 0) 
        {
            return;
        }
        
        var player = Player.Instance;
        if (player == null) 
        {
            return;
        }
        
        foreach (int equipId in equippedItems)
        {
            if (equipId <= 0) continue;
            
            // 获取装备配置
            var equipReader = ConfigManager.Instance.GetReader("Equip");
            if (equipReader == null || !equipReader.HasKey(equipId)) 
            {
                Debug.LogWarning($"[EquipManager] Equipment config not found: {equipId}");
                continue;
            }
            
            EquipPart equipPart = equipReader.GetValue<EquipPart>(equipId, "Type", EquipPart.None);
            if (equipPart == EquipPart.None)
            {
                Debug.LogWarning($"[EquipManager] Invalid equipment part for item {equipId}");
                continue;
            }
            
            // 直接装备到Player
            bool equipSuccess = player.Equip(equipId);
            if (equipSuccess)
            {
                // 直接存储装备ID
                _equippedItems[equipPart] = equipId;
            }
            else
            {
                Debug.LogWarning($"[EquipManager] Failed to equip {equipId} to {equipPart}");
            }
        }
        
        // 存档加载完成后，发布装备刷新事件，通知UI更新所有装备槽位
        if (_equippedItems.Count > 0)
        {
            EventManager.Instance.Publish(new EquipRefreshEvent(_equippedItems.Count));
        }
    }
    
    /// <summary>
    /// 同步Player的装备状态（用于调试和状态检查）
    /// </summary>
    public void SyncPlayerEquipmentState()
    {
        var player = Player.Instance;
        if (player == null) return;
        
        Debug.Log($"[EquipManager] Syncing Player equipment state, current managed items: {_equippedItems.Count}");
        
        // 检查Player实际装备的组件是否与EquipManager管理的状态一致
        foreach (var kvp in _equippedItems)
        {
            var equipPart = kvp.Key;
            var managedEquipId = kvp.Value;
            
            var actualEquip = GetEquipComponentByPart(equipPart);
            if (actualEquip != null)
            {
                // 通过装备组件的EquipPart属性验证部位匹配
                if (actualEquip.EquipPart != equipPart)
                {
                    Debug.LogWarning($"[EquipManager] Equipment part mismatch for {equipPart}: managed={managedEquipId}, actual part={actualEquip.EquipPart}");
                }
            }
            else
            {
                Debug.LogWarning($"[EquipManager] Equipment component not found for {equipPart}, managed ID: {managedEquipId}");
            }
        }
    }

    /// <summary>
    /// 根据部位获取装备组件
    /// </summary>
    private EquipBase GetEquipComponentByPart(EquipPart equipPart)
    {
        var player = Player.Instance;
        if (player == null) return null;
        
        // 使用反射获取Player的装备列表
        var equipField = typeof(CombatEntity).GetField("_equips", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (equipField != null)
        {
            var equipsList = equipField.GetValue(player) as List<EquipBase>;
            if (equipsList != null)
            {
                foreach (var equip in equipsList)
                {
                    if (equip != null && equip.EquipPart == equipPart)
                    {
                        return equip;
                    }
                }
            }
        }
        
            return null;
    }
    

    
    /// <summary>
    /// 从Player移除装备组件
    /// </summary>
    private void RemoveEquipComponent(Player player, EquipBase equip)
    {
        if (equip == null) return;
        
        // 调用装备的卸下方法
        equip.OnUnequip();
        
        // 使用反射从Player的装备列表中移除
        var equipField = typeof(CombatEntity).GetField("_equips", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (equipField != null)
        {
            var equipsList = equipField.GetValue(player) as List<EquipBase>;
            if (equipsList != null)
            {
                equipsList.Remove(equip);
            }
        }
        
        // 销毁装备组件
        Object.Destroy(equip);
    }
} 