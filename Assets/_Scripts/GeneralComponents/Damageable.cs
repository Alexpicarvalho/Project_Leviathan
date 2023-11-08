using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Damageable : MonoBehaviour
{
    //private properties
    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    //public methods

    public void TakeDamage(float amount, bool isPercentual = false)
    { 
        if(!isPercentual) _health.LoseHealth(amount);
        else _health.LoseHealth(_health.MaxHealth * amount);
    }
}
