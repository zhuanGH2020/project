using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputController : MonoBehaviour
{
    [System.Serializable]
    public class MoveInputEvent : UnityEvent<Vector2> { }
    
    [Header("Input Settings")]
    public MoveInputEvent onMoveInput;
    [Range(0.01f, 1f)]
    public float inputSmoothness = 0.1f;
    
    [Header("Input Keys")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    
    private Vector2 currentInputVector;
    private Vector2 targetInputVector;
    
    // Start is called before the first frame update
    void Start()
    {
        // 初始化事件
        if (onMoveInput == null)
            onMoveInput = new MoveInputEvent();
            
        currentInputVector = Vector2.zero;
        targetInputVector = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // 检测输入
        DetectInput();
        
        // 平滑输入处理
        SmoothInput();
        
        // 发送输入事件
        if (currentInputVector != Vector2.zero || targetInputVector != Vector2.zero)
        {
            onMoveInput?.Invoke(currentInputVector);
        }
    }
    
    /// <summary>
    /// 检测WASD键盘输入
    /// </summary>
    private void DetectInput()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // 检测水平输入 (A/D)
        if (Input.GetKey(leftKey))
            horizontal -= 1f;
        if (Input.GetKey(rightKey))
            horizontal += 1f;
            
        // 检测垂直输入 (W/S)
        if (Input.GetKey(forwardKey))
            vertical += 1f;
        if (Input.GetKey(backwardKey))
            vertical -= 1f;
            
        // 设置目标输入向量
        targetInputVector = new Vector2(horizontal, vertical);
        
        // 对角线移动时进行归一化，保持速度一致
        if (targetInputVector.magnitude > 1f)
        {
            targetInputVector.Normalize();
        }
    }
    
    /// <summary>
    /// 平滑输入处理，提供更好的手感
    /// </summary>
    private void SmoothInput()
    {
        // 使用插值实现平滑输入
        currentInputVector = Vector2.Lerp(currentInputVector, targetInputVector, inputSmoothness * Time.deltaTime * 10f);
        
        // 当输入向量很小时，直接设为零，避免微小抖动
        if (currentInputVector.magnitude < 0.01f)
        {
            currentInputVector = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 获取当前输入向量（用于调试或其他需要）
    /// </summary>
    public Vector2 GetCurrentInput()
    {
        return currentInputVector;
    }
    
    /// <summary>
    /// 设置输入平滑度
    /// </summary>
    public void SetInputSmoothness(float smoothness)
    {
        inputSmoothness = Mathf.Clamp01(smoothness);
    }
} 