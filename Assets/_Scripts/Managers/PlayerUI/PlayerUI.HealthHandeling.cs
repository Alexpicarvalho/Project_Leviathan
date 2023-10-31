using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public partial class PlayerUI // Health Handeling
{
    [Header("Health")]
    //Serialized References
    [SerializeField] private TextMeshProUGUI _healthText;

    //Script References
    private Health _playerHealth;

    private void ShowHealth(float health)
    {
        _healthText.text = ((int)health).ToString();
    }
}
