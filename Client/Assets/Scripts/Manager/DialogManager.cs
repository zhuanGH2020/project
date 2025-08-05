using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 对话框类 - 管理单个3D文本对话框
/// </summary>
public class Dialog
{
    public int Id { get; private set; }
    public GameObject TextObject { get; private set; }
    public TextMeshPro TmpText { get; private set; }
    public Transform Parent { get; private set; }
    public bool IsActive { get; private set; }
    public float LifeTime { get; private set; } // 生命周期，-1表示永久
    private float _currentTime;

    /// <summary>
    /// 创建对话框
    /// </summary>
    /// <param name="id">对话框ID</param>
    /// <param name="parent">父对象Transform</param>
    /// <param name="text">显示文本</param>
    /// <param name="offset">相对父对象的偏移位置</param>
    /// <param name="lifeTime">生命周期（秒），-1表示永久</param>
    public Dialog(int id, Transform parent, string text, Vector3 offset, float lifeTime = -1)
    {
        Id = id;
        Parent = parent;
        LifeTime = lifeTime;
        _currentTime = 0f;
        IsActive = true;

        CreateTextObject(text, offset);
    }

    private void CreateTextObject(string text, Vector3 offset)
    {
        // 创建子对象并添加TextMeshPro组件
        TextObject = new GameObject($"Dialog_{Id}");
        TextObject.transform.SetParent(Parent);
        TextObject.transform.localPosition = offset;
        TextObject.transform.localRotation = Quaternion.identity;

        TmpText = TextObject.AddComponent<TextMeshPro>();
        
        // 加载中文字体
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Font/zh_cn_body");
        if (font != null)
        {
            TmpText.font = font;
        }
        else
        {
            Debug.LogWarning("[Dialog] 无法加载中文字体，使用默认字体");
        }

        // 配置文本属性
        TmpText.text = text;
        TmpText.color = Color.white;
        TmpText.fontSize = 5f;
        TmpText.alignment = TextAlignmentOptions.Center;
        TmpText.enableWordWrapping = false;

        // 设置渲染排序（避免被其他物体遮挡）
        TmpText.sortingOrder = 100;
        TmpText.renderer.sortingOrder = 100;
    }

    /// <summary>
    /// 更新文本内容
    /// </summary>
    public void UpdateText(string newText)
    {
        if (TmpText != null)
            TmpText.text = newText;
    }

    /// <summary>
    /// 更新文本颜色
    /// </summary>
    public void UpdateColor(Color newColor)
    {
        if (TmpText != null)
            TmpText.color = newColor;
    }

    /// <summary>
    /// 更新文本位置偏移
    /// </summary>
    public void UpdateOffset(Vector3 newOffset)
    {
        if (TextObject != null)
            TextObject.transform.localPosition = newOffset;
    }

    /// <summary>
    /// 更新文本大小
    /// </summary>
    public void UpdateFontSize(float fontSize)
    {
        if (TmpText != null)
            TmpText.fontSize = fontSize;
    }

    /// <summary>
    /// 设置显示状态
    /// </summary>
    public void SetActive(bool active)
    {
        IsActive = active;
        if (TextObject != null)
            TextObject.SetActive(active);
    }

    /// <summary>
    /// 更新生命周期
    /// </summary>
    public bool UpdateLifeTime(float deltaTime)
    {
        if (LifeTime <= 0) return true; // 永久对话框

        _currentTime += deltaTime;
        return _currentTime < LifeTime;
    }

    /// <summary>
    /// 使文本面向摄像机
    /// </summary>
    public void LookAtCamera(Camera camera)
    {
        if (camera != null && TmpText != null && IsActive)
        {
            Vector3 lookDirection = Parent.position - camera.transform.position;
            TmpText.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    /// <summary>
    /// 销毁对话框
    /// </summary>
    public void Destroy()
    {
        if (TextObject != null)
        {
            Object.Destroy(TextObject);
            TextObject = null;
            TmpText = null;
        }
        IsActive = false;
    }
}

/// <summary>
/// 对话框管理器 - 统一管理所有3D文本对话框
/// </summary>
public class DialogManager
{
    private static DialogManager _instance;
    public static DialogManager Instance => _instance ??= new DialogManager();

    private Dictionary<int, Dialog> _dialogs = new Dictionary<int, Dialog>();
    private Camera _targetCamera;
    private int _nextDialogId = 1;
    private bool _globalVisible = true;

    private DialogManager()
    {
        // 获取主摄像机
        _targetCamera = Camera.main;
    }

    /// <summary>
    /// 创建对话框
    /// </summary>
    /// <param name="parent">父对象Transform</param>
    /// <param name="text">显示文本</param>
    /// <param name="offset">相对父对象的偏移位置</param>
    /// <param name="lifeTime">生命周期（秒），-1表示永久</param>
    /// <returns>对话框ID</returns>
    public int CreateDialog(Transform parent, string text, Vector3 offset = default, float lifeTime = -1)
    {
        if (parent == null)
        {
            Debug.LogError("[DialogManager] 创建对话框失败：parent不能为null");
            return -1;
        }

        if (offset == default)
            offset = new Vector3(0, 2f, 0);

        int dialogId = _nextDialogId++;
        Dialog dialog = new Dialog(dialogId, parent, text, offset, lifeTime);
        dialog.SetActive(_globalVisible);
        _dialogs[dialogId] = dialog;

        Debug.Log($"[DialogManager] 创建对话框 ID:{dialogId}, 文本:'{text}'");
        return dialogId;
    }

    /// <summary>
    /// 获取对话框
    /// </summary>
    public Dialog GetDialog(int dialogId)
    {
        _dialogs.TryGetValue(dialogId, out Dialog dialog);
        return dialog;
    }

    /// <summary>
    /// 更新指定对话框的文本
    /// </summary>
    public void UpdateDialogText(int dialogId, string newText)
    {
        if (_dialogs.TryGetValue(dialogId, out Dialog dialog))
        {
            dialog.UpdateText(newText);
        }
    }

    /// <summary>
    /// 销毁对话框
    /// </summary>
    public void DestroyDialog(int dialogId)
    {
        if (_dialogs.TryGetValue(dialogId, out Dialog dialog))
        {
            dialog.Destroy();
            _dialogs.Remove(dialogId);
            Debug.Log($"[DialogManager] 销毁对话框 ID:{dialogId}");
        }
    }

    /// <summary>
    /// 销毁指定父对象下的所有对话框
    /// </summary>
    public void DestroyDialogsByParent(Transform parent)
    {
        List<int> toRemove = new List<int>();
        foreach (var kvp in _dialogs)
        {
            if (kvp.Value.Parent == parent)
            {
                kvp.Value.Destroy();
                toRemove.Add(kvp.Key);
            }
        }

        foreach (int id in toRemove)
        {
            _dialogs.Remove(id);
        }

        if (toRemove.Count > 0)
        {
            Debug.Log($"[DialogManager] 销毁 {toRemove.Count} 个对话框（父对象：{parent.name}）");
        }
    }

    /// <summary>
    /// 设置全局显示状态
    /// </summary>
    public void SetGlobalVisible(bool visible)
    {
        _globalVisible = visible;
        foreach (var dialog in _dialogs.Values)
        {
            dialog.SetActive(visible);
        }
        Debug.Log($"[DialogManager] 设置全局显示状态：{visible}");
    }

    /// <summary>
    /// 设置目标摄像机
    /// </summary>
    public void SetTargetCamera(Camera camera)
    {
        _targetCamera = camera;
    }

    /// <summary>
    /// 获取当前对话框数量
    /// </summary>
    public int GetDialogCount()
    {
        return _dialogs.Count;
    }

    /// <summary>
    /// 清理所有对话框
    /// </summary>
    public void ClearAllDialogs()
    {
        foreach (var dialog in _dialogs.Values)
        {
            dialog.Destroy();
        }
        _dialogs.Clear();
        Debug.Log("[DialogManager] 清理所有对话框");
    }

    /// <summary>
    /// 更新所有对话框 - 供GameMain调用
    /// </summary>
    public void Update()
    {
        if (_dialogs.Count == 0) return;

        List<int> toRemove = new List<int>();

        foreach (var kvp in _dialogs)
        {
            Dialog dialog = kvp.Value;

            // 更新生命周期
            if (!dialog.UpdateLifeTime(Time.deltaTime))
            {
                toRemove.Add(kvp.Key);
                continue;
            }

            // 更新面向摄像机
            dialog.LookAtCamera(_targetCamera);
        }

        // 移除过期的对话框
        foreach (int id in toRemove)
        {
            DestroyDialog(id);
        }
    }

    /// <summary>
    /// 清理资源 - 供GameMain在OnDestroy时调用
    /// </summary>
    public void Cleanup()
    {
        ClearAllDialogs();
        Debug.Log("[DialogManager] 清理完成");
    }
}