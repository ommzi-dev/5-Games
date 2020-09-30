using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolGravityChanges : MonoBehaviour
{
    
    //private void Awake()
    //{
    //    physics = GetComponent<Physics>();
    //}
    private void OnEnable()
    {
        Physics.gravity = new Vector3(0, 0, 10f);
    }
    private void OnDisable()
    {
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }
}
