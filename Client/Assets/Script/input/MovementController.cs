using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0.1f, 20f)]
    public float moveSpeed = 5f;
    [Range(0.1f, 20f)]
    public float rotationSpeed = 10f;
    
    [Header("Movement Options")]
    public bool enableRotation = true;
    public bool useLocalSpace = true;
    
    private Vector2 currentInputVector;
    private Transform cachedTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        // 缓存Transform组件提升性能
        cachedTransform = transform;
        currentInputVector = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // 如果有输入，执行移动
        if (currentInputVector != Vector2.zero)
        {
            PerformMovement();
            
            // 如果启用旋转，让对象面向移动方向
            if (enableRotation)
            {
                PerformRotation();
            }
        }
    }
    
    /// <summary>
    /// 接收输入事件并移动GameObject
    /// 这个方法会被InputController的UnityEvent调用
    /// </summary>
    /// <param name="inputVector">输入向量，x为左右，y为前后</param>
    public void OnMoveInput(Vector2 inputVector)
    {
        currentInputVector = inputVector;
    }
    
    /// <summary>
    /// 执行移动操作
    /// </summary>
    private void PerformMovement()
    {
        Vector3 moveDirection = Vector3.zero;
        
        if (useLocalSpace)
        {
            // 使用本地空间移动（相对于对象自身的方向）
            moveDirection = cachedTransform.right * currentInputVector.x + 
                           cachedTransform.forward * currentInputVector.y;
        }
        else
        {
            // 使用世界空间移动
            moveDirection = Vector3.right * currentInputVector.x + 
                           Vector3.forward * currentInputVector.y;
        }
        
        // 应用移动
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        cachedTransform.position += movement;
    }
    
    /// <summary>
    /// 执行旋转操作，让对象面向移动方向
    /// </summary>
    private void PerformRotation()
    {
        Vector3 lookDirection = Vector3.zero;
        
        if (useLocalSpace)
        {
            // 基于本地空间计算旋转方向
            lookDirection = cachedTransform.right * currentInputVector.x + 
                           cachedTransform.forward * currentInputVector.y;
        }
        else
        {
            // 基于世界空间计算旋转方向
            lookDirection = Vector3.right * currentInputVector.x + 
                           Vector3.forward * currentInputVector.y;
        }
        
        if (lookDirection != Vector3.zero)
        {
            // 计算目标旋转
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            
            // 平滑旋转
            cachedTransform.rotation = Quaternion.Slerp(
                cachedTransform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }
    
    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="speed">新的移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.1f, speed);
    }
    
    /// <summary>
    /// 设置旋转速度
    /// </summary>
    /// <param name="speed">新的旋转速度</param>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0.1f, speed);
    }
    
    /// <summary>
    /// 获取当前移动速度
    /// </summary>
    /// <returns>当前移动速度</returns>
    public float GetCurrentMoveSpeed()
    {
        return moveSpeed;
    }
    
    /// <summary>
    /// 获取当前输入向量（用于调试）
    /// </summary>
    /// <returns>当前输入向量</returns>
    public Vector2 GetCurrentInput()
    {
        return currentInputVector;
    }
    
    /// <summary>
    /// 停止移动
    /// </summary>
    public void StopMovement()
    {
        currentInputVector = Vector2.zero;
    }
} 