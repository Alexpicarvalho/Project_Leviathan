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

    public void TakeDamage(float amount)
    { 
        _health.LoseHealth(amount);
    }
}
