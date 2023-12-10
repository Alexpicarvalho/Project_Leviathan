using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grounded : MonoBehaviour
{

    [Header("Ground Checking")]
    [SerializeField] protected Transform _groundChecker;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private float _checkRadius = .1f;
    [SerializeField] private float _graceTime = .1f;


    public bool IsGrounded { get; private set; } = true;

    private float _graceTimeTimer = 0.0f;

    public event Action OnUngrounding;
    public event Action OnLanding;
    public event Action OnOutsideGrounding;

    private void FixedUpdate()
    {
        GroundCheck();
    }

    private void GroundCheck()
    {
        bool wasGrounded = IsGrounded;

        if (Physics.CheckSphere(_groundChecker.position, _checkRadius, _groundLayers))
        {
            IsGrounded = true;
            _graceTimeTimer = 0;
        }
        else
        {
            _graceTimeTimer += Time.deltaTime;
        }

        if (!wasGrounded && IsGrounded) OnLanding?.Invoke();

        if (_graceTimeTimer < _graceTime) return;

        IsGrounded = false;
        if (wasGrounded && !IsGrounded) OnUngrounding?.Invoke();
    }

    public void OutsideGrounding()
    {
        OnOutsideGrounding?.Invoke();
    }

    public void JumpListening()
    {
        _graceTimeTimer = _graceTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_groundChecker.position, 0.1f);
    }
}
