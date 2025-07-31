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

    // 公共属性
    public int SelectedTypeId => _selectedTypeId;
    public List<MakeTypeData> MakeTypes => new List<MakeTypeData>(_makeTypes);

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
} 