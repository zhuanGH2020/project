using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 烹饪槽位数据
/// </summary>
[System.Serializable]
public class CookingSlotData
{
    public int itemId;
    public int count;
    
    public CookingSlotData()
    {
        itemId = 0;
        count = 0;
    }
    
    public bool IsEmpty => itemId <= 0 || count <= 0;
    
    public void Clear()
    {
        itemId = 0;
        count = 0;
    }
    
    public void SetItem(int id, int amount)
    {
        itemId = id;
        count = amount;
    }
}

/// <summary>
/// 食谱数据
/// </summary>
[System.Serializable]
public class Recipe
{
    public int id;
    public string name;
    public List<int> materials;
    public int resultItemId;
    public int resultCount;
    
    public Recipe()
    {
        materials = new List<int>();
    }
}

/// <summary>
/// 烹饪数据模型 - 管理烹饪界面状态和逻辑
/// </summary>
public class CookingModel
{
    // 单例实现
    private static CookingModel _instance;
    public static CookingModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CookingModel();
            }
            return _instance;
        }
    }

    // 常量定义
    public const int COOKING_SLOTS = 4;  // 烹饪槽位数量

    // 私有字段
    private List<CookingSlotData> _cookingSlots = new List<CookingSlotData>();
    private List<Recipe> _recipes = new List<Recipe>();
    private bool _isUIOpen = false;

    // 公共属性
    public List<CookingSlotData> CookingSlots => _cookingSlots;
    public bool IsUIOpen => _isUIOpen;

    /// <summary>
    /// 私有构造函数
    /// </summary>
    private CookingModel()
    {
        InitializeCookingSlots();
        LoadRecipes();
    }

    /// <summary>
    /// 初始化烹饪槽位
    /// </summary>
    private void InitializeCookingSlots()
    {
        _cookingSlots.Clear();
        for (int i = 0; i < COOKING_SLOTS; i++)
        {
            _cookingSlots.Add(new CookingSlotData());
        }
    }

    /// <summary>
    /// 从配置表加载食谱数据
    /// </summary>
    private void LoadRecipes()
    {
        _recipes.Clear();
        
        var reader = ConfigManager.Instance.GetReader("Recipe");
        if (reader == null)
        {
            Debug.LogWarning("[CookingModel] Recipe配置表未找到");
            return;
        }

        foreach (var id in reader.GetAllKeysOfType<int>())
        {
            Recipe recipe = new Recipe();
            recipe.id = id;
            recipe.name = reader.GetValue<string>(id, "Name", "");
            recipe.resultItemId = reader.GetValue<int>(id, "ResultItemId", 0);
            recipe.resultCount = reader.GetValue<int>(id, "ResultCount", 1);

            // 解析材料
            for (int i = 1; i <= 4; i++)
            {
                string materialField = $"Material{i}";
                string materialValue = reader.GetValue<string>(id, materialField, "");
                
                if (!string.IsNullOrEmpty(materialValue) && int.TryParse(materialValue, out int materialId))
                {
                    recipe.materials.Add(materialId);
                }
            }

            _recipes.Add(recipe);
        }

        Debug.Log($"[CookingModel] 加载了 {_recipes.Count} 个食谱");
    }

    /// <summary>
    /// 打开烹饪界面
    /// </summary>
    public void OpenCookingUI()
    {
        _isUIOpen = true;
        EventManager.Instance.Publish(new CookingUIOpenEvent());
        Debug.Log("[CookingModel] 打开烹饪界面");
    }

    /// <summary>
    /// 关闭烹饪界面
    /// </summary>
    public void CloseCookingUI()
    {
        if (_isUIOpen)
        {
            _isUIOpen = false;
            
            // 将槽位中的物品退还到背包
            ReturnItemsToPackage();
            
            EventManager.Instance.Publish(new CookingUICloseEvent());
            Debug.Log("[CookingModel] 关闭烹饪界面");
        }
    }

    /// <summary>
    /// 将物品放置到烹饪槽位
    /// </summary>
    public bool PlaceItemToSlot(int slotIndex, int itemId, int count)
    {
        if (slotIndex < 0 || slotIndex >= COOKING_SLOTS)
        {
            Debug.LogWarning($"[CookingModel] 无效的槽位索引: {slotIndex}");
            return false;
        }

        if (itemId <= 0 || count <= 0)
        {
            Debug.LogWarning($"[CookingModel] 无效的物品数据: itemId={itemId}, count={count}");
            return false;
        }

        // 检查槽位是否为空
        if (!_cookingSlots[slotIndex].IsEmpty)
        {
            Debug.LogWarning($"[CookingModel] 槽位 {slotIndex} 已被占用");
            return false;
        }

        // 放置物品
        _cookingSlots[slotIndex].SetItem(itemId, count);
        
        // 发布槽位更新事件
        EventManager.Instance.Publish(new CookingSlotUpdateEvent(slotIndex, itemId, count));
        
        Debug.Log($"[CookingModel] 将物品 {itemId}({count}) 放入槽位 {slotIndex}");
        return true;
    }

    /// <summary>
    /// 从烹饪槽位移除物品
    /// </summary>
    public bool RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= COOKING_SLOTS)
        {
            Debug.LogWarning($"[CookingModel] 无效的槽位索引: {slotIndex}");
            return false;
        }

        var slot = _cookingSlots[slotIndex];
        if (slot.IsEmpty)
        {
            Debug.LogWarning($"[CookingModel] 槽位 {slotIndex} 为空");
            return false;
        }

        // 将物品退还到背包
        PackageModel.Instance.AddItem(slot.itemId, slot.count);
        
        // 清空槽位
        slot.Clear();
        
        // 发布槽位更新事件
        EventManager.Instance.Publish(new CookingSlotUpdateEvent(slotIndex, 0, 0));
        
        Debug.Log($"[CookingModel] 从槽位 {slotIndex} 移除物品");
        return true;
    }

    /// <summary>
    /// 检查是否所有槽位都已填满
    /// </summary>
    public bool AllSlotsFilled()
    {
        foreach (var slot in _cookingSlots)
        {
            if (slot.IsEmpty)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 匹配食谱并烹饪
    /// </summary>
    public bool Cook()
    {
        if (!AllSlotsFilled())
        {
            EventManager.Instance.Publish(new NoticeEvent("请先填满所有材料槽位"));
            return false;
        }

        // 获取当前槽位中的材料ID列表
        List<int> currentMaterials = new List<int>();
        foreach (var slot in _cookingSlots)
        {
            currentMaterials.Add(slot.itemId);
        }

        // 匹配食谱
        Recipe matchedRecipe = FindMatchingRecipe(currentMaterials);
        if (matchedRecipe == null)
        {
            EventManager.Instance.Publish(new NoticeEvent("没有找到匹配的食谱"));
            return false;
        }

        // 执行烹饪
        return ExecuteCooking(matchedRecipe);
    }

    /// <summary>
    /// 查找匹配的食谱
    /// </summary>
    private Recipe FindMatchingRecipe(List<int> materials)
    {
        foreach (var recipe in _recipes)
        {
            if (MaterialsMatch(recipe.materials, materials))
            {
                return recipe;
            }
        }
        return null;
    }

    /// <summary>
    /// 检查材料是否匹配（不考虑顺序）
    /// </summary>
    private bool MaterialsMatch(List<int> recipeMaterials, List<int> currentMaterials)
    {
        if (recipeMaterials.Count != currentMaterials.Count)
            return false;

        // 创建材料计数字典
        var recipeCount = new Dictionary<int, int>();
        var currentCount = new Dictionary<int, int>();

        // 统计食谱材料
        foreach (int material in recipeMaterials)
        {
            recipeCount[material] = recipeCount.ContainsKey(material) ? recipeCount[material] + 1 : 1;
        }

        // 统计当前材料
        foreach (int material in currentMaterials)
        {
            currentCount[material] = currentCount.ContainsKey(material) ? currentCount[material] + 1 : 1;
        }

        // 比较两个字典
        if (recipeCount.Count != currentCount.Count)
            return false;

        foreach (var kvp in recipeCount)
        {
            if (!currentCount.ContainsKey(kvp.Key) || currentCount[kvp.Key] != kvp.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 执行烹饪
    /// </summary>
    private bool ExecuteCooking(Recipe recipe)
    {
        // 清空所有槽位（消耗材料）
        foreach (var slot in _cookingSlots)
        {
            slot.Clear();
        }

        // 产出食物到背包
        bool success = PackageModel.Instance.AddItem(recipe.resultItemId, recipe.resultCount);
        
        if (success)
        {
            // 发布烹饪成功事件
            EventManager.Instance.Publish(new CookingSuccessEvent(recipe.resultItemId, recipe.resultCount));
            EventManager.Instance.Publish(new NoticeEvent($"烹饪成功：{recipe.name}"));
            
            // 更新所有槽位UI
            for (int i = 0; i < COOKING_SLOTS; i++)
            {
                EventManager.Instance.Publish(new CookingSlotUpdateEvent(i, 0, 0));
            }
            
            Debug.Log($"[CookingModel] 烹饪成功：{recipe.name}");
        }
        else
        {
            EventManager.Instance.Publish(new NoticeEvent("背包已满，烹饪失败"));
        }

        return success;
    }

    /// <summary>
    /// 将槽位中的物品退还到背包
    /// </summary>
    private void ReturnItemsToPackage()
    {
        foreach (var slot in _cookingSlots)
        {
            if (!slot.IsEmpty)
            {
                PackageModel.Instance.AddItem(slot.itemId, slot.count);
                slot.Clear();
            }
        }
    }
}