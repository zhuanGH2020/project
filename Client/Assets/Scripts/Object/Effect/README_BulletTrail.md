# 子弹轨迹线系统使用说明

## 功能概述
使用LineRenderer实现的子弹轨迹线效果，适用于射线类武器（如UZI、Shotgun）。

## 核心组件

### BulletTrail.cs
- **位置**: `Assets/Scripts/Object/Effect/BulletTrail.cs`
- **功能**: 管理LineRenderer的创建、动画和销毁
- **特点**: 自动淡出动画、内存安全的材质管理

### 集成方式

#### HandEquipBase.cs
添加了 `ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)` 方法，所有手部武器都可以调用。

#### 武器实现
- **Uzi**: 每次射击显示一条轨迹线
- **Shotgun**: 每发子弹都显示独立的轨迹线（散射效果）

## 使用方法

### 基本调用
```csharp
// 在武器的Use()方法中
Vector3 startPoint = GetAttackPoint();  // 枪口位置
Vector3 endPoint = hit ? hitInfo.point : startPoint + direction * range;
ShowBulletTrail(startPoint, endPoint);
```

### 自定义材质
```csharp
// 在HandEquipBase中设置_trailMaterial
[SerializeField] protected Material _trailMaterial;
```

## 材质设置建议

### 推荐材质属性
- **Shader**: Unlit或Sprites/Default
- **颜色**: 明亮色彩（黄色、白色、红色）
- **混合模式**: Additive（加法混合）
- **透明度**: 支持Alpha通道

### Unity材质创建步骤
1. 在Project窗口右键 → Create → Material
2. 命名为 `BulletTrail_Material`
3. 设置Shader为 `Unlit/Color` 或 `Sprites/Default`
4. 调整颜色和透明度
5. 将材质拖拽到武器的Trail Material字段

## 性能特点
- ✅ 自动回收：轨迹线会在0.2秒后自动销毁
- ✅ 内存安全：自动释放创建的材质
- ✅ 高性能：使用LineRenderer，比粒子系统更轻量
- ✅ 可配置：支持自定义材质和动画曲线

## 效果调整

### BulletTrail组件参数
- `_trailDuration`: 轨迹线持续时间（默认0.2秒）
- `_widthCurve`: 宽度动画曲线
- `_alphaCurve`: 透明度动画曲线

### 视觉效果调整
- 修改LineRenderer的startWidth/endWidth改变粗细
- 调整AnimationCurve改变淡出效果
- 更换材质改变颜色和发光效果 