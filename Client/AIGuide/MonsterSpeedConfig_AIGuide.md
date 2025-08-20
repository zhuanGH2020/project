# Monster 速度配置总结

## 配置表结构（Monster.csv）

| 怪物ID | 名称 | MoveSpeed（追击） | IdleSpeed（空闲） | 说明 |
|--------|------|------------------|-------------------|------|
| 5000   | 僵尸 | 2.0              | 0.5               | 慢速移动，空闲时几乎静止 |
| 5001   | 公鸡 | 3.5              | 1.0               | 中等速度，空闲时缓慢移动 |
| 5002   | 母鸡 | 3.0              | 0.8               | 中等偏慢，空闲时较慢 |
| 5003   | 鸡王 | 4.0              | 1.2               | Boss级别，速度最快 |

## 代码实现

### 配置读取
```csharp
// LoadConfigValues()中的读表逻辑
_chaseSpeed = config.GetValue(configId, "MoveSpeed", 3.5f);   // 第13列：追击速度
_idleSpeed = config.GetValue(configId, "IdleSpeed", 1f);      // 第19列：空闲速度

// 设置基类默认速度
_moveSpeed = _idleSpeed;
```

### 动态速度设置
```csharp
// MoveTo调用时动态设置速度
protected void MoveTo(Vector3 direction, float speed)
{
    // 设置速度（只在实际移动时设置）
    _moveSpeed = speed;
    SetMoveSpeed(speed);
    
    // ... 移动逻辑
}

// 使用示例
MoveTo(direction, _idleSpeed);    // 巡逻时使用空闲速度
MoveTo(direction, _chaseSpeed);   // 追击时使用追击速度
```

## 验证结果

✅ **配置表字段正确**: MoveSpeed 和 IdleSpeed 字段存在并有合理数值  
✅ **读表逻辑正确**: 从正确的配置列读取速度参数  
✅ **简化实现**: 移除冗余的 `_currentSpeed` 字段和预设置逻辑  
✅ **动态设置**: 只在实际移动时设置速度，避免不必要的重复设置  
✅ **基类同步**: 基类的 `_moveSpeed` 字段与NavMeshAgent保持同步  

## 使用示例

```csharp
// 怪物初始化
monster.Init(5001);  // 初始化为公鸡
// 此时 _moveSpeed = 1.0 (IdleSpeed - 默认值)

// 巡逻移动
MoveTo(patrolDirection, _idleSpeed);
// 此时 _moveSpeed = 1.0，NavMeshAgent.speed = 1.0

// 追击移动  
MoveTo(chaseDirection, _chaseSpeed);
// 此时 _moveSpeed = 3.5，NavMeshAgent.speed = 3.5

// 速度只在实际移动时设置，不在状态切换时预设置
``` 