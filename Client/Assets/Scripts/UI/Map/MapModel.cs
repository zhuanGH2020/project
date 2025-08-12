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
    private List<GameObject> _createdGameObjects = new List<GameObject>(); // 记录所有通过MapModel创建的GameObject

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

        // 删除对应的GameObject
        RemoveGameObjectByUID(mapData.buildingUID);
        
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
        
        // 将创建的GameObject添加到跟踪列表
        _createdGameObjects.Add(buildingInstance);
        
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
    /// 根据UID删除对应的GameObject
    /// </summary>
    private void RemoveGameObjectByUID(int uid)
    {
        if (uid <= 0) return;
        
        for (int i = _createdGameObjects.Count - 1; i >= 0; i--)
        {
            var gameObj = _createdGameObjects[i];
            if (gameObj == null)
            {
                // 清理空引用
                _createdGameObjects.RemoveAt(i);
                continue;
            }
            
            var building = gameObj.GetComponent<Building>();
            if (building != null && building.UID == uid)
            {
                GameObject.Destroy(gameObj);
                _createdGameObjects.RemoveAt(i);
                Debug.Log($"[MapModel] 删除UID为 {uid} 的GameObject");
                return;
            }
        }
        
        Debug.LogWarning($"[MapModel] 未找到UID为 {uid} 的GameObject");
    }
    
    /// <summary>
    /// 清理GameObject列表中的空引用
    /// </summary>
    public void CleanupNullGameObjects()
    {
        int removedCount = 0;
        for (int i = _createdGameObjects.Count - 1; i >= 0; i--)
        {
            if (_createdGameObjects[i] == null)
            {
                _createdGameObjects.RemoveAt(i);
                removedCount++;
            }
        }
        
        if (removedCount > 0)
        {
            Debug.Log($"[MapModel] 清理了 {removedCount} 个空GameObject引用");
        }
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
            
            // 重新生成建筑物GameObject（LoadBuildingPrefab会自动添加到_createdGameObjects列表）
            LoadBuildingPrefab(mapData.itemId, mapData.posX, mapData.posY, mapData.buildingUID);
        }
        
        Debug.Log($"[MapModel] 恢复了 {buildingData.Count} 个建筑物，创建了 {_createdGameObjects.Count} 个GameObject");
    }
    
    /// <summary>
    /// 清空所有建筑数据和GameObject
    /// </summary>
    public void ClearAllBuildings()
    {
        int destroyedCount = 0;
        
        // 1. 销毁所有通过MapModel创建的GameObject
        for (int i = _createdGameObjects.Count - 1; i >= 0; i--)
        {
            var gameObj = _createdGameObjects[i];
            if (gameObj != null)
            {
                GameObject.Destroy(gameObj);
                destroyedCount++;
            }
        }
        _createdGameObjects.Clear();
        
        // 2. 清理其他可能存在的游戏对象（不是通过MapModel创建的）
        ClearOtherGameObjects(ref destroyedCount);
        
        // 3. 清空地图数据
        _mapDataList.Clear();
        _selectedMapData = null;
        
        // 4. 清理ObjectManager中的相关注册（可选，因为OnDisable会自动清理）
        if (ObjectManager.HasInstance)
        {
            ObjectManager.Instance.CleanupNullReferences();
        }
        
        Debug.Log($"[MapModel] 已清空所有建筑物和相关GameObject，共销毁 {destroyedCount} 个对象");
    }
    
    /// <summary>
    /// 清理其他可能存在的游戏对象（Partner、Monster、子弹、特效等）
    /// </summary>
    private void ClearOtherGameObjects(ref int destroyedCount)
    {
        // 销毁所有Partner（伙伴）GameObject
        var allPartners = GameObject.FindObjectsOfType<Partner>();
        foreach (var partner in allPartners)
        {
            if (partner != null && partner.gameObject != null)
            {
                GameObject.Destroy(partner.gameObject);
                destroyedCount++;
            }
        }
        
        // 销毁所有Monster（怪物）GameObject
        var allMonsters = GameObject.FindObjectsOfType<Monster>();
        foreach (var monster in allMonsters)
        {
            if (monster != null && monster.gameObject != null)
            {
                GameObject.Destroy(monster.gameObject);
                destroyedCount++;
            }
        }
        
        // 销毁所有PartnerBullet（伙伴子弹）GameObject
        var allBullets = GameObject.FindObjectsOfType<PartnerBullet>();
        foreach (var bullet in allBullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                GameObject.Destroy(bullet.gameObject);
                destroyedCount++;
            }
        }
        
        // 销毁所有BulletTrail（子弹轨迹）GameObject
        var allTrails = GameObject.FindObjectsOfType<BulletTrail>();
        foreach (var trail in allTrails)
        {
            if (trail != null && trail.gameObject != null)
            {
                GameObject.Destroy(trail.gameObject);
                destroyedCount++;
            }
        }
    }
} 