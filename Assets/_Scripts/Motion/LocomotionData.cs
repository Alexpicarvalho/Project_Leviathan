using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Locomotion/Base Locomotion" ,fileName = "LocomotionData")]
public class LocomotionData : ScriptableObject
{
    [field: Header("Speed Values")]
    [field: SerializeField] public float BaseMaxSpeed { get; private set; }
    [field: SerializeField] public float BaseStartSpeed { get; private set; }
    [field: SerializeField] public float BaseAcceleration { get; private set; }
    [field: SerializeField] public float BaseDeceleration { get; private set; }

    [field: Header("Jump Values")]
    [field: SerializeField] public float BaseJumpTime { get; private set; }
    [field: SerializeField] public float BaseJumpSpeed { get; private set; }
    [field: SerializeField] public AnimationCurve JumpSpeedCurve { get; private set; }

    [field: SerializeField] public bool CanJump { get; private set; } = true;

}
