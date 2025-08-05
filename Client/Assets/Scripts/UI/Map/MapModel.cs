using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图数据
/// </summary>
[System.Serializable]
public class MapData
{
    public int itemId;
    public float posX;
    public float posY;
    public float modifyTime;
    public int buildingUID = 0;  // 建筑物唯一标识（时间戳，只有建筑物才有）

    public MapData(int itemId, float posX, float posY, int buildingUID = 0)
    {
        this.itemId = itemId;
        this.posX = posX;
        this.posY = posY;
        this.buildingUID = buildingUID;
        this.modifyTime = Time.time;
    }
}

/// <summary>
/// 地图数据模型 - 管理地图上物体的位置信息
/// </summary>
public class MapModel
{
    // 单例实现
    private static MapModel _instance;
    public static MapModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MapModel();
            }
            return _instance;
        }
    }

    // 私有字段
    private List<MapData> _mapDataList = new List<MapData>();
    private MapData _selectedMapData; // 当前选中的地图数据

    // 公共属性
    public List<MapData> MapDataList => _mapDataList;
    public MapData SelectedMapData => _selectedMapData;

    // 私有构造函数
    private MapModel()
    {
        // 初始化数据
    }

    // 公共方法
    /// <summary>
    /// 添加地图数据
    /// </summary>
    public bool AddMapData(int itemId, float posX, float posY, int buildingUID = 0)
    {
        // 检查是否已存在相同位置的数据
        MapData existingData = _mapDataList.Find(data => 
            Mathf.Approximately(data.posX, posX) && Mathf.Approximately(data.posY, posY));
        
        if (existingData != null)
        {
            Debug.LogWarning($"[MapModel] 位置 ({posX}, {posY}) 已存在地图数据");
            return false;
        }

        MapData newMapData = new MapData(itemId, posX, posY, buildingUID);
        _mapDataList.Add(newMapData);

        // 加载并实例化建筑物预制体
        int finalUID = LoadBuildingPrefab(itemId, posX, posY, buildingUID);
        if (finalUID > 0)
        {
            newMapData.buildingUID = finalUID; // 更新实际生成的UID
        }

        // 发布地图数据添加事件
        EventManager.Instance.Publish(new MapDataAddedEvent(newMapData));
        Debug.Log($"[MapModel] 添加地图数据: ItemId={itemId}, Pos=({posX}, {posY}), UID={buildingUID}");
        
        return true;
    }

    /// <summary>
    /// 删除地图数据
    /// </summary>
    public bool RemoveMapData(float posX, float posY)
    {
        MapData mapData = _mapDataList.Find(data => 
            Mathf.Approximately(data.posX, posX) && Mathf.Approximately(data.posY, posY));
        
        if (mapData == null)
        {
            Debug.LogWarning($"[MapModel] 位置 ({posX}, {posY}) 不存在地图数据");
            return false;
        }

        _mapDataList.Remove(mapData);

        // 如果删除的是当前选中的数据，清空选中状态
        if (_selectedMapData == mapData)
        {
            _selectedMapData = null;
            EventManager.Instance.Publish(new MapDataSelectedEvent(null, false));
        }

        // 发布地图数据删除事件
        EventManager.Instance.Publish(new MapDataRemovedEvent(mapData));
        Debug.Log($"[MapModel] 删除地图数据: ItemId={mapData.itemId}, Pos=({posX}, {posY})");
        
        return true;
    }

    /// <summary>
    /// 根据ItemId删除地图数据
    /// </summary>
    public bool RemoveMapDataByItemId(int itemId)
    {
        MapData mapData = _mapDataList.Find(data => data.itemId == itemId);
        
        if (mapData == null)
        {
            Debug.LogWarning($"[MapModel] ItemId {itemId} 不存在地图数据");
            return false;
        }

        return RemoveMapData(mapData.posX, mapData.posY);
    }

    /// <summary>
    /// 选中地图数据
    /// </summary>
    public bool SelectMapData(float posX, float posY)
    {
        MapData mapData = _mapDataList.Find(data => 
            Mathf.Approximately(data.posX, posX) && Mathf.Approximately(data.posY, posY));
        
        if (mapData == null)
        {
            Debug.LogWarning($"[MapModel] 位置 ({posX}, {posY}) 不存在地图数据");
            return false;
        }

        MapData previousSelected = _selectedMapData;
        _selectedMapData = mapData;

        // 发布地图数据选中事件
        EventManager.Instance.Publish(new MapDataSelectedEvent(mapData, true));
        Debug.Log($"[MapModel] 选中地图数据: ItemId={mapData.itemId}, Pos=({posX}, {posY})");
        
        return true;
    }

    /// <summary>
    /// 取消选中地图数据
    /// </summary>
    public void UnselectMapData()
    {
        if (_selectedMapData != null)
        {
            MapData previousSelected = _selectedMapData;
            _selectedMapData = null;
            
            // 发布地图数据取消选中事件
            EventManager.Instance.Publish(new MapDataSelectedEvent(previousSelected, false));
            Debug.Log($"[MapModel] 取消选中地图数据: ItemId={previousSelected.itemId}");
        }
    }

    /// <summary>
    /// 根据位置获取地图数据
    /// </summary>
    public MapData GetMapDataByPosition(float posX, float posY)
    {
        return _mapDataList.Find(data => 
            Mathf.Approximately(data.posX, posX) && Mathf.Approximately(data.posY, posY));
    }

    /// <summary>
    /// 根据ItemId获取地图数据
    /// </summary>
    public MapData GetMapDataByItemId(int itemId)
    {
        return _mapDataList.Find(data => data.itemId == itemId);
    }

    /// <summary>
    /// 获取所有地图数据
    /// </summary>
    public List<MapData> GetAllMapData()
    {
        return new List<MapData>(_mapDataList);
    }

    /// <summary>
    /// 检查是否有选中的地图数据
    /// </summary>
    public bool HasSelectedMapData()
    {
        return _selectedMapData != null;
    }

    /// <summary>
    /// 清空所有地图数据 - 用于加载存档
    /// </summary>
    public void ClearAllMapData()
    {
        _mapDataList.Clear();
        _selectedMapData = null;
        Debug.Log("[MapModel] All map data cleared");
    }

    /// <summary>
    /// 从存档加载地图数据
    /// </summary>
    public void LoadMapDataFromSave(List<MapData> mapDataList)
    {
        _mapDataList.Clear();
        _selectedMapData = null;
        
        if (mapDataList != null)
        {
            foreach (var mapData in mapDataList)
            {
                if (mapData != null)
                {
                    _mapDataList.Add(mapData);
                }
            }
        }
        
        Debug.Log($"[MapModel] Loaded {_mapDataList.Count} map data from save");
    }

    // 私有方法
    private void OnMapDataChanged()
    {
        // 数据变化时的通用处理
    }
    
    /// <summary>
    /// 加载并实例化建筑物预制体
    /// </summary>
    private int LoadBuildingPrefab(int itemId, float posX, float posY, int uid = 0)
    {
        // 从Item.csv获取预制体路径
        var itemReader = ConfigManager.Instance.GetReader("Item");
        if (itemReader == null || !itemReader.HasKey(itemId))
        {
            Debug.LogWarning($"[MapModel] Item config not found for ID: {itemId}");
            return 0;
        }
        
        string prefabPath = itemReader.GetValue<string>(itemId, "PrefabPath", "");
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogWarning($"[MapModel] No prefab path found for building: {itemId}");
            return 0;
        }
        
        // 使用ResourceManager加载预制体
        GameObject prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[MapModel] Failed to load building prefab: {prefabPath}");
            return 0;
        }
        
        // 实例化预制体到世界位置
        Vector3 worldPosition = new Vector3(posX, 0, posY);
        GameObject buildingInstance = GameObject.Instantiate(prefab, worldPosition, Quaternion.identity);
        
        // 添加Building组件并初始化
        var buildingComponent = buildingInstance.GetComponent<Building>();
        if (buildingComponent == null)
        {
            buildingComponent = buildingInstance.AddComponent<Building>();
        }
        
        // 初始化建筑物数据
        buildingComponent.Initialize(itemId, new Vector2(posX, posY), uid);
        
        Debug.Log($"[MapModel] 成功加载建筑物预制体: {prefabPath} at ({posX}, {posY}), UID: {buildingComponent.UID}");
        
        return buildingComponent.UID;
    }
    
    /// <summary>
    /// 根据UID移除建筑物
    /// </summary>
    public bool RemoveBuildingByUID(int uid)
    {
        if (uid <= 0) return false;
        
        MapData mapData = _mapDataList.Find(data => data.buildingUID == uid);
        if (mapData == null)
        {
            Debug.LogWarning($"[MapModel] Building with UID {uid} not found");
            return false;
        }
        
        return RemoveMapData(mapData.posX, mapData.posY);
    }
    
    /// <summary>
    /// 根据UID获取建筑物数据
    /// </summary>
    public MapData GetBuildingByUID(int uid)
    {
        if (uid <= 0) return null;
        return _mapDataList.Find(data => data.buildingUID == uid);
    }
    
    /// <summary>
    /// 从存档数据中加载建筑物
    /// </summary>
    public void LoadBuildingsFromSave(List<MapData> buildingData)
    {
        if (buildingData == null) return;
        
        // 先清空现有建筑数据和GameObject
        ClearAllBuildings();
        
        // 恢复建筑数据并生成GameObject
        foreach (var mapData in buildingData)
        {
            _mapDataList.Add(mapData);
            
            // 重新生成建筑物GameObject
            LoadBuildingPrefab(mapData.itemId, mapData.posX, mapData.posY, mapData.buildingUID);
        }
        
        Debug.Log($"[MapModel] 恢复了 {buildingData.Count} 个建筑物");
    }
    
    /// <summary>
    /// 清空所有建筑数据和GameObject
    /// </summary>
    public void ClearAllBuildings()
    {
        // 销毁所有建筑GameObject
        var allBuildings = GameObject.FindObjectsOfType<Building>();
        foreach (var building in allBuildings)
        {
            if (building != null && building.gameObject != null)
            {
                GameObject.Destroy(building.gameObject);
            }
        }
        
        // 清空地图数据
        _mapDataList.Clear();
        _selectedMapData = null;
        
        Debug.Log("[MapModel] 已清空所有建筑物");
    }
} 