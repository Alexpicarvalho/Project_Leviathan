using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AnimationControl : MonoBehaviour
{
    //private fields
    private Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        GetScriptReferences();
    }

    private void GetScriptReferences()
    {
        _locomotor = GetComponent<Locomotor>();
    }

    private void OnEnable()
    {
        _locomotor.OnJump += Jump;
        _locomotor.GetComponent<Grounded>().OnLanding += Landed;

        _dasher = GetComponent<Dasher>();
        _dasher.OnDash += Dash;
        _dasher.OnDashEnd += DashEnded;
    }
}
