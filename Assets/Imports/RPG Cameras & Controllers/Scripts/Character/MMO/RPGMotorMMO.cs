using JohnStairs.RCC.Enums;
using UnityEngine;

namespace JohnStairs.RCC.Character.MMO {
    [RequireComponent(typeof(CharacterController))]
    public class RPGMotorMMO : RPGMotor {
        /// <summary>
        /// The multiplier for computing the walking backwards speed
        /// </summary>
        public float BackwardsSpeedMultiplier = 0.5f;
        /// <summary>
        /// Approximately the degrees per second the character needs for turning into a new direction
        /// </summary>
        public float RotationSpeed = 180.0f;
        /// <summary>
        /// If set to true, the number of allowed moves while in midair/airborne is unlimited
        /// </summary>
        public bool UnlimitedMidairMoves = false;
        /// <summary>
        /// The number of allowed movement impulses while in midair/airborne
        /// </summary>
        public int AllowedMidairMoves = 1;

        /// <summary>
        /// Rotation around the local Y axis which should be performed in the next frame
        /// </summary>    
        protected float _yRotation = 0;
        /// <summary>
        /// Total degrees the character rotated around the local Y axis
        /// </summary>
        protected float _yRotationDegrees = 0;
        /// <summary>
        /// True if the character should start diving
        /// </summary>
        protected bool _startDiving = false;
        /// <summary>
        /// True if the character should stop diving
        /// </summary>
        protected bool _stopDiving = false;
        /// <summary>
        /// True if airborne movement is currently allowed
        /// </summary>
        protected bool _allowMidairMovement = false;
        /// <summary>
        /// Number of already performed moves during the current midair period
        /// </summary>
        protected int _midairMovesCount = 0;

        public override void StartMotor() {
            base.StartMotor();

            if (!_canMove) {
                // Prevent any movement caused by the controller
                _movementDirection.x = 0;
                _movementDirection.z = 0;
                if (EnableSliding && !_swimming) {
                    // Still apply sliding
                    ApplySliding();
                }
            } else if (_grounded || _enable3dMovement) {
                // GROUNDED AND 3D MOVEMENT BEHAVIOR
                // Reset the counter for the number of remaining midair jumps
                _midairJumpsCount = 0;
                // Reset the counter for the number of remaining midair moves
                _midairMovesCount = 0;
                // Reset the running jump flag
                _runningJump = false;

                if (_autorunning) {
                    _inputDirection.z = 1.0f;
                }

                if (IsCombatLockActive()) {
                    RotateTowards(_characterInfo?.GetRotationTowardsTarget() ?? Quaternion.identity);
                }

                // Transform the local movement direction to world space
                _movementDirection = transform.TransformDirection(_inputDirection);

                // Normalize the player's movement direction
                if (_movementDirection.magnitude > 1) {
                    _movementDirection = Vector3.Normalize(_movementDirection);
                }

                #region Calculate the movement speed
                float resultingSpeed = RunSpeed;
                // Compute the speed combined of strafe and run speed
                if (_inputDirection.x != 0 || _inputDirection.z != 0) {
                    resultingSpeed = ((StrafeSpeed + (_characterInfo?.GetMovementSpeedInfluence(StrafeSpeed) ?? 0)) * Mathf.Abs(_inputDirection.x)
                            + (RunSpeed + (_characterInfo?.GetMovementSpeedInfluence(RunSpeed) ?? 0)) * Mathf.Abs(_inputDirection.z))
                            / (Mathf.Abs(_inputDirection.x) + Mathf.Abs(_inputDirection.z));
                }

                resultingSpeed = ApplyMovementSpeedMultipliers(resultingSpeed);
                // Adjust the speed if moving backwards and not walking
                if (_inputDirection.z < 0 && !_walking) {
                    resultingSpeed *= BackwardsSpeedMultiplier;
                }
                #endregion Calculate the movement speed

                // Apply the resulting movement speed
                _movementDirection *= resultingSpeed;

                if (_enable3dMovement) {
                    if (_swimming) {
                        PreventSwimmingAboveSurface();
                    }

                    if (_surface) {
                        _surface = false;
                        // Character should surface
                        _movementDirection.y = resultingSpeed;

                        if (_swimming) {
                            if (EnableSwimmingJumps) {
                                if (_animator && IsCloseToWaterSurface()) {
                                    _animator.SetTrigger("Jump");
                                }
                            } else {
                                PreventSwimmingAboveSurface();
                            }
                        }
                    }
                } else {
                    // Set the falling threshold
                    SetFallingThreshold();

                    if (_jump) {
                        // The character is not swimming and should jump this frame
                        PerformJump();
                    }
                }

                if (EnableSliding && !_swimming) {
                    // Apply sliding
                    ApplySliding();
                }
            } else {
                // MIDAIR BEHAVIOR
                if (_allowMidairMovement) {
                    // Movement while airborne is possible during this frame
                    _allowMidairMovement = false;

                    if (_inputDirection.magnitude > 0 &&
                        (EnableMidairMovement == MidairMovement.Always
                            || EnableMidairMovement == MidairMovement.OnlyAfterStandingJump && !_runningJump)) {
                        Vector3 inputDirectionWorld = transform.TransformDirection(_inputDirection);

                        // Normalize the player's movement direction
                        if (inputDirectionWorld.magnitude > 1) {
                            inputDirectionWorld = Vector3.Normalize(inputDirectionWorld);
                        }

                        // Apply the airborne speed
                        inputDirectionWorld *= MidairSpeed;
                        // Set the x and z direction to move the character
                        _movementDirection.x = inputDirectionWorld.x;
                        _movementDirection.z = inputDirectionWorld.z;
                    }
                }

                if (_jump) {
                    PerformJump();
                }
            }

            ApplyGravity();

            // Move the character
            Move(_movementDirection * Time.deltaTime);

            #region Rotate the character
            if (_canRotate && !IsCombatLockActive()) {
                // Rotate the character along the global Y axis
                transform.Rotate(Vector3.up * _yRotation, Space.World);

                if (_enable3dMovement && (!DiveOnlyWhenSwimmingForward || _inputDirection.z != 0)) {
                    // Character is swimming and should dive => clamp the character's local X axis rotation between [-89.5, 89.5] (euler angles)
                    if (transform.eulerAngles.x > 180.0f) {
                        if (transform.eulerAngles.x + _xRotation < 270.5f) {
                            _xRotation = Mathf.Min(270.5f - _xRotation, 270.5f - transform.eulerAngles.x);
                        }
                    } else {
                        if (transform.eulerAngles.x + _xRotation > 89.5f) {
                            _xRotation = Mathf.Min(89.5f - _xRotation, 89.5f - transform.eulerAngles.x);
                        }
                    }

                    // Rotate the character around the local X axis
                    transform.Rotate(Vector3.right * _xRotation, Space.Self);
                }
            }
            #endregion Rotate the character

            // Pass values to the animator
            SetAnimatorParameters(_animator);
            // Store the rotated degrees
            _yRotationDegrees = _yRotation;
            // Reset input character rotations
            _yRotation = 0;
            _xRotation = 0;
        }

        public override void SetAnimatorParameters(Animator animator) {
            if (!animator) {
                return;
            }

            base.SetAnimatorParameters(animator);

            animator.SetFloat("Input Direction X", _inputDirection.x, 0.1f, Time.deltaTime);
            animator.SetFloat("Input Direction Z", _inputDirection.z, 0.1f, Time.deltaTime);
            animator.SetFloat("Input Magnitude", _inputDirection.magnitude);
            animator.SetFloat("Turning Direction", _yRotation, 0.1f, Time.deltaTime);
        }

        /// <summary>
        /// Signals the motor to try a midair movement 
        /// </summary>
        /// <param name="movement">If a midair movement should be tried or not</param>
        public virtual void TryMidairMovement(bool movement) {
            if (EnableMidairMovement == MidairMovement.Never) {
                return;
            }

            if (UnlimitedMidairMoves) {
                _allowMidairMovement = true;
                return;
            }

            // Allow midair movement for the current frame and increase the midair moves counter
            if (movement && _midairMovesCount < AllowedMidairMoves) {
                _allowMidairMovement = true;
                _midairMovesCount++;
            }
        }

        /// <summary>
        /// Signalizes the motor if the character should start to dive while swimming
        /// </summary>
        /// <param name="allow">If true, diving is allowed</param>
        public virtual void StartDiving(bool allow) {
            _startDiving = allow && _canMove;
        }

        /// <summary>
        /// Returns if the character is allowed to start diving during this frame
        /// </summary>
        /// <returns>True if the character is allowed to dive</returns>
        public virtual bool StartsDiving() {
            return _startDiving;
        }

        /// <summary>
        /// Signalizes the motor if the character should stop diving
        /// </summary>
        /// <param name="stop">If true, diving is not allowed</param>
        public virtual void StopDiving(bool stop) {
            _stopDiving = stop;
        }

        /// <summary>
        /// Returns if the character should stop diving during this frame
        /// </summary>
        /// <returns>True if the character should stop diving</returns>
        public virtual bool StopsDiving() {
            return _stopDiving;
        }

        /// <summary>
        /// Gets the degrees the character is currently rotated around the local Y axis
        /// </summary>
        /// <returns>Value of internal variable _yRotationDegrees</returns>
        public virtual float GetYRotationDegrees() {
            return _yRotationDegrees;
        }

        /// <summary>
        /// Returns if the character moves forward or backward in this frame
        /// </summary>
        /// <returns>True if the character moves forward or backward</returns>
        public virtual bool HasVerticalMovementInput() {
            return _inputDirection.z != 0;
        }

        /// <summary>
        /// Rotates the character around a given rotation axis by input and speed
        /// </summary>
        /// <param name="rotationAxis">Rotation axis to use</param>
        /// <param name="input">Input value for the rotation</param>
        /// <param name="speed">Rotation speed</param>
        public virtual void RotateAroundAxis(Axis rotationAxis, float input, float speed = -1) {
            if (_characterInfo?.LockOnTarget() ?? false) {
                _inputDirection.x = input;
                return;
            }

            if (speed < 0) {
                speed = RotationSpeed * Time.deltaTime;
            }

            if (rotationAxis == Axis.X) {
                _xRotation += input * speed;
            } else if (rotationAxis == Axis.Y) {
                _yRotation += input * speed;
            }
        }

        public override void RotateTowards(Quaternion lookAtRotation) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, RotationSpeed);
        }

        public override void TeleportTo(Transform target, bool withYrotation = false) {
            if (!Time.inFixedTimeStep) {
                _characterController.enabled = false;
            }
            transform.position = target.position;
            if (withYrotation) {
                transform.rotation = Quaternion.Euler(new Vector3(0, target.rotation.eulerAngles.y, 0));
            }
            UpdateColliderBottom();
            if (!Time.inFixedTimeStep) {
                _characterController.enabled = true;
            }
        }
    }
}
