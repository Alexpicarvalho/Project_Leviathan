using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl : MonoBehaviour //Look
{

    //REMAKE

    public CinemachineVirtualCamera virtualCamera;
    public float lookSpeed = 10f;

    private CinemachineOrbitalTransposer orbitalTransposer;
    private float xAxisValue = 0f;

    private void Start()
    {
        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            orbitalTransposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            xAxisValue = orbitalTransposer.m_XAxis.Value;
        }
        else
        {
            Debug.LogError("ThirdPersonCameraController: No virtualCamera reference set.");
        }

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
    }

    private void Update()
    {
        if (orbitalTransposer != null)
        {
            xAxisValue += Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
            orbitalTransposer.m_XAxis.Value = xAxisValue;

            // Add this line if you want vertical movement as well
            // orbitalTransposer.m_FollowOffset.y += Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
        }
    }
}
