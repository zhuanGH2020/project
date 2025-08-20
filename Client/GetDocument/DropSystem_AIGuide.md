# 掉落系统重构总结

## 简介

成功将HarvestableObject的掉落逻辑移到DamageableObject基类中，并创建了统一的掉落接口`IDroppable`。现在所有可损坏对象都能支持掉落功能，不仅限于可采集物。

## 详细接口

### IDroppable 接口
```csharp
public interface IDroppable
{
    List<DropItem> GetDropItems();           // 获取掉落物品列表
    void ProcessDrops(IAttacker killer);     // 处理掉落逻辑
    void CreateDroppedItem(int itemId, int count); // 创建掉落物品到世界
    bool IsDropEnabled { get; }              // 是否启用掉落功能
}
```

### DamageableObject 掉落方法
```csharp
// 掉落控制
public virtual void EnableDrop()             // 启用掉落功能
public virtual void DisableDrop()            // 禁用掉落功能
public virtual bool IsDropEnabled            // 检查掉落是否启用

// 掉落配置
public virtual void AddDrop(DropItem dropItem) // 添加掉落物品
public virtual void AddDrop(int itemId, int minCount, int maxCount, float dropRate = 1.0f) // 简化添加
public virtual void ClearDrops()             // 清除所有掉落物品
public virtual List<DropItem> GetDropItems() // 获取掉落列表

// 掉落处理
public virtual void ProcessDrops(IAttacker killer)    // 处理掉落逻辑
public virtual void CreateDroppedItem(int itemId, int count) // 创建掉落物
```

### 死亡处理流程
```csharp
protected virtual void OnDeath()
{
    // 1. 处理死亡掉落（如果启用）
    if (IsDropEnabled && _lastAttacker != null)
    {
        ProcessDrops(_lastAttacker);
    }
    
    // 2. 调用子类自定义死亡逻辑
    OnDeathCustom();
}

protected virtual void OnDeathCustom()
{
    // 子类重写实现死亡逻辑
}
```

## 最佳实践

### 1. 基础使用 - 为Monster添加掉落
```csharp
// 创建怪物
var monster = monsterGO.AddComponent<Monster>();
monster.Init(monsterId);

// 添加掉落物品
monster.AddDrop(10001, 1, 2, 0.8f);  // 80%几率掉落1-2个物品10001
monster.AddDrop(10002, 1, 1, 0.3f);  // 30%几率掉落1个物品10002

// 启用掉落功能
monster.EnableDrop();
```

### 2. 自定义掉落物对象
```csharp
public class CustomDroppableObject : DamageableObject
{
    public override float MaxHealth => 50f;
    public override float Defense => 0f;
    public override bool CanInteract => CurrentHealth > 0;
    public override float GetInteractionRange() => 3f;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 设置掉落物品
        AddDrop(20001, 1, 3, 0.7f);
        AddDrop(20002, 1, 1, 0.3f);
        
        EnableDrop(); // 启用掉落功能
    }
}
```

### 3. 掉落管理器使用
```csharp
// 为任何可掉落对象设置掉落
DropManager.SetupDrops(droppableObject, 
    new DropItem(itemId1, 1, 2, 0.8f),
    new DropItem(itemId2, 1, 1, 0.5f)
);

// 快速设置单个掉落
DropManager.SetupSingleDrop(droppableObject, itemId, 1, 3, 0.9f);

// 检查是否有掉落
if (DropManager.HasDrops(droppableObject))
{
    Debug.Log("This object will drop items when destroyed");
}
```

### 4. Monster扩展方法
```csharp
// 设置标准掉落（根据等级）
monster.SetStandardDrops(level: 5);

// 设置BOSS掉落
monster.SetBossDrops();
```

### 5. 掉落处理流程
```csharp
public virtual void ProcessDrops(IAttacker killer)
{
    foreach (var drop in _drops)
    {
        int actualCount = drop.GetActualDropCount();
        if (actualCount > 0)
        {
            // 优先尝试添加到玩家背包
            if (killer != null && killer == Player.Instance && 
                PackageModel.Instance.AddItem(drop.itemId, actualCount))
            {
                // 成功添加到背包
            }
            else
            {
                // 创建掉落物到世界
                CreateDroppedItem(drop.itemId, actualCount);
            }
        }
    }
}
```

## 重构对比

### 重构前
- 掉落逻辑只在HarvestableObject中
- 重复的掉落处理代码
- 其他对象无法使用掉落功能
- 掉落逻辑与采集逻辑耦合

### 重构后
- 掉落逻辑在DamageableObject基类中
- 统一的掉落接口IDroppable
- 所有可损坏对象都能使用掉落功能
- 掉落逻辑与具体对象类型解耦

## 系统架构

```
IDroppable (接口)
    ↑
DamageableObject (基类实现)
    ↑
┌─HarvestableObject (采集物)
├─Monster (怪物)
├─Building (建筑)
└─CustomDroppableObject (自定义对象)
```

## 死亡掉落流程

```
TakeDamage() → 记录攻击者
    ↓
CurrentHealth <= 0
    ↓
OnDeath()
    ↓
IsDropEnabled? → ProcessDrops()
    ↓               ↓
OnDeathCustom()   尝试添加到背包
    ↓               ↓
子类自定义逻辑    失败→CreateDroppedItem()
```

## 配置示例

### Monster配置表 (假设添加掉落字段)
```csv
Id,MaxHealth,DropItem1,DropCount1,DropRate1,DropItem2,DropCount2,DropRate2
1001,100,10001,1-2,0.8,10002,1,0.3
1002,150,10003,2-4,1.0,10004,1,0.5
```

### 代码中配置掉落
```csharp
// 从配置表加载掉落数据
int dropItem1 = config.GetValue(configId, "DropItem1", 0);
int dropCount1 = config.GetValue(configId, "DropCount1", 1);
float dropRate1 = config.GetValue(configId, "DropRate1", 1.0f);

if (dropItem1 > 0)
{
    AddDrop(dropItem1, dropCount1, dropCount1, dropRate1);
    EnableDrop();
}
```

## 注意事项

1. **默认状态**: 掉落功能默认关闭，需要调用`EnableDrop()`启用
2. **攻击者记录**: 系统自动记录最后攻击者用于掉落处理
3. **背包优先**: 优先尝试将掉落物添加到玩家背包，失败后才创建掉落物
4. **继承关系**: 继承自DamageableObject的类自动获得掉落功能
5. **接口实现**: 可以在其他类中实现IDroppable接口来自定义掉落行为

## 参考代码位置

- 掉落接口定义: `Assets/Scripts/Object/Interface/IDroppable.cs`
- 基类实现: `Assets/Scripts/Object/Base/DamageableObject.cs` (第45-130行: 掉落系统)
- 使用示例: `Assets/Scripts/Object/Data/DropExample.cs`
- 数据结构: `Assets/Scripts/Object/Data/HarvestData.cs` (第20-44行: DropItem定义)
- 采集物应用: `Assets/Scripts/Object/Item/HarvestableObject.cs` (第95-108行: 掉落配置加载) 