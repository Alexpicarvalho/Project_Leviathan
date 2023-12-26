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
        //Debug.LogFormat("Moving at {0} mps towards {1}  ", CurrentSpeed, MovementDirection);
        _characterController.Move((movement * CurrentSpeed + _verticalDirection) * Time.deltaTime);
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

    public override void WarpTo(Vector3 position)
    {
        base.WarpTo(position);
        _characterController.Move(position - transform.position);
    }

    public override void ProcessJump()
    {
        if (!_grounded.IsGrounded) return;
        base.ProcessJump();
        _jumping = true;

        // Apply jump force
        _verticalDirection.y = Mathf.Sqrt(LocomotionData.BaseJumpSpeed * -3f * -9.81f);

        //Tell Grounded to ignore grace time
        _grounded.JumpListening();
    }

    public void ProcessDash(float dashDistance, float dashDuration, AnimationCurve dashSpeedCurve, Dasher dasherScript, float verticalMultiplier)
    {
        StartCoroutine(Dash(dashDistance, dashDuration, dashSpeedCurve, dasherScript, verticalMultiplier));
    }


    public bool IsGrounded()
    {
        return _grounded.IsGrounded;
    }

    private void Update()
    {
        if (!_grounded.IsGrounded) _verticalDirection += 3 * Time.deltaTime * Physics.gravity;
    }

    private IEnumerator Dash(float dashDistance, float dashDuration, AnimationCurve dashCurve, Dasher dasherScript, float verticalMultiplier)
    {
        float timer = 0;
        while (timer < dashDuration)
        {
            float curveValue = dashCurve.Evaluate(timer / dashDuration);
            float distance = dashDistance * curveValue;

            //IF GOING UP WE WANT TO GIVE A BOOST
            if (_verticalDirection.y > 0)
            {
                _characterController.Move(distance * Time.deltaTime * (MovementDirection +
                _verticalDirection * verticalMultiplier));
            }
            else
            {
                _characterController.Move(distance * Time.deltaTime * MovementDirection);
            }

            timer += Time.deltaTime;

            yield return null;
        }
        dasherScript.DashEnd();
    }

    private void ResetVerticalDirection()
    {
        _verticalDirection = Vector3.zero;
    }

    private void OnEnable()
    {
        _grounded.OnLanding += ResetVerticalDirection;
        _grounded.OnOutsideGrounding += ResetVerticalDirection;
    }
    private void OnDisable()
    {
        _grounded.OnLanding -= ResetVerticalDirection;
        _grounded.OnOutsideGrounding -= ResetVerticalDirection;
    }
    
}
