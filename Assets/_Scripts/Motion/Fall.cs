using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Locomotor), typeof(Damageable))]

public class Fall : MonoBehaviour
{
    [Header("Damage Calculations")]
    [SerializeField] private float _minDistanceForDamage;
    [SerializeField] private float _distanceForMaxDamage;
    [SerializeField] AnimationCurve _hpPercentDamage;

    private Locomotor _locomotor;
    private Damageable _damageable;

    private float _highestPoint;
    private float _lowestPoint;
    private bool _stopChecking = true;

    private void Awake()
    {
        _locomotor = GetComponent<Locomotor>();
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
        _locomotor.OnUngrounding += StartFalling;
        _locomotor.OnLanding += StopFalling;
    }
}
