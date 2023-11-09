using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grounded), typeof(Damageable))]

public class Fall : MonoBehaviour
{
    [Header("Damage Calculations")]
    [SerializeField] private float _minDistanceForDamage;
    [SerializeField] private float _distanceForMaxDamage;
    [SerializeField] AnimationCurve _hpPercentDamage;

    private Grounded _grounded;
    private Damageable _damageable;

    private float _highestPoint;
    private float _lowestPoint;
    private bool _stopChecking = true;

    //Events
    public event Action<float> OnTookFallDamage;

    private void Awake()
    {
        _grounded = GetComponent<Grounded>();
        _damageable = GetComponent<Damageable>();
    }

    private void FixedUpdate()
    {
        if (_stopChecking) return;

        if(transform.position.y >= _highestPoint) _highestPoint = transform.position.y;
        else _stopChecking = true;
    }

    private void StartFalling()
    {
        _highestPoint = transform.position.y;
        _stopChecking = false;
    }

    private void StopFalling()
    {
        _lowestPoint = transform.position.y;
        float fallDistance = _highestPoint - _lowestPoint;
        _stopChecking = true;

        CalculateDamage(fallDistance);
    }

    private void CalculateDamage(float fallDistance)
    {
        float ratio = fallDistance / _distanceForMaxDamage;
        float damage = _hpPercentDamage.Evaluate(ratio);
        _damageable.TakeDamage(damage, true);
    }

    private void OnEnable()
    {
        _grounded.OnUngrounding += StartFalling;
        _grounded.OnLanding += StopFalling;
    }
}
