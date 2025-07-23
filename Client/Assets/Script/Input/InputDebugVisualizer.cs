using UnityEngine;
using System.Collections.Generic;

namespace InputSystem
{
    /// <summary>
    /// 输入调试可视化器
    /// 提供实时的输入状态可视化功能
    /// </summary>
    public class InputDebugVisualizer : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// 可视化是否启用
        /// </summary>
        public bool IsVisualizationEnabled { get; private set; } = true;
        #endregion

        #region Serialized Fields
        [Header("=== 可视化设置 ===")]
        [SerializeField] private bool enableInputVector = true;
        [SerializeField] private bool enableInputHistory = true;
        [SerializeField] private bool enableInputMagnitude = true;
        [SerializeField] private bool enableDirectionIndicator = true;

        [Header("=== 显示位置 ===")]
        [SerializeField] private Vector2 displayPosition = new Vector2(50, 50);
        [SerializeField] private Vector2 displaySize = new Vector2(200, 200);
        [SerializeField] private bool centerOnScreen = false;

        [Header("=== 视觉样式 ===")]
        [SerializeField] private Color inputVectorColor = Color.green;
        [SerializeField] private Color historyColor = Color.yellow;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.3f);
        [SerializeField] private Color gridColor = new Color(1, 1, 1, 0.2f);
        [SerializeField] private float lineWidth = 2f;

        [Header("=== 历史轨迹 ===")]
        [SerializeField] private int historyLength = 50;
        [SerializeField] private float historyFadeTime = 2f;
        [SerializeField] private bool showHistoryTrails = true;

        [Header("=== 3D可视化 ===")]
        [SerializeField] private bool enable3DVisualization = true;
        [SerializeField] private Transform visualizationRoot;
        [SerializeField] private GameObject inputIndicatorPrefab;
        [SerializeField] private float indicatorScale = 1f;
        #endregion

        #region Private Fields
        // 配置
        private InputConfiguration config;

        // 输入数据
        private Vector2 currentInput = Vector2.zero;
        private List<InputHistoryPoint> inputHistory = new List<InputHistoryPoint>();
        private float inputMagnitude = 0f;
        private float inputAngle = 0f;

        // GUI样式
        private GUIStyle backgroundStyle;
        private GUIStyle textStyle;
        private bool stylesInitialized = false;

        // 3D可视化
        private GameObject currentIndicator;
        private LineRenderer historyLineRenderer;
        private Material lineMaterial;

        // 性能优化
        private float lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.016f; // 60 FPS

        // 坐标系统
        private Rect visualizationRect;
        private Vector2 centerPoint;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            SetupVisualizationRect();
            Initialize3DVisualization();
        }

        private void Update()
        {
            if (IsVisualizationEnabled && Time.time - lastUpdateTime > UPDATE_INTERVAL)
            {
                UpdateVisualization();
                lastUpdateTime = Time.time;
            }
        }

        private void OnGUI()
        {
            if (IsVisualizationEnabled)
            {
                DrawVisualization();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 初始化可视化器
        /// </summary>
        public void Initialize()
        {
            inputHistory = new List<InputHistoryPoint>(historyLength);
            InitializeGUIStyles();
            CreateLineMaterial();

            Debug.Log("[InputDebugVisualizer] 输入调试可视化器初始化完成");
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        public void SetConfiguration(InputConfiguration newConfig)
        {
            config = newConfig;
            
            if (config != null)
            {
                IsVisualizationEnabled = config.enableInputVisualization;
            }
        }

        /// <summary>
        /// 设置可视化矩形
        /// </summary>
        private void SetupVisualizationRect()
        {
            if (centerOnScreen)
            {
                visualizationRect = new Rect(
                    (Screen.width - displaySize.x) * 0.5f,
                    (Screen.height - displaySize.y) * 0.5f,
                    displaySize.x,
                    displaySize.y
                );
            }
            else
            {
                visualizationRect = new Rect(displayPosition.x, displayPosition.y, displaySize.x, displaySize.y);
            }

            centerPoint = new Vector2(
                visualizationRect.x + visualizationRect.width * 0.5f,
                visualizationRect.y + visualizationRect.height * 0.5f
            );
        }

        /// <summary>
        /// 初始化GUI样式
        /// </summary>
        private void InitializeGUIStyles()
        {
            if (stylesInitialized) return;

            backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = CreateColorTexture(backgroundColor);

            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 12;

            stylesInitialized = true;
        }

        /// <summary>
        /// 初始化3D可视化
        /// </summary>
        private void Initialize3DVisualization()
        {
            if (!enable3DVisualization) return;

            // 创建可视化根节点
            if (visualizationRoot == null)
            {
                GameObject rootObj = new GameObject("InputVisualizationRoot");
                rootObj.transform.position = transform.position + Vector3.up * 0.1f;
                visualizationRoot = rootObj.transform;
                visualizationRoot.SetParent(transform);
            }

            // 创建历史轨迹线渲染器
            CreateHistoryLineRenderer();
        }

        /// <summary>
        /// 创建历史轨迹线渲染器
        /// </summary>
        private void CreateHistoryLineRenderer()
        {
            GameObject lineObj = new GameObject("HistoryLineRenderer");
            lineObj.transform.SetParent(visualizationRoot);
            
            historyLineRenderer = lineObj.AddComponent<LineRenderer>();
            historyLineRenderer.material = lineMaterial;
            historyLineRenderer.color = historyColor;
            historyLineRenderer.startWidth = 0.02f;
            historyLineRenderer.endWidth = 0.01f;
            historyLineRenderer.positionCount = 0;
            historyLineRenderer.useWorldSpace = false;
        }

        /// <summary>
        /// 创建线条材质
        /// </summary>
        private void CreateLineMaterial()
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = historyColor;
        }
        #endregion

        #region Visualization Updates
        /// <summary>
        /// 更新输入可视化
        /// </summary>
        public void UpdateInputVisualization(Vector2 inputVector)
        {
            currentInput = inputVector;
            inputMagnitude = inputVector.magnitude;
            inputAngle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg;

            // 记录输入历史
            if (inputMagnitude > 0.01f)
            {
                RecordInputHistory(inputVector);
            }

            // 更新3D可视化
            if (enable3DVisualization)
            {
                Update3DVisualization();
            }
        }

        /// <summary>
        /// 更新可视化
        /// </summary>
        private void UpdateVisualization()
        {
            // 更新历史点的生命周期
            UpdateInputHistoryLifetime();
            
            // 更新3D指示器
            if (enable3DVisualization && currentIndicator != null)
            {
                UpdateInputIndicator();
            }
        }

        /// <summary>
        /// 记录输入历史
        /// </summary>
        private void RecordInputHistory(Vector2 input)
        {
            var historyPoint = new InputHistoryPoint
            {
                inputVector = input,
                timestamp = Time.time,
                magnitude = input.magnitude
            };

            inputHistory.Add(historyPoint);

            // 限制历史长度
            while (inputHistory.Count > historyLength)
            {
                inputHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// 更新输入历史生命周期
        /// </summary>
        private void UpdateInputHistoryLifetime()
        {
            float currentTime = Time.time;
            
            for (int i = inputHistory.Count - 1; i >= 0; i--)
            {
                if (currentTime - inputHistory[i].timestamp > historyFadeTime)
                {
                    inputHistory.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 更新3D可视化
        /// </summary>
        private void Update3DVisualization()
        {
            if (visualizationRoot == null) return;

            // 更新输入指示器
            UpdateInputIndicator();
            
            // 更新历史轨迹
            if (showHistoryTrails)
            {
                UpdateHistoryTrails();
            }
        }

        /// <summary>
        /// 更新输入指示器
        /// </summary>
        private void UpdateInputIndicator()
        {
            if (inputMagnitude > 0.01f)
            {
                // 创建或更新指示器
                if (currentIndicator == null && inputIndicatorPrefab != null)
                {
                    currentIndicator = Instantiate(inputIndicatorPrefab, visualizationRoot);
                }

                if (currentIndicator != null)
                {
                    // 更新位置和缩放
                    Vector3 indicatorPosition = new Vector3(currentInput.x, 0, currentInput.y) * indicatorScale;
                    currentIndicator.transform.localPosition = indicatorPosition;
                    currentIndicator.transform.localScale = Vector3.one * inputMagnitude;
                    currentIndicator.SetActive(true);
                }
            }
            else if (currentIndicator != null)
            {
                currentIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// 更新历史轨迹
        /// </summary>
        private void UpdateHistoryTrails()
        {
            if (historyLineRenderer == null || inputHistory.Count < 2) return;

            // 设置线段点数
            historyLineRenderer.positionCount = inputHistory.Count;

            // 更新线段位置
            for (int i = 0; i < inputHistory.Count; i++)
            {
                Vector2 input = inputHistory[i].inputVector;
                Vector3 position = new Vector3(input.x, 0, input.y) * indicatorScale;
                historyLineRenderer.SetPosition(i, position);
            }

            // 更新透明度渐变
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

            colorKeys[0] = new GradientColorKey(historyColor, 0f);
            colorKeys[1] = new GradientColorKey(historyColor, 1f);
            
            alphaKeys[0] = new GradientAlphaKey(0.2f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            gradient.SetKeys(colorKeys, alphaKeys);
            historyLineRenderer.colorGradient = gradient;
        }
        #endregion

        #region GUI Drawing
        /// <summary>
        /// 绘制可视化
        /// </summary>
        private void DrawVisualization()
        {
            if (!stylesInitialized)
            {
                InitializeGUIStyles();
            }

            // 绘制背景
            GUI.Box(visualizationRect, "", backgroundStyle);

            // 绘制网格
            DrawGrid();

            // 绘制输入向量
            if (enableInputVector)
            {
                DrawInputVector();
            }

            // 绘制输入历史
            if (enableInputHistory && showHistoryTrails)
            {
                DrawInputHistory();
            }

            // 绘制方向指示器
            if (enableDirectionIndicator)
            {
                DrawDirectionIndicator();
            }

            // 绘制输入信息
            DrawInputInfo();
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawGrid()
        {
            float gridSize = 20f;
            int gridLines = Mathf.FloorToInt(displaySize.x / gridSize);

            // 绘制垂直线
            for (int i = 0; i <= gridLines; i++)
            {
                float x = visualizationRect.x + i * gridSize;
                DrawLine(
                    new Vector2(x, visualizationRect.y),
                    new Vector2(x, visualizationRect.y + visualizationRect.height),
                    gridColor
                );
            }

            // 绘制水平线
            gridLines = Mathf.FloorToInt(displaySize.y / gridSize);
            for (int i = 0; i <= gridLines; i++)
            {
                float y = visualizationRect.y + i * gridSize;
                DrawLine(
                    new Vector2(visualizationRect.x, y),
                    new Vector2(visualizationRect.x + visualizationRect.width, y),
                    gridColor
                );
            }

            // 绘制中心十字
            DrawLine(
                new Vector2(centerPoint.x, visualizationRect.y),
                new Vector2(centerPoint.x, visualizationRect.y + visualizationRect.height),
                Color.white
            );
            DrawLine(
                new Vector2(visualizationRect.x, centerPoint.y),
                new Vector2(visualizationRect.x + visualizationRect.width, centerPoint.y),
                Color.white
            );
        }

        /// <summary>
        /// 绘制输入向量
        /// </summary>
        private void DrawInputVector()
        {
            if (inputMagnitude > 0.01f)
            {
                Vector2 endPoint = centerPoint + currentInput * (displaySize.x * 0.4f);
                
                // 绘制向量线
                DrawLine(centerPoint, endPoint, inputVectorColor, lineWidth);
                
                // 绘制箭头
                DrawArrow(centerPoint, endPoint, inputVectorColor);
            }
        }

        /// <summary>
        /// 绘制输入历史
        /// </summary>
        private void DrawInputHistory()
        {
            if (inputHistory.Count < 2) return;

            float currentTime = Time.time;
            
            for (int i = 0; i < inputHistory.Count - 1; i++)
            {
                var point1 = inputHistory[i];
                var point2 = inputHistory[i + 1];
                
                // 计算透明度
                float age1 = currentTime - point1.timestamp;
                float age2 = currentTime - point2.timestamp;
                float alpha1 = 1f - (age1 / historyFadeTime);
                float alpha2 = 1f - (age2 / historyFadeTime);
                
                if (alpha1 > 0 && alpha2 > 0)
                {
                    Vector2 pos1 = centerPoint + point1.inputVector * (displaySize.x * 0.4f);
                    Vector2 pos2 = centerPoint + point2.inputVector * (displaySize.x * 0.4f);
                    
                    Color color = historyColor;
                    color.a = (alpha1 + alpha2) * 0.5f;
                    
                    DrawLine(pos1, pos2, color);
                }
            }
        }

        /// <summary>
        /// 绘制方向指示器
        /// </summary>
        private void DrawDirectionIndicator()
        {
            if (inputMagnitude > 0.01f)
            {
                // 绘制方向扇形
                float angleRange = 15f;
                float startAngle = inputAngle - angleRange;
                float endAngle = inputAngle + angleRange;
                
                DrawArc(centerPoint, displaySize.x * 0.3f, startAngle, endAngle, inputVectorColor);
            }
        }

        /// <summary>
        /// 绘制输入信息
        /// </summary>
        private void DrawInputInfo()
        {
            Rect infoRect = new Rect(
                visualizationRect.x + 5,
                visualizationRect.y + visualizationRect.height - 60,
                visualizationRect.width - 10,
                55
            );

            GUILayout.BeginArea(infoRect);
            
            if (enableInputMagnitude)
            {
                GUILayout.Label($"幅度: {inputMagnitude:F3}", textStyle);
                GUILayout.Label($"角度: {inputAngle:F1}°", textStyle);
            }
            
            GUILayout.Label($"输入: ({currentInput.x:F2}, {currentInput.y:F2})", textStyle);
            GUILayout.Label($"历史: {inputHistory.Count}", textStyle);
            
            GUILayout.EndArea();
        }
        #endregion

        #region Drawing Utilities
        /// <summary>
        /// 绘制线条
        /// </summary>
        private void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1f)
        {
            Vector3[] points = new Vector3[] { start, end };
            Handles.color = color;
            Handles.DrawAAPolyLine(width, points);
        }

        /// <summary>
        /// 绘制箭头
        /// </summary>
        private void DrawArrow(Vector2 start, Vector2 end, Color color)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            
            float arrowSize = 8f;
            Vector2 arrowPoint1 = end - direction * arrowSize + perpendicular * arrowSize * 0.5f;
            Vector2 arrowPoint2 = end - direction * arrowSize - perpendicular * arrowSize * 0.5f;
            
            DrawLine(end, arrowPoint1, color, lineWidth);
            DrawLine(end, arrowPoint2, color, lineWidth);
        }

        /// <summary>
        /// 绘制圆弧
        /// </summary>
        private void DrawArc(Vector2 center, float radius, float startAngle, float endAngle, Color color)
        {
            int segments = 20;
            float angleStep = (endAngle - startAngle) / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = startAngle + i * angleStep;
                float angle2 = startAngle + (i + 1) * angleStep;
                
                Vector2 point1 = center + new Vector2(
                    Mathf.Cos(angle1 * Mathf.Deg2Rad),
                    Mathf.Sin(angle1 * Mathf.Deg2Rad)
                ) * radius;
                
                Vector2 point2 = center + new Vector2(
                    Mathf.Cos(angle2 * Mathf.Deg2Rad),
                    Mathf.Sin(angle2 * Mathf.Deg2Rad)
                ) * radius;
                
                DrawLine(point1, point2, color);
            }
        }

        /// <summary>
        /// 创建颜色纹理
        /// </summary>
        private Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        #endregion

        #region Public API
        /// <summary>
        /// 切换可视化状态
        /// </summary>
        public void ToggleVisualization()
        {
            IsVisualizationEnabled = !IsVisualizationEnabled;
        }

        /// <summary>
        /// 设置可视化启用状态
        /// </summary>
        public void SetVisualizationEnabled(bool enabled)
        {
            IsVisualizationEnabled = enabled;
        }

        /// <summary>
        /// 清空输入历史
        /// </summary>
        public void ClearInputHistory()
        {
            inputHistory.Clear();
        }

        /// <summary>
        /// 设置显示位置
        /// </summary>
        public void SetDisplayPosition(Vector2 position)
        {
            displayPosition = position;
            SetupVisualizationRect();
        }

        /// <summary>
        /// 设置显示大小
        /// </summary>
        public void SetDisplaySize(Vector2 size)
        {
            displaySize = size;
            SetupVisualizationRect();
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            // 销毁创建的游戏对象
            if (currentIndicator != null)
            {
                DestroyImmediate(currentIndicator);
            }

            if (historyLineRenderer != null)
            {
                DestroyImmediate(historyLineRenderer.gameObject);
            }

            if (lineMaterial != null)
            {
                DestroyImmediate(lineMaterial);
            }

            // 清理数据
            inputHistory?.Clear();
        }
        #endregion
    }

    #region Helper Classes
    /// <summary>
    /// 输入历史点
    /// </summary>
    public class InputHistoryPoint
    {
        public Vector2 inputVector;
        public float timestamp;
        public float magnitude;
    }
    #endregion
}

// 为了兼容性，添加Handles的简单实现
#if !UNITY_EDITOR
namespace UnityEditor
{
    public static class Handles
    {
        public static Color color { get; set; } = Color.white;
        
        public static void DrawAAPolyLine(float width, params Vector3[] points)
        {
            // 在运行时，这个方法不会绘制任何内容
            // 实际项目中可以使用GL类或其他绘制方法
        }
    }
}
#else
using UnityEditor;
#endif 