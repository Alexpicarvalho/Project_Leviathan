using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))] // OR EQUIVALENT IF I MAKE A CUSTOM CONTROLLER
public class CC_Locomotor : Locomotor
{
    //private fields
    private CharacterController _characterController;


    //Methods
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    
    protected override void ProcessMovement(Vector3 movement)
    {
        _characterController.Move(movement);
    }

    //TEMPORARY - TESTS ONLY

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ProcessJump();
        }

        //if (Input.GetKeyDown)
    }
}
