using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotor : MonoBehaviour
{
    // This is the base class for all locomotor scripts.
    // It is meant to be inherited from, and not used directly.

    //private fields
    [SerializeField] protected LocomotionData _locomotionData;

    //runtime fields
    #region runtime fields
    protected float _currentSpeed = 5f;
    protected float _currentMaxSpeed = 5f;
    protected float _currentStartSpeed = 5f;
    protected float _currentAcceleration = 5f;
    protected float _currentDeceleration = 5f;
    #endregion

    //public properties
    #region public properties
    public float BaseMaxSpeed => _locomotionData.BaseMaxSpeed;
    public float BaseStartSpeed => _locomotionData.BaseStartSpeed;
    public float BaseAcceleration => _locomotionData.BaseAcceleration;
    public float BaseDeceleration => _locomotionData.BaseDeceleration;
    public float CurrentSpeed => _currentSpeed;
    public float CurrentMaxSpeed => _currentMaxSpeed;
    public float CurrentStartSpeed => _currentStartSpeed;
    public float CurrentAcceleration => _currentAcceleration;
    public float CurrentDeceleration => _currentDeceleration;
    #endregion

    //Overridable methods

    protected virtual void ProcessMovement(Vector2 movementDirection) { }
    protected virtual void ProcessMovement(Vector3 movementDirection) { }
    protected virtual void IncreaseSpeed(float amount) { }  //Swap these for a multiplier value, not a flat amount
    protected virtual void DecreaseSpeed(float amount) { } 
    protected virtual void SetSpeed(float amount) { }
    protected virtual void ProcessJump() {}
    protected virtual bool CanJump() { return _locomotionData.CanJump; }
}
