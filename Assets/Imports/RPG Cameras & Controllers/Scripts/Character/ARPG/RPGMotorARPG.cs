using UnityEngine;

namespace JohnStairs.RCC.Character.ARPG {
    [RequireComponent(typeof(CharacterController))]
    public class RPGMotorARPG : RPGMotor {
        /// <summary>
        /// If set to true and while standing, the character begins moving forward after it completely turned into the new direction
        /// </summary>
        public bool CompleteTurnWhileStanding = true;
        /// <summary>
        /// Approximately the time in seconds the character needs for turning into a new direction
        /// </summary>
        public float RotationTime = 0.1f;
        /// <summary>
        /// Time in seconds which has to pass before the character can rotate up/down via camera rotation
        /// </summary>
        public float FlyingTimeout = 0.2f;

        /// <summary>
        /// Direction the character should face
        /// </summary>
        protected Vector3 _facingDirection;
        /// <summary>
        /// Turning direction of the character around the global Y axis
        /// </summary>
        protected float _turningDirectionY;
        /// <summary>
        /// Current rotation velocity for turning the character
        /// </summary>
        protected float _rotationVelocity;
        /// <summary>
        /// Buffer for measuring how long the character is flying
        /// </summary>
        protected float _flyingBuffer;

        protected override void Awake() {
            base.Awake();

            if (!_rpgCamera) {
                Debug.LogWarning("No RPGCamera component attached to game object " + name + ". Please assign the script to use this motor.");
            }
        }

        public override void StartMotor() {
            base.StartMotor();

            if (IsFlying()) {
                _flyingBuffer -= Time.deltaTime;
            } else {
                _flyingBuffer = FlyingTimeout;
            }

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
                // Reset the running jump flag
                _runningJump = false;
                bool inMotionAlready = IsInMotion();

                if (_autorunning) {
                    _inputDirection.z = 1.0f;
                }

                _movementDirection = CalculateMovementDirection();

                if (_movementDirection != Vector3.zero) {
                    _facingDirection = _movementDirection;
                }

                // Calculate the movement speed
                float resultingSpeed = ApplyMovementSpeedMultipliers(RunSpeed);

                // Check if the character should turn first towards the facing direction before moving
                if (CompleteTurnWhileStanding
                    && !inMotionAlready // to prevent stopping and turning while already in motion
                    && !IsLookingInDirection(_facingDirection)) {
                    resultingSpeed = 0;
                }

                // Apply the resulting movement speed
                _movementDirection *= resultingSpeed;

                if (_enable3dMovement) {
                    if (_swimming) {
                        PreventSwimmingAboveSurface();
                    }

                    if (_surface) {
                        // Character should surface
                        _movementDirection.y = resultingSpeed;
                        _inputDirection.y = 1.0f;

                        if (_swimming) {
                            if (IsCloseToWaterSurface()) {
                                _facingDirection = Vector3.Cross(transform.right, Vector3.up);
                            } else {
                                _facingDirection = _movementDirection;
                            }

                            if (EnableSwimmingJumps) {
                                if (_animator && IsCloseToWaterSurface()) {
                                    _animator.SetTrigger("Jump");
                                }
                            } else {
                                PreventSwimmingAboveSurface();
                            }
                        }
                    } else if (_dive) {
                        _movementDirection.y = -resultingSpeed;
                        _inputDirection.y = -1.0f;
                        _facingDirection = _movementDirection;
                    } else if (_inputDirection == Vector3.zero) {
                        _facingDirection = Vector3.Cross(transform.right, Vector3.up);
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
                Vector3 movementDirection = CalculateMovementDirection();
                if (movementDirection != Vector3.zero &&
                    (EnableMidairMovement == MidairMovement.Always
                        || EnableMidairMovement == MidairMovement.OnlyAfterStandingJump && !_runningJump)) {
                    _facingDirection = movementDirection;

                    movementDirection *= MidairSpeed;

                    _movementDirection.x = movementDirection.x;
                    _movementDirection.z = movementDirection.z;
                }

                if (_jump) {
                    PerformJump();
                }
            }

            ApplyGravity();

            // Move the character
            Move(_movementDirection * Time.deltaTime);

            #region Rotate the character
            _turningDirectionY = 0;
            if (!IsLookingInDirection(_facingDirection)) {
                _turningDirectionY = transform.rotation.eulerAngles.y;

                Vector3 upReference = _enable3dMovement && (_surface || _dive) ? Vector3.Cross(_facingDirection, transform.right) : Vector3.up;

                Quaternion targetRotation = Quaternion.LookRotation(_facingDirection, upReference);
                // Smoothly rotate to the target rotation
                float delta = Quaternion.Angle(transform.rotation, targetRotation);
                if (delta >= 180.0f) {
                    // Decrease delta under 180 so that SmoothDampAngle decreases
                    delta = 179.0f;
                }
                float t = Mathf.SmoothDampAngle(delta, 0, ref _rotationVelocity, RotationTime);
                t = 1.0f - t / delta;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);

                _turningDirectionY = transform.rotation.eulerAngles.y - _turningDirectionY;

                if (_turningDirectionY < -180.0f) {
                    _turningDirectionY += 360.0f;
                }
            }

            if (_canRotate) {
                // Rotate the character along the local X axis
                if (_enable3dMovement) {
                    // Character should dive => clamp the character's local X axis rotation between [-89.5, 89.5] (euler angles)
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
            _surface = false;
            _dive = false;
            // Reset the input character X rotation
            _xRotation = 0;
        }

        public override void SetAnimatorParameters(Animator animator) {
            if (!animator) {
                return;
            }

            base.SetAnimatorParameters(animator);

            float inputMagnitude = IsInMotion() ? _inputDirection.magnitude : 0;
            float inputMagnitudeSmoothing = _crouching || _swimming ? 0.1f : 0;
            animator.SetFloat("Input Magnitude", inputMagnitude, inputMagnitudeSmoothing, Time.deltaTime);
            animator.SetFloat("Turning Direction", _turningDirectionY, 0.1f, Time.deltaTime);

            animator.SetBool("Crouching", _crouching);
        }

        /// <summary>
        /// Calculates the movement direction based on the perspective of the used camera and the given input
        /// </summary>
        /// <returns>The resulting movement direction</returns>
        protected virtual Vector3 CalculateMovementDirection() {
            Vector3 movementDirection = Vector3.zero;

            if (IsCombatLockActive()) {
                Vector3 forward = (_characterInfo.GetTargetPosition() - transform.position).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward);

                movementDirection = right * _inputDirection.x + forward * _inputDirection.z;
            } else if (_rpgCamera && _rpgCamera.CameraToUse) {
                Vector3 right = _rpgCamera.CameraToUse.transform.right;
                //Vector3 forward = Vector3.Cross(right, _enable3dMovement ? _rpgCamera.CameraToUse.transform.up : Vector3.up);
                Vector3 forward = Vector3.Cross(right, _swimming || _flyingBuffer < 0 ? _rpgCamera.CameraToUse.transform.up : Vector3.up);

                movementDirection = right * _inputDirection.x + forward * _inputDirection.z;
            }

            // Normalize the player's movement direction
            if (movementDirection.magnitude > 1) {
                movementDirection = Vector3.Normalize(movementDirection);
            }

            return movementDirection;
        }

        public override void RotateTowards(Quaternion lookAtRotation) {
            _facingDirection = lookAtRotation.eulerAngles;
        }

        /// <summary>
        /// Aligns the character with the view direction of the camera. Also called by the RPGBuilder integration
        /// </summary>
        public virtual void AlignWithCamera() {
            _facingDirection = Vector3.Cross(_rpgCamera.CameraToUse.transform.right, Vector3.up);
        }

        public override void Rotate(Quaternion rotation) {
            base.Rotate(rotation);
            if (!IsCombatLockActive() && !IsInMotion()) {
                _facingDirection = transform.forward;
            }
        }

        /// <summary>
        /// Teleports the character to the Transform target. If withYrotation is set to true, the character's 
        /// Y axis rotation is additionally aligned with the Y axis rotation of the given Transform
        /// </summary>
        /// <param name="target">Transform to teleport to</param>
        /// <param name="withYrotation">If true, the character's Y axis rotation is additionally aligned with the Y axis rotation of the given Transform</param>
        public override void TeleportTo(Transform target, bool withYrotation = false) {
            if (!Time.inFixedTimeStep) {
                _characterController.enabled = false;
            }
            transform.position = target.position;
            if (withYrotation) {
                _facingDirection = target.forward;
                _facingDirection.y = 0;
            }
            UpdateColliderBottom();
            if (!Time.inFixedTimeStep) {
                _characterController.enabled = true;
            }
        }
    }
}
