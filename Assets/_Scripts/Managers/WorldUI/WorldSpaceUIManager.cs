using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceUIManager : MonoBehaviour
{
    public static WorldSpaceUIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
