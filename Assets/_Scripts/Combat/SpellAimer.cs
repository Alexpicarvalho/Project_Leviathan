using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellAimer : MonoBehaviour
{
    public static SpellAimer Instance;

    //Refs
    private Camera _mainCamera;
    [SerializeField] private Transform _player;

    [Header("Debug")]
    public bool _debugToggle = true;
    public GameObject _debugSphere;

    [Header("")]
    [SerializeField] private LayerMask _canBeTargetedAoE;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(transform.gameObject);
        }

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_debugToggle) GetRaycastPosition(30.0f);
    }

    public Vector3 GetCameraForward()
    {
        return _mainCamera.transform.forward;
    }

    // For player
    public Vector3 GetRaycastPosition(float maxDistance = Mathf.Infinity, bool castsDown = false)
    {
        float distanceFromPlayer = Vector3.Distance(_mainCamera.transform.position, _player.position);
        maxDistance += distanceFromPlayer;

        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, _canBeTargetedAoE))
        {
            if (_debugToggle) _debugSphere.transform.position = hit.point;
            return hit.point;
        }
        else if(castsDown)
        {
            Vector3 radiusLimitPosition = _mainCamera.transform.position + _mainCamera.transform.forward * maxDistance;
            Vector3 returnPos = GetRaycastPosition(radiusLimitPosition, Vector3.down);
            if (_debugToggle) _debugSphere.transform.position = returnPos;
            return returnPos;
        }
        else
        {
            return  _mainCamera.transform.forward;
        }
    }

    // For enemies
    public Vector3 GetRaycastPosition(Vector3 startPosition, Vector3 direction, float maxDistance = Mathf.Infinity)
    {
        Ray ray = new Ray(startPosition, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, _canBeTargetedAoE))
        {
            return hit.point;
        }
        else
        {
            return Vector3.zero;
        }
        
    }
}
