using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AnimationControl : MonoBehaviour //Movement
{
    //Serialized Fields
    [SerializeField] private string _isMovingParam = "IsMoving";
    [SerializeField] private string JumpParam = "Jump"; 
    [SerializeField] private string LandedParam = "Landed";
    [SerializeField] private string DashingParam = "Dashing";

    //private Refs
    private Locomotor _locomotor;
    private Dasher _dasher;

    private void Update()
    {
        if(_locomotor.MovementDirection.magnitude >= 0.1f) _animator.SetBool(_isMovingParam, true);
        else _animator.SetBool(_isMovingParam, false);
    }

    private void Jump()
    {
        _animator.SetTrigger(JumpParam);
    }

    private void Dash()
    {
        _animator.SetBool(DashingParam, true);
    }

    private void DashEnded()
    {
        _animator.SetBool(DashingParam, false);
    }

    private void Landed()
    {
        _animator.SetTrigger("Landed");
        Debug.Log("Landed");
    }
}
