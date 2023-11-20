using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_Dasher : Dasher
{
    //Private References
    private CC_Locomotor _locomotor;
    private MeshTrailFX _dashFX;

    private void Awake()
    {
        _locomotor = GetComponent<CC_Locomotor>();
    }

    override protected void Dash()
    {
        base.Dash();
        _locomotor.ProcessDash(_dashDistance, _dashDuration, _dashSpeedCurve, this, _verticalMultiplier);
    }

    public override void DashEnd()
    {
        base.DashEnd();
    }

    //TESTING ONLY
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
    }
}
