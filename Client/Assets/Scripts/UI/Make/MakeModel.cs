using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 制作数据项
/// </summary>
[System.Serializable]
public class MakeTypeData
{
    public int typeId;
    public string typeName;
    public string imagePath;
    
    public MakeTypeData(int typeId, string typeName, string imagePath = "")
    {
        this.typeId = typeId;
        this.typeName = typeName;
        this.imagePath = imagePath;
    }
}

/// <summary>
/// 制作数据模型 - 管理制作类型和状态
/// </summary>
public class MakeModel
{
    // 单例实现
    private static MakeModel _instance;
    public static MakeModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MakeModel();
            }
            return _instance;
        }
    }

    // 私有字段
    private List<MakeTypeData> _makeTypes = new List<MakeTypeData>();
    private int _selectedTypeId = -1;
    private List<int> _installedBuildings = new List<int>();

    // 公共属性
    public int SelectedTypeId => _selectedTypeId;
    public List<MakeTypeData> MakeTypes => new List<MakeTypeData>(_makeTypes);
    public List<int> InstalledBuildings => new List<int>(_installedBuildings);

    /// <summary>
    /// 私有构造函数，防止外部实例化
    /// </summary>
    private MakeModel()
    {
        LoadMakeTypes();
    }

    /// <summary>
    /// 从配置加载制作类型数据
    /// </summary>
    private void LoadMakeTypes()
    {
        _makeTypes.Clear();
        
        var reader = ConfigManager.Instance.GetReader("Make");
        if (reader == null) return;

        foreach (var id in reader.GetAllKeysOfType<int>())
        {
            string name = reader.GetValue<string>(id, "Name", "");
            string imagePath = reader.GetValue<string>(id, "Image", "");
            
            _makeTypes.Add(new MakeTypeData(id, name, imagePath));
        }
    }

    /// <summary>
    /// 选择制作类型
    /// </summary>
    public void SelectMakeType(int typeId)
    {
        var makeType = _makeTypes.Find(t => t.typeId == typeId);
        if (makeType == null) return;

        int previousTypeId = _selectedTypeId;
        _selectedTypeId = typeId;

        // 发布制作类型选择事件
        EventManager.Instance.Publish(new MakeTypeSelectedEvent(typeId, makeType.typeName));
        
        // 发布制作菜单打开事件
        EventManager.Instance.Publish(new MakeMenuOpenEvent(typeId));
    }

    /// <summary>
    /// 关闭制作菜单
    /// </summary>
    public void CloseMakeMenu()
    {
        _selectedTypeId = -1;
        EventManager.Instance.Publish(new MakeMenuCloseEvent());
    }

    /// <summary>
    /// 获取制作类型数据
    /// </summary>
    public MakeTypeData GetMakeTypeData(int typeId)
    {
        return _makeTypes.Find(t => t.typeId == typeId);
    }

    /// <summary>
    /// 清除选择
    /// </summary>
    public void ClearSelection()
    {
        _selectedTypeId = -1;
    }
    
    // 建筑管理接口
    
    /// <summary>
    /// 检查建筑是否未安装
    /// </summary>
    public bool IsBuildingInstalled(int itemId)
    {
        return _installedBuildings.Contains(itemId);
    }
    
    /// <summary>
    /// 添加未安装的建筑
    /// </summary>
    public bool AddBuilding(int itemId)
    {
        if (_installedBuildings.Contains(itemId))
        {
            Debug.LogWarning($"[MakeModel] 建筑 {itemId} 已经存在于已安装列表中");
            return false;
        }
        
        _installedBuildings.Add(itemId);
        Debug.Log($"[MakeModel] 添加已安装建筑: {itemId}");
        return true;
    }
    
    /// <summary>
    /// 移除已安装的建筑
    /// </summary>
    public bool RemoveBuilding(int itemId)
    {
        if (!_installedBuildings.Contains(itemId))
        {
            Debug.LogWarning($"[MakeModel] 建筑 {itemId} 不在已安装列表中");
            return false;
        }
        
        _installedBuildings.Remove(itemId);
        Debug.Log($"[MakeModel] 移除已安装建筑: {itemId}");
        return true;
    }

    /// <summary>
    /// 制作物品 - 统一的制作逻辑处理
    /// 检查材料、消耗材料、创建物品、关闭界面
    /// </summary>
    public bool MakeItem(int itemId)
    {
        if (itemId <= 0)
        {
            Debug.LogWarning("[MakeModel] 无效的物品ID");
            return false;
        }

        // 检查建筑是否已安装（已制作但未放置），如果是则直接进入放置模式
        if (IsBuildingInstalled(itemId))
        {
            // 建筑已制作但未放置，直接进入建筑放置状态
            EventManager.Instance.Publish(new BuildingPendingPlaceEvent(itemId));
            // 关闭制作菜单界面
            EventManager.Instance.Publish(new MakeMenuCloseEvent());
            Debug.Log($"[MakeModel] 进入建筑放置模式: (ID: {itemId})");
            return true;
        }

        var reader = ConfigManager.Instance.GetReader("MakeMenu");
        if (reader == null)
        {
            Debug.LogError("[MakeModel] MakeMenu配置读取器未找到");
            return false;
        }

        if (!reader.HasKey(itemId))
        {
            Debug.LogError($"[MakeModel] 物品ID {itemId} 在MakeMenu配置中不存在");
            return false;
        }

        // 获取材料需求
        List<MaterialRequirement> materialRequirements = GetMaterialRequirements(reader, itemId);
        
        // 检查材料是否充足
        if (!CheckMaterialsAvailable(materialRequirements))
        {
            EventManager.Instance.Publish(new NoticeEvent("材料不足"));
            return false;
        }

        // 消耗材料
        foreach (var material in materialRequirements)
        {
            PackageModel.Instance.RemoveItem(material.ItemId, material.RequiredQuantity);
        }

        // 获取物品配置信息
        var itemConfig = ItemManager.Instance.GetItem(itemId);
        string itemName = itemConfig?.Csv.GetValue<string>(itemId, "Name", "物品") ?? "物品";

        // 根据物品类型决定处理方式
        if (itemConfig != null && itemConfig.IsBuilding())
        {
            // 建筑物：添加到待放置建筑列表
            AddBuilding(itemId);
            
            // 发布建筑物待放置事件
            EventManager.Instance.Publish(new BuildingPendingPlaceEvent(itemId));
            Debug.Log($"[MakeModel] 建筑物制作完成，等待放置: {itemName} (ID: {itemId})");
        }
        else
        {
            // 非建筑物：正常添加到背包
            PackageModel.Instance.AddItem(itemId, 1);
            Debug.Log($"[MakeModel] 物品添加到背包: {itemName} (ID: {itemId})");
        }

        // 发送制作成功通知
        EventManager.Instance.Publish(new NoticeEvent($"制作成功：{itemName}"));

        // 检查是否需要关闭界面
        bool shouldClose = reader.GetValue<bool>(itemId, "isClose", true);
        if (shouldClose)
        {
            // 关闭制作详情界面和制作菜单界面（制作成功后立即关闭，无延迟）
            EventManager.Instance.Publish(new MakeDetailCloseEvent(false));
            EventManager.Instance.Publish(new MakeMenuCloseEvent());
        }

        Debug.Log($"[MakeModel] 制作完成: {itemName} (ID: {itemId})");
        return true;
    }

    /// <summary>
    /// 获取物品的材料需求
    /// </summary>
    private List<MaterialRequirement> GetMaterialRequirements(ConfigReader reader, int itemId)
    {
        List<MaterialRequirement> requirements = new List<MaterialRequirement>();
        
        // 获取材料数据
        string material1 = reader.GetValue<string>(itemId, "Material1", "");
        string material2 = reader.GetValue<string>(itemId, "Material2", "");
        string material3 = reader.GetValue<string>(itemId, "Material3", "");
        
        // 解析材料需求
        ParseMaterialRequirement(material1, requirements);
        ParseMaterialRequirement(material2, requirements);
        ParseMaterialRequirement(material3, requirements);
        
        return requirements;
    }

    /// <summary>
    /// 解析单个材料需求
    /// 材料格式: "物品ID;数量" 例如: "1000;3"
    /// </summary>
    private void ParseMaterialRequirement(string materialData, List<MaterialRequirement> requirements)
    {
        if (string.IsNullOrEmpty(materialData)) return;
        
        string[] parts = materialData.Split(';');
        if (parts.Length != 2) return;
        
        if (int.TryParse(parts[0], out int materialId) && int.TryParse(parts[1], out int quantity))
        {
            requirements.Add(new MaterialRequirement
            {
                ItemId = materialId,
                RequiredQuantity = quantity
            });
        }
    }

    /// <summary>
    /// 检查所有材料是否足够
    /// </summary>
    private bool CheckMaterialsAvailable(List<MaterialRequirement> materialRequirements)
    {
        foreach (var material in materialRequirements)
        {
            if (!PackageModel.Instance.HasEnoughItem(material.ItemId, material.RequiredQuantity))
            {
                return false;
            }
        }
        return true;
    }
} 