using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public partial class PlayerUIManager : MonoBehaviour
{
    /* This is a partial class responsible for handeling everything related to the persistant UI
     * seen by the player (Stats, skills, weapons etc...). Each part of the class will handle the
     references and methods for that specific part*/

    
    //Singleton
    public static PlayerUIManager Instance { get; private set; }

    [Header("General References")]
    //Player Reference
    [SerializeField] private Transform _player;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CreateScriptReferences();
    }

    private void CreateScriptReferences()
    {
        _playerHealth = _player.GetComponent<Health>();
    }

    //Activate Event Listening
    private void OnEnable()
    {
        _playerHealth.OnHealthChanged += ShowHealth;
        ShowHealth(_playerHealth.CurrentHealth);

    }

    //Deactivate Event Listening
    private void OnDisable()
    {
        _playerHealth.OnHealthChanged -= ShowHealth;
    }
}
