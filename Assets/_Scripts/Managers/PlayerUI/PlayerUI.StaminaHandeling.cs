using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerUI // Stamina Handeling
{
    [Header("Stamina")]
    //Serialized References
    [SerializeField] private TextMeshProUGUI _staminaText;
    [SerializeField] private RectTransform _staminaBar;

    //private References
    private Slider _fastStaminaSlider;
    private Slider _slowStaminaSlider;

    //Script References
    private Stamina _playerStamina;

    private void ShowStamina(float stamina)
    {
        if (_fastStaminaSlider == null && _slowStaminaSlider == null)
        {
            Slider[] sliders = _staminaBar.GetComponentsInChildren<Slider>();
            _fastStaminaSlider = sliders[1];
            _slowStaminaSlider = sliders[0];
        }

        _staminaText.text = ((int)stamina).ToString();
        _fastStaminaSlider.DOValue(stamina / _playerStamina.MaxStamina, .2f);

        DOVirtual.DelayedCall(
            .4f,
            () => _slowStaminaSlider.DOValue(stamina / _playerStamina.MaxStamina, .6f)
            );
    }
}
