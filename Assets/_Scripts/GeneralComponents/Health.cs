using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    //private properties
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _healthRegenPerSecond;
    [SerializeField] private float _healthRegenTickCooldown;
    private float _healthRegenTickTimer = 0f;

    //public properties
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public float HealthRegenPerSecond => _healthRegenPerSecond;
    
    //Events
    public event Action<float> OnHealthChanged;

    //public methods

    public void LoseHealth(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth);
    }

    public void GainHealth(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth);
    }

    private void Update()
    {
        _healthRegenTickTimer += Time.deltaTime;
        if (_healthRegenTickTimer >= _healthRegenTickCooldown) RegenTick();


        //TESTING
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoseHealth(10);
        }
    }

    private void RegenTick()
    {
        _healthRegenTickTimer = 0f;
        GainHealth(_healthRegenPerSecond * _healthRegenTickCooldown);
    }
}
