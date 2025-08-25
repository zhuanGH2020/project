# CombatEntity 移动系统技术文档

**创建日期**: 2024年12月
**版本**: 2.1
**状态**: 持续更新

## 简介
为CombatEntity基类添加了统一的NavMeshAgent移动接口，Player和Monster类都可以通过统一接口进行NavMesh寻路，同时保持各自特有的移动参数和逻辑。最新版本增加了智能朝向系统、2D Blend Tree动画系统、Layer Mask装备动画系统和寻路优化。

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

// 动画系统初始化（可重写）
protected virtual void InitializeAnimationSystem()
```

## 子类实现

### Player 类实现

#### 基础移动设置
- **移动速度设置**: 在Awake()中从NavMeshAgent获取速度，如果没有则使用默认值5f
- **特殊逻辑**: 增加建筑放置模式检查，在建筑模式下禁止移动
- **重写方法**: `MoveToPosition()` - 增加建筑放置模式检查

```csharp
// Player在Awake中设置移动速度
protected override void Awake()
{
    base.Awake();
    
    // 从NavMeshAgent获取移动速度（如果存在的话）
    if (_navMeshAgent != null)
    {
        _moveSpeed = _navMeshAgent.speed;
    }
    else
    {
        // 如果没有NavMeshAgent，使用默认速度
        _moveSpeed = 5f;
    }
}

// Player专用移动方法
public bool MoveToPlayerPosition(Vector3 targetPosition)
{
    // 如果在建筑放置模式，不响应移动命令
    if (_inBuildingPlacementMode)
    {
        return false;
    }
    
    // 调用基类的统一移动接口
    return base.MoveToPosition(targetPosition);
}
```

#### 智能朝向系统（v2.0新增）
Player类实现了智能朝向系统，能够根据当前状态自动切换朝向模式：

**朝向模式切换**：
```csharp
private void HandleMouseRotation()
{
    // 如果正在寻路中，不朝向鼠标，而是朝向移动方向
    if (IsMovingToTarget())
    {
        HandleMovementRotation();
        return;
    }
    
    // 不在寻路时，正常朝向鼠标
    // ... 原有的鼠标朝向逻辑
}
```

**寻路状态检测**：
```csharp
private bool IsMovingToTarget()
{
    if (_navMeshAgent == null) return false;
    
    // 检查是否有路径且正在移动
    return _navMeshAgent.hasPath && 
           _navMeshAgent.remainingDistance > 0.1f && 
           _navMeshAgent.velocity.magnitude > 0.1f;
}
```

**移动朝向处理**：
```csharp
private void HandleMovementRotation()
{
    if (_navMeshAgent == null || !_navMeshAgent.hasPath) return;
    
    // 获取移动方向
    Vector3 moveDirection = _navMeshAgent.velocity.normalized;
    if (moveDirection != Vector3.zero)
    {
        // 只考虑水平方向
        moveDirection.y = 0;
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            float rotationSpeed = 15f; // 移动时旋转速度稍慢，更自然
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
```

**鼠标朝向处理**：
```csharp
private void HandleRotationToPosition(Vector3 targetPosition)
{
    // 计算朝向目标的方向（忽略Y轴）
    Vector3 directionToTarget = (targetPosition - transform.position).normalized;
    directionToTarget.y = 0;
    
    // 只有当方向不为零时才进行旋转
    if (directionToTarget != Vector3.zero)
    {
        // 计算目标旋转
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        
        // 平滑旋转朝向目标（提高旋转速度确保响应及时）
        float rotationSpeed = 20f; // 进一步提高旋转速度
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
```

#### 动画系统集成（v2.0重构）
Player类集成了Animator系统，实现了移动动画的自动控制。最新版本经过重构，消除了重复代码，建立了统一的动画控制中心：

**统一的动画控制中心**：
```csharp
private void UpdateAnimatorMovementParams()
{
    if (_animator == null) return;
    
    // 如果正在寻路移动，设置动画参数为前进
    if (IsMovingToTarget())
    {
        _animator.SetFloat("MoveX", 0f);
        _animator.SetFloat("MoveY", 1f);
        return;
    }
    
    // 键盘移动时，根据实际移动方向设置动画参数
    if (_moveDirection != Vector3.zero)
    {
        // 获取世界坐标的移动方向
        Vector3 worldMoveDirection = _moveDirection.normalized;
        
        // 将世界坐标的移动方向转换为角色本地坐标
        Vector3 localMoveDirection = transform.InverseTransformDirection(worldMoveDirection);
        
        // 设置Animator参数
        // MoveX: 左右移动 (-1左, 0中, 1右)
        // MoveY: 前后移动 (-1后, 0中, 1前)
        _animator.SetFloat("MoveX", localMoveDirection.x);
        _animator.SetFloat("MoveY", localMoveDirection.z);
    }
    else
    {
        // 停止移动时，将动画参数设为0
        _animator.SetFloat("MoveX", 0f);
        _animator.SetFloat("MoveY", 0f);
    }
}
```

**重构后的架构优势**：
- **消除重复代码**：所有动画参数设置都集中在`UpdateAnimatorMovementParams()`方法中
- **避免参数冲突**：寻路和键盘移动的动画控制不会相互覆盖
- **统一更新机制**：所有动画参数更新都在一个地方处理
- **性能优化**：减少了不必要的Animator参数设置调用

**智能的动画状态管理**：
```csharp
// 优先级：寻路移动 > 键盘移动 > 停止状态
if (IsMovingToTarget())           // 寻路：前进动画
{
    // MoveX=0, MoveY=1
}
else if (_moveDirection != Vector3.zero)  // 键盘：方向动画
{
    // 根据实际移动方向设置
}
else                               // 停止：静止动画
{
    // MoveX=0, MoveY=0
}
```

**移动动画控制**：
```csharp
private void HandleMovement()
{
    if (_moveDirection != Vector3.zero)
    {
        // 停止NavMesh移动，使用键盘移动
        StopMovement();
        
        // 获取当前速度（优先使用NavMeshAgent的speed，否则使用_moveSpeed）
        float currentSpeed = (_navMeshAgent != null) ? _navMeshAgent.speed : _moveSpeed;
        
        // 移动
        transform.position += _moveDirection * currentSpeed * Time.deltaTime;
    }
    
    // 统一更新动画参数（包括停止移动的情况）
    UpdateAnimatorMovementParams();
}
```

**停止移动动画控制**：
```csharp
public override void StopMovement()
{
    base.StopMovement();
    
    // 停止移动时，将动画参数设为0
    if (_animator != null)
    {
        _animator.SetFloat("MoveX", 0f);
        _animator.SetFloat("MoveY", 0f);
    }
}
```

**动画系统工作流程**：
1. **Update()** → 调用`HandleMovement()`和`HandleMouseRotation()`
2. **HandleMovement()** → 处理键盘移动，调用`UpdateAnimatorMovementParams()`
3. **HandleMouseRotation()** → 处理朝向，调用`HandleMovementRotation()`
4. **UpdateAnimatorMovementParams()** → 统一设置动画参数
5. **HandleMovementRotation()** → 只处理寻路时的朝向旋转，不设置动画参数

#### 装备动画系统（v2.1新增）
Player类新增了装备动画系统，支持根据装备动态加载不同的Animator控制器：

**装备动画配置**：
- **AnimatorPath字段**：在Equip.csv配置表中新增AnimatorPath字段，指定装备对应的动画控制器路径
- **动态加载**：装备时自动从Resources加载对应的Animator控制器
- **装备层控制**：使用Animator的Layer系统，装备动画在Layer 1上播放

**装备动画接口**：
```csharp
// 装备时自动加载动画控制器
public override bool Equip(int equipId)

// 卸下装备时恢复基础动画
protected override void UnequipByPart(EquipPart part)
```

**武器使用动画控制**：
```csharp
// 使用武器时触发攻击动画
private void TriggerAttackAnimation()

// 装备时切换动画控制器
protected virtual void SwitchAnimatorController(EquipBase equip)

// 卸下装备时恢复基础动画控制器
protected virtual void RestoreBaseAnimatorController()
```

**装备动画工作流程**：
1. **装备物品** → 从装备对象获取AnimatorPath → 加载对应Animator控制器 → 切换动画控制器
2. **使用武器** → 设置Apply触发器 → 装备Animator播放攻击动画 → 支持边移动边攻击
3. **连续攻击** → 持续设置Apply触发器 → 装备Animator持续播放攻击动画
4. **卸下装备** → 恢复基础Animator控制器 → 回到基础动画状态

**配置表示例**：
```csv
Id,Name,Type,EquipType,AnimatorPath,Damage,Defense
2001,冲锋枪,Hand,Uzi,Animation/HandGun/HandGunAnimator,15,0
2003,铁剑,Hand,Sword,Animation/Knife/KnifeAnimator,25,0
```

**注意事项**：
- AnimatorPath必须指向Resources文件夹内的.controller文件
- 装备动画控制器应包含2D Blend Tree处理移动动画和Apply触发器处理攻击动画
- 基础动画控制器负责基础移动动画，装备动画控制器负责装备相关的移动和攻击动画
- 系统会自动处理动画控制器的切换，装备时自动切换，卸下时自动恢复

#### 寻路优化（v2.0新增）
Player类优化了寻路系统，提供了更好的用户体验：

**寻路状态管理**：
- **自动朝向切换**：寻路时自动朝向移动方向，结束后恢复鼠标朝向
- **平滑过渡**：不同朝向模式之间的自然切换
- **性能优化**：只在需要时进行朝向计算

**寻路行为表现**：
- **寻路过程中**：玩家朝向移动方向，表现更自然
- **寻路结束后**：玩家恢复朝向鼠标，响应更及时
- **键盘移动时**：自动停止寻路，恢复鼠标朝向控制

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

### 5. 朝向系统使用（v2.0新增）
```csharp
// Player朝向系统自动工作，无需手动调用
// 寻路时自动朝向移动方向
// 非寻路时自动朝向鼠标位置
// 两种模式之间自动切换
```

### 6. 动画系统使用（v2.0重构）
```csharp
// Player动画系统自动工作，无需手动调用
// 移动时自动设置动画参数
// 停止时自动重置动画参数
// 寻路和键盘移动使用不同的动画逻辑
```

### 7. 装备动画系统使用（v2.1新增）
```csharp
// Player装备动画系统自动工作，无需手动调用
// 装备时自动加载对应动画控制器
// 使用武器时自动启用装备层
// 卸下装备时自动恢复基础动画
```

### 8. Layer Mask动画系统使用（v2.1新增）
```csharp
// 使用Layer Mask实现边移动边攻击
// 攻击时设置Apply触发器，装备的Animator自动播放攻击动画
// 移动动画和攻击动画通过Layer Mask自然混合
// 系统简洁高效，支持复杂的动画组合
```

## 注意事项

1. **NavMeshAgent依赖**: 移动功能依赖GameObject上的NavMeshAgent组件
2. **地形要求**: 目标位置必须在NavMesh上，否则会自动寻找最近的有效位置
3. **性能考虑**: Monster类在NavMesh失败时会回退到直接移动，确保AI正常工作
4. **状态管理**: Monster类会根据AI状态自动调整移动速度，无需手动设置
5. **朝向系统**（v2.0新增）: Player朝向系统自动检测寻路状态，无需手动控制
6. **动画系统**（v2.0重构）: Player动画系统自动同步移动状态，确保动画表现正确
7. **寻路优化**（v2.0新增）: 寻路时朝向更自然，结束后响应更及时
8. **2D Blend Tree动画系统**（v2.0新增）: 使用2D Blend Tree处理8方向移动动画，统一的动画控制中心
9. **装备动画系统**（v2.1新增）: 装备动画控制器必须包含2D Blend Tree和Apply触发器，AnimatorPath必须指向Resources文件夹内的.controller文件
10. **时序安全**（v2.1新增）: 装备动画系统已优化时序问题，自动保存基础动画控制器，避免装备/卸载时的动画丢失
11. **Layer Mask系统**（v2.1新增）: 使用Layer Mask实现更自然的动画混合，支持边移动边攻击

## 兼容性
- **Player类**: 保持原有的键盘移动和鼠标点击移动功能，新增智能朝向、动画系统和装备动画系统
- **Monster类**: 保持原有的AI状态机和避让逻辑
- **向后兼容**: 所有原有的移动调用都能正常工作
- **新增功能**: 智能朝向、动画系统、装备动画系统不影响原有功能

## Layer Mask动画系统技术细节（v2.1）

### 系统架构
v2.1版本新增了Layer Mask动画系统，实现了更简洁高效的动画混合系统：

#### 核心组件
- **CombatEntity基类**: 统一的动画管理，负责动画控制器切换和触发器设置
- **装备Animator**: 每个装备类型有独立的动画控制器，包含移动和攻击动画
- **Layer Mask**: 通过Unity的Layer Mask系统实现动画的自然混合

#### 工作流程
```
Player攻击 → UseHandEquip() → CombatEntity基类.UseHandEquip() → 
TriggerAttackAnimation() → 设置Apply触发器 → 装备Animator播放攻击动画
```

### 技术实现

#### 1. 动画控制器管理
```csharp
// CombatEntity基类中的动画控制器切换
protected virtual void SwitchAnimatorController(EquipBase equip)
{
    // 从装备对象获取AnimatorPath
    string animatorPath = equip.AnimatorPath;
    if (string.IsNullOrEmpty(animatorPath)) return;
    
    // 加载并设置动画控制器
    var animatorController = ResourceManager.Instance.Load<RuntimeAnimatorController>(animatorPath);
    if (animatorController != null)
    {
        _animator.runtimeAnimatorController = animatorController;
    }
}
```

#### 2. 攻击动画触发
```csharp
// 在UseHandEquip中触发动画
protected virtual void UseHandEquip()
{
    var handEquip = GetEquipByPart(EquipPart.Hand);
    if (handEquip != null && handEquip.CanUse)
    {
        // 触发攻击动画
        TriggerAttackAnimation();
        
        // 使用装备
        handEquip.Use();
    }
}

// 设置Apply触发器
public virtual void TriggerAttackAnimation()
{
    if (_animator != null)
    {
        _animator.SetTrigger("Apply");
    }
}
```

#### 3. 装备配置要求
装备的Animator控制器必须满足以下要求：
- **包含2D Blend Tree**: 处理8方向移动动画
- **Apply触发器**: 用于触发攻击动画
- **Layer Mask设置**: 确保移动和攻击动画能自然混合

### 系统优势

#### Layer Mask系统特点
- ✅ 使用Unity原生Layer Mask系统
- ✅ 动画自然混合，无需手动控制
- ✅ 支持边移动边攻击
- ✅ 代码结构简洁，易于维护
- ✅ 性能更好，支持复杂的动画组合

### 配置示例

#### Equip.csv配置
```csv
Id,Name,Type,EquipType,AnimatorPath,Damage,Defense
30002,斧头,Hand,Axe,Animation/Knife/KnifeAnimator,20,0
2001,冲锋枪,Hand,Uzi,Animation/HandGun/HandGunAnimator,15,0
```

#### Animator控制器结构
```
Layers
├── Base Layer
│   └── 2D Blend Tree (MoveX, MoveY)
│       ├── 8方向移动动画
│       └── 移动动画混合
└── Equip Layer
    └── 攻击状态 (Apply触发器)
```

## 版本更新记录

### v2.1
- **新增装备动画系统**: 支持根据装备动态加载不同的Animator控制器
- **AnimatorPath配置**: 在Equip.csv中新增AnimatorPath字段，支持装备特定的动画控制器
- **Layer Mask系统**: 使用Layer Mask实现动画混合，支持边移动边攻击
- **动态动画切换**: 装备时自动加载对应动画控制器，卸下时恢复基础动画
- **时序安全优化**: 装备动画系统优化时序问题，自动保存基础动画控制器
- **回退机制**: 添加默认基础动画控制器加载，作为最后的回退方案
- **安全检查**: 装备/卸载时的多重安全检查，确保动画系统稳定运行



### v2.0
- **新增智能朝向系统**: Player在寻路时朝向移动方向，非寻路时朝向鼠标
- **新增2D Blend Tree动画系统**: 使用2D Blend Tree处理8方向移动动画，支持MoveX和MoveY参数
- **新增寻路优化**: 更自然的寻路表现，更及时的响应切换
- **新增状态检测**: 自动检测寻路状态，智能切换朝向模式
- **动画系统重构**: 建立统一的动画控制中心

### v1.0
- **基础移动系统**: 统一的NavMeshAgent移动接口
- **Player移动**: 建筑放置模式检查和基础移动功能
- **Monster移动**: 配置表驱动的速度管理和AI状态同步

## 参考代码位置
- **基类实现**: `Assets/Scripts/Object/Base/CombatEntity.cs:291-374`
- **Player实现**: `Assets/Scripts/Object/Actor/Player.cs:38-188`
- **Monster实现**: `Assets/Scripts/Object/Actor/Monster.cs:38-608`
- **装备系统**: `Assets/Scripts/Object/Equip/Base/EquipBase.cs` 