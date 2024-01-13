using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrasnsformCopier : MonoBehaviour
{
    [SerializeField] private Transform _target;
    private void Awake()
    {
        transform.parent = _target;
    }
}
