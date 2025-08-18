# 装备系统技术文档

## 简介

装备系统是游戏的核心系统之一，负责管理玩家角色的装备穿戴、属性加成、装备交互等功能。采用数据驱动设计，支持配置表驱动的装备属性和类型系统。

## 核心组件

### 1. EquipManager - 装备管理器
**位置**: `Assets/Scripts/Manager/EquipManager.cs`

**职责**: 统一管理所有装备相关逻辑，作为装备系统的单一数据源和操作入口

**核心功能**:
- 装备状态管理：`_equippedItems`字典直接存储装备ID，避免序列化问题
- 装备操作：`EquipItem()`、`UnequipItem()`等核心方法，返回操作结果
- 数据同步：与Player组件保持状态一致，支持存档持久化
- 事件通知：`OnEquipmentChanged`事件通知UI更新，参数为装备ID

**关键方法**:
```csharp
// 装备物品到指定部位
public bool EquipItem(int itemId, EquipPart equipPart)

// 卸下指定部位的装备
public bool UnequipItem(EquipPart equipPart)

// 获取指定部位的装备ID
public int GetEquippedItemId(EquipPart equipPart)

// 获取所有已装备物品的ID字典
public Dictionary<EquipPart, int> GetAllEquippedItemIds()

// 检查指定部位是否有装备
public bool HasEquippedItem(EquipPart equipPart)

// 从存档加载装备数据
public void LoadEquippedItemsFromSave(List<int> equippedItems)

// 同步Player装备状态（调试用）
public void SyncPlayerEquipmentState()
```

### 2. PackageView - 装备UI界面
**位置**: `Assets/Scripts/UI/Package/PackageView.cs`

**职责**: 处理装备槽位显示和用户交互

**核心功能**:
- 装备槽位管理：`cellHead`、`cellBody`、`cellHand`三个装备槽
- 右键装备：背包物品右键点击自动装备
- 装备卸下：点击装备槽位卸下装备
- UI同步：实时更新装备槽位显示

**关键方法**:
```csharp
// 尝试装备物品
private void TryEquipItem(int itemId)

// 更新装备槽位显示
private void UpdateEquipSlot(EquipPart equipPart)

// 处理装备槽位点击
private void OnEquipSlotClicked(EquipPart equipPart)
```

### 3. ConfigManager - 配置管理
**位置**: `Assets/Scripts/Core/Config/ConfigManager.cs`

**职责**: 提供装备配置信息查询

**配置表**: `Assets/Resources/Configs/Equip.csv`

### 4. ItemManager - 物品管理
**位置**: `Assets/Scripts/Manager/ItemManager.cs`

**职责**: 判断物品类型，提供物品配置查询

## 装备类型系统

### 装备部位枚举 (EquipPart)
```csharp
public enum EquipPart
{
    None = 0,       // 无部位
    Head = 1,       // 头部
    Body = 2,       // 身体
    Hand = 3        // 手部
}
```

### 装备类型枚举 (EquipType)
```csharp
public enum EquipType
{
    None = 0,       // 无类型
    Weapon = 1,     // 武器
    Armor = 2,      // 护甲
    Tool = 3,       // 工具
    Accessory = 4   // 饰品
}
```

## 装备基类体系

### EquipBase - 装备基类
**位置**: `Assets/Scripts/Object/Equip/Base/EquipBase.cs`

**继承关系**: `MonoBehaviour` → `EquipBase` → 具体装备类

**核心属性**:
```csharp
public class EquipBase : MonoBehaviour, IEquipable
{
    protected int _configId;           // 配置ID
    protected EquipPart _equipPart;    // 装备部位
    protected float _damage;           // 攻击力
    protected float _defense;          // 防御力
    protected float _maxDurability;    // 最大耐久度
    protected float _currentDurability; // 当前耐久度
    protected float _useCooldown;      // 使用冷却时间
    protected float _range;            // 攻击范围
}
```

**核心方法**:
```csharp
// 根据配置ID初始化装备
public virtual void Init(int configId)

// 装备时调用
public virtual void OnEquip()

// 卸下时调用
public virtual void OnUnequip()

// 使用装备
public virtual void Use()
```

### 具体装备基类

#### HandEquipBase - 手部装备基类
**位置**: `Assets/Scripts/Object/Equip/Base/HandEquipBase.cs`

**特点**: 支持攻击、显示特效、处理命中

**核心方法**:
```csharp
// 显示子弹轨迹
protected virtual void ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)

// 处理攻击命中
protected virtual void HandleHit(IDamageable target, Vector3 hitPoint)
```

#### HeadEquipBase - 头部装备基类
**位置**: `Assets/Scripts/Object/Equip/Base/HeadEquipBase.cs`

**特点**: 支持材质替换、视觉效果

#### BodyEquipBase - 身体装备基类
**位置**: `Assets/Scripts/Object/Equip/Base/BodyEquipBase.cs`

**特点**: 支持材质替换、护甲效果

## 装备配置系统

### Equip.csv 配置表结构
```csv
ID,Name,EquipType,Type,Damage,Defense,Durability,DurabilityLoss,UseCooldown,Range,ModelPath,MaterialPath
```

**字段说明**:
- `ID`: 装备唯一标识
- `Name`: 装备名称
- `EquipType`: 装备类型（Weapon/Armor/Tool/Accessory）
- `Type`: 装备部位（Head/Body/Hand）
- `Damage`: 攻击力加成
- `Defense`: 防御力加成
- `Durability`: 最大耐久度
- `DurabilityLoss`: 每次使用耐久度损失
- `UseCooldown`: 使用冷却时间
- `Range`: 攻击/使用范围
- `ModelPath`: 3D模型路径
- `MaterialPath`: 材质路径

### 配置查询示例
```csharp
// 获取装备配置
var equipReader = ConfigManager.Instance.GetReader("Equip");

// 查询装备部位
EquipPart equipPart = equipReader.GetValue<EquipPart>(equipId, "Type", EquipPart.None);

// 查询装备属性
float damage = equipReader.GetValue<float>(equipId, "Damage", 0f);
float defense = equipReader.GetValue<float>(equipId, "Defense", 0f);
```

## 装备操作流程

### 装备穿戴流程
```
用户右键点击背包物品 → PackageView.TryEquipItem() → EquipManager.EquipItem() → Player.Equip() → 装备组件创建 → 状态存储 → UI刷新
```

**详细步骤**:
1. **物品验证**: 检查物品是否为装备类型
2. **部位匹配**: 验证装备部位是否匹配
3. **背包检查**: 确认背包中有足够的物品
4. **卸下旧装备**: 如果该部位已有装备，先卸下
5. **移除物品**: 从背包中移除1个装备物品
6. **装备到Player**: 调用`Player.Equip()`创建装备组件
7. **状态更新**: 将装备组件存储到`EquipManager._equippedItems`字典
8. **UI刷新**: 触发`OnEquipmentChanged`事件，更新装备槽位显示

### 装备卸下流程
```
用户点击装备槽位 → PackageView.OnEquipSlotClicked() → EquipManager.UnequipItem() → Player组件移除 → 物品回背包 → UI刷新
```

**详细步骤**:
1. **装备检查**: 确认该部位确实有装备
2. **获取装备ID**: 通过反射获取装备组件的配置ID
3. **移除状态**: 从`EquipManager._equippedItems`字典中移除
4. **移除组件**: 从Player身上移除装备组件
5. **销毁组件**: 调用`Object.Destroy(equip)`销毁装备组件
6. **放回背包**: 将装备物品添加回背包
7. **UI刷新**: 触发`OnEquipmentChanged`事件，清空装备槽位显示

## 存档系统集成

### 存档保存
```csharp
// SaveModel.cs - 保存装备数据
var equippedItemIds = EquipManager.Instance.GetAllEquippedItemIds();
foreach (var kvp in equippedItemIds)
{
    if (kvp.Value > 0)
    {
        saveData.equippedItems.Add(kvp.Value);
    }
}
```

### 存档加载
```csharp
// SaveModel.cs - 加载装备数据
EquipManager.Instance.LoadEquippedItemsFromSave(saveData.equippedItems);
```

**加载流程**:
1. 从存档获取装备ID列表
2. 调用`EquipManager.LoadEquippedItemsFromSave()`
3. 遍历装备ID，查询配置获取装备部位
4. 调用`Player.Equip()`装备到Player
5. 获取装备组件并存储到`_equippedItems`字典

## 事件系统集成

### 装备变化事件
```csharp
// EquipManager中的事件定义
public event System.Action<EquipPart, int, bool> OnEquipmentChanged;

// 事件参数说明
// equipPart: 装备部位
// equipId: 装备物品ID
// isEquipped: true表示装备，false表示卸下
```

### 事件订阅示例
```csharp
// PackageView中订阅装备变化事件
EquipManager.Instance.OnEquipmentChanged += OnEquipmentChanged;

private void OnEquipmentChanged(EquipPart equipPart, int equipId, bool isEquipped)
{
    if (isEquipped)
    {
        UpdateEquipSlot(equipPart);
        Debug.Log($"装备 {equipId} 到 {equipPart} 部位");
    }
    else
    {
        UpdateEquipSlot(equipPart);
        Debug.Log($"从 {equipPart} 部位卸下装备 {equipId}");
    }
}
```

## 性能优化

### 缓存机制
- `_equippedItems`字典缓存当前装备状态
- 避免频繁的反射操作
- 事件驱动减少不必要的UI更新

### 内存管理
- 装备组件销毁时及时清理
- 字典操作后及时更新引用
- 减少临时对象创建

## 扩展指南

### 添加新装备类型
1. **继承对应基类**:
   ```csharp
   public class NewWeapon : HandEquipBase
   {
       protected override void Use()
       {
           // 实现特定攻击逻辑
       }
   }
   ```

2. **更新配置表**: 在`Equip.csv`中添加新装备配置

3. **配置GameObject**: 添加对应的脚本组件

### 添加新装备部位
1. **扩展枚举**:
   ```csharp
   public enum EquipPart
   {
       // ... 现有部位
       Feet = 4,        // 脚部
       Back = 5         // 背部
   }
   ```

2. **更新UI**: 在`PackageView`中添加对应的装备槽位

3. **更新配置**: 确保新部位在配置表中有对应数据

## 调试和测试

### 装备状态检查
```csharp
// 检查装备状态一致性
EquipManager.Instance.SyncPlayerEquipmentState();
```

### 装备功能测试
1. **右键装备**: 在背包中右键点击装备物品
2. **装备卸下**: 点击装备槽位中的装备
3. **存档测试**: 装备后保存游戏，重新加载验证状态

### 常见问题排查
1. **装备不显示**: 检查`EquipManager._equippedItems`状态
2. **装备失败**: 检查物品类型和部位匹配
3. **存档不同步**: 检查`LoadEquippedItemsFromSave`调用时机

## 版本历史

### v3.1 (当前版本)
- **接口清理**: 删除冗余的`GetEquippedItem()`和`GetAllEquippedItems()`方法
- **数据存储优化**: `_equippedItems`直接存储装备ID，避免序列化问题
- **事件系统更新**: `OnEquipmentChanged`事件参数改为装备ID，提升性能
- **代码简化**: 移除复杂的反射操作，直接访问装备ID数据

### v3.0
- **架构重构**: 装备管理完全迁移到`EquipManager`
- **数据流向优化**: 存档 → EquipManager → Player的单向数据流
- **接口增强**: 新增装备ID获取接口，简化代码
- **职责分离**: `PackageModel`专注于背包管理，`EquipManager`管理装备状态

### v2.0
- **装备系统重构**: 引入`EquipManager`统一管理
- **事件驱动架构**: 使用事件系统实现UI同步
- **配置表驱动**: 装备属性完全由配置表控制

### v1.0
- **基础装备系统**: 支持装备穿戴、卸下、属性加成
- **装备基类体系**: 建立`EquipBase`继承体系
- **配置表支持**: 基础装备配置表结构

## 总结

装备系统采用数据驱动设计，通过`EquipManager`统一管理装备状态，实现了：

1. **清晰的架构**: 职责分离，数据流向明确
2. **高效的性能**: 缓存机制，减少反射操作
3. **灵活的扩展**: 支持新装备类型和部位
4. **稳定的状态**: 与存档系统完美集成

装备系统为游戏提供了强大的装备管理能力，支持复杂的装备交互和属性系统。 