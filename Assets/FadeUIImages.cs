using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeUIImages : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _onFadePercentage = 0.5f;

    [Header("Animate")]
    [SerializeField] private float _fadeSpeed = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _shouldFade = false;
    private List<UIImageDetails> _uiImages = new();

    private void Awake()
    {
        CreateUiImageReferences();
        if (_shouldFade) FadeImages();
    }

    public void FadeImages()
    {
        foreach (var image in _uiImages)
        {
            var newAlpha = image.StartAlpha * _onFadePercentage;
            image.Image.DOFade(newAlpha, _fadeSpeed);
            image.CurrentAlpha = newAlpha;
        }
    }

    public void UnfadeImages()
    {
        foreach (var item in _uiImages)
        {
            item.Image.DOFade(item.StartAlpha, _fadeSpeed);
            item.CurrentAlpha = item.StartAlpha;
        }

    }

    private void CreateUiImageReferences()
    {
        var uiImages = GetComponentsInChildren<Image>(true);

        if (uiImages.Length == 0)
        {
            Debug.LogError("No UI Images found");
            return;
        }

        foreach (var uiImage in uiImages)
        {
            var uiImageDetails = new UIImageDetails(uiImage, uiImage.color.a);
            _uiImages.Add(uiImageDetails);
        }

    }
}
