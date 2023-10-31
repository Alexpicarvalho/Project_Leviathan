using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))] // OR EQUIVALENT IF I MAKE A CUSTOM CONTROLLER
public class CC_Locomotor : Locomotor
{
    //private fields
    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
}
