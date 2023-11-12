using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    #region private properties
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _healthRegenPerSecond;
    [SerializeField] private float _healthRegenTickCooldown;
    private float _healthRegenTickTimer = 0f;
    #endregion

    #region public properties
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public float HealthRegenPerSecond => _healthRegenPerSecond;
    #endregion

    #region  Events
    public event Action<float> OnHealthChanged;
    public event Action OnHealthZero;
    #endregion

    //public methods

    public void LoseHealth(float amount)
    {
        Debug.LogFormat("Losing {0} health", amount);
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0) OnHealthZero?.Invoke();
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
    }

    private void RegenTick()
    {
        _healthRegenTickTimer = 0f;
        GainHealth(_healthRegenPerSecond * _healthRegenTickCooldown);
    }
}
