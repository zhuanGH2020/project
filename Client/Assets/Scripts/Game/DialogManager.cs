using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public Camera facing_camera;
    [Header("文本设置")]
    public string text_content = "example text";
    public Color textColor = Color.white;
    public float fontSize = 12f;
    public Vector3 textOffset = new Vector3(0, 2f, 0); // 文本相对于父对象的偏移

    private TextMeshPro tmpText;

    void Start()
    {
        // 如果没有指定摄像机，使用主摄像机
        if (facing_camera == null)
        {
            facing_camera = Camera.main;
        }

        // 创建文本对象
        CreateTextObject();
    }

    void CreateTextObject()
    {
        // 创建子对象并添加TextMeshPro组件
        GameObject textObj = new GameObject("FloatingText3D");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = textOffset;
        textObj.transform.localRotation = Quaternion.identity;

        tmpText = textObj.AddComponent<TextMeshPro>();
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Font/zh_cn_body");
        if (font != null)
        {
            tmpText.font = font;
        }
        else
        {
            Debug.LogWarning("无法加载中文字体，使用默认字体");
        }
        // 配置文本属性
        tmpText.text = text_content;
        tmpText.color = textColor;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.enableWordWrapping = false;

        // 设置渲染排序（避免被其他物体遮挡）
        tmpText.sortingOrder = 100;
        tmpText.renderer.sortingOrder = 100;
    }
    void Update()
    {
        test();
    }
    void LateUpdate()
    {
        if (facing_camera != null && tmpText != null)
        {
            // 使文本始终面向摄像机
            Vector3 lookDirection = transform.position - facing_camera.transform.position;
            tmpText.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    // 公开方法用于更新文本内容
    public void UpdateText(string newText)
    {
        if (tmpText != null)
            tmpText.text = newText;
    }

    // 公开方法更新文本颜色
    public void UpdateColor(Color newColor)
    {
        if (tmpText != null)
            tmpText.color = newColor;
    }

    // 公开方法更新文本位置偏移
    public void UpdateOffset(Vector3 newOffset)
    {
        if (tmpText != null)
            tmpText.transform.localPosition = newOffset;
    }

    void test()
    {
        Monster monsterComponent = GetComponent<Monster>();

        if (monsterComponent != null)
        {
            // 如果是怪物，显示怪物信息
            UpdateText("我是怪物");
        }
        else
        {
            int currentDay = ClockModel.Instance.ClockDay;
            TimeOfDay currentTime = ClockModel.Instance.CurrentTimeOfDay;

            string directionText;
            Vector3 forward = transform.forward;

            if (forward == Vector3.zero)
            {
                directionText = "原地不动";
            }
            else
            {
                // 归一化方向向量
                Vector3 normalizedDirection = forward.normalized;

                // 计算与四个主要方向的点积
                float eastDot = Vector3.Dot(normalizedDirection, Vector3.right);    // 东（+X）
                float westDot = Vector3.Dot(normalizedDirection, Vector3.left);     // 西（-X）
                float northDot = Vector3.Dot(normalizedDirection, Vector3.forward); // 北（+Z）
                float southDot = Vector3.Dot(normalizedDirection, Vector3.back);    // 南（-Z）

                // 找出最大的点积值，确定主要方向
                float maxDot = Mathf.Max(eastDot, westDot, northDot, southDot);

                if (maxDot == eastDot)
                    directionText = "东";
                else if (maxDot == westDot)
                    directionText = "西";
                else if (maxDot == northDot)
                    directionText = "北";
                else
                    directionText = "南";
            }

            // 构建基础消息
            string message = $"现在是第{currentDay}天了，我在向{directionText}走";

            // 如果是黑天（黄昏或夜晚），添加害怕信息
            if (currentTime == TimeOfDay.Dusk || currentTime == TimeOfDay.Night)
            {
                message += "\n黑天了，我很害怕";
            }
            UpdateText(message);
        }
    }
}
