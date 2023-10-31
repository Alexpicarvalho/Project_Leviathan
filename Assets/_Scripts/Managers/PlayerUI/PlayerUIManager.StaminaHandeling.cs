using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public partial class PlayerUIManager // Stamina Handeling
{
    [Header("Stamina")]
    //Serialized References
    [SerializeField] private TextMeshProUGUI _staminaText;

    //Script References
    private Stamina _playerStamina;

    private void ShowStamina(float stamina)
    {
        _staminaText.text = ((int)stamina).ToString();
    }
}
