using UnityEngine;

/// <summary>
/// 地图管理器 - 负责地图随机生成、怪物刷新等游戏逻辑
/// </summary>
public class MapManager
{
    // 单例实现
    private static MapManager _instance;
    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MapManager();
            }
            return _instance;
        }
    }

    // 私有字段
    private float _monsterSpawnInterval = GameSettings.MapSpawnInterval;  // 怪物生成间隔（秒）
    private Vector3 _spawnPosition = GameSettings.MapDefaultSpawnPosition;  // 生成位置
    private bool _enableSpawn = true;  // 是否启用生成
    private bool _randomSpawn = true;  // 是否随机生成怪物类型
    private int[] _spawnableMonsterIds = GameSettings.MapDefaultMonsterIds;  // 可生成的怪物ID列表
    
    private float _lastSpawnTime;  // 上次生成时间

    // 公共属性
    public float SpawnInterval => _monsterSpawnInterval;
    public Vector3 SpawnPosition => _spawnPosition;
    public bool IsSpawnEnabled => _enableSpawn;
    public bool IsRandomSpawn => _randomSpawn;
    public int[] SpawnableMonsterIds => _spawnableMonsterIds;

    // 私有构造函数
    private MapManager()
    {
        ValidateParameters();
        _lastSpawnTime = Time.time;
        Debug.Log("[MapManager] MapManager initialized");
    }

    /// <summary>
    /// 更新怪物生成逻辑 - 需要外部定期调用
    /// </summary>
    public void UpdateSpawning()
    {
        if (!_enableSpawn) return;
        
        // 检查是否到达生成时间
        if (Time.time - _lastSpawnTime >= _monsterSpawnInterval)
        {
            SpawnMonster();
            _lastSpawnTime = Time.time;
        }
    }

    /// <summary>
    /// 开始怪物生成（重置计时器）
    /// </summary>
    public void StartMonsterSpawn()
    {
        _lastSpawnTime = Time.time;
    }

    /// <summary>
    /// 停止怪物生成
    /// </summary>
    public void StopMonsterSpawn()
    {
        _enableSpawn = false;
    }

    /// <summary>
    /// 在指定位置生成怪物
    /// </summary>
    private void SpawnMonster()
    {
        if (!_enableSpawn) return;

        // 选择要生成的怪物ID
        int selectedMonsterId = SelectMonsterToSpawn();
        if (selectedMonsterId <= 0) return;

        // 从配置表获取怪物信息
        var monsterConfig = ConfigManager.Instance.GetReader("Monster");
        if (monsterConfig == null || !monsterConfig.HasKey(selectedMonsterId))
        {
            CreatePlaceholderMonster(selectedMonsterId);
            return;
        }

        // 从配置表读取预制体路径
        string prefabPath = monsterConfig.GetValue<string>(selectedMonsterId, "PrefabPath", "");
        if (string.IsNullOrEmpty(prefabPath))
        {
            CreatePlaceholderMonster(selectedMonsterId);
            return;
        }

        // 使用ResourceManager加载怪物预制体
        GameObject monsterPrefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
        if (monsterPrefab == null)
        {
            CreatePlaceholderMonster(selectedMonsterId);
            return;
        }

        // 在指定位置实例化怪物
        GameObject monsterInstance = Object.Instantiate(monsterPrefab, _spawnPosition, Quaternion.identity);
        
        // 获取Monster组件并初始化
        var monsterComponent = monsterInstance.GetComponent<Monster>();
        if (monsterComponent == null)
        {
            // 尝试获取增强版AI组件
            var enhancedMonster = monsterInstance.GetComponent<MonsterAI_Enhanced>();
            if (enhancedMonster != null)
            {
                enhancedMonster.Init(selectedMonsterId);
            }
        }

        // 发布怪物生成事件
        EventManager.Instance.Publish(new MonsterSpawnedEvent(monsterInstance, _spawnPosition));
    }

    /// <summary>
    /// 选择要生成的怪物ID
    /// </summary>
    private int SelectMonsterToSpawn()
    {
        if (_spawnableMonsterIds == null || _spawnableMonsterIds.Length == 0)
            return 0;

        if (_randomSpawn)
        {
            // 随机选择一个怪物ID
            int randomIndex = Random.Range(0, _spawnableMonsterIds.Length);
            return _spawnableMonsterIds[randomIndex];
        }
        else
        {
            // 按顺序循环生成
            int index = Time.frameCount % _spawnableMonsterIds.Length;
            return _spawnableMonsterIds[index];
        }
    }

    /// <summary>
    /// 创建占位符怪物（当预制体不存在时）
    /// </summary>
    private void CreatePlaceholderMonster(int monsterId)
    {
        // 创建一个简单的立方体作为占位符怪物
        GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholder.name = $"PlaceholderMonster_{monsterId}";
        placeholder.transform.position = _spawnPosition;
        placeholder.transform.localScale = Vector3.one * 0.5f;
        
        // 添加一个简单的颜色来区分
        var renderer = placeholder.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        // 发布怪物生成事件
        EventManager.Instance.Publish(new MonsterSpawnedEvent(placeholder, _spawnPosition));
    }

    /// <summary>
    /// 设置怪物生成间隔
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        _monsterSpawnInterval = interval;
    }

    /// <summary>
    /// 设置怪物生成位置
    /// </summary>
    public void SetSpawnPosition(Vector3 position)
    {
        _spawnPosition = position;
    }

    /// <summary>
    /// 设置可生成的怪物ID列表
    /// </summary>
    public void SetSpawnableMonsterIds(int[] monsterIds)
    {
        _spawnableMonsterIds = monsterIds;
    }

    /// <summary>
    /// 添加可生成的怪物ID
    /// </summary>
    public void AddSpawnableMonster(int monsterId)
    {
        if (_spawnableMonsterIds == null)
        {
            _spawnableMonsterIds = new int[] { monsterId };
        }
        else
        {
            var newList = new int[_spawnableMonsterIds.Length + 1];
            _spawnableMonsterIds.CopyTo(newList, 0);
            newList[_spawnableMonsterIds.Length] = monsterId;
            _spawnableMonsterIds = newList;
        }
    }

    /// <summary>
    /// 设置是否随机生成
    /// </summary>
    public void SetRandomSpawn(bool random)
    {
        _randomSpawn = random;
    }

    /// <summary>
    /// 设置是否启用怪物生成
    /// </summary>
    public void SetSpawnEnabled(bool enabled)
    {
        _enableSpawn = enabled;
        
        if (enabled)
        {
            StartMonsterSpawn();
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Cleanup()
    {
        _enableSpawn = false;
    }

    /// <summary>
    /// 验证配置参数
    /// </summary>
    private void ValidateParameters()
    {
        // 确保生成间隔不小于最小值
        if (_monsterSpawnInterval < GameSettings.MapMinSpawnInterval)
        {
            _monsterSpawnInterval = GameSettings.MapMinSpawnInterval;
        }
        
        // 确保怪物ID列表不为空
        if (_spawnableMonsterIds == null || _spawnableMonsterIds.Length == 0)
        {
            _spawnableMonsterIds = GameSettings.MapDefaultMonsterIds;
        }
    }
} 