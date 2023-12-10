using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class SwapBelts : MonoBehaviour
{
    [Header("Refs and Settings")]
    [SerializeField] private RectTransform _mainBelt;
    [SerializeField] private RectTransform _offBelt;
    private FadeUIImages _mainBeltFadeImages;
    private FadeUIImages _offBeltFadeImages;

    private bool _beltsAreSwapped = false;

    [Header("Animate")]
    [SerializeField] private float _swapSpeed = 0.5f;

    private float _offBeltScale;
    private float _mainBeltScale;
    private Vector3 _mainBeltPosition;
    private Vector3 _offBeltPosition;

    [Header("Debug/Testing")]
    [SerializeField] private KeyCode _swapKey = KeyCode.Tab;

    private void Awake()
    {
        _offBeltPosition = _offBelt.position;
        _mainBeltPosition = _mainBelt.position;

        _offBeltScale = _offBelt.localScale.x;
        _mainBeltScale = _mainBelt.localScale.x;

        _mainBeltFadeImages = _mainBelt.GetComponent<FadeUIImages>();
        _offBeltFadeImages = _offBelt.GetComponent<FadeUIImages>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(_swapKey))
        {
            SwapBelt();
        }
        
    }

    private void SwapBelt()
    {
        if (_beltsAreSwapped)
        {
            _beltsAreSwapped = false;

            _mainBelt.DOMove(_mainBeltPosition, _swapSpeed);
            _mainBelt.DOScale(_mainBeltScale, _swapSpeed);

            _offBelt.DOMove(_offBeltPosition, _swapSpeed);
            _offBelt.DOScale(_offBeltScale, _swapSpeed);  

            _offBeltFadeImages.FadeImages();
            _mainBeltFadeImages.UnfadeImages();
        }
        else
        {
            _beltsAreSwapped = true;

            _mainBelt.DOMove(_offBeltPosition, _swapSpeed);
            _mainBelt.DOScale(_offBeltScale, _swapSpeed);

            _offBelt.DOMove(_mainBeltPosition, _swapSpeed);
            _offBelt.DOScale(_mainBeltScale, _swapSpeed);

            _mainBeltFadeImages.FadeImages();
            _offBeltFadeImages.UnfadeImages();  
        }
        
    }
}
