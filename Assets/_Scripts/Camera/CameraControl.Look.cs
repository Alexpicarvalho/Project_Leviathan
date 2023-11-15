using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl : MonoBehaviour //Look
{
    [Header("Aiming")]
    [SerializeField] private CinemachineVirtualCamera _aimCam;
    [SerializeField] private float _aimBlendSpeed;
    private bool _isAiming = false;

    //REMAKE

    //Testing

    private void Update()
    {
       
    }

    private void ActivateAimCam()
    {
        if (_isAiming) return;
        _isAiming = true;
        _aimCam.Priority = 99;
    }
    private void DeactivateAimCam()
    {
        if (!_isAiming) return;
        _isAiming = false;
        _aimCam.Priority = 10;
    }

   
}
