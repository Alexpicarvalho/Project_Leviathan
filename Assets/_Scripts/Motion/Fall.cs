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
        _grounded.OnLanding += StopFalling;
        _highestPoint = transform.position.y;
        _stopChecking = false;
    }

    private void StopFalling()
    {
        _lowestPoint = transform.position.y;
        float fallDistance = _highestPoint - _lowestPoint;
        _stopChecking = true;
        _grounded.OnLanding -= StopFalling;

        if(fallDistance < _minDistanceForDamage) return;    

        CalculateDamage(fallDistance);
    }

    private void CalculateDamage(float fallDistance)
    {
        float ratio = fallDistance / _distanceForMaxDamage;
        float damage = _hpPercentDamage.Evaluate(ratio);

        Debug.LogFormat("Fell {0} meters for {1}% damage", _highestPoint - _lowestPoint,damage * 100);

        _damageable.TakeDamage(damage, true);
    }

    public void CancelFall()
    {
        _stopChecking = true;
        _grounded.OnLanding -= StopFalling;
    }

    private void OnEnable()
    {
        _grounded.OnUngrounding += StartFalling;
        _grounded.OnOutsideGrounding += CancelFall;
    }
}
