# 角色管理框架技术文档
## 指导AI实现规范

### 1. 项目概述

本文档旨在指导AI实现一个完整的角色管理框架，适用于Unity游戏开发。框架需要支持多种角色类型，并提供状态机和Buff系统的扩展接口。

### 2. 代码风格规范

#### 2.1 基于现有项目风格
参考项目中 `Assets/Script/input/InputController.cs` 和 `Assets/Script/input/MovementController.cs` 的代码风格：

- 使用完整的XML文档注释（/// <summary>）
- 使用[Header]属性进行Inspector分组
- 使用[Range]属性限制数值范围
- 缓存Transform等组件提升性能
- 使用UnityEvent进行事件系统
- 方法命名使用PascalCase
- 私有字段使用camelCase
- 提供公共接口方法进行参数设置

#### 2.2 命名约定
```csharp
// 类名：PascalCase
public class ActorMain : Actor

// 方法名：PascalCase  
public void SetHealth(float health)

// 私有字段：camelCase
private float currentHealth;

// 公共属性：PascalCase
public float MaxHealth { get; private set; }

// 事件：PascalCase，使用On前缀
public UnityEvent OnDeath;
```

### 3. 架构设计

#### 3.1 核心架构图
```
Actor (基类)
├── ActorMain (主角)
├── Monster (怪物)
├── Partner (伙伴) 
├── Pet (宠物)
└── NPC (非玩家角色)

支持系统：
- IActorStateMachine (状态机接口)
- IBuffSystem (Buff系统接口)
- ActorManager (角色管理器)
```

#### 3.2 文件结构规划
```
Assets/Script/
├── Actor/
│   ├── Base/
│   │   ├── Actor.cs (基类)
│   │   ├── ActorData.cs (角色数据)
│   │   └── ActorConfig.cs (角色配置)
│   ├── Types/
│   │   ├── ActorMain.cs
│   │   ├── Monster.cs
│   │   ├── Partner.cs
│   │   ├── Pet.cs
│   │   └── NPC.cs
│   ├── StateMachine/
│   │   ├── IActorStateMachine.cs
│   │   ├── ActorState.cs
│   │   └── ActorStateMachine.cs
│   ├── Buff/
│   │   ├── IBuffSystem.cs
│   │   ├── BuffBase.cs
│   │   └── BuffManager.cs
│   └── Manager/
│       └── ActorManager.cs
```

### 4. 核心类实现指导

#### 4.1 Actor基类设计要求

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Actor : MonoBehaviour
{
    [Header("基础属性")]
    [SerializeField] protected string actorName;
    [SerializeField] protected int actorId;
    [SerializeField, Range(1f, 1000f)] protected float maxHealth = 100f;
    
    [Header("移动属性")]
    [SerializeField, Range(0.1f, 20f)] protected float moveSpeed = 5f;
    
    [Header("事件")]
    public UnityEvent OnDeath;
    public UnityEvent<float> OnHealthChanged;
    
    // 私有字段
    protected float currentHealth;
    protected Transform cachedTransform;
    protected bool isDead;
    
    // 接口引用
    protected IActorStateMachine stateMachine;
    protected IBuffSystem buffSystem;
    
    // 公共属性
    public string ActorName => actorName;
    public int ActorId => actorId;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    
    // 抽象方法 - 子类必须实现
    protected abstract void InitializeActor();
    protected abstract void UpdateActor();
    protected abstract void OnActorDeath();
}
```

**实现要点：**
1. 继承MonoBehaviour，与现有代码保持一致
2. 使用Header属性进行编辑器分组
3. 使用Range属性限制数值范围
4. 缓存Transform组件
5. 提供UnityEvent事件系统
6. 预留状态机和Buff系统接口
7. 定义抽象方法供子类实现

#### 4.2 具体角色类型实现要求

**ActorMain (主角) 特殊要求：**
- 支持经验值和等级系统
- 支持装备系统接口
- 支持技能系统接口
- 参考 `MovementController.cs` 的移动实现

**Monster (怪物) 特殊要求：**
- AI行为接口
- 掉落物品接口
- 攻击范围和仇恨系统

**Partner (伙伴) 特殊要求：**
- 跟随主角逻辑
- 简化AI行为
- 忠诚度系统

**Pet (宠物) 特殊要求：**
- 亲密度系统
- 成长系统
- 特殊技能

**NPC 特殊要求：**
- 对话系统接口
- 任务系统接口
- 商店系统接口

#### 4.3 状态机接口设计

```csharp
public interface IActorStateMachine
{
    void EnterState(ActorState newState);
    void UpdateState();
    void ExitCurrentState();
    ActorState GetCurrentState();
    bool CanTransitionTo(ActorState targetState);
}

public abstract class ActorState
{
    protected Actor owner;
    
    public virtual void OnEnter(Actor actor) { owner = actor; }
    public abstract void OnUpdate();
    public virtual void OnExit() { }
}
```

#### 4.4 Buff系统接口设计

```csharp
public interface IBuffSystem
{
    void AddBuff(BuffBase buff);
    void RemoveBuff(int buffId);
    void UpdateBuffs();
    List<BuffBase> GetActiveBuffs();
    bool HasBuff(int buffId);
}

public abstract class BuffBase
{
    public int BuffId { get; protected set; }
    public float Duration { get; protected set; }
    public Actor Target { get; protected set; }
    
    public abstract void OnApply(Actor target);
    public abstract void OnUpdate();
    public abstract void OnRemove();
}
```

### 5. 管理器实现要求

#### 5.1 ActorManager设计
```csharp
public class ActorManager : MonoBehaviour
{
    [Header("管理器设置")]
    [SerializeField] private int maxActorCount = 1000;
    
    // 参考现有代码的事件系统
    [System.Serializable]
    public class ActorEvent : UnityEvent<Actor> { }
    
    public ActorEvent OnActorSpawned;
    public ActorEvent OnActorDestroyed;
    
    // 单例模式
    public static ActorManager Instance { get; private set; }
    
    // 角色字典管理
    private Dictionary<int, Actor> actors;
    private Dictionary<System.Type, List<Actor>> actorsByType;
}
```

### 6. 实现优先级和步骤

#### 6.1 第一阶段：核心框架
1. 实现Actor基类
2. 实现基础的ActorData和ActorConfig
3. 创建状态机和Buff系统的接口定义
4. 实现ActorManager单例

#### 6.2 第二阶段：具体角色
1. 实现ActorMain类
2. 实现Monster类基础版本
3. 测试基础功能

#### 6.3 第三阶段：扩展系统
1. 完整实现状态机系统
2. 完整实现Buff系统
3. 实现其余角色类型（Partner、Pet、NPC）

#### 6.4 第四阶段：集成和优化
1. 集成到现有的输入系统
2. 性能优化和内存管理
3. 添加调试和编辑器工具

### 7. 代码质量要求

#### 7.1 性能考虑
- 使用对象池管理角色实例
- 缓存常用组件引用
- 避免在Update中进行复杂计算
- 使用事件系统减少耦合

#### 7.2 扩展性考虑
- 使用接口设计确保系统可扩展
- 支持配置文件驱动的角色属性
- 预留自定义状态和Buff的扩展点

#### 7.3 调试支持
- 提供详细的日志输出
- 支持编辑器内可视化调试
- 添加性能监控接口

### 8. 集成指导

#### 8.1 与现有输入系统集成
参考 `InputController.cs` 和 `MovementController.cs`：
- ActorMain 应该能接收输入事件
- 使用相同的UnityEvent模式
- 保持相同的代码注释风格

#### 8.2 时间和显示工具统一
遵循用户规则：查询项目中是否有统一的时间计算或显示接口，如果有则使用现有接口。

### 9. 测试和验证

#### 9.1 单元测试要求
- 每个角色类型的基础功能测试
- 状态机转换逻辑测试
- Buff系统的添加/移除测试

#### 9.2 集成测试要求
- 多角色交互测试
- 性能压力测试
- 内存泄漏检测

### 10. 文档和注释要求

所有代码必须包含：
- 完整的XML文档注释
- 复杂逻辑的行内注释
- 使用示例注释
- 性能注意事项说明

按照此文档实现的角色管理框架将具备良好的扩展性、可维护性和性能表现，完全符合项目现有的代码规范和架构设计。 