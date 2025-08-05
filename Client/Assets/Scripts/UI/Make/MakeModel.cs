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
} 