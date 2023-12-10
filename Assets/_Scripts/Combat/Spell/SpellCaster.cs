using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [Header("Spells")]
    [SerializeField] private List<SpellSO> _spells;

    [Header("Fireball - TEMPORARY")]
    [SerializeField] private GameObject _fireball;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireballSpeed;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CastFireball();
        }
    }

    private void CastFireball()
    {
        Vector3 moveDirection = SpellAimer.Instance.GetCameraForward(); 

        var fireball = Instantiate(_fireball, _firePoint.position + moveDirection, Quaternion.identity);
        fireball.GetComponent<Rigidbody>().velocity = moveDirection.normalized * _fireballSpeed;

    }
}
