using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerUI // Health Handeling
{
    [Header("Health")]
    //Serialized References
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private RectTransform _healthBar;

    //private References
    private Slider _fastHealthSlider;
    private Slider _slowHealthSlider;

    //Script References
    private Health _playerHealth;

    private void ShowHealth(float health)
    {
        if (_fastHealthSlider == null && _slowHealthSlider == null)
        {
            Slider[] sliders = _healthBar.GetComponentsInChildren<Slider>();
            _fastHealthSlider = sliders[1];
            _slowHealthSlider = sliders[0];
        }

        _healthText.text = ((int)health).ToString();
        _fastHealthSlider.DOValue(health / _playerHealth.MaxHealth, .2f);

        DOVirtual.DelayedCall(
            .7f, 
            () => _slowHealthSlider.DOValue(health / _playerHealth.MaxHealth, .7f)
            );
        
    }
}
