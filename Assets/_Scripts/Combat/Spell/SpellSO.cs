using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Combat/Spells", fileName = "SpellSO")]
public class SpellSO : ScriptableObject
{
    [Header("Values")]
    [SerializeField] private float _cooldown;

    [Header("Visuals And Description")]
    [SerializeField] private string _spellName = "Spell Name";
    [SerializeField] private Sprite _icon;
    [Multiline][SerializeField] private string _description = "This is where the spell description goes!";

    //Public Properties
    public float Cooldown => _cooldown;
    public string SpellName => _spellName;
    public string Description => _description;

    //Methods
    
    public virtual void CastSpell(){}
    public virtual void UpdateSpell() { }
}
