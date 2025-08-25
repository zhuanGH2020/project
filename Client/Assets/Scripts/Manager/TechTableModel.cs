using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 科技台数据模型 - 管理科技台升级状态和材料收集
/// 单例模式，负责所有科技台的升级逻辑
/// 支持多级升级系统（1-3级）
/// </summary>
public class TechTableModel
{
    // 单例实现
    private static TechTableModel _instance;
    public static TechTableModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TechTableModel();
            }
            return _instance;
        }
    }

    // 常量定义
    private const int MAX_LEVEL = 3;
    private const int MIN_LEVEL = 1;

    /// <summary>
    /// 私有构造函数，防止外部实例化
    /// </summary>
    private TechTableModel()
    {
        _collectedMaterials = new Dictionary<int, int>();
        _currentLevel = MIN_LEVEL;
        LoadRequiredMaterialsForNextLevel();
    }

    // 私有字段
    private Dictionary<int, int> _collectedMaterials;  // 已收集的材料：物品ID -> 数量
    private Dictionary<int, int> _requiredMaterials;   // 当前升级所需材料：物品ID -> 数量
    private int _currentLevel = MIN_LEVEL;             // 当前科技台等级（1-3）
    
    // 公共属性
    public Dictionary<int, int> CollectedMaterials => new Dictionary<int, int>(_collectedMaterials);
    public Dictionary<int, int> RequiredMaterials => new Dictionary<int, int>(_requiredMaterials);
    public int CurrentLevel => _currentLevel;
    public bool IsMaxLevel => _currentLevel >= MAX_LEVEL;
    public bool CanUpgrade => !IsMaxLevel;

    /// <summary>
    /// 科技台是否可以发光（3级及以上）
    /// </summary>
    public bool CanEmitLight => _currentLevel >= 3;

    /// <summary>
    /// 是否已经升级完成（达到最高等级）
    /// </summary>
    public bool IsUpgraded => IsMaxLevel;

    /// <summary>
    /// 加载下一级升级所需的材料配置
    /// </summary>
    private void LoadRequiredMaterialsForNextLevel()
    {
        _requiredMaterials = new Dictionary<int, int>();

        if (_currentLevel >= MAX_LEVEL)
        {
            return; // 已达到最高等级，无需材料
        }

        int targetLevel = _currentLevel + 1;
        Dictionary<int, int> materials = null;

        switch (targetLevel)
        {
            case 2:
                materials = GameSettings.TechTableLevel2UpgradeMaterials;
                break;
            case 3:
                materials = GameSettings.TechTableLevel3UpgradeMaterials;
                break;
            default:
                Debug.LogError($"[TechTableModel] 未找到等级 {targetLevel} 的升级材料配置");
                return;
        }

        if (materials != null)
        {
            foreach (var kvp in materials)
            {
                _requiredMaterials[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// 尝试添加选中的材料到科技台
    /// </summary>
    /// <param name="selectedItem">选中的物品</param>
    /// <returns>返回消耗的数量，0表示失败</returns>
    public int TryAddSelectedMaterial(PackageItem selectedItem)
    {
        if (selectedItem == null)
        {
            EventManager.Instance.Publish(new NoticeEvent("没有选中的物品"));
            return 0;
        }

        // 检查是否已经达到最高等级
        if (IsMaxLevel)
        {
            EventManager.Instance.Publish(new NoticeEvent($"科技台已达到最高等级({MAX_LEVEL}级)"));
            return 0;
        }

        int itemId = selectedItem.itemId;
        int count = selectedItem.count;

        // 检查该材料是否是所需材料
        if (!_requiredMaterials.ContainsKey(itemId))
        {
            // 获取物品名称用于提示
            string itemName = ResourceUtils.GetItemName(itemId);
            int nextLevel = _currentLevel + 1;
            EventManager.Instance.Publish(new NoticeEvent($"升级到{nextLevel}级不需要{itemName}"));
            return 0;
        }

        // 计算当前已收集的数量
        int currentCollected = _collectedMaterials.ContainsKey(itemId) ? _collectedMaterials[itemId] : 0;
        int required = _requiredMaterials[itemId];
        
        // 检查是否已经收集满
        if (currentCollected >= required)
        {
            string itemName = ResourceUtils.GetItemName(itemId);
            EventManager.Instance.Publish(new NoticeEvent($"{itemName}已经收集满了"));
            return 0;
        }

        // 计算实际可以添加的数量（不超过所需数量）
        int actualAddCount = Mathf.Min(count, required - currentCollected);
        
        // 添加到已收集材料中
        if (_collectedMaterials.ContainsKey(itemId))
        {
            _collectedMaterials[itemId] += actualAddCount;
        }
        else
        {
            _collectedMaterials[itemId] = actualAddCount;
        }

        // 获取物品名称
        string materialName = ResourceUtils.GetItemName(itemId);
        
        // 发布材料添加事件
        EventManager.Instance.Publish(new TechTableMaterialAddedEvent(itemId, actualAddCount, materialName));
        
        // 显示剩余材料提示
        ShowRemainingMaterials();
        
        // 检查是否所有材料都收集完成
        CheckUpgradeComplete();
        
        return actualAddCount;
    }

    /// <summary>
    /// 尝试添加材料到科技台（从背包中移除）
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <param name="count">数量</param>
    /// <returns>是否成功添加</returns>
    public bool TryAddMaterial(int itemId, int count)
    {
        // 检查是否已经达到最高等级
        if (IsMaxLevel)
        {
            EventManager.Instance.Publish(new NoticeEvent($"科技台已达到最高等级({MAX_LEVEL}级)"));
            return false;
        }

        // 检查该材料是否是所需材料
        if (!_requiredMaterials.ContainsKey(itemId))
        {
            // 获取物品名称用于提示
            string itemName = ResourceUtils.GetItemName(itemId);
            int nextLevel = _currentLevel + 1;
            EventManager.Instance.Publish(new NoticeEvent($"升级到{nextLevel}级不需要{itemName}"));
            return false;
        }

        // 计算当前已收集的数量
        int currentCollected = _collectedMaterials.ContainsKey(itemId) ? _collectedMaterials[itemId] : 0;
        int required = _requiredMaterials[itemId];
        
        // 检查是否已经收集满
        if (currentCollected >= required)
        {
            string itemName = ResourceUtils.GetItemName(itemId);
            EventManager.Instance.Publish(new NoticeEvent($"{itemName}已经收集满了"));
            return false;
        }

        // 计算实际可以添加的数量（不超过所需数量）
        int actualAddCount = Mathf.Min(count, required - currentCollected);
        
        // 从背包中移除对应数量的物品
        bool removeSuccess = PackageModel.Instance.RemoveItem(itemId, actualAddCount);
        if (!removeSuccess)
        {
            EventManager.Instance.Publish(new NoticeEvent("背包中材料不足"));
            return false;
        }

        // 添加到已收集材料中
        if (_collectedMaterials.ContainsKey(itemId))
        {
            _collectedMaterials[itemId] += actualAddCount;
        }
        else
        {
            _collectedMaterials[itemId] = actualAddCount;
        }

        // 获取物品名称
        string materialName = ResourceUtils.GetItemName(itemId);
        
        // 发布材料添加事件
        EventManager.Instance.Publish(new TechTableMaterialAddedEvent(itemId, actualAddCount, materialName));
        
        // 显示剩余材料提示
        ShowRemainingMaterials();
        
        // 检查是否所有材料都收集完成
        CheckUpgradeComplete();
        
        return true;
    }

    /// <summary>
    /// 显示剩余升级材料信息
    /// </summary>
    public void ShowRemainingMaterials()
    {
        if (IsMaxLevel)
        {
            EventManager.Instance.Publish(new NoticeEvent($"科技台已达到最高等级({MAX_LEVEL}级)"));
            return;
        }

        int nextLevel = _currentLevel + 1;
        List<string> remainingItems = new List<string>();
        
        foreach (var kvp in _requiredMaterials)
        {
            int itemId = kvp.Key;
            int required = kvp.Value;
            int collected = _collectedMaterials.ContainsKey(itemId) ? _collectedMaterials[itemId] : 0;
            int remaining = required - collected;
            
            if (remaining > 0)
            {
                string itemName = ResourceUtils.GetItemName(itemId);
                remainingItems.Add($"{itemName}x{remaining}");
            }
        }
        
        if (remainingItems.Count > 0)
        {
            string message = $"升级到{nextLevel}级需要材料：" + string.Join("，", remainingItems);
            EventManager.Instance.Publish(new NoticeEvent(message));
        }
        else
        {
            EventManager.Instance.Publish(new NoticeEvent($"材料已收集完成，可以升级到{nextLevel}级了"));
        }
    }

    /// <summary>
    /// 检查升级是否完成
    /// </summary>
    private void CheckUpgradeComplete()
    {
        foreach (var kvp in _requiredMaterials)
        {
            int itemId = kvp.Key;
            int required = kvp.Value;
            int collected = _collectedMaterials.ContainsKey(itemId) ? _collectedMaterials[itemId] : 0;
            
            if (collected < required)
            {
                return; // 还有材料未收集完成
            }
        }
        
        // 所有材料都收集完成，执行升级
        CompleteUpgrade();
    }

    /// <summary>
    /// 完成升级
    /// </summary>
    private void CompleteUpgrade()
    {
        int oldLevel = _currentLevel;
        _currentLevel++;
        
        // 清空已收集的材料
        _collectedMaterials.Clear();
        
        // 加载下一级的升级材料配置
        LoadRequiredMaterialsForNextLevel();
        
        // 发布升级完成事件
        EventManager.Instance.Publish(new TechTableLevelUpEvent(oldLevel, _currentLevel));
        
        if (IsMaxLevel)
        {
            EventManager.Instance.Publish(new NoticeEvent($"科技台升级到{_currentLevel}级，已达到最高等级！"));
        }
        else
        {
            EventManager.Instance.Publish(new NoticeEvent($"科技台升级到{_currentLevel}级成功！"));
        }
    }

    /// <summary>
    /// 重置科技台状态（用于测试或重新开始）
    /// </summary>
    public void ResetUpgradeStatus()
    {
        _collectedMaterials.Clear();
        _currentLevel = MIN_LEVEL;
        LoadRequiredMaterialsForNextLevel();
    }

    /// <summary>
    /// 设置科技台等级（用于存档加载）
    /// </summary>
    /// <param name="level">目标等级</param>
    public void SetLevel(int level)
    {
        int clampedLevel = Mathf.Clamp(level, MIN_LEVEL, MAX_LEVEL);
        if (clampedLevel != level)
        {
            Debug.LogError($"[TechTableModel] 等级 {level} 超出有效范围，已限制为 {clampedLevel}");
        }
        
        _currentLevel = clampedLevel;
        _collectedMaterials.Clear();
        LoadRequiredMaterialsForNextLevel();
    }

    /// <summary>
    /// 获取材料收集进度文本
    /// </summary>
    /// <returns>进度文本</returns>
    public string GetProgressText()
    {
        if (IsMaxLevel)
        {
            return $"等级：{_currentLevel}级（最高等级）";
        }

        int nextLevel = _currentLevel + 1;
        List<string> progressItems = new List<string>();
        
        foreach (var kvp in _requiredMaterials)
        {
            int itemId = kvp.Key;
            int required = kvp.Value;
            int collected = _collectedMaterials.ContainsKey(itemId) ? _collectedMaterials[itemId] : 0;
            
            string itemName = ResourceUtils.GetItemName(itemId);
            progressItems.Add($"{itemName}:{collected}/{required}");
        }
        
        string materialsProgress = progressItems.Count > 0 ? string.Join(" ", progressItems) : "已准备就绪";
        return $"等级：{_currentLevel}级 升级到{nextLevel}级 {materialsProgress}";
    }

    /// <summary>
    /// 获取当前等级的简短描述
    /// </summary>
    /// <returns>等级描述</returns>
    public string GetLevelDescription()
    {
        switch (_currentLevel)
        {
            case 1:
                return "基础科技台";
            case 2:
                return "进阶科技台";
            case 3:
                return "高级科技台（可发光）";
            default:
                return $"{_currentLevel}级科技台";
        }
    }
}