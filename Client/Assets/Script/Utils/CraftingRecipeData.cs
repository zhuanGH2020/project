using System.Collections.Generic;
using UnityEngine;
using static EventSystemEnums;

/// <summary>
/// 制作配方数据类
/// 支持项目制作系统功能
/// 遵循项目命名约定和代码风格规范
/// </summary>
[System.Serializable]
public class CraftingRecipeData
{
    #region 序列化字段
    
    [Header("配方基础信息")]
    [SerializeField] private int recipeId;
    [SerializeField] private string recipeName;
    [SerializeField, TextArea(2, 4)] private string recipeDescription;
    [SerializeField] private Sprite recipeIcon;
    
    [Header("制作结果")]
    [SerializeField] private ItemData resultItem;
    [SerializeField, Range(1, 99)] private int resultQuantity = 1;
    
    [Header("所需材料")]
    [SerializeField] private List<CraftingMaterialData> requiredMaterials;
    
    [Header("制作条件")]
    [SerializeField] private bool requiresCraftingTable = false;
    [SerializeField, Range(0.1f, 300f)] private float craftingTime = 1f;
    [SerializeField, Range(1, 100)] private int requiredLevel = 1;
    [SerializeField] private bool isUnlocked = false;
    
    #endregion
    
    #region 公共属性
    
    /// <summary>
    /// 配方唯一标识符
    /// </summary>
    public int RecipeId => recipeId;
    
    /// <summary>
    /// 配方名称
    /// </summary>
    public string RecipeName => recipeName;
    
    /// <summary>
    /// 配方描述
    /// </summary>
    public string RecipeDescription => recipeDescription;
    
    /// <summary>
    /// 配方图标
    /// </summary>
    public Sprite RecipeIcon => recipeIcon;
    
    /// <summary>
    /// 制作结果物品
    /// </summary>
    public ItemData ResultItem => resultItem;
    
    /// <summary>
    /// 制作结果数量
    /// </summary>
    public int ResultQuantity => resultQuantity;
    
    /// <summary>
    /// 所需材料列表
    /// </summary>
    public List<CraftingMaterialData> RequiredMaterials => requiredMaterials;
    
    /// <summary>
    /// 是否需要制作台
    /// </summary>
    public bool RequiresCraftingTable => requiresCraftingTable;
    
    /// <summary>
    /// 制作耗时
    /// </summary>
    public float CraftingTime => craftingTime;
    
    /// <summary>
    /// 所需等级
    /// </summary>
    public int RequiredLevel => requiredLevel;
    
    /// <summary>
    /// 是否已解锁
    /// </summary>
    public bool IsUnlocked => isUnlocked;
    
    #endregion
    
    #region 构造函数
    
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public CraftingRecipeData()
    {
        requiredMaterials = new List<CraftingMaterialData>();
    }
    
    /// <summary>
    /// 带参数的构造函数
    /// </summary>
    /// <param name="id">配方ID</param>
    /// <param name="name">配方名称</param>
    /// <param name="result">制作结果</param>
    public CraftingRecipeData(int id, string name, ItemData result)
    {
        recipeId = id;
        recipeName = name;
        resultItem = result;
        requiredMaterials = new List<CraftingMaterialData>();
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 解锁配方
    /// </summary>
    public void UnlockRecipe()
    {
        isUnlocked = true;
    }
    
    /// <summary>
    /// 检查是否满足制作条件
    /// </summary>
    /// <param name="playerLevel">玩家等级</param>
    /// <param name="hasCraftingTable">是否有制作台</param>
    /// <returns>是否满足条件</returns>
    public bool CanCraft(int playerLevel, bool hasCraftingTable)
    {
        return isUnlocked && 
               playerLevel >= requiredLevel && 
               (!requiresCraftingTable || hasCraftingTable);
    }
    
    /// <summary>
    /// 检查材料是否充足
    /// </summary>
    /// <param name="availableMaterials">可用材料列表</param>
    /// <returns>材料是否充足和缺少的材料列表</returns>
    public (bool hasEnoughMaterials, List<CraftingMaterialData> missingMaterials) CheckMaterials(
        Dictionary<int, int> availableMaterials)
    {
        List<CraftingMaterialData> missingMaterials = new List<CraftingMaterialData>();
        
        foreach (var requiredMaterial in requiredMaterials)
        {
            int availableQuantity = availableMaterials.ContainsKey(requiredMaterial.MaterialItem.ItemId) 
                ? availableMaterials[requiredMaterial.MaterialItem.ItemId] : 0;
                
            if (availableQuantity < requiredMaterial.RequiredQuantity)
            {
                int missingQuantity = requiredMaterial.RequiredQuantity - availableQuantity;
                missingMaterials.Add(new CraftingMaterialData(requiredMaterial.MaterialItem, missingQuantity));
            }
        }
        
        return (missingMaterials.Count == 0, missingMaterials);
    }
    
    #endregion
}

/// <summary>
/// 制作材料数据类
/// 用于描述制作配方所需的材料和数量
/// </summary>
[System.Serializable]
public class CraftingMaterialData
{
    [SerializeField] private ItemData materialItem;
    [SerializeField, Range(1, 999)] private int requiredQuantity = 1;
    
    /// <summary>
    /// 材料物品
    /// </summary>
    public ItemData MaterialItem => materialItem;
    
    /// <summary>
    /// 所需数量
    /// </summary>
    public int RequiredQuantity => requiredQuantity;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="item">材料物品</param>
    /// <param name="quantity">所需数量</param>
    public CraftingMaterialData(ItemData item, int quantity)
    {
        materialItem = item;
        requiredQuantity = quantity;
    }
} 