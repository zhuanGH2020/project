using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 输入工具类 - 提供鼠标点击检测和UI交互判断功能
/// 支持UI点击检测、世界射线检测和详细的点击对象信息打印
/// </summary>
public static class InputUtils
{
    private static Camera _mainCamera;
    private static readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
    
    // 缓存主摄像机引用以提升性能
    private static Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            return _mainCamera;
        }
    }

    // 检测鼠标是否点击在UI上
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem not found in scene. UI detection may not work properly.");
            return false;
        }

        return EventSystem.current.IsPointerOverGameObject();
    }



    /// <summary>
    /// 获取鼠标在世界空间的射线检测结果
    /// </summary>
    /// <param name="hit">射线检测结果</param>
    /// <param name="maxDistance">最大检测距离，默认无限远</param>
    /// <param name="layerMask">检测层级，默认检测所有层</param>
    /// <returns>是否检测到物体</returns>
    public static bool GetMouseWorldHit(out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = -1)
    {
        if (MainCamera == null)
        {
            Debug.LogError("Main Camera not found. Cannot perform raycast.");
            hit = default;
            return false;
        }

        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, maxDistance, layerMask);
    }

    // 获取UI射线检测结果列表，使用缓存列表避免GC分配
    public static List<RaycastResult> GetUIRaycastResults()
    {
        _raycastResults.Clear();
        
        if (EventSystem.current == null)
            return _raycastResults;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        EventSystem.current.RaycastAll(pointerData, _raycastResults);
        return _raycastResults;
    }

    // 打印点击的UI对象详细信息
    public static void PrintClickedUIObjects()
    {
        List<RaycastResult> uiResults = GetUIRaycastResults();
        if (uiResults.Count == 0)
            return;

        System.Text.StringBuilder uiInfo = new System.Text.StringBuilder();
        uiInfo.Append($"=== UI点击检测 - 共检测到 {uiResults.Count} 个UI对象 ===");
        
        for (int i = 0; i < uiResults.Count; i++)
        {
            RaycastResult result = uiResults[i];
            GameObject uiObject = result.gameObject;
            string layerName = LayerMask.LayerToName(uiObject.layer);
            
            uiInfo.Append($" | [UI-{i}] {uiObject.name} (Tag: {uiObject.tag}, Layer: {layerName}, Module: {result.module?.GetType().Name})");
        }
        
        Debug.Log(uiInfo.ToString());
    }

    // 打印点击的UI对象路径信息
    public static void PrintClickedUIPath()
    {
        List<RaycastResult> uiResults = GetUIRaycastResults();
        if (uiResults.Count == 0)
            return;

        // 只获取最上面的那个UI对象（第一个）
        RaycastResult topResult = uiResults[0];
        GameObject topUIObject = topResult.gameObject;
        
        // 如果点击的是TMP相关组件，使用其父级对象
        if (topUIObject.name.Contains("TMP") && topUIObject.transform.parent != null)
        {
            topUIObject = topUIObject.transform.parent.gameObject;
        }
        
        string objectPath = GetGameObjectPath(topUIObject);
        
        Debug.Log($"UI路径检测 Path: {objectPath}");
    }

    // 获取GameObject的完整层级路径
    public static string GetGameObjectPath(GameObject obj)
    {
        if (obj == null)
            return string.Empty;
            
        System.Text.StringBuilder path = new System.Text.StringBuilder();
        Transform current = obj.transform;
        
        // 从当前对象向上遍历到根节点
        while (current != null)
        {
            if (path.Length > 0)
                path.Insert(0, "/");
            path.Insert(0, current.name);
            current = current.parent;
        }
        
        return path.ToString();
    }

    // 打印点击的世界GameObject详细信息
    public static void PrintClickedWorldObject(RaycastHit hit)
    {
        GameObject worldObject = hit.collider.gameObject;
        string layerName = LayerMask.LayerToName(worldObject.layer);
        string objectPath = GetGameObjectPath(worldObject);
        
        // 打印组件信息，避免不必要的数组分配
        Component[] components = worldObject.GetComponents<Component>();
        System.Text.StringBuilder componentNames = new System.Text.StringBuilder();
        for (int i = 0; i < components.Length; i++)
        {
            if (i > 0) componentNames.Append(", ");
            componentNames.Append(components[i].GetType().Name);
        }
        
        Debug.Log($"=== 世界点击检测 === GameObject: {worldObject.name} | Path: {objectPath} | Tag: {worldObject.tag} | Layer: {layerName} | Position: {hit.point} | Distance: {hit.distance:F2} | Collider: {hit.collider.GetType().Name} | Components ({components.Length}): {componentNames}");
    }

    // 执行安全的鼠标点击检测，只有在不点击UI时才执行游戏世界回调，点击UI时打印UI信息
    public static void HandleSafeMouseClick(System.Action onClickWorld)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (IsPointerOverUI())
        {
            PrintClickedUIObjects();
        }
        else
        {
            onClickWorld?.Invoke();
        }
    }

    // 综合的世界点击处理方法，自动判断UI/世界点击并打印相应信息，然后执行对应回调
    public static void HandleWorldClick(System.Action<RaycastHit> onHitWorld, System.Action onClickEmpty = null, int layerMask = -1)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (IsPointerOverUI())
        {
            PrintClickedUIObjects();
            return;
        }

        if (GetMouseWorldHit(out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            PrintClickedWorldObject(hit);
            onHitWorld?.Invoke(hit);
        }
        else
        {
            onClickEmpty?.Invoke();
        }
    }

    // 完整的点击分析方法，执行详细的点击检测并打印所有相关信息
    public static bool AnalyzeClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return false;
        
        // 检查UI点击
        if (IsPointerOverUI())
        {
            PrintClickedUIObjects();
            return true;
        }

        // 检查世界点击
        if (GetMouseWorldHit(out RaycastHit hit))
        {
            PrintClickedWorldObject(hit);
            return false;
        }
        return false;
    }

    // 清理缓存的摄像机引用，在场景切换时调用以避免空引用
    public static void ClearCachedReferences()
    {
        _mainCamera = null;
    }

    /// <summary>
    /// 角色移动专用的右键点击检测方法
    /// 只有在不点击UI时才执行移动回调，使用鼠标右键
    /// </summary>
    public static void HandlePlayerMoveClick(System.Action onClickMove)
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        if (IsPointerOverUI())
        {
            // 右键点击UI时不打印信息，静默处理
            return;
        }
        
        onClickMove?.Invoke();
    }

    /// <summary>
    /// 角色移动专用的世界点击处理方法
    /// 使用鼠标右键进行移动控制，自动过滤UI点击
    /// </summary>
    public static void HandlePlayerMoveWorldClick(System.Action<RaycastHit> onHitMove, System.Action onClickEmptyMove = null, int layerMask = -1)
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        if (IsPointerOverUI())
        {
            // 右键点击UI时静默处理，不执行移动
            return;
        }

        if (GetMouseWorldHit(out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            onHitMove?.Invoke(hit);
        }
        else
        {
            onClickEmptyMove?.Invoke();
        }
    }
} 