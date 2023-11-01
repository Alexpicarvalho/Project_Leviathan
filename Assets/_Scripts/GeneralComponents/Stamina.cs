using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    #region private properties
    [SerializeField] private float _maxStamina;
    [SerializeField] private float _currentStamina;
    [SerializeField] private float _staminaRegenPerSecond;
    [SerializeField] private float _staminaRegenTickCooldown;
    private float _staminaRegenTickTimer = 0f;
    #endregion

    #region public properties
    public float MaxStamina => _maxStamina;
    public float CurrentStamina => _currentStamina;
    public float StaminaRegenPerSecond => _staminaRegenPerSecond;
    #endregion

    #region  Events
    public event Action<float> OnStaminaChange;
    public event Action OnStaminaZero; //Fatigue Mechanic
    #endregion

    #region public methods

    public void LoseStamina(float amount)
    {
        _currentStamina -= amount;
        _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
        OnStaminaChange?.Invoke(_currentStamina);
    }

    public void GainStamina(float amount)
    {
        _currentStamina += amount;
        _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
        OnStaminaChange?.Invoke(_currentStamina);
    }
    #endregion

    #region private methods
    private void Update()
    {
        _staminaRegenTickTimer += Time.deltaTime;
        if (_staminaRegenTickTimer >= _staminaRegenTickCooldown) RegenTick();


        //TESTING
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoseStamina(10);
        }
    }

    private void RegenTick()
    {
        _staminaRegenTickTimer = 0f;
        GainStamina(_staminaRegenPerSecond * _staminaRegenTickCooldown);
    }
    #endregion
}
