using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : ScriptableObject
{
    [SerializeField] private List<State> _canTransitionTo = new List<State>();

    public void GetState() { }
    public void EnterState() { }
    public void ExitState() { }

}
