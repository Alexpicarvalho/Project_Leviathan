using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerLook : MonoBehaviour
{
    [Header("Requirements")]
    private Camera _camera;
    [SerializeField] private Camera _handCamera;

    [Header("Properties")]
    public float _cameraSensitivity = 30f;
    [SerializeField] float _lookClamp = 80f;

    [Header("Values")]
    private float xRotation = 0f;
    public float viewX;
    public float viewY;

    private void Awake()
    {
        _camera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ProcessLook(Vector2 input)
    {
        viewX = input.x;
        viewY = input.y;

        xRotation -= viewY * Time.deltaTime * _cameraSensitivity;
        xRotation = Mathf.Clamp(xRotation, -_lookClamp, _lookClamp);

        _camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        //_handCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * Time.deltaTime * viewX * _cameraSensitivity);
    }

    public void Aim(bool toggle, float lerpDuration)
    {
        if (toggle)
        {
            StartCoroutine(LerpAim(lerpDuration));
        }
        else
        {
            StartCoroutine(ResetAim(lerpDuration));
        }
    }

    IEnumerator LerpAim(float lerpDuration)
    {
        float elapsedTime = 0;

        while (elapsedTime < lerpDuration)
        {
            _camera.fieldOfView = (int)Mathf.Lerp(90, 70, elapsedTime / lerpDuration);
            //_handCamera.fieldOfView = (int)Mathf.Lerp(60, 70, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator ResetAim(float lerpDuration)
    {
        float elapsedTime = 0;

        while (elapsedTime < lerpDuration)
        {
            _camera.fieldOfView = (int)Mathf.Lerp(70, 90, elapsedTime / lerpDuration);
            //_handCamera.fieldOfView = (int)Mathf.Lerp(60, 70, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
