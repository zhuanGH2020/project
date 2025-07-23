using System;
using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// 输入提供者接口
    /// 定义所有输入源必须实现的标准接口
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// 移动输入事件，传递标准化的方向向量
        /// </summary>
        event Action<Vector2> OnMovementInput;
        
        /// <summary>
        /// 输入开始事件
        /// </summary>
        event Action OnInputStart;
        
        /// <summary>
        /// 输入结束事件
        /// </summary>
        event Action OnInputEnd;
        
        /// <summary>
        /// 启用/禁用输入提供者
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// 获取当前输入状态
        /// </summary>
        Vector2 CurrentInput { get; }
        
        /// <summary>
        /// 初始化输入提供者
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 清理资源
        /// </summary>
        void Cleanup();
    }
} 