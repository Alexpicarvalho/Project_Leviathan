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
    [SerializeField] private RectTransform _staminaCrosshairCircle;
    [SerializeField] private float _circleFadeOutDuration = 1f;
    [SerializeField] private float _circleFadeInDuration = .2f;

    //private References
    private Slider _fastStaminaSlider;
    private Slider _slowStaminaSlider;
    private Slider _fastStaminaCircleSlider;
    private Slider _slowStaminaCircleSlider;
    private Image[] _staminaCircleImages;
    private List<Tween> _staminaCircleTweens = new();
    private bool _staminaCircleFaded = false;

    //Script References
    private Stamina _playerStamina;

    private void SetStaminaReferences()
    {
        Slider[] circularSliders = _staminaCrosshairCircle.GetComponentsInChildren<Slider>();
        _staminaCircleImages = _staminaCrosshairCircle.GetComponentsInChildren<Image>();

        Slider[] sliders = _staminaBar.GetComponentsInChildren<Slider>();

        if (sliders.Length >= 2)
        {
            _fastStaminaSlider = sliders[1];
            _slowStaminaSlider = sliders[0];
        }
        else throw new Exception("Missing Stamina Sliders");

        if (circularSliders.Length >= 2)
        {
            _fastStaminaCircleSlider = circularSliders[1];
            _slowStaminaCircleSlider = circularSliders[0];
        }
        else throw new Exception("Missing Stamina Circular Sliders");
    }

    private void ShowStamina(float stamina)
    {
        if (_fastStaminaSlider == null && _slowStaminaSlider == null) SetStaminaReferences();

        if (stamina == _playerStamina.MaxStamina && !_staminaCircleFaded) FadeStaminaCircle(false);
        else if (_staminaCircleFaded && stamina < _playerStamina.MaxStamina) FadeStaminaCircle(true);

        // HUD Bars
        //_staminaText.text = ((int)stamina).ToString();
        _fastStaminaSlider.DOValue(stamina / _playerStamina.MaxStamina, .2f);

        DOVirtual.DelayedCall(
            .4f,
            () => _slowStaminaSlider.DOValue(stamina / _playerStamina.MaxStamina, .6f)
            );

        //Circle
        _fastStaminaCircleSlider.DOValue(stamina / _playerStamina.MaxStamina, .2f);

        DOVirtual.DelayedCall(
            .4f,
            () => _slowStaminaCircleSlider.DOValue(stamina / _playerStamina.MaxStamina, .6f)
            );
    }

    private void FadeStaminaCircle(bool on)
    {
        CirclePopAnimation(on);
        // Clear existing tweens in the list
        foreach (Tween tween in _staminaCircleTweens)
        {
            tween.Kill();
        }
        _staminaCircleTweens.Clear();
        foreach (Image image in _staminaCircleImages)
        {
            if (on)
            {
                Tween newTween = image.DOFade(1, _circleFadeInDuration); // Store the new tween
                _staminaCircleTweens.Add(newTween);
                _staminaCircleFaded = false;
            }
            else
            {
                Tween newTween = image.DOFade(0, _circleFadeOutDuration); // Store the new tween
                _staminaCircleTweens.Add(newTween);
                _staminaCircleFaded = true;
            }
        }
    }

    private void CirclePopAnimation(bool on)
    {
        float startScale = _staminaCrosshairCircle.localScale.x;

        if (!on)
        {
            _staminaCrosshairCircle.DOScale(startScale * 1.2f, .1f);
            _staminaCrosshairCircle.DOScale(startScale, .1f).SetDelay(.1f);
        }
        else
        {
            _staminaCrosshairCircle.DOScale(0, 0);
            _staminaCrosshairCircle.DOScale(startScale, .1f);
        }
        
    }
}
