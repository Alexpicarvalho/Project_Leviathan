using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour
{

    [Header("Ground Checking")]
    [SerializeField] protected Transform _groundChecker;
    [SerializeField] private LayerMask _groundLayers;
    public bool IsGrounded { get; private set;} = true;
    public event Action OnUngrounding;
    public event Action OnLanding;

    private void FixedUpdate()
    {
        GroundCheck();
    }
    private void GroundCheck()
    {
        bool wasGrounded = IsGrounded;

        if (Physics.CheckSphere(_groundChecker.position, 0.1f, _groundLayers)) IsGrounded = true;
        else IsGrounded = false;

        if (wasGrounded && !IsGrounded) OnUngrounding?.Invoke();
        if (!wasGrounded && IsGrounded) OnLanding?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_groundChecker.position, 0.1f);
    }

}
