using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // 初始化各个Model
        var clockModel = ClockModel.Instance;
    }

    void Update()
    {
        // 更新时间系统
        ClockModel.Instance.UpdateTime();
    }
}
