using UnityEngine;

namespace JohnStairs.RCC.Character.ARPG {
    [RequireComponent(typeof(RPGMotorARPG))]
    public class RPGControllerARPG : RPGController {
        /// <summary>
        /// If true, smoothing of input direction changes is enabled, e.g. for using a keyboard
        /// </summary>
        public bool SmoothDirectionInputChanges = true;

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
            // Get all actions that can cancel an active autorun
            bool stopAutorunning = _inputMovementStart && _inputMovement.y != 0
                                    || CombinedMoveForwardStart();
            // Signal the usage of actions cancelling the autorunning
            _rpgMotor.StopAutorun(stopAutorunning);

            // Get the horizontal movement direction/input
            float horizontal = _inputMovement.x;

            // Create and set the player's input direction inside the motor
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
            _rpgMotor.SetInputDirection(inputDirection);

            // Enable sprinting inside the motor if the sprint modifier is pressed down
            _rpgMotor.Sprint(_inputSprint);

            // Toggle walking inside the motor
            _rpgMotor.ToggleWalking(_inputToggleWalking);

            // Toggle crouching inside the motor
            _rpgMotor.ToggleCrouching(_inputToggleCrouching);

            // Check if the jump button is pressed down
            if (_inputJump) {
                // Signal the motor to jump
                _rpgMotor.Jump();
            }

            // Signal the motor to surface when swimming
            _rpgMotor.Surface(_inputSurface);
            // Signal the motor to dive when swimming
            _rpgMotor.Dive(_inputDive);
            #endregion Process movement inputs

            // Start the motor
            _rpgMotor.StartMotor();

            ConsumeEventInputs();
        }
    }
}
