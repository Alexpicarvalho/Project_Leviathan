using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dasher : MonoBehaviour
{
    //We might want this to be a spell

    //This is a base class for dashing behavior. It is meant to be used with it's
    // respective motor class, which will be responsible for the actual movement.

    [Header("Dash Values")]
    [SerializeField] protected float _dashDistance;
    [SerializeField] protected float _dashDuration;
    [SerializeField] protected float _dashCooldown;
    [SerializeField] protected AnimationCurve _dashSpeedCurve;

    //Readonly properties
    public float DashDistance => _dashDistance;
    public float DashDuration => _dashDuration;
    public float DashCooldown => _dashCooldown;

    //Events
    public event Action OnDash;

    //Overrideable methods

    protected virtual void Dash(){ OnDash?.Invoke(); }
}
