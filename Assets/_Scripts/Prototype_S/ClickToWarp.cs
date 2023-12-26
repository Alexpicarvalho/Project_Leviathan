using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToWarp : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Assuming this script is attached to the same GameObject as the camera
        mainCamera = Camera.main;

        // If the script is attached to a different GameObject, find the main camera:
        // mainCamera = Camera.main;
    }

    void Update()
    {
        // Check for right mouse click
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) transform.GetComponent<Locomotor>().WarpTo(hit.point);
        }
    }
}
