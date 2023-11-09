using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl : MonoBehaviour
{
    //Cinemachine Components
    [Header("Cinemachine Components")]
    [SerializeField] private Transform _cinemachineBrain_T;
    [SerializeField] private Transform _cinemachineCam_T;

    private CinemachineBrain _cinemachineBrain;
    private CinemachineVirtualCamera _virtualCamera;

    private void Awake()
    {
        _cinemachineBrain = _cinemachineBrain_T.GetComponent<CinemachineBrain>();
        _virtualCamera = _cinemachineCam_T.GetComponent<CinemachineVirtualCamera>();
    }
}