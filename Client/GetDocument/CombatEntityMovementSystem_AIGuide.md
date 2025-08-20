# CombatEntity 移动系统技术文档

## 简介
为CombatEntity基类添加了统一的NavMeshAgent移动接口，Player和Monster类都可以通过统一接口进行NavMesh寻路，同时保持各自特有的移动参数和逻辑。

## 详细接口

### CombatEntity 基类移动接口

#### 核心移动方法
```csharp
// 移动到指定位置 - 主要接口
public virtual bool MoveToPosition(Vector3 targetPosition)

// 移动到指定目标 - 重载方法
public virtual bool MoveToTarget(Transform target)

// 停止移动
public virtual void StopMovement()

// 设置移动速度
public virtual void SetMoveSpeed(float speed)
```

#### 状态查询属性
```csharp
// 移动相关属性
public float MoveSpeed => _moveSpeed;
public bool IsMoving => _navMeshAgent != null && _navMeshAgent.hasPath && _navMeshAgent.remainingDistance > 0.1f;
public bool HasNavMeshAgent => _navMeshAgent != null;
public Vector3 Destination => _navMeshAgent != null && _navMeshAgent.hasPath ? _navMeshAgent.destination : transform.position;
```

#### 辅助方法
```csharp
// 获取到目标的剩余距离
public virtual float GetRemainingDistance()

// 检查是否能到达指定位置
public virtual bool CanReachPosition(Vector3 targetPosition)

// 暂停/恢复NavMeshAgent
public virtual void SetNavMeshEnabled(bool enabled)

// NavMeshAgent初始化（可重写）
protected virtual void InitializeNavMeshAgent()
```

## 子类实现

### Player 类实现
- **移动速度设置**: 直接在Awake()中设置基类的 `_moveSpeed = 5f`
- **特殊逻辑**: 增加建筑放置模式检查，在建筑模式下禁止移动
- **重写方法**: `MoveToPosition()` - 增加建筑放置模式检查

```csharp
// Player在Awake中设置移动速度
protected override void Awake()
{
    base.Awake();
    _moveSpeed = 5f;  // 直接设置基类的移动速度
    // ... 其他初始化代码
}

// Player专用移动方法
public void MoveToPlayerPosition(Vector3 targetPosition)
{
    if (_inBuildingPlacementMode) return false;
    return base.MoveToPosition(targetPosition);
}
```

### Monster 类实现
- **移动速度参数**: 从Monster.csv配置表读取 `MoveSpeed`（追击速度）和 `IdleSpeed`（空闲速度）
- **速度初始化**: 在 `LoadConfigValues()` 中设置基类的 `_moveSpeed = _idleSpeed`
- **智能移动**: 优先使用NavMesh寻路，失败时使用原有的避让逻辑
- **状态管理**: 在不同AI状态下同步更新基类和NavMeshAgent的速度

```csharp
// Monster从配置表加载速度参数
private void LoadConfigValues()
{
    _chaseSpeed = config.GetValue(configId, "MoveSpeed", 3.5f);   // 追击速度
    _idleSpeed = config.GetValue(configId, "IdleSpeed", 1f);      // 空闲速度
    
    // 设置基类的移动速度为默认的空闲速度
    _moveSpeed = _idleSpeed;
}

// Monster AI状态切换时同步更新速度
case MonsterState.Idle:
case MonsterState.Patrol:
    _moveSpeed = _idleSpeed;       // 同步更新基类速度
    SetMoveSpeed(_idleSpeed);      // 设置NavMeshAgent速度
    break;
case MonsterState.Chase:
    _moveSpeed = _chaseSpeed;      // 同步更新基类速度
    SetMoveSpeed(_chaseSpeed);     // 设置NavMeshAgent速度
    break;
```

## 最佳实践

### 1. 基本移动使用
```csharp
// 移动到指定位置
bool success = combatEntity.MoveToPosition(targetPosition);

// 移动到目标对象
bool success = combatEntity.MoveToTarget(targetTransform);

// 检查是否正在移动
if (combatEntity.IsMoving)
{
    float remainingDistance = combatEntity.GetRemainingDistance();
    Debug.Log($"Moving to target, {remainingDistance:F1}m remaining");
}
```

### 2. Player专用移动
```csharp
// 玩家移动（自动处理建筑放置模式检查）
Player.Instance.MoveToPlayerPosition(clickPosition);

// 停止玩家移动
Player.Instance.StopMovement();
```

### 3. Monster AI移动
```csharp
// Monster在AI状态机中使用
private void UpdateChaseState()
{
    Vector3 direction = (chaseTarget - transform.position).normalized;
    MoveTo(direction, _chaseSpeed); // 内部会自动使用NavMesh或直接移动
}

// 检查Monster是否能到达目标
if (monster.CanReachPosition(playerPosition))
{
    monster.MoveToPosition(playerPosition);
}
```

### 4. 速度管理
```csharp
// 动态调整移动速度
combatEntity.SetMoveSpeed(newSpeed);

// Monster根据状态自动设置速度
ChangeState(MonsterState.Chase); // 自动设置为_chaseSpeed
```

## 注意事项

1. **NavMeshAgent依赖**: 移动功能依赖GameObject上的NavMeshAgent组件
2. **地形要求**: 目标位置必须在NavMesh上，否则会自动寻找最近的有效位置
3. **性能考虑**: Monster类在NavMesh失败时会回退到直接移动，确保AI正常工作
4. **状态管理**: Monster类会根据AI状态自动调整移动速度，无需手动设置

## 兼容性
- **Player类**: 保持原有的键盘移动和鼠标点击移动功能
- **Monster类**: 保持原有的AI状态机和避让逻辑
- **向后兼容**: 所有原有的移动调用都能正常工作

## 参考代码位置
- **基类实现**: `Assets/Scripts/Object/Base/CombatEntity.cs:291-374`
- **Player实现**: `Assets/Scripts/Object/Actor/Player.cs:38-188`
- **Monster实现**: `Assets/Scripts/Object/Actor/Monster.cs:38-608` 