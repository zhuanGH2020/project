using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// 移动处理器接口
    /// 定义处理移动逻辑的标准化接口
    /// </summary>
    public interface IMovementHandler
    {
        /// <summary>
        /// 处理移动输入
        /// </summary>
        /// <param name="inputVector">标准化的输入向量</param>
        void HandleMovement(Vector2 inputVector);
        
        /// <summary>
        /// 设置移动处理器的启用状态
        /// </summary>
        /// <param name="enabled">是否启用</param>
        void SetMovementEnabled(bool enabled);
        
        /// <summary>
        /// 获取当前移动状态
        /// </summary>
        bool IsMoving { get; }
        
        /// <summary>
        /// 获取当前速度
        /// </summary>
        Vector3 CurrentVelocity { get; }
        
        /// <summary>
        /// 初始化移动处理器
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 清理资源
        /// </summary>
        void Cleanup();
    }
} 