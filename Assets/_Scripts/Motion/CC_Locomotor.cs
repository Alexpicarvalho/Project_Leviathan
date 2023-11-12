using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Grounded))] // OR EQUIVALENT IF I MAKE A CUSTOM CONTROLLER
public class CC_Locomotor : Locomotor
{
    //private fields
    private CharacterController _characterController;
    private Grounded _grounded;

    [Header("Rotation Smoothing Values")]
    [SerializeField] private float _turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;

    //Jumping
    private bool _jumping = false;
    private Vector3 _verticalDirection = Vector3.zero;

    //Methods
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _grounded = GetComponent<Grounded>();
        _currentSpeed = BaseMaxSpeed;
    }

    //Override Methods
    public override void ProcessMovement(Vector3 movement)
    {
        base.ProcessMovement(movement);
        // Move the character controller
        Debug.LogFormat("Moving at {0} mps towards {1}  ", CurrentSpeed, MovementDirection);
        _characterController.Move((movement * CurrentSpeed + _verticalDirection) * Time.deltaTime );
    }

    public override void ProcessRotation(float rotation)
    {
        //Maybe we can use DOTween to smooth the rotation?
        float angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y, 
            rotation, 
            ref _turnSmoothVelocity, 
            _turnSmoothTime);

        // Rotate the character controller
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    public override void ProcessJump()
    {
        if (!_grounded.IsGrounded) return;
        base.ProcessJump();
        _jumping = true;

        // Apply jump force
        _verticalDirection.y = Mathf.Sqrt(LocomotionData.BaseJumpSpeed * -3f * -9.81f);

        //StartCoroutine(Jump());
    }

    public bool IsGrounded()
    {
        return _grounded.IsGrounded;
    }

    private void Update()
    {
        if(!_grounded.IsGrounded) _verticalDirection += Physics.gravity * 3 * Time.deltaTime;
    }
    //private IEnumerator Jump()
    //{
    //    float timer = 0;
    //    while (timer < LocomotionData.BaseJumpTime)
    //    {
    //        float curveValue = LocomotionData.JumpSpeedCurve.Evaluate(timer / LocomotionData.BaseJumpTime);
    //        float verticalSpeed = LocomotionData.BaseJumpSpeed * curveValue;

    //        _characterController.Move(new Vector3(0, verticalSpeed, 0) * Time.deltaTime);
    //        timer += Time.deltaTime;

    //        yield return null;
    //    }
    //}
}
