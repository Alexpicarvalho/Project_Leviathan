using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotor : MonoBehaviour
{
    // This is the base class for all locomotor scripts.
    // It is meant to be inherited from, and not used directly.

    //private fields
    public LocomotionData LocomotionData;

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
    public float BaseMaxSpeed => LocomotionData.BaseMaxSpeed;
    public float BaseStartSpeed => LocomotionData.BaseStartSpeed;
    public float BaseAcceleration => LocomotionData.BaseAcceleration;
    public float BaseDeceleration => LocomotionData.BaseDeceleration;
    public float CurrentSpeed => _currentSpeed;
    public float CurrentMaxSpeed => _currentMaxSpeed;
    public float CurrentStartSpeed => _currentStartSpeed;
    public float CurrentAcceleration => _currentAcceleration;
    public float CurrentDeceleration => _currentDeceleration;
    [SerializeField] public Vector3 MovementDirection { get; protected set; }
    #endregion

    //Events
    public event Action OnJump;

    //Overridable methods

    public virtual void ProcessMovement(Vector2 movementDirection) { MovementDirection = movementDirection; }
    public virtual void ProcessMovement(Vector3 movementDirection) { MovementDirection = movementDirection; }
    public virtual void ProcessRotation(float rotation) { }
    public virtual void IncreaseSpeed(float amount) { }  //Swap these for a multiplier value, not a flat amount
    public virtual void DecreaseSpeed(float amount) { } 
    public virtual void SetSpeed(float amount) { }
    public virtual void ProcessJump() { OnJump?.Invoke(); }
    public virtual bool CanJump() { return LocomotionData.CanJump; }
}
