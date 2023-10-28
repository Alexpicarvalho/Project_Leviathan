using UnityEngine;

namespace JohnStairs.RCC.Character {
	public interface ICharacterInfo {
		/// <summary>
		/// Checks if the character is able to move
		/// </summary>
		/// <returns>True if the character can move, otherwise false</returns>
		bool CanMove();
		
		/// <summary>
		/// Checks if the character is able to rotate
		/// </summary>
		/// <returns>True if the character can rotate, otherwise false</returns>
		bool CanRotate();

		/// <summary>
		/// Checks if the character is able to fly
		/// </summary>
		/// <returns>True if the character can fly, otherwise false</returns>
		bool CanFly();

		/// <summary>
		/// Checks if the character is able to sprint
		/// </summary>
		/// <returns>True if the character can sprint, otherwise false</returns>
		bool CanSprint();

		/// <summary>
		/// Checks if the character locked onto a target, e.g. for combat
		/// </summary>
		/// <returns>True if the character locked onto a target, otherwise false</returns>
		bool LockOnTarget();
		
		/// <summary>
		/// Applies all movement improving and/or impairing effects on the given value and returns the result
		/// </summary>
		/// <param name="basisValue">Basis value of the calculation</param>
		/// <returns>Resulting movement speed which is improved/impaired by effects</returns>
		float GetMovementSpeedInfluence(float basisValue);

		/// <summary>
		/// Gets the target's position in world coordinates.
		/// Only used for ARPG if LockOnTarget() returns true 
		/// </summary>
		/// <returns>Target position in world coordinates</returns>
		Vector3 GetTargetPosition();

		/// <summary>
		/// Gets the rotation towards the target's current position
		/// Only used for MMO if LockOnTarget() returns true
		/// </summary>
		/// <returns>Rotation towards the current target</returns>
		Quaternion GetRotationTowardsTarget();
	}
}
