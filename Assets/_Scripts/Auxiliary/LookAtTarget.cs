using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] private bool _lookAtCamera;
    [SerializeField] private Transform _target;

    private void Awake()
    {
        if (_lookAtCamera) _target = Camera.main.transform;
    }

    private void Update()
    {
        transform.LookAt(_target, Vector3.up);
    }
}
