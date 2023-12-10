using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEditor.UI;
using System.Linq;

public class EditorTesting : MonoBehaviour
{
    [Header("Toggle")]
    [SerializeField] private bool _toggleEditorTesting;

    [Header("Refs")]
    [SerializeField] private Transform _player;

    [Header("Visual")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private GameObject _activateInfoTxt;

    [Header("Values")]
    [SerializeField] private float _takeDamageAmount = 10f;
    [SerializeField] private float _healAmount = 10f;

    [Header("Key Mapping")]
    [SerializeField] private KeyCode _toggleEditorTestingKey = KeyCode.F1;
    [SerializeField] private KeyCode _resetPlayerPosition = KeyCode.Alpha1;
    [SerializeField] private KeyCode _takeDamageKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode _healKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode _healToFull = KeyCode.Alpha4;
    [SerializeField] private KeyCode _toggleInfoPanel = KeyCode.I;


    //Refs
    private Damageable _playerDamageable;
    private Healable _playerHealable;
    private Vector3 _playerStartPosition;
    private CharacterController _characterController;

    private List<MonoBehaviour> _playerScripts = new List<MonoBehaviour>();

    private void Awake()
    {
        _playerScripts = _player.GetComponents<MonoBehaviour>().ToList();
        _characterController = _player.GetComponent<CharacterController>();
        _playerStartPosition = _player.position;
        _playerDamageable = _player.GetComponent<Damageable>();
        _playerHealable = _player.GetComponent<Healable>();
        SetText();
    }

    private void Update()
    {
        Toggle_Editor();

        if (Input.GetKeyDown(_toggleInfoPanel))
        {
            _infoPanel.SetActive(!_infoPanel.activeSelf);
            _activateInfoTxt.SetActive(!_activateInfoTxt.activeSelf);
        }

        if (!_toggleEditorTesting) return;


        ResetPlayerPosition();
        TakeXDamage();
        HealXAmount();
        HealToFull();

        

    }

    private void HealToFull()
    {
        if (Input.GetKeyDown(_healToFull))
        {
            _playerHealable.Heal(100000000000);
        }
    }

    private void HealXAmount()
    {
        if (Input.GetKeyDown(_healKey))
        {
            _playerHealable.Heal(_healAmount);
        }
    }

    private void TakeXDamage()
    {
        if (Input.GetKeyDown(_takeDamageKey))
        {
            _playerDamageable.TakeDamage(_takeDamageAmount);
        }
    }


    private void ResetPlayerPosition()
    {
        if (Input.GetKeyDown(_resetPlayerPosition))
        {
            
            _characterController.Move(_playerStartPosition - _player.position + Vector3.up * 10);
            _player.GetComponent<Fall>().CancelFall();
        }
    }

    private void Toggle_Editor()
    {
        if (Input.GetKeyDown(_toggleEditorTestingKey))
        {
            _toggleEditorTesting = !_toggleEditorTesting;
            SetText();
        }
    }

    private void SetText()
    {
        _text.text =
            "Editor Testing: " + GetColor(_toggleEditorTesting) + _toggleEditorTesting.ToString() + "\n\n <color=\"white\">" +
            "Toggle Editor Testing: <color=\"orange\">" + _toggleEditorTestingKey.ToString() + "\n <color=\"white\">" +
            "Reset player position: <color=\"orange\">" + _resetPlayerPosition.ToString() + "\n <color=\"white\">" +
            "Take " + _takeDamageAmount + " Damage: <color=\"orange\">" + _takeDamageKey.ToString() + "\n <color=\"white\">" +
            "Heal " + _healAmount + " HP: <color=\"orange\">" + _healKey.ToString() + "\n <color=\"white\">" +
            "Heal to Full: <color=\"orange\">" + _healToFull.ToString() + "\n <color=\"white\">" +
            "\n \n \n Toggle This Panel: <color=\"yellow\">" + _toggleInfoPanel.ToString() +  "<color=\"white\">"
            ;
        _activateInfoTxt.transform.GetComponentInChildren<TextMeshProUGUI>().text = "To Activate Info Press  <color=\"yellow\">" + 
            _toggleInfoPanel.ToString();
    }

    private string GetColor(bool value)
    {
        return value ? "<color=\"green\">" : "<color=\"red\">";
    }
}
