using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Pushable : MonoBehaviour
{
    //private fields
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void GetPushed(Vector3 pushDirection, float pushIntensity)
    {
        _rb.AddForce(pushDirection * pushIntensity, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
    }
}
