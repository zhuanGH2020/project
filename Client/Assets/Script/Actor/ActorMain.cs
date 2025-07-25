using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMain : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    
    private Transform _transform;

    void Start()
    {
        _transform = transform;
    }

    void Update()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
        _transform.Translate(moveDirection * _moveSpeed * Time.deltaTime);
    }
}
