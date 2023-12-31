using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Healable : MonoBehaviour
{
    //private properties
    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    //public methods

    public void Heal(float amount)
    {
        _health.GainHealth(amount);
    }
}
