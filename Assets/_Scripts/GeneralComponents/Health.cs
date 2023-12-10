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
    [SerializeField] private bool _isRegenActive;
    private float _healthRegenTickTimer = 0f;
    #endregion

    #region public properties
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public float HealthRegenPerSecond => _healthRegenPerSecond;
    #endregion

    #region  Events
    public delegate void HealthChanged(float health, bool healthLost);
    public HealthChanged OnHealthChanged;
    public event Action OnHealthZero;
    #endregion

    //public methods

    public void LoseHealth(float amount)
    {
        Debug.LogFormat("Losing {0} health", amount);
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, true);

        if (_currentHealth <= 0) OnHealthZero?.Invoke();
    }

    public void GainHealth(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, false);
    }

    private void Update()
    {
        _healthRegenTickTimer += Time.deltaTime;
        if (_healthRegenTickTimer >= _healthRegenTickCooldown) RegenTick();
    }

    private void RegenTick()
    {
        if (!_isRegenActive || CurrentHealth >= MaxHealth) return;
        _healthRegenTickTimer = 0f;
        GainHealth(_healthRegenPerSecond * _healthRegenTickCooldown);
    }
}
