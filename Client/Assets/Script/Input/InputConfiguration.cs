using UnityEngine;
using System;

namespace InputSystem
{
    /// <summary>
    /// 输入系统配置文件
    /// 使用ScriptableObject实现配置化和可扩展性设计
    /// </summary>
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Input System/Input Configuration")]
    public class InputConfiguration : ScriptableObject
    {
        [Header("=== 键位设置 ===")]
        [Tooltip("前进键")]
        public KeyCode forwardKey = KeyCode.W;
        
        [Tooltip("后退键")]
        public KeyCode backKey = KeyCode.S;
        
        [Tooltip("左移键")]
        public KeyCode leftKey = KeyCode.A;
        
        [Tooltip("右移键")]
        public KeyCode rightKey = KeyCode.D;

        [Header("=== 移动参数 ===")]
        [Tooltip("基础移动速度")]
        [Range(1f, 20f)]
        public float moveSpeed = 5f;
        
        [Tooltip("加速度")]
        [Range(5f, 50f)]
        public float acceleration = 10f;
        
        [Tooltip("减速度")]
        [Range(5f, 50f)]
        public float deceleration = 15f;

        [Header("=== 输入处理 ===")]
        [Tooltip("输入死区，小于此值的输入将被忽略")]
        [Range(0f, 0.5f)]
        public float inputDeadZone = 0.1f;
        
        [Tooltip("输入平滑时间")]
        [Range(0f, 0.5f)]
        public float inputSmoothTime = 0.1f;
        
        [Tooltip("输入灵敏度")]
        [Range(0.5f, 3f)]
        public float inputSensitivity = 1f;
        
        [Tooltip("是否启用8方向输入")]
        public bool enable8DirectionalInput = true;

        [Header("=== 调试选项 ===")]
        [Tooltip("启用输入可视化调试")]
        public bool enableInputVisualization = true;
        
        [Tooltip("启用输入录制功能")]
        public bool enableInputRecording = false;
        
        [Tooltip("显示详细调试信息")]
        public bool enableDetailedDebugInfo = false;
        
        [Tooltip("调试UI颜色")]
        public Color debugUIColor = Color.green;

        [Header("=== 性能优化 ===")]
        [Tooltip("输入更新频率（FPS）")]
        [Range(30, 120)]
        public int inputUpdateRate = 60;
        
        [Tooltip("启用输入缓存优化")]
        public bool enableInputCaching = true;
        
        [Tooltip("最大输入历史记录数量")]
        [Range(10, 100)]
        public int maxInputHistoryCount = 50;

        [Header("=== 扩展功能 ===")]
        [Tooltip("启用组合键支持")]
        public bool enableComboKeys = true;
        
        [Tooltip("组合键超时时间（秒）")]
        [Range(0.1f, 2f)]
        public float comboKeyTimeout = 0.5f;
        
        [Tooltip("快捷键配置")]
        public ShortcutKeyConfig[] shortcutKeys = new ShortcutKeyConfig[0];

        /// <summary>
        /// 验证配置的有效性
        /// </summary>
        public bool ValidateConfiguration()
        {
            if (moveSpeed <= 0) return false;
            if (acceleration <= 0) return false;
            if (deceleration <= 0) return false;
            if (inputSmoothTime < 0) return false;
            return true;
        }

        private void OnValidate()
        {
            // 确保配置值的合理性
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            acceleration = Mathf.Max(0.1f, acceleration);
            deceleration = Mathf.Max(0.1f, deceleration);
            inputDeadZone = Mathf.Clamp01(inputDeadZone);
            inputSmoothTime = Mathf.Max(0f, inputSmoothTime);
        }
    }

    /// <summary>
    /// 快捷键配置
    /// </summary>
    [Serializable]
    public class ShortcutKeyConfig
    {
        [Tooltip("快捷键名称")]
        public string keyName;
        
        [Tooltip("主键")]
        public KeyCode primaryKey;
        
        [Tooltip("修饰键（可选）")]
        public KeyCode modifierKey = KeyCode.None;
        
        [Tooltip("是否启用")]
        public bool enabled = true;
        
        [Tooltip("描述")]
        public string description;
    }
} 