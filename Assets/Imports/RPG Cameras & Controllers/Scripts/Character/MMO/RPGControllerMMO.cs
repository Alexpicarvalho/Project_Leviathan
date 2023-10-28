using UnityEngine;
using JohnStairs.RCC.Enums;

namespace JohnStairs.RCC.Character.MMO {
    [RequireComponent(typeof(RPGMotorMMO))]
    public class RPGControllerMMO : RPGController {
        /// <summary>
        /// Reference to the MMO RPGMotor script
        /// </summary>
        protected RPGMotorMMO _rpgMotorMMO;

        protected override void Awake() {
            base.Awake();

            _rpgMotorMMO = GetComponent<RPGMotorMMO>();
        }

        protected override void Update() {                    
            base.Update(); 

            if (!ActivateCharacterControl) {
                // Early return if the controls are disabled
                _rpgMotor.SetInputDirection(Vector3.zero);
                _rpgMotor.StartMotor();
                return;
            }

            #region Process movement inputs
            // Get the vertical movement direction/input
            float vertical = _inputMovement.y;
            // Check if both select buttons are pressed
            if (_inputMoveForwardHalf1 && _inputMoveForwardHalf2) {
                // Let the character move forward
                vertical = 1.0f;
            }

            // Check the autorun input
            _rpgMotor.ToggleAutorun(_inputToggleAutorunning);
            // Get all actions that start an active vertical movement
            bool forwardMovementStarted = _inputMovementStart && _inputMovement.y != 0
                                            || CombinedMoveForwardStart();

            _rpgMotorMMO.StartDiving(forwardMovementStarted);
            // Signal the usage of actions canceling the autorunning
            _rpgMotor.StopAutorun(forwardMovementStarted);

            _rpgMotorMMO.StopDiving(_inputMovementStop
                                    || CombinedMoveForwardStop());

            // Get the horizontal movement direction/input
            float rotation = _inputMovement.x;
            // Get the horizontal strafe direction/input				
            float strafe = _inputStrafe;
            // Strafe if the character should rotate but the rotation modifier is pressed
            if (_inputRotationModifier && _inputMovement.x != 0) {
                // Let the character strafe instead rotating
                strafe = rotation;
                rotation = 0;
            }
            // Create and set the player's input direction inside the motor
            Vector3 inputDirection = new Vector3(strafe, 0, vertical);
            _rpgMotor.SetInputDirection(inputDirection);

            // Allow midair movement if the player wants to move forward/backwards or strafe
            _rpgMotorMMO.TryMidairMovement(forwardMovementStarted
                                        || _inputStrafeStart
                                        || RotationModificationStart());

            // Set the local Y axis rotation input inside motor
            _rpgMotorMMO.RotateAroundAxis(Axis.Y, rotation);

            // Enable sprinting inside the motor if the sprint modifier is pressed down
            _rpgMotor.Sprint(_inputSprint);

            // Toggle walking inside the motor
            _rpgMotor.ToggleWalking(_inputToggleWalking);

            // Check if the jump button is pressed down
            if (_inputJump) {
                // Signal the motor to jump
                _rpgMotor.Jump();
            }

            // Signal the motor to surface when swimming
            _rpgMotor.Surface(_inputSurface);
            #endregion Process movement inputs            

            // Start the motor
            _rpgMotor.StartMotor();

            ConsumeEventInputs();
        }
    }
}
