using UnityEngine;
using System.Collections;
using JohnStairs.RCC.Enums;

namespace JohnStairs.RCC.Character.MMO {
    [RequireComponent(typeof(RPGViewFrustum))]
    public class RPGCameraMMO : RPGCamera {
        /// <summary>
        /// Determines if the camera should also rotate when the character starts to rotate. If set to "PreventOnInput", input "Pause Rotation With Character" is used for pausing rotation 
        /// </summary>
        public RotationWithCharacter RotateWithCharacter = RotationWithCharacter.PreventOnInput;
        /// <summary>
        /// Determines when the character's view/walking direction should be set to the camera's view direction. If set to "OnAlignmentInput", input "Align Character" is used to trigger the alignment
        /// </summary>
        public CharacterAlignment AlignCharacter = CharacterAlignment.OnAlignmentInput;
        /// <summary>
        /// Controls the character-to-camera turning speed if "AlignCharacter" is not set to "Never"
        /// </summary>
        public float AlignCharacterSpeed = 10.0f;
        /// <summary>
        /// If set to true, the camera view direction aligns with the character's view/walking direction once the character starts to move (forward/backwards/strafe)
        /// </summary>
        public bool AlignCameraWhenMoving = true;
        /// <summary>
        /// Let the camera face the front of the character when walking backwards. Needs "AlignCameraWhenMoving" to be true
        /// </summary>
        public bool SupportWalkingBackwards = true;
        /// <summary>
        /// The time needed to align with the character if "AlignCameraWhenMoving" is set to true
        /// </summary>
        public float AlignCameraSmoothTime = 0.2f;

        /// <summary>
        /// Reference to the MMO RPGMotor script
        /// </summary>
        protected RPGMotorMMO _rpgMotorMMO;        
        /// <summary>
        /// True if AlignCameraWhenMoving was disabled at runtime to track if it should be enabled again at a later time
        /// </summary>
        protected bool _alignCameraWhenMovingDisabled;
        /// <summary>
        /// If true, the character's Y rotation was already aligned with the camera 
        /// </summary>
        protected bool _characterYrotationAligned = false;
        /// <summary>
        /// If true, the character is currently turning to the direction the camera is facing
        /// </summary>
        protected bool _turningRoutineStarted = false;
        /// <summary>
        /// Currently running character turning routine to fit the camera's Y [and X] axis rotation
        /// </summary>
        protected Coroutine _turningCoroutine;
        /// <summary>
        /// True if the character swam the last frame
        /// </summary>
        protected bool _swamLastFrame;
        /// <summary>
        /// True if the character flew the last frame
        /// </summary>
        protected bool _flewLastFrame;
        #region Input values
        /// <summary>
        /// Input to align the character with the camera view direction
        /// </summary>
        protected bool _inputAlignCharacter = false;
        /// <summary>
        /// If true, pressing _inputAlignCharacter started this frame
        /// </summary>
        protected bool _inputAlignCharacterStart = false;
        /// <summary>
        /// If true, pressing _inputAlignCharacter stopped this frame
        /// </summary>
        protected bool _inputAlignCharacterStop = false;
        /// <summary>
        /// Input for preventing camera rotation in cases the camera implicitely rotates with the character, e.g. because the character rotates
        /// </summary>
        protected bool _inputPauseRotationWithCharacter = false;
        #endregion Input values

        /// <summary>
        /// Enum for controlling if the camera should rotate together with the character
        /// </summary>
        public enum RotationWithCharacter {
            /// <summary>
            /// Never rotate with the character
            /// </summary>
            Never,
            /// <summary>
            /// The rotation stops when the stopping input is pressed. The input can be set inside the RPGCamera
            /// </summary>
            PreventOnInput,
            /// <summary>
            /// Always rotate together with the character
            /// </summary>
            Always
        }

        /// <summary>
        /// Enum for starting turning routines only along certain character axes
        /// </summary>
        public enum TurningRotation {
            BothAxes,
            OnlyYaxis,
            ResetXaxis
        }

        /// <summary>
        /// Enum for controlling the alignment of the character with the camera view
        /// </summary>
        public enum CharacterAlignment {
            /// <summary>
            /// Never align the character with the camera
            /// </summary>
            Never,
            /// <summary>
            /// Only align when the alignment input set inside the RPGCamera scripts is pressed
            /// </summary>
            OnAlignmentInput,
            /// <summary>
            /// Always align the character with the camera
            /// </summary>
            Always
        }

        protected override void Awake() {
            base.Awake();

            _rpgMotorMMO = GetComponent<RPGMotorMMO>();
        }

        protected override void LateUpdate() {
            base.LateUpdate();

            // Make AlwaysRotateCamera and AlignCameraWhenMoving mutual exclusive
            if (AlwaysRotateCamera && AlignCameraWhenMoving) {
                // Turn off AlignCameraWhenMoving to prevent interference with AlwaysRotateCamera logic
                AlignCameraWhenMoving = false;
                _alignCameraWhenMovingDisabled = true;
            } else if (!AlwaysRotateCamera && _alignCameraWhenMovingDisabled) {
                // Turn AlignCameraWhenMoving back on
                AlignCameraWhenMoving = true;
                _alignCameraWhenMovingDisabled = false;
            }

            // Check if the camera's Y rotation is contrained by terrain
            bool enableCameraLookUp = !(_rpgMotorMMO?.IsSwimming() ?? false)
                                        && _rpgViewFrustum.IsTouchingGround(CameraToUse, _cameraPivotPosition);

            float smoothTime = RotationSmoothTime;
            float rotationYMinLimit = _rotationY;
            bool combatLock = _characterInfo?.LockOnTarget() ?? false;
            bool enable3dMovement = _rpgMotor?.Is3dMovementEnabled() ?? false;
            bool isSwimming = _rpgMotor?.IsSwimming() ?? false;
            bool stoppedSwimming = _swamLastFrame && !isSwimming;
            bool isFlying = _rpgMotor?.IsFlying() ?? false;
            bool stoppedFlying = _flewLastFrame && !isFlying;

            #region Process rotation input axes
            if (ActivateCameraControl
                && !_orbitingStartedOverGUI
                && (_inputAllowOrbiting || _inputAllowOrbitingWithCharRotation || AlwaysRotateCamera)) {
                bool alsoRotateCharacter = (_characterInfo?.CanRotate() ?? true)
                                        && (_inputAllowOrbitingWithCharRotation
                                        || AlignCharacter == CharacterAlignment.Always
                                        || AlignCharacter == CharacterAlignment.OnAlignmentInput && _inputAlignCharacter);

                #region Rotation X input processing
                if (!LockRotationX) {
                    float rotationXinput = 0;

                    rotationXinput = (InvertRotationX ? 1 : -1) * _inputRotationAmount.x;

                    if (alsoRotateCharacter
                        && !(_pointerInfo?.IsPointerOverGUI() ?? false)) {
                        // Let the character rotate according to the rotation X axis input
                        _rpgMotorMMO?.RotateAroundAxis(Axis.Y, rotationXinput, RotationXSensitivity);
                    } else {
                        // Allow the camera to orbit
                        _rotationX += rotationXinput * RotationXSensitivity;
                    }

                    if (ConstrainRotationX) {
                        // Clamp the rotation in X axis direction
                        _rotationX = Mathf.Clamp(_rotationX, RotationXMin, RotationXMax);
                    }
                }
                #endregion Rotation X input processing

                #region Rotation Y input processing
                if (!LockRotationY) {
                    _desiredRotationY += (InvertRotationY ? -1 : 1) * _inputRotationAmount.y * RotationYSensitivity;

                    if (alsoRotateCharacter
                        && enable3dMovement
                        && !(_pointerInfo?.IsPointerOverGUI() ?? false)) {
                        _rpgMotorMMO.RotateAroundAxis(Axis.X, (InvertRotationY ? -1 : 1) * _inputRotationAmount.y, RotationYSensitivity);
                    }
                }

                // Check if a look-up should be performed because _rotationY is constrained by terrain
                if (enableCameraLookUp) {
                    _rotationY = Mathf.Clamp(_desiredRotationY, Mathf.Max(rotationYMinLimit, RotationYMin), RotationYMax);
                    // Set the desired rotation Y rotation to compute the degrees of looking up with the camera
                    _desiredRotationY = Mathf.Max(_desiredRotationY, _rotationY - 90.0f);
                } else {
                    // Clamp the rotation between the maximum values
                    _rotationY = Mathf.Clamp(_desiredRotationY, RotationYMin, RotationYMax);
                }

                _desiredRotationY = Mathf.Clamp(_desiredRotationY, RotationYMin, RotationYMax);
                #endregion Rotation Y input processing
            }
            #endregion Process rotation input axes

            #region Character alignment using a turning coroutine
            // Check the character alignment mode
            if (_rpgMotorMMO
                && (_characterInfo?.CanRotate() ?? true)
                && !combatLock) {
                bool startDiving = _rpgMotorMMO.StartsDiving()
                                    || _rpgMotorMMO.StartedAutorunning();

                bool stopDiving = _rpgMotorMMO.DiveOnlyWhenSwimmingForward &&
                                    (_rpgMotorMMO.StopsDiving()
                                    || _rpgMotorMMO.StoppedAutorunning());

                if (AlignCharacter == CharacterAlignment.Always) {
                    // Always align the character with the camera view direction
                    if (!_characterYrotationAligned) {
                        // Character's Y rotation needs to be aligned
                        AlignCharacterWithCamera(TurningRotation.OnlyYaxis);
                        _characterYrotationAligned = true;
                    }

                    if (enable3dMovement) {
                        if (startDiving) {
                            AlignCharacterWithCamera(TurningRotation.BothAxes);
                        } else if (stopDiving) {
                            AlignCharacterWithCamera(TurningRotation.ResetXaxis);
                        }
                    }
                } else if (AlignCharacter == CharacterAlignment.OnAlignmentInput
                            && !(_pointerInfo?.IsPointerOverGUI() ?? false)) {
                    // Align the character only when the alignment input is pressed
                    if (enable3dMovement) {
                        if (_inputAlignCharacter && startDiving) {
                            // Align both axis rotation of the character to the camera to start diving
                            AlignCharacterWithCamera(TurningRotation.BothAxes);
                        } else if (stopDiving) {
                            // Reset the X axis rotation every time the character stops diving
                            AlignCharacterWithCamera(TurningRotation.ResetXaxis);
                        } else if (_inputAlignCharacterStart && _rpgMotorMMO.HasVerticalMovementInput()) {
                            // Alignment input started while the character moves forward => dive in the new direction
                            AlignCharacterWithCamera(TurningRotation.BothAxes);
                        } else if (_inputAlignCharacterStart) {
                            AlignCharacterWithCamera(TurningRotation.OnlyYaxis);
                        }
                    } else {
                        if (!_characterYrotationAligned && _inputAlignCharacterStart) {
                            // Character's Y rotation needs to be aligned
                            AlignCharacterWithCamera(TurningRotation.OnlyYaxis);
                            _characterYrotationAligned = true;
                        } else if (_inputAlignCharacterStop) {
                            _characterYrotationAligned = false;
                        }
                    }
                } else if (AlignCharacter == CharacterAlignment.Never) {
                    // Never align the character with the camera
                    if (enable3dMovement && stopDiving) {
                        // Only take care that the character X axis rotation is reset after diving
                        AlignCharacterWithCamera(TurningRotation.ResetXaxis);
                    }
                }

                if (stoppedSwimming || stoppedFlying) {
                    if (_turningCoroutine != null) {
                        StopCoroutine(_turningCoroutine);
                    }
                    _turningRoutineStarted = false;
                }
            }
            #endregion Character alignment using a turning coroutine

            // Check if the camera should stay in place and not rotate with the character
            float deltaX = _rpgMotorMMO?.GetYRotationDegrees() ?? 0;
            if (deltaX != 0
                && !_inputAllowOrbitingWithCharRotation) {
                // The character turns and does not strafe => check the RotateWithCharacter value
                if (RotateWithCharacter == RotationWithCharacter.Never
                    || RotateWithCharacter == RotationWithCharacter.PreventOnInput && _inputPauseRotationWithCharacter) {
                    // Counter the character's rotation so that the camera stays in place
                    _rotationX -= deltaX;
                    _rotationXSmooth -= deltaX;
                }
            }

            if (ActivateCameraControl) {
                ComputeDesiredDistance();
            }

            #region Camera alignment with the character
            if (_rpgMotorMMO
                && AlignCameraWhenMoving
                && !_turningRoutineStarted) {
                // Get the input direction set by the controller
                Vector3 inputDirection = _rpgMotorMMO.GetInputDirection();

                if ((inputDirection.z != 0 || inputDirection.x != 0 || combatLock)
                    && !_inputAllowOrbiting
                    && !_inputAllowOrbiting) {
                    // Align the camera
                    AlignCameraWithCharacter(!SupportWalkingBackwards
                                                || inputDirection.z > 0
                                                || inputDirection.x != 0);
                    smoothTime = AlignCameraSmoothTime;
                }
            }
            #endregion Camera alignment with the character

            _rotationXSmooth = Mathf.SmoothDamp(_rotationXSmooth, _rotationX, ref _rotationXCurrentVelocity, smoothTime);
            _rotationYSmooth = Mathf.SmoothDamp(_rotationYSmooth, _rotationY, ref _rotationYCurrentVelocity, smoothTime);

            // Compute the new camera position            
            CameraToUse.transform.position = ComputeNewCameraPosition();
            // Check if we are in third or first person and adjust the camera rotation behavior
            if (_distanceSmooth > 0.1f) {
                // In third person => orbit camera
                CameraToUse.transform.LookAt(_cameraPivotPosition);
            } else {
                // In first person => normal camera rotation
                Quaternion characterRotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0));
                Quaternion cameraRotation = Quaternion.Euler(new Vector3(_rotationYSmooth, _rotationXSmooth, 0));
                CameraToUse.transform.rotation = characterRotation * cameraRotation;
            }

            if (enableCameraLookUp /*|| _distanceSmooth <= 0.1f*/) {
                // Camera lies on terrain => enable looking up			
                float lookUpDegrees = _desiredRotationY - _rotationY;
                CameraToUse.transform.Rotate(Vector3.right, lookUpDegrees);
            }

            _flewLastFrame = isFlying;
            _swamLastFrame = isSwimming;

            ConsumeEventInputs();
        }

        /// <summary>
        /// Tries to get the inputs used by this script and logs warnings if they could not be found if logWarnings is true
        /// </summary>
        /// <param name="logWarnings">If true, warnings are logged whenever an input could not be found</param>
        protected override void GetInputs(bool logWarnings = false) {
            base.GetInputs(logWarnings);

            if (UseNewInputSystem) {
                _inputAlignCharacter = _inputActions.Character.AlignCharacter.ReadValue<float>() > 0;
                _inputPauseRotationWithCharacter = _inputActions.Character.PauseRotationWithCharacter.ReadValue<float>() > 0;
            } else {
                _inputAlignCharacter = Utils.TryGetButton(Utils.InputPhase.Pressed, "Align Character", logWarnings);
                _inputAlignCharacterStart = Utils.TryGetButton(Utils.InputPhase.Down, "Align Character", logWarnings);
                _inputAlignCharacterStop = Utils.TryGetButton(Utils.InputPhase.Up, "Align Character", logWarnings);
                _inputPauseRotationWithCharacter = Utils.TryGetButton(Utils.InputPhase.Pressed, "Pause Rotation With Character", logWarnings);
            }
        }

        protected override void SetUpInputActionCallbacks() {
            base.SetUpInputActionCallbacks();

            _inputActions.Character.AlignCharacter.started += context => _inputAlignCharacterStart = true;
            _inputActions.Character.AlignCharacter.canceled += context => _inputAlignCharacterStop = true;
        }

        protected override void ConsumeEventInputs() {
            base.ConsumeEventInputs();

            _inputAlignCharacterStart = false;
            _inputAlignCharacterStop = false;
        }

        protected override Vector3 ComputeCameraPosition(float xAxisDegrees, float yAxisDegrees, float distance) {
            Vector3 offset = Vector3.zero;

            // Project the character's X axis onto the X-Z plane
            Vector3 charXaxisMappedToGroundLayer = transform.right;
            charXaxisMappedToGroundLayer.y = 0;
            charXaxisMappedToGroundLayer.Normalize();

            // Retrieve the projected, negative forward vector of the character
            offset = Vector3.Cross(Vector3.up, charXaxisMappedToGroundLayer);
            // Apply the given distance
            offset *= distance;

            // Create the combined rotation of X and Y axis rotation
            Quaternion rotXaxis = Quaternion.AngleAxis(xAxisDegrees, charXaxisMappedToGroundLayer);
            Quaternion rotYaxis = Quaternion.AngleAxis(yAxisDegrees, Vector3.up);
            Quaternion rotation = rotYaxis * rotXaxis;

            return _cameraPivotPosition + rotation * offset;
        }

        protected override Vector3 ComputePivotPosition(float yAxisDegrees) {
            Quaternion rotYaxis = Quaternion.AngleAxis(yAxisDegrees + transform.eulerAngles.y, Vector3.up);
            return transform.position + rotYaxis * CameraPivotLocalPosition;
        }

        public override void MoveTo(Vector3 newPosition, bool smoothTransition = false) {
            Vector3 lookDirection = _cameraPivotPosition - newPosition;
            float distance = lookDirection.magnitude;

            float rotationX = Utils.SignedAngle(transform.forward, lookDirection, Vector3.up);
            float rotationY = Vector3.Angle(Vector3.up, lookDirection) - 90.0f;

            SetPosition(rotationX, rotationY, distance, smoothTransition);
        }

        /// <summary>
        /// Aligns the camera with the character depending on behindCharacter. If behindCharacter is true, the camera aligns 
        /// behind the character, otherwise it aligns so that it faces the character's front
        /// </summary>
        /// <param name="behindCharacter">If true, the camera aligns behind the character, otherwise it aligns so that it faces the character's front</param>
        protected virtual void AlignCameraWithCharacter(bool behindCharacter) {
            float offsetToCameraRotation = Utils.CustomModulo(_rotationX, 360.0f);
            float targetRotation = behindCharacter ? 0 : 180.0f;

            if (offsetToCameraRotation == targetRotation) {
                // There is no offset to the camera rotation => no alignment computation required
                return;
            }

            int numberOfFullRotations = (int)(_rotationX) / 360;

            if (_rotationX < 0) {
                if (offsetToCameraRotation < -180) {
                    numberOfFullRotations--;
                } else {
                    targetRotation = -targetRotation;
                }
            } else {
                if (offsetToCameraRotation > 180) {
                    // The shortest way to rotate behind the character is to fulfill the current rotation
                    numberOfFullRotations++;
                    targetRotation = -targetRotation;
                }
            }

            _rotationX = numberOfFullRotations * 360.0f + targetRotation;
        }

        /// <summary>
        /// Starts a character turning routine for aligning the character's axes with the camera axes
        /// </summary>
        /// <param name="rotationAxes">Rotation axes which should be considered for alignment</param>
        public virtual void AlignCharacterWithCamera(TurningRotation rotationAxes) {
            if (_turningRoutineStarted) {
                StopCoroutine(_turningCoroutine);
            }

            _turningCoroutine = StartCoroutine(TurningCoroutine(rotationAxes));
        }

        /// <summary>
        /// Routine for turning the character over AlignCharacterSpeed time according to the given TurningRotation.
        /// TurningRotation.ResetXaxis resets the character's X axis rotation to 0 (e.g. when not diving),
        /// whereas the other two align the character's forward vector to the camera's forward vector (with and
        /// without X axis alignment)
        /// </summary>
        /// <param name="rotationAxes">Rotation axes which should be considered for alignment</param>
        /// <returns>Resulting turning coroutine</returns>
        protected virtual IEnumerator TurningCoroutine(TurningRotation rotationAxes) {
            _turningRoutineStarted = true;

            _rotationX = _rotationXSmooth;
            _rotationXCurrentVelocity = 0;

            _rotationY = _rotationYSmooth;
            _rotationYCurrentVelocity = 0;

            Vector3 targetRotation = GetCharacterTargetRotation(rotationAxes);

            while (Quaternion.Angle(transform.rotation, Quaternion.Euler(targetRotation)) > 2.0f) {
                // The difference between the character's orientation and the camera's orientation is greater than 2 degrees
                Quaternion before = transform.rotation;

                // Rotate the character towards the view direction of the camera
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRotation), AlignCharacterSpeed * 100.0f * Time.deltaTime);

                float deltaY = transform.rotation.eulerAngles.y - before.eulerAngles.y;
                if (Mathf.Abs(deltaY) > 0.1f) {
                    // Camera is not completely aligned yet => counter the implicite camera rotation 
                    _rotationX -= deltaY;
                    _rotationXSmooth -= deltaY;
                }

                yield return null;

                // Update the values for the next loop iteration
                targetRotation = GetCharacterTargetRotation(rotationAxes);
            }

            _turningRoutineStarted = false;
        }

        /// <summary>
        /// Get the target rotation euler angles for a turning routine depending on the turning rotation which should be performed
        /// </summary>
        /// <param name="rotationAxes">Rotation axes which should be considered</param>
        /// <returns>The target rotation in euler angles</returns>
        protected virtual Vector3 GetCharacterTargetRotation(TurningRotation rotationAxes) {
            float targetXrotation = 0;
            float targetYrotation = 0;

            if (rotationAxes == TurningRotation.BothAxes) {
                targetXrotation = CameraToUse.transform.eulerAngles.x;
                targetYrotation = CameraToUse.transform.eulerAngles.y;
            } else if (rotationAxes == TurningRotation.OnlyYaxis) {
                targetXrotation = transform.eulerAngles.x;
                targetYrotation = CameraToUse.transform.eulerAngles.y;
            } else if (rotationAxes == TurningRotation.ResetXaxis) {
                targetXrotation = 0;
                targetYrotation = transform.eulerAngles.y;
            }

            return new Vector3(targetXrotation, targetYrotation, 0);
        }
    }
}
