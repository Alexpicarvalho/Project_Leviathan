using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Killable : MonoBehaviour
{
    //private fields
    private Health _health;
    private bool _canDie = true;

    //Events
    public event Action OnDeath;

    //Methods

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void Die()
    {
        if (!_canDie) return;

        //Code To Handle Death
        //Raise Event Of Death to be seen by UI , Animation Control etc...
    }

    private void OnEnable()
    {
        _health.OnHealthZero += Die;
    }
}
