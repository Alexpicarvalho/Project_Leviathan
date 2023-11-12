using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CC_Locomotor))]
public class PlayerMovement : MonoBehaviour
{
    private CC_Locomotor _locomotor;
    private Transform _cam => Camera.main.transform;

    private void Awake()
    {
        _locomotor = GetComponent<CC_Locomotor>();
    }

    private void Update()
    {
        Vector3 direction = CalculateDirection();
        Vector3 moveDirection = direction;

        if (direction.magnitude > 0.1f)
        {
            //Calculate and apply rotation
            float targetAngle = Mathf.Atan2(direction.x, direction.z) 
                * Mathf.Rad2Deg
                + _cam.eulerAngles.y;

            //Process the rotation by calling the overridden method (rotation is calculated in the motor)
            _locomotor.ProcessRotation(targetAngle);

            //Calculate Final Movement Direction
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            moveDirection.Normalize();
        }
        // Process the movement by calling the overridden method (speed is calculated in the motor)
        _locomotor.ProcessMovement(moveDirection); //We call this method even if the magnitude is 0,
                                                   //so that the motor can handle the deceleration / stopping


        //TEMPORARY - TESTS ONLY
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _locomotor.ProcessJump();
        }
    }

    private Vector3 CalculateDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical);

        // Normalize the move vector to ensure consistent movement speed in all directions
        direction.Normalize();

        return direction;
    }

}
