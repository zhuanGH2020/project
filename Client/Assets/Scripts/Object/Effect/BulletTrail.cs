using UnityEngine;
using System.Collections;

/// <summary>
/// 使用LineRenderer实现的子弹轨迹特效
/// </summary>
public class BulletTrail : MonoBehaviour
{
    [Header("轨迹设置")]
    [SerializeField] private float _trailDuration = 0.2f;    // 轨迹显示持续时间
    [SerializeField] private AnimationCurve _widthCurve = AnimationCurve.Linear(0, 1, 1, 0); // 宽度动画曲线
    [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.Linear(0, 1, 1, 0); // 透明度动画曲线
    
    private LineRenderer _lineRenderer;
    private Material _trailMaterial;
    private Color _originalColor;
    private float _originalWidth;
    
    /// <summary>
    /// 初始化并显示子弹轨迹
    /// </summary>
    /// <param name="startPoint">轨迹起点（枪口位置）</param>
    /// <param name="endPoint">轨迹终点（命中点或最大射程）</param>
    /// <param name="trailMaterial">轨迹材质（可选）</param>
    public void ShowTrail(Vector3 startPoint, Vector3 endPoint, Material trailMaterial = null)
    {
        SetupLineRenderer(trailMaterial);
        
        // 设置轨迹点
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
        
        // 开始淡出动画
        StartCoroutine(FadeOutTrail());
        
        Debug.Log($"[BulletTrail] Showing trail from {startPoint} to {endPoint}");
    }
    
    private void SetupLineRenderer(Material trailMaterial)
    {
        // Get or create LineRenderer
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Setup material
        if (trailMaterial != null)
        {
            _trailMaterial = trailMaterial;
        }
        else
        {
            // Create default material if none provided
            _trailMaterial = CreateDefaultTrailMaterial();
        }
        
        _lineRenderer.material = _trailMaterial;
        _originalColor = _trailMaterial.color;
        
        // Setup LineRenderer properties
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.02f;
        _originalWidth = _lineRenderer.startWidth;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.sortingOrder = 10;
        
        // Disable shadows and lighting
        _lineRenderer.receiveShadows = false;
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    
    private Material CreateDefaultTrailMaterial()
    {
        // Create a simple unlit material for the trail
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.yellow;
        return mat;
    }
    
    private IEnumerator FadeOutTrail()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < _trailDuration)
        {
            float normalizedTime = elapsedTime / _trailDuration;
            
            // Animate width
            float widthMultiplier = _widthCurve.Evaluate(normalizedTime);
            _lineRenderer.startWidth = _originalWidth * widthMultiplier;
            _lineRenderer.endWidth = _originalWidth * 0.5f * widthMultiplier;
            
            // Animate alpha
            float alpha = _alphaCurve.Evaluate(normalizedTime);
            Color currentColor = _originalColor;
            currentColor.a = alpha;
            _trailMaterial.color = currentColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Clean up
        DestroyTrail();
    }
    
    private void DestroyTrail()
    {
        // Destroy the created material to prevent memory leaks
        if (_trailMaterial != null)
        {
            Destroy(_trailMaterial);
        }
        
        // Destroy this GameObject
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 运行时创建子弹轨迹特效
    /// </summary>
    /// <param name="startPoint">轨迹起点</param>
    /// <param name="endPoint">轨迹终点</param>
    /// <param name="trailMaterial">可选的轨迹材质</param>
    /// <returns>创建的BulletTrail组件</returns>
    public static BulletTrail CreateTrail(Vector3 startPoint, Vector3 endPoint, Material trailMaterial = null)
    {
        GameObject trailObj = new GameObject("BulletTrail");
        BulletTrail trail = trailObj.AddComponent<BulletTrail>();
        trail.ShowTrail(startPoint, endPoint, trailMaterial);
        return trail;
    }
} 