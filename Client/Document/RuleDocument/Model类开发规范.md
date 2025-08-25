# Model类开发规范

## 核心要求

1. **必须实现单例模式**
2. **数据变化必须发布事件**（使用EventManager）
3. **类名以"Model"结尾**

## 单例实现模板

```csharp
public class XXXModel
{
    private static XXXModel _instance;
    public static XXXModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new XXXModel();
            }
            return _instance;
        }
    }

    private XXXModel()
    {
        // 初始化数据
    }

    // 数据变化时必须发布事件
    private void OnDataChanged()
    {
        EventManager.Instance.Publish(new YourDataChangeEvent());
    }
}
```

## 代码结构顺序

1. 单例实现
2. 私有字段
3. 公共属性
4. 公共方法
5. 私有方法

## GameMain集成

Model不继承MonoBehaviour，需要GameMain驱动：

```csharp
public class GameMain : MonoBehaviour
{
    void Start()
    {
        // 初始化各个Model
        var xxxModel = XxxModel.Instance;
    }

    void Update()
    {
        // 驱动需要更新的Model
        XxxModel.Instance.UpdateTime();
    }
}
```

## 参考示例

**正确示例**：
- `Assets/Scripts/UI/Clock/ClockModel.cs` (行1-123)

**GameMain集成**：`Assets/Scripts/GameMain.cs`