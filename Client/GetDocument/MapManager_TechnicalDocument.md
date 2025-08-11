# MapManager 技术文档

## 简介

MapManager 是负责地图随机生成和怪物刷新的纯C#单例类，通过GameSettings配置驱动，每隔指定时间在指定位置生成怪物。

## 详细接口

### 核心属性
```csharp
// 只读属性
public float SpawnInterval => _monsterSpawnInterval;     // 当前生成间隔
public Vector3 SpawnPosition => _spawnPosition;          // 当前生成位置
public bool IsSpawnEnabled => _enableSpawn;             // 是否启用生成
public bool IsRandomSpawn => _randomSpawn;              // 是否随机生成
public int[] SpawnableMonsterIds => _spawnableMonsterIds; // 可生成怪物ID列表
```

### 生成控制方法
```csharp
// 更新生成逻辑 - 需要外部定期调用（通常在GameMain.Update中）
public void UpdateSpawning()

// 开始怪物生成（重置计时器）
public void StartMonsterSpawn()

// 停止怪物生成
public void StopMonsterSpawn()

// 设置是否启用怪物生成
public void SetSpawnEnabled(bool enabled)
```

### 配置方法
```csharp
// 设置怪物生成间隔
public void SetSpawnInterval(float interval)

// 设置怪物生成位置
public void SetSpawnPosition(Vector3 position)

// 设置可生成的怪物ID列表
public void SetSpawnableMonsterIds(int[] monsterIds)

// 添加可生成的怪物ID
public void AddSpawnableMonster(int monsterId)

// 设置是否随机生成
public void SetRandomSpawn(bool random)
```

### 生命周期方法
```csharp
// 清理资源
public void Cleanup()
```

## GameSettings 配置

MapManager 使用以下 GameSettings 配置：

```csharp
// 地图怪物生成间隔（秒）
GameSettings.MapSpawnInterval = 5f;

// 默认怪物生成位置
GameSettings.MapDefaultSpawnPosition = Vector3.zero;

// 默认可生成的怪物ID列表
GameSettings.MapDefaultMonsterIds = { 5001 };

// 最小生成间隔限制（秒）
GameSettings.MapMinSpawnInterval = 0.1f;
```

## 最佳实践

### 基本使用
```csharp
// 获取单例实例
var mapManager = MapManager.Instance;

// 开始生成怪物
mapManager.StartMonsterSpawn();

// 停止生成怪物
mapManager.StopMonsterSpawn();
```

### 配置调整
```csharp
// 设置生成间隔为3秒
mapManager.SetSpawnInterval(3f);

// 设置生成位置
mapManager.SetSpawnPosition(new Vector3(10, 0, 10));

// 设置可生成的怪物ID列表
mapManager.SetSpawnableMonsterIds(new int[] { 5001, 5002, 5003 });

// 启用随机生成
mapManager.SetRandomSpawn(true);
```

### 事件监听
```csharp
// 监听怪物生成事件
EventManager.Instance.Subscribe<MonsterSpawnedEvent>(OnMonsterSpawned);

private void OnMonsterSpawned(MonsterSpawnedEvent eventData)
{
    Debug.Log($"怪物生成: {eventData.MonsterInstance.name} 在位置 {eventData.SpawnPosition}");
}
```

### GameMain 集成
```csharp
// 在GameMain.Start()中初始化
var mapManager = MapManager.Instance;

// 在GameMain.Update()中驱动更新
MapManager.Instance.UpdateSpawning();

// 在GameMain.OnDestroy()中清理
MapManager.Instance.Cleanup();
```

### 配置表设置
```csharp
// Monster.csv 配置示例
Id,Name,PrefabPath
5001,公鸡,Prefabs/Monsters/zombie
5002,母鸡,Prefabs/Monsters/tank_zombie
5003,鸡王,Prefabs/Monsters/poison_zombie
```

## 注意事项

### 架构设计
- MapManager 是纯C#单例类，不继承MonoBehaviour
- 需要在GameMain中调用UpdateSpawning()驱动生成逻辑
- 使用GameSettings进行配置，编译时确定数值

### 容错机制
- 当配置表或预制体不存在时，自动创建红色立方体占位符
- 参数验证确保生成间隔不小于最小值
- 确保怪物ID列表不为空

### 性能优化
- 使用定时器而非协程，避免MonoBehaviour依赖
- 配置值通过GameSettings静态常量访问，零运行时开销
- 事件系统解耦，避免直接引用

### 事件系统
- 每次生成怪物发布MonsterSpawnedEvent事件
- 包含生成的怪物实例和位置信息
- 其他系统可监听此事件做出响应

## 其他需要补充的关键点

### 怪物组件支持
- 自动检测Monster组件并初始化
- 支持MonsterAI_Enhanced增强AI组件
- 通过Init(configId)方法传递配置ID

### 占位符系统
```csharp
// 当预制体不存在时创建占位符
private void CreatePlaceholderMonster(int monsterId)
{
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
    placeholder.name = $"PlaceholderMonster_{monsterId}";
    placeholder.GetComponent<Renderer>().material.color = Color.red;
}
```

### 生成策略
- **随机生成**: 从怪物ID列表中随机选择
- **循环生成**: 按顺序循环生成所有怪物类型
- 通过SetRandomSpawn()方法切换策略 