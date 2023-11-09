using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Grounded))] // OR EQUIVALENT IF I MAKE A CUSTOM CONTROLLER
public class CC_Locomotor : Locomotor
{
    //private fields
    private CharacterController _characterController;
    private Grounded _grounded;

    //Methods
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _grounded = GetComponent<Grounded>();
        _currentSpeed = BaseMaxSpeed;
    }

    //Override Methods
    protected override void ProcessMovement(Vector3 movement)
    {
        // Move the character controller
        _characterController.Move(movement * Time.deltaTime);
    }

    protected override void ProcessJump()
    {
        if(_grounded.IsGrounded) return;
        base.ProcessJump();

        StartCoroutine(Jump());
    }

    //TEMPORARY - TESTS ONLY

    private void Update()
    {
        Vector3 move = new Vector3(0, 0, 0);

        // Use Input.GetKey and check for WASD keys to calculate the move vector
        if (Input.GetKey(KeyCode.W))
        {
            move += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            move -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            move -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            move += transform.right;
        }

        // Normalize the move vector to ensure consistent movement speed in all directions
        move.Normalize();

        // Check if the character is grounded; if not, you might want to apply gravity
        if (!_characterController.isGrounded)
        {
            move += Physics.gravity;
        }

        // Process the movement by calling the overridden method
        ProcessMovement(move * _currentSpeed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ProcessJump();
        }
    }

    private IEnumerator Jump()
    {
        float timer = 0;
        while (timer < _locomotionData.BaseJumpTime )
        {
            float curveValue = _locomotionData.JumpSpeedCurve.Evaluate(timer / _locomotionData.BaseJumpTime);
            float verticalSpeed = _locomotionData.BaseJumpSpeed * curveValue;

            _characterController.Move(new Vector3(0, verticalSpeed, 0) * Time.deltaTime);
            timer += Time.deltaTime;

            yield return null;
        }
    }
}
