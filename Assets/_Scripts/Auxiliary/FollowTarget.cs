using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform _target;
    private Vector3 _offSet;

    private void Awake()
    {
        _offSet = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        transform.position = _target.position + _offSet;
    }
}
