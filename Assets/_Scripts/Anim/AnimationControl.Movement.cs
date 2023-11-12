using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AnimationControl : MonoBehaviour //Movement
{
    //Serialized Fields
    [SerializeField] private string _isMovingParam = "IsMoving";
    [SerializeField] private string JumpParam = "Jump"; 
    [SerializeField] private string LandedParam = "Landed";
    //private Refs
    private Locomotor _locomotor;

    private void Update()
    {
        if(_locomotor.MovementDirection.magnitude >= 0.1f) _animator.SetBool(_isMovingParam, true);
        else _animator.SetBool(_isMovingParam, false);
    }

    private void Jump()
    {
        _animator.SetTrigger(JumpParam);
    }

    private void Landed()
    {
        _animator.SetTrigger("Landed");
        Debug.Log("Landed");
    }
}
