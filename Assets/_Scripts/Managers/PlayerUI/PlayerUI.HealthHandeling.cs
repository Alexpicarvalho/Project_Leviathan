using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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

    private void ShowHealth(float health, bool isLesser)
    {

        var healthRatio = health / _playerHealth.MaxHealth;;

        if (!_fastHealthSlider || !_slowHealthSlider)
        {
            InitializeSliders();
        }

        _healthText.text = ((int)health).ToString();

        if(isLesser)
        {
            _fastHealthSlider.DOValue(health / _playerHealth.MaxHealth, .2f);

            DOVirtual.DelayedCall(
                .7f,
                () => _slowHealthSlider.DOValue(healthRatio, .7f)
                );
        }
        else
        {
            _slowHealthSlider.DOValue(healthRatio, .7f);

            DOVirtual.DelayedCall(
                .7f,
                () => _fastHealthSlider.DOValue(healthRatio, .2f)
                );
        }
    }

    private void InitializeSliders()
    {
        Slider[] sliders = _healthBar.GetComponentsInChildren<Slider>();
        _fastHealthSlider = sliders[1];
        _slowHealthSlider = sliders[0];
    }
}
