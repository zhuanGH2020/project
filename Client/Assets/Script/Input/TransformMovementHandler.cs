using UnityEngine;
using System.Collections;

namespace InputSystem
{
    /// <summary>
    /// Transform移动处理器
    /// 负责将输入转换为实际的GameObject移动
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class TransformMovementHandler : MonoBehaviour, IMovementHandler
    {
        #region Properties
        public bool IsMoving { get; private set; }
        public Vector3 CurrentVelocity { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("=== 组件引用 ===")]
        [SerializeField] private InputConfiguration config;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Rigidbody targetRigidbody;
        [SerializeField] private CharacterController characterController;

        [Header("=== 移动设置 ===")]
        [SerializeField] private MovementMode movementMode = MovementMode.Transform;
        [SerializeField] private bool useLocalSpace = false;
        [SerializeField] private bool constrainToGround = true;
        [SerializeField] private LayerMask groundLayerMask = -1;

        [Header("=== 调试选项 ===")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color velocityGizmoColor = Color.red;
        [SerializeField] private Color targetGizmoColor = Color.green;
        #endregion

        #region Private Fields
        // 移动状态
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 targetVelocity = Vector3.zero;
        private Vector3 velocitySmoothing = Vector3.zero;
        
        // 输入状态
        private Vector2 currentInput = Vector2.zero;
        private Vector2 previousInput = Vector2.zero;
        private bool movementEnabled = true;
        
        // 性能优化
        private float lastMoveTime = 0f;
        private Vector3 lastPosition = Vector3.zero;
        private bool isGrounded = true;
        
        // 缓存组件
        private Transform cachedTransform;
        private Camera mainCamera;
        
        // 对象池
        private static readonly System.Collections.Generic.Queue<Vector3> vector3Pool = 
            new System.Collections.Generic.Queue<Vector3>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            // 缓存常用组件
            cachedTransform = targetTransform != null ? targetTransform : transform;
            mainCamera = Camera.main;
            lastPosition = cachedTransform.position;
            
            StartCoroutine(MovementUpdateCoroutine());
        }

        private void OnValidate()
        {
            // 自动获取组件引用
            if (targetTransform == null)
                targetTransform = transform;
                
            if (targetRigidbody == null)
                targetRigidbody = GetComponent<Rigidbody>();
                
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
        #endregion

        #region IMovementHandler Implementation
        public void HandleMovement(Vector2 inputVector)
        {
            if (!movementEnabled || config == null) return;

            currentInput = inputVector;
            IsMoving = inputVector.magnitude > 0.01f;
            
            // 记录移动时间用于性能优化
            if (IsMoving)
            {
                lastMoveTime = Time.time;
            }
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            
            if (!enabled)
            {
                currentInput = Vector2.zero;
                targetVelocity = Vector3.zero;
                IsMoving = false;
            }
        }

        public void Initialize()
        {
            // 重置移动状态
            currentVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;
            velocitySmoothing = Vector3.zero;
            currentInput = Vector2.zero;
            previousInput = Vector2.zero;
            IsMoving = false;
            movementEnabled = true;
            
            Debug.Log("[TransformMovementHandler] 初始化完成");
        }

        public void Cleanup()
        {
            StopAllCoroutines();
        }
        #endregion

        #region Movement Processing
        private IEnumerator MovementUpdateCoroutine()
        {
            WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
            
            while (true)
            {
                if (movementEnabled && config != null)
                {
                    ProcessMovement();
                    UpdateVelocityTracking();
                }
                
                yield return waitForFixedUpdate;
            }
        }

        /// <summary>
        /// 处理移动逻辑
        /// </summary>
        private void ProcessMovement()
        {
            // 计算目标速度
            CalculateTargetVelocity();
            
            // 应用速度平滑
            ApplyVelocitySmoothing();
            
            // 执行移动
            ExecuteMovement();
            
            // 检查地面状态
            if (constrainToGround)
            {
                CheckGroundStatus();
            }
        }

        /// <summary>
        /// 计算目标速度
        /// </summary>
        private void CalculateTargetVelocity()
        {
            if (currentInput.magnitude < 0.01f)
            {
                targetVelocity = Vector3.zero;
                return;
            }

            // 获取移动方向
            Vector3 moveDirection = GetMovementDirection(currentInput);
            
            // 使用基础移动速度
            float currentMoveSpeed = config.moveSpeed;
            
            // 计算目标速度
            targetVelocity = moveDirection * currentMoveSpeed;
            
            // 如果启用地面约束，移除Y轴速度
            if (constrainToGround)
            {
                targetVelocity.y = 0f;
            }
        }

        /// <summary>
        /// 获取移动方向
        /// </summary>
        private Vector3 GetMovementDirection(Vector2 input)
        {
            Vector3 direction = Vector3.zero;

            if (useLocalSpace)
            {
                // 使用本地空间坐标
                direction = cachedTransform.right * input.x + cachedTransform.forward * input.y;
            }
            else
            {
                // 使用世界空间坐标，考虑摄像机方向
                if (mainCamera != null)
                {
                    Vector3 forward = mainCamera.transform.forward;
                    Vector3 right = mainCamera.transform.right;
                    
                    // 移除Y轴分量
                    forward.y = 0f;
                    right.y = 0f;
                    
                    forward.Normalize();
                    right.Normalize();
                    
                    direction = right * input.x + forward * input.y;
                }
                else
                {
                    // 默认世界坐标
                    direction = Vector3.right * input.x + Vector3.forward * input.y;
                }
            }

            return direction.normalized;
        }

        /// <summary>
        /// 应用速度平滑
        /// </summary>
        private void ApplyVelocitySmoothing()
        {
            float smoothTime = IsMoving ? 
                (1f / config.acceleration) : 
                (1f / config.deceleration);

            currentVelocity = Vector3.SmoothDamp(
                currentVelocity,
                targetVelocity,
                ref velocitySmoothing,
                smoothTime,
                Mathf.Infinity,
                Time.fixedDeltaTime
            );

            // 更新公共属性
            CurrentVelocity = currentVelocity;
        }

        /// <summary>
        /// 执行移动
        /// </summary>
        private void ExecuteMovement()
        {
            if (currentVelocity.magnitude < 0.001f) return;

            Vector3 deltaMovement = currentVelocity * Time.fixedDeltaTime;

            switch (movementMode)
            {
                case MovementMode.Transform:
                    ExecuteTransformMovement(deltaMovement);
                    break;
                case MovementMode.Rigidbody:
                    ExecuteRigidbodyMovement();
                    break;
                case MovementMode.CharacterController:
                    ExecuteCharacterControllerMovement(deltaMovement);
                    break;
            }
        }

        /// <summary>
        /// 执行Transform移动
        /// </summary>
        private void ExecuteTransformMovement(Vector3 deltaMovement)
        {
            cachedTransform.position += deltaMovement;
        }

        /// <summary>
        /// 执行Rigidbody移动
        /// </summary>
        private void ExecuteRigidbodyMovement()
        {
            if (targetRigidbody != null)
            {
                targetRigidbody.velocity = new Vector3(
                    currentVelocity.x,
                    targetRigidbody.velocity.y, // 保持Y轴速度
                    currentVelocity.z
                );
            }
        }

        /// <summary>
        /// 执行CharacterController移动
        /// </summary>
        private void ExecuteCharacterControllerMovement(Vector3 deltaMovement)
        {
            if (characterController != null)
            {
                characterController.Move(deltaMovement);
            }
        }

        /// <summary>
        /// 检查地面状态
        /// </summary>
        private void CheckGroundStatus()
        {
            float rayDistance = 1.1f;
            Vector3 rayOrigin = cachedTransform.position + Vector3.up * 0.1f;
            
            isGrounded = Physics.Raycast(
                rayOrigin, 
                Vector3.down, 
                rayDistance, 
                groundLayerMask
            );
        }

        /// <summary>
        /// 更新速度跟踪
        /// </summary>
        private void UpdateVelocityTracking()
        {
            Vector3 currentPosition = cachedTransform.position;
            Vector3 actualVelocity = (currentPosition - lastPosition) / Time.fixedDeltaTime;
            lastPosition = currentPosition;
            
            // 更新实际速度（用于调试）
            if (showDebugGizmos)
            {
                CurrentVelocity = actualVelocity;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 设置输入配置
        /// </summary>
        public void SetConfiguration(InputConfiguration newConfig)
        {
            config = newConfig;
            Initialize();
        }

        /// <summary>
        /// 强制停止移动
        /// </summary>
        public void StopMovement()
        {
            currentInput = Vector2.zero;
            targetVelocity = Vector3.zero;
            currentVelocity = Vector3.zero;
            IsMoving = false;
            
            if (targetRigidbody != null)
            {
                targetRigidbody.velocity = Vector3.zero;
            }
        }

        /// <summary>
        /// 瞬间移动到指定位置
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            StopMovement();
            cachedTransform.position = position;
            lastPosition = position;
        }

        /// <summary>
        /// 获取移动统计信息
        /// </summary>
        public MovementStats GetMovementStats()
        {
            return new MovementStats
            {
                isMoving = IsMoving,
                currentSpeed = CurrentVelocity.magnitude,
                targetSpeed = targetVelocity.magnitude,
                isGrounded = isGrounded,
                movementMode = movementMode
            };
        }
        #endregion

        #region Debugging
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || cachedTransform == null) return;

            Vector3 position = cachedTransform.position;

            // 绘制当前速度向量
            if (CurrentVelocity.magnitude > 0.1f)
            {
                Gizmos.color = velocityGizmoColor;
                Gizmos.DrawRay(position, CurrentVelocity);
                Gizmos.DrawWireSphere(position + CurrentVelocity, 0.1f);
            }

            // 绘制目标速度向量
            if (targetVelocity.magnitude > 0.1f)
            {
                Gizmos.color = targetGizmoColor;
                Gizmos.DrawRay(position + Vector3.up * 0.1f, targetVelocity);
            }

            // 绘制地面检测射线
            if (constrainToGround)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawRay(position + Vector3.up * 0.1f, Vector3.down * 1.1f);
            }
        }

        private void OnGUI()
        {
            if (config != null && config.enableDetailedDebugInfo)
            {
                DrawMovementDebugInfo();
            }
        }

        private void DrawMovementDebugInfo()
        {
            GUILayout.BeginArea(new Rect(320, 10, 300, 150));
            GUILayout.Label("=== 移动处理器调试信息 ===");
            GUILayout.Label($"移动模式: {movementMode}");
            GUILayout.Label($"当前速度: {CurrentVelocity.magnitude:F2} m/s");
            GUILayout.Label($"目标速度: {targetVelocity.magnitude:F2} m/s");
            GUILayout.Label($"是否移动: {IsMoving}");
            GUILayout.Label($"是否接地: {isGrounded}");
            GUILayout.EndArea();
        }
        #endregion
    }

    #region Enums and Structs
    /// <summary>
    /// 移动模式枚举
    /// </summary>
    public enum MovementMode
    {
        Transform,           // 直接修改Transform位置
        Rigidbody,           // 使用Rigidbody物理移动
        CharacterController  // 使用CharacterController移动
    }

    /// <summary>
    /// 移动统计信息
    /// </summary>
    public struct MovementStats
    {
        public bool isMoving;
        public float currentSpeed;
        public float targetSpeed;
        public bool isGrounded;
        public MovementMode movementMode;
    }
    #endregion
} 