using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZPos : MonoBehaviour
{
    void Start()
    {
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        rb.inertiaTensorRotation = Quaternion.identity;
    }

    void Update()
    {
        
    }
}
