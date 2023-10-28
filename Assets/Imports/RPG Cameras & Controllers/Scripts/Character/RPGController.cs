using UnityEngine;
using JohnStairs.RCC.Inputs;

namespace JohnStairs.RCC.Character {
    public abstract class RPGController : MonoBehaviour {
        /// <summary>
        /// If set to false, all character controls are disabled
        /// </summary>
        public bool ActivateCharacterControl = true;        
        /// <summary>
        /// If true, Unity's new input system is used
        /// </summary>
        public bool UseNewInputSystem = true;
        /// <summary>
        /// Logs warnings for legacy inputs which are not set up in the project but used in this script
        /// </summary>
        public bool LogInputWarnings = true;

        /// <summary>
        /// Reference to the used RPGMotor script
        /// </summary>
        protected RPGMotor _rpgMotor;
        /// <summary>
        /// Interface for getting character information, e.g. movement impairing effects
        /// </summary>
        protected RPGInputActions _inputActions;
        #region Input values
        /// <summary>
        /// 2D axis input for the movement direction
        /// </summary>
        protected Vector2 _inputMovement;
        /// <summary>
        /// If true, character movement starts this frame
        /// </summary>
        protected bool _inputMovementStart = false;
        /// <summary>
        /// If true, character movement stops this frame
        /// </summary>
        protected bool _inputMovementStop = false;
        /// <summary>
        /// Strafe input axis, i.e. sideward movement
        /// </summary>
        protected float _inputStrafe = 0;
        /// <summary>
        /// If true, strafing started this frame
        /// </summary>
        protected bool _inputStrafeStart = false;
        /// <summary>
        /// First input for forward movement via two combined inputs
        /// </summary>
        protected bool _inputMoveForwardHalf1 = false;
        protected bool _inputMoveForwardHalf1Start = false;
        protected bool _inputMoveForwardHalf1Stop = false;
        /// <summary>
        /// Second input for forward movement via two combined inputs
        /// </summary>
        protected bool _inputMoveForwardHalf2 = false;
        protected bool _inputMoveForwardHalf2Start = false;
        protected bool _inputMoveForwardHalf2Stop = false;
        /// <summary>
        /// If true, rotation should be modified
        /// </summary>
        protected bool _inputRotationModifier = false;
        /// <summary>
        /// If true, rotation modification started this frame
        /// </summary>
        protected bool _inputRotationModifierStart = false;
        /// <summary>
        /// Jump input
        /// </summary>
        protected bool _inputJump = false;
        /// <summary>
        /// Sprint input (run speed modifier)
        /// </summary>
        protected bool _inputSprint = false;
        /// <summary>
        /// If true, the character should dive while swimming
        /// </summary>
        protected bool _inputDive;
        /// <summary>
        /// If true, the character should surface while swimming
        /// </summary>
        protected bool _inputSurface = false;
        /// <summary>
        /// If true, autorunning is toggled
        /// </summary>
        protected bool _inputToggleAutorunning = false;
        /// <summary>
        /// If true, walking is toggled
        /// </summary>
        protected bool _inputToggleWalking = false;
		/// <summary>
		/// If true, crouching is toggled
		/// </summary>
        protected bool _inputToggleCrouching = false;
        #endregion Input values

        protected virtual void Awake() {   
            _rpgMotor = GetComponent<RPGMotor>();
            _inputActions = RPGInputManager.GetInputActions();
        }

        protected virtual void Start() {   
            GetInputs(LogInputWarnings);
            SetupInputActionCallbacks();
        }

        protected virtual void Update() {
            GetInputs();
        }

        /// <summary>
        /// Enables the controller
        /// </summary>
        public virtual void Enable() {
            this.enabled = true;
        }

        /// <summary>
        /// Disables the controller, including physics like sliding. For disabling only the user input, use ActivateCharacterControl
        /// </summary>
        public virtual void Disable() {
            this.enabled = false;
        }

        /// <summary>
        /// Checks if forward movement via two combined inputs starts this frame 
        /// </summary>
        /// <returns>True if combined input forward movement starts this frame, otherwise false</returns>
        protected virtual bool CombinedMoveForwardStart() {
            return _inputMoveForwardHalf1Start && _inputMoveForwardHalf2
                    || _inputMoveForwardHalf1 && _inputMoveForwardHalf2Start;
        }

        /// <summary>
        /// Checks if forward movement via two combined inputs stops this frame 
        /// </summary>
        /// <returns>True if combined input forward movement stops this frame, otherwise false</returns>
        protected virtual bool CombinedMoveForwardStop() {
            return _inputMoveForwardHalf1Stop && _inputMoveForwardHalf2
                    || _inputMoveForwardHalf1 && _inputMoveForwardHalf2Stop;
        }

        /// <summary>
        /// Checks if rotation modification starts this frame
        /// </summary>
        /// <returns>True if rotation modification starts this frame, otherwise false</returns>
        protected virtual bool RotationModificationStart() {
            return _inputRotationModifierStart && _inputMovement.x != 0
                    || _inputRotationModifier && _inputMovementStart;
        }

        /// <summary>
        /// Tries to get the inputs used by this script. If logWarnings is true, warnings are logged if inputs of Unity's legacy input system are not set up
        /// </summary>
        /// <param name="logWarnings">If true and Unity's legacy input system is used, warnings are logged whenever an input could not be found</param>
        protected virtual void GetInputs(bool logWarnings = false) {
            if (UseNewInputSystem) {
                // Poll inputs
                // Movement input
                _inputMovement = _inputActions.Character.Movement.ReadValue<Vector2>();
                
                // Strafe input
                _inputStrafe = _inputActions.Character.Strafe.ReadValue<float>();
                
                // Rotation modifier input
                _inputRotationModifier = _inputActions.Character.RotationModifier.ReadValue<float>() > 0;

                // Inputs for forward movement via two combined inputs
                _inputMoveForwardHalf1 = _inputActions.Character.MoveForwardHalf1.ReadValue<float>() > 0;
                _inputMoveForwardHalf2 = _inputActions.Character.MoveForwardHalf2.ReadValue<float>() > 0;
                
                // Action inputs
                _inputSprint = _inputActions.Character.Sprint.ReadValue<float>() > 0;
                _inputDive = _inputActions.Character.Dive.ReadValue<float>() > 0;
                _inputSurface = _inputActions.Character.Surface.ReadValue<float>() > 0;
            } else {
                // Try to get Unity legacy inputs
                // Movement input
                float inputVerticalMovement = Utils.TryGetAxis(Utils.InputPhase.Raw, "Vertical", logWarnings);
                _inputMovementStart = Utils.TryGetButton(Utils.InputPhase.Down, "Vertical", logWarnings);
                _inputMovementStop = Utils.TryGetButton(Utils.InputPhase.Up, "Vertical", logWarnings);
                float inputRotation = Utils.TryGetAxis(Utils.InputPhase.Raw, "Horizontal", logWarnings);
                _inputMovement = new Vector2(inputRotation, inputVerticalMovement);
                
                // Strafe input
                _inputStrafe = Utils.TryGetAxis(Utils.InputPhase.Raw, "Strafe", logWarnings);
                _inputStrafeStart = Utils.TryGetButton(Utils.InputPhase.Down, "Strafe", logWarnings);
                
                // Rotation modifier input
                _inputRotationModifier = Utils.TryGetButton(Utils.InputPhase.Pressed, "Fire2", logWarnings);
                _inputRotationModifierStart = Utils.TryGetButton(Utils.InputPhase.Down, "Fire2", logWarnings);
                
                // Inputs for forward movement via two combined inputs
                _inputMoveForwardHalf1 = Utils.TryGetButton(Utils.InputPhase.Pressed, "Fire1", logWarnings);
                _inputMoveForwardHalf1Start = Utils.TryGetButton(Utils.InputPhase.Down, "Fire1", logWarnings);
                _inputMoveForwardHalf1Stop = Utils.TryGetButton(Utils.InputPhase.Up, "Fire1", logWarnings);
                _inputMoveForwardHalf2 = Utils.TryGetButton(Utils.InputPhase.Pressed, "Fire2", logWarnings);
                _inputMoveForwardHalf2Start = Utils.TryGetButton(Utils.InputPhase.Down, "Fire2", logWarnings);
                _inputMoveForwardHalf2Stop = Utils.TryGetButton(Utils.InputPhase.Up, "Fire2", logWarnings);

                // Action inputs
                _inputJump = Utils.TryGetButton(Utils.InputPhase.Down, "Jump", logWarnings);
                _inputSprint = Utils.TryGetButton(Utils.InputPhase.Pressed, "Sprint", logWarnings);
                _inputDive = Utils.TryGetButton(Utils.InputPhase.Pressed, "Dive", logWarnings);
                _inputSurface = Utils.TryGetButton(Utils.InputPhase.Pressed, "Jump", logWarnings);
                _inputToggleAutorunning = Utils.TryGetButton(Utils.InputPhase.Down, "Autorun Toggle", logWarnings);
                _inputToggleWalking = Utils.TryGetButton(Utils.InputPhase.Up, "Walk Toggle", logWarnings);
                _inputToggleCrouching = Utils.TryGetButton(Utils.InputPhase.Down, "Crouch Toggle", logWarnings);
            }
        }

        /// <summary>
        /// Sets up all input action callbacks
        /// </summary>
        protected virtual void SetupInputActionCallbacks() {
            if (!UseNewInputSystem) {
                return;
            }
            // Movement input
            _inputActions.Character.Movement.started += context => _inputMovementStart = true;
            _inputActions.Character.Movement.canceled += context => _inputMovementStop = true;
            
            // Strafe input
            _inputActions.Character.Strafe.started += context => _inputStrafeStart = true;

            // Rotation modifier input
            _inputActions.Character.RotationModifier.started += context => _inputRotationModifierStart = true;

            // Inputs for forward movement via two combined inputs
            _inputActions.Character.MoveForwardHalf1.started += context => _inputMoveForwardHalf1Start = true;
            _inputActions.Character.MoveForwardHalf1.canceled += context => _inputMoveForwardHalf1Stop = true;
            _inputActions.Character.MoveForwardHalf2.started += context => _inputMoveForwardHalf2Start = true;
            _inputActions.Character.MoveForwardHalf2.canceled += context => _inputMoveForwardHalf2Stop = true;

            // Action inputs
            _inputActions.Character.Jump.performed += context => _inputJump = true;
            _inputActions.Character.Autorun.performed += context => _inputToggleAutorunning = true;
            _inputActions.Character.Walk.performed += context => _inputToggleWalking = true;
            _inputActions.Character.Crouch.performed += context => _inputToggleCrouching = true;
        }

        /// <summary>
        /// Resets all variables which are set by input action callbacks
        /// </summary>
        protected virtual void ConsumeEventInputs() {
            if (!UseNewInputSystem) {
                return;
            }
            _inputRotationModifierStart = false;
            _inputMoveForwardHalf1Start = false;
            _inputMoveForwardHalf1Stop = false;
            _inputMoveForwardHalf2Start = false;
            _inputMoveForwardHalf2Stop = false;
            _inputMovementStart = false;
            _inputMovementStop = false;
            _inputStrafeStart = false;
            _inputJump = false;
            _inputToggleAutorunning = false;
            _inputToggleWalking = false;
            _inputToggleCrouching = false;
        }
    }
}
