using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    private static readonly GameMain m_Instance = new GameMain();
    public static GameMain Instance { get { return m_Instance; } }

    // Start is called before the first frame update
    void Start()
    {
        ConfigExample.Example();
        ConfigExample.AdvancedExample();
        ConfigExample.ValidationExample();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
