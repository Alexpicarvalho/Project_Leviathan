using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    #region private methods
    [SerializeField] private DestructionType _destructionType;
    #endregion

    #region Private Methods
    private void DestroyMe()
    {

    }
    #endregion

}

//We Might want to swap this to 2 subclasses
public enum DestructionType
{
    VFXDestruction,         // Plays a VFX on Destruction
    PhysicsDestruction      // Activates Physics Objects and Adds Forces, think a Vase breaking
}
