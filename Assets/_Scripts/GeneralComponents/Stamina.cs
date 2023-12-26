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
    [SerializeField] private float _startRegenDelay = 1.0f;
    private float _staminaRegenTickTimer = 0f;
    private float _staminaRegenDelayTimer = 0f;
    private bool _regenActive = true;
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

        _regenActive = false;
        _staminaRegenDelayTimer = 0f;
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
        _staminaRegenDelayTimer += Time.deltaTime;

        if(!_regenActive && _staminaRegenDelayTimer >= _startRegenDelay) _regenActive = true;

        if (_staminaRegenTickTimer >= _staminaRegenTickCooldown && _regenActive) RegenTick();


        //TESTING
        if (Input.GetKeyDown(KeyCode.P))
        {
            LoseStamina(30);
        }
    }

    private void RegenTick()
    {
        _staminaRegenTickTimer = 0f;
        GainStamina(_staminaRegenPerSecond * _staminaRegenTickCooldown);
    }
    #endregion
}
