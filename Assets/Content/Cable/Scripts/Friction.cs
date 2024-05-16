using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friction : MonoBehaviour
{
    [Range(0,1)]
    public float friction;

    protected Rigidbody Rigidbody;
    
    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Rigidbody.velocity = Rigidbody.velocity * (1 - friction);
        Rigidbody.angularVelocity = Rigidbody.angularVelocity * (1 - friction);
    }
}
