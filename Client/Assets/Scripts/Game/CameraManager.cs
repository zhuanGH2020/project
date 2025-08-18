using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Vector3 _offset = new Vector3(0, 13, -12);

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 从transform初始位置计算偏移量（如果Player已存在）
        if (Player.Instance != null)
        {
            _offset = transform.position;
        }
    }

    void Update()
    {
        if (Player.Instance != null)
        {
            Camera.main.transform.position = Player.Instance.transform.position + _offset;
        }
    }
}
