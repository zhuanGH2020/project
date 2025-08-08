简介：提供统一的对象基础、对象管理和状态系统，支撑玩家、怪物、建筑等对象的一致生命周期与交互。

详细接口：
- ObjectBase（MonoBehaviour，挂载到需要被全局追踪的对象）：
  - 属性：Uid、ObjectType、Position
  - 方法：SetUid(int)、SetObjectType(ObjectType)、GetOrAddComponent<T>()、GetObjectState()
  - 生命周期：Awake() 生成Uid；OnEnable()/OnDisable() 自动注册/反注册到ObjectManager
- ObjectManager（单例，非Mono）：
  - Register(ObjectBase)、Unregister(ObjectBase)
  - FindByUid(int)/FindByUid<T>(int)
  - FindAllByType(ObjectType)/FindAllByType<T>(ObjectType)
- ObjectState（MonoBehaviour，挂到对象上）：
  - StartState(StateBase state)、EndState()、IsWorking、Update驱动当前状态Tick()
- StateBase/StateIdle/StateMove/StateAttack/StateDead（MonoBehaviour）：
  - 生命周期：EnterState()/ExitState()/Tick()
- 新枚举 ObjectType：Other/Player/Monster/Building/Item

最佳实践：
- 玩家、怪物、建筑、可采集物继承链：`ObjectBase` → `DamageableObject` → `CombatEntity`/`Building`/`HarvestableObject`
- 在子类的 Awake 中调用 `SetObjectType(...)` 指定类别。例如：
```csharp
protected override void Awake(){ base.Awake(); SetObjectType(ObjectType.Player); }
```
- 使用对象管理器查询：
```csharp
var player = ObjectManager.Instance.FindAllByType<Player>(ObjectType.Player);
var harvestables = ObjectManager.Instance.FindAllByType<HarvestableObject>(ObjectType.Item);
var some = ObjectManager.Instance.FindByUid(uid);
```
- 使用状态：
```csharp
var st = gameObject.GetOrAddComponent<StateIdle>();
GetComponent<ObjectState>().StartState(st);
```

对象类型说明：
- ObjectType.Player: 玩家角色
- ObjectType.Monster: 怪物和敌对生物
- ObjectType.Building: 建筑物和构造物
- ObjectType.Item: 可采集物品和掉落物（HarvestableObject及其子类）
- ObjectType.Other: 其他未分类对象

注意事项：
- 每个 MonoBehaviour 只负责单一职责；`StateBase` 不直接耦合业务逻辑，具体逻辑放到派生状态中。
- HarvestableObject（可采集物）归类为 Item 类型。
- 保持与现有 `EventManager`、`ConfigManager`、`ResourceUtils` 的用法一致；禁止擅自修改既有接口。
- 桌面端项目，不考虑移动端输入。 