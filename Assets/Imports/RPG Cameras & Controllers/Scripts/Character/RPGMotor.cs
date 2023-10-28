using System.Collections.Generic;
using UnityEngine;

namespace JohnStairs.RCC.Character {
    public abstract class RPGMotor : MonoBehaviour {
        /// <summary>
        /// The speed of character movement when walking is toggled off
        /// </summary>
        public float RunSpeed = 6.0f;
        /// <summary>
        /// The speed of character movement while solely strafing. A combined speed is computed when also running/walking forward/backwards
        /// </summary>
        public float StrafeSpeed = 6.0f;
        /// <summary>
        /// The speed of character movement when walking is toggled on
        /// </summary>
        public float WalkSpeed = 2.0f;
        /// <summary>
        /// The speed of character movement when crouching is toggled on
        /// </summary>
        public float CrouchSpeed = 1.5f;
        /// <summary>
        /// The multiplier for computing the sprinting speed while the sprint button is pressed
        /// </summary>
        public float SprintSpeedMultiplier = 1.5f;
        /// <summary>
        /// The multiplier for computing the swim speed
        /// </summary>
        public float SwimSpeedMultiplier = 1.0f;
        /// <summary>
        /// The speed of the movement impulses while in midair/airborne
        /// </summary>
        public float MidairSpeed = 2.0f;
        /// <summary>
        /// The height the character jumps in world units
        /// </summary>
        public float JumpHeight = 1.5f;
        /// <summary>
        /// If set to true, jumps while the character is in midair are allowed. Used in combination with variable "AllowedMidairJumps"
        /// </summary>
        public bool EnableMidairJumps = false;
        /// <summary>
        /// The number of allowed jumps while the character is in midair/airborne, e.g. use value 1 for allowing double jumps
        /// </summary>
        public int AllowedMidairJumps = 1;
        /// <summary>
        /// Enum value for setting up when midair movement is allowed, e.g. never, only after a standing jump or always
        /// </summary>
        public MidairMovement EnableMidairMovement = MidairMovement.OnlyAfterStandingJump;
        /// <summary>
        /// Local water height at which the character should start to swim. Enable Gizmos for easier setup
        /// </summary>
        public float SwimmingStartHeight = 1.15f;
        /// <summary>
        /// If set to true, jumps while swimming at the water surface are possible
        /// </summary>
        public bool EnableSwimmingJumps = true;
        /// <summary>
        /// If set to true, the character starts to dive only when moving forward (or backwards). Otherwise, the character remains in an upright stance
        /// </summary>
        public bool DiveOnlyWhenSwimmingForward = true;
        /// <summary>
        /// If set to true, the character passively moves with moving objects, e.g. moving platforms, when standing on them
        /// </summary>
        public bool MoveWithMovingGround = true;
        /// <summary>
        /// If set to true, the character passively rotates with rotating objects, e.g. rotating platforms, when standing on them. Requires MoveWithMovingGround
        /// </summary>
        public bool RotateWithRotatingGround = true;
        /// <summary>
        /// If set to true, the character’s jumping direction is affected by the ground object, i.e. performing a standing jump on a moving object lets the character always land on the same spot of the object
        /// </summary>
        public bool GroundAffectsJumping = true;
        /// <summary>
        /// Layers which are ignored by motor physics logic, i.e. not considered for the grounded check, sliding check, etc.
        /// </summary>
        public LayerMask IgnoredLayers;
        /// <summary>
        /// If set to true, the character starts to slide on slopes steeper than the assigned CharacterController.slopeLimit
        /// </summary>
        public bool EnableSliding = true;
        /// <summary>
        /// Time in seconds which has to pass before sliding logic is applied
        /// </summary>
        public float SlidingTimeout = 0.1f;
        /// <summary>
        /// Sliding time in seconds which has to pass before the anti-stuck mechanic enables
        /// </summary>
        public float AntiStuckTimeout = 0.1f;
        /// <summary>
        /// If set to true, the standing character is pushed away by moving objects on collision
        /// </summary>
        public bool EnableCollisionMovement = true;
        /// <summary>
        /// Tolerance height used for the grounded check. The larger the value, the larger the distance to the ground which sets _grounded to true. Useful for tweaking movement on debris
        /// </summary>
        public float GroundedTolerance = 0.16f;
        /// <summary>
        /// A value representing the degree at which the character starts to fall. The default value is 6 to let the character be grounded when walking down small hills
        /// </summary>
        public float FallingThreshold = 6.0f;
        /// <summary>
        /// A value representing the downward force of gravity
        /// </summary>
        public float Gravity = 20.0f;

        /// <summary>
        /// The used character controller component
        /// </summary>
        protected CharacterController _characterController;
        /// <summary>
        /// The used animator component
        /// </summary>
        protected Animator _animator;
        /// <summary>
        /// Reference to the RPGCamera script
        /// </summary>
        protected RPGCamera _rpgCamera;
        /// <summary>
        /// Interface for getting character information, e.g. movement impairing effects
        /// </summary>
        protected ICharacterInfo _characterInfo;
        /// <summary>
        /// Direction given into the motor by the controller
        /// </summary>
        protected Vector3 _inputDirection;
        /// <summary>
        /// Movement direction in world coordinates
        /// </summary>
        protected Vector3 _movementDirection;
        /// <summary>
        /// Rotation around the local X axis which should be performed in the next frame
        /// </summary>
        protected float _xRotation = 0;
        /// <summary>
        /// True if the character is grounded
        /// </summary>
        protected bool _grounded = true;
        /// <summary>
        /// True if the character's X and Z rotation should be reset with the next fixed update
        /// </summary>
        protected bool _resetXandZRotations = false;
        /// <summary>
        /// True if the character should jump
        /// </summary>
        protected bool _jump = false;
        /// <summary>
        /// True if the character is automatically moving forward
        /// </summary>
        protected bool _autorunning = false;
        /// <summary>
        /// True if autorunning was started in the current frame
        /// </summary>
        protected bool _autorunningStarted = false;
        /// <summary>
        /// False if autorunning was started in the current frame
        /// </summary>
        protected bool _autorunningStopped = false;
        /// <summary>
        /// True if the character should crouch
        /// </summary>
        protected bool _crouching = false;
        /// <summary>
        /// True if the character should walk
        /// </summary>
        protected bool _walking = false;
        /// <summary>
        /// True if the character is swimming
        /// </summary>
        protected bool _swimming = false;
        /// <summary>
        /// True if the character should surface
        /// </summary>
        protected bool _surface = false;
        /// <summary>
        /// True if the character should dive down
        /// </summary>
        protected bool _dive = false;
        /// <summary>
        /// True if the character is sprinting
        /// </summary>
        protected bool _sprinting = false;
        /// <summary>
        /// True if the character is sliding
        /// </summary>
        protected bool _sliding = false;
        /// <summary>
        /// Buffer for measuring how long consecutive sliding events occured
        /// </summary>
        protected float _slidingBuffer;
        /// <summary>
        /// True if the anti-stuck mechanic is enabled, i.e. the anti-stuck timeout occured
        /// </summary>
        protected bool _antiStuckEnabled;
        /// <summary>
        /// True if the character is able to move
        /// </summary>
        protected bool _canMove = true;
        /// <summary>
        /// True if the character is able to move
        /// </summary>
        protected bool _canRotate = true;
        /// <summary>
        /// True if movement in all three directions should be possible, e.g. while swimming or flying
        /// </summary>
        protected bool _enable3dMovement;
        /// <summary>
        /// Number of already performed jumps during the current midair period
        /// </summary>
        protected int _midairJumpsCount = 0;
        /// <summary>
        /// True if the character performed a jump while running
        /// </summary>
        protected bool _runningJump = false;
        /// <summary>
        /// Height in world coordinates below which the character starts swimming
        /// </summary>
        protected float _swimmingStartHeight = -Mathf.Infinity;
        /// <summary>
        /// Stores all scripts of touched waters, sorted by water height/level
        /// </summary>
        protected SortedSet<Water> _touchedWaters;
        /// <summary>
        /// Water level on the world Y axis
        /// </summary>
        protected float _currentWaterHeight = -Mathf.Infinity;
        /// <summary>
        /// Bottom sphere of the character controller capsule collider in world space
        /// </summary>
        protected Sphere _colliderBottom;
        /// <summary>
        /// Stores the offset from the character position to the bottom of the character controller collider
        /// </summary>
        protected float _colliderBottomOffset;

        /// <summary>
        /// Helper structure, e.g. for representing the bottom of the character controller capsule collider
        /// </summary>
        protected struct Sphere {
            /// <summary>
            /// Center of the sphere
            /// </summary>
            public Vector3 center;
            /// <summary>
            /// Radius of the sphere
            /// </summary>
            public float radius;
        }

        /// <summary>
        /// Enum for specifying a side of a collider, e.g. character controller collider
        /// </summary>
        public enum ColliderSide {
            Top,
            Bottom
        }

        /// <summary>
        /// Enum for controlling when movement in midair is allowed
        /// </summary>
        public enum MidairMovement {
            /// <summary>
            /// Never allow midair movement
            /// </summary>
            Never,
            /// <summary>
            /// Only allow midair movement after performing a standing jump
            /// </summary>
            OnlyAfterStandingJump,
            /// <summary>
            /// Always allow midair movement
            /// </summary>
            Always
        }

        protected virtual void Awake() {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _characterInfo = GetComponent<ICharacterInfo>();
            _rpgCamera = GetComponent<RPGCamera>();
            _touchedWaters = new SortedSet<Water>(new Water.WaterComparer());
        }

        protected virtual void Start() {
            if (!Utils.LayerInLayerMask(gameObject.layer, IgnoredLayers)) {
                Debug.LogWarning("RPGMotor variable \"Ignored Layers\" does not contain the character's layer! Negative side effects might occur (see Manual section 1.2.3)");

                if (IgnoredLayers == 0) {
                    IgnoredLayers = 1 << LayerMask.NameToLayer("Player");
                    Debug.LogWarning("RPGMotor variable \"Ignored Layers\" was set to \"Nothing\". Defaulted the layer mask to layer \"Player\"");
                }
            }
            UpdateColliderBottom();
            _colliderBottomOffset = (_colliderBottom.center + Vector3.down * _colliderBottom.radius).y - transform.position.y;
        }

        protected virtual void FixedUpdate() {
            if (_resetXandZRotations
                && _grounded
                && !_swimming) {
                _resetXandZRotations = false;
                // Ensure that the character's X and Z axes rotations are reset after swimming (and flying) to prevent tilted ground movement
                ResetXandZrotations();
            }

            if (EnableCollisionMovement) {
                ApplyCollisionMovement();
            }
        }

        /// <summary>
        /// Starts motor computations based on external input
        /// </summary>
        public virtual void StartMotor() {
            _canMove = _characterInfo?.CanMove() ?? true;
            _canRotate = _characterInfo?.CanRotate() ?? true;

            if (_slidingBuffer < -AntiStuckTimeout && _characterController.velocity.magnitude < 0.05f) {
                _antiStuckEnabled = true;
            }

            if (_antiStuckEnabled) {
                _grounded = true;
            } else if (_movementDirection.y < 0 || _swimming) {
                // Check for grounded only if we are drawn towards the ground
                _grounded = Physics.CheckSphere(_colliderBottom.center + Vector3.down * GroundedTolerance, _colliderBottom.radius, ~IgnoredLayers, QueryTriggerInteraction.Ignore);
            } else {
                // Upward movement, e.g. jumping => disable grounded check so that colliders from the side cannot interrupt the movement
                _grounded = false;
            }

            _swimmingStartHeight = transform.position.y + SwimmingStartHeight;
            // Get the current water height
            _currentWaterHeight = GetCurrentWaterHeight();
            // Store if the character's global start swimming height is under the current water level
            _swimming = _swimmingStartHeight <= _currentWaterHeight;

            if (_swimming || !_grounded) {
                _resetXandZRotations = true;
            }

            _enable3dMovement = IsSwimming() || IsFlying();

            if (_animator.GetBool("Jump")) {
                _animator.ResetTrigger("Jump");
            }
        }

        /// <summary>
        /// Sets the animator parameters according to internal variable values
        /// </summary>
        /// <param name="animator">Target animator where the parameters will be set</param>
        public virtual void SetAnimatorParameters(Animator animator) {
            animator.SetBool("Grounded", _grounded);
            animator.SetBool("Walking", _walking);
            animator.SetBool("Sprinting", _sprinting);
            animator.SetBool("Sliding", _slidingBuffer < -SlidingTimeout); // buffer full due to many consecutive sliding events => fire animation
            animator.SetBool("Swimming", _swimming);

            if (_animator.GetBool("Jump")) {
                animator.SetTrigger("Jump");
            }
        }

        /// <summary>
        /// Immediately translates the character
        /// </summary>
        /// <param name="translation">Translation vector</param>
        protected virtual void Translate(Vector3 translation) {
            transform.position += translation;
            UpdateColliderBottom();
        }

        /// <summary>
        /// Immediately rotates the character by rotation
        /// </summary>
        /// <param name="rotation">Delta rotation to be applied</param>
        public virtual void Rotate(Quaternion rotation) {
            if (IsCombatLockActive()) {
                // Do not rotate if the character is locked on target
                return;
            }
            // Rotate the character
            transform.rotation = rotation * transform.rotation;
        }

        /// <summary>
        /// Applies passive movement when other colliders intersect with the character controller collider
        /// </summary>
        protected virtual void ApplyCollisionMovement() {
            Vector3 moveDirection = Vector3.zero;

            Vector3 start = _colliderBottom.center;
            Vector3 end = start + Vector3.up * (_characterController.height - _characterController.radius);
            Collider[] colliders = Physics.OverlapCapsule(start, end, _characterController.radius, ~IgnoredLayers, QueryTriggerInteraction.Ignore);

            foreach (Collider c in colliders) {
                if (c is MeshCollider || c is TerrainCollider) {
                    // Mesh colliders do not support the "ClosestPoint" method => skip
                    continue;
                }

                // Get the point which is closest to the character, i.e. deepest inside the character collider
                Vector3 closestPoint = c.ClosestPoint(transform.position);
                // Project the closest point to the same height as the character
                closestPoint.y = transform.position.y;
                Vector3 closestPointDirection = closestPoint - transform.position;
                // Add delta movement needed to move the character collider out of collider c 
                moveDirection += closestPointDirection.normalized * (closestPointDirection.magnitude - _characterController.radius);
            }

            if (moveDirection != Vector3.zero) {
                // Apply the combined delta movements
                Translate(moveDirection);
            }
        }

        /// <summary>
        /// Applies gravity to the character if needed
        /// </summary>
        protected virtual void ApplyGravity() {
            // Apply gravity only if we are not swimming
            if (!_enable3dMovement) {
                _movementDirection.y -= Gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// Calculates the jump height based on the gravity
        /// </summary>
        /// <returns>Resulting jump height</returns>
        protected virtual float CalculateJumpHeight() {
            return Mathf.Sqrt(2 * JumpHeight * Gravity);
        }

        /// <summary>
        /// Immediately resets the X and Z axes rotations of the character
        /// </summary>
        protected virtual void ResetXandZrotations() {
            // The character controller collider is not rotatable, i.e. it can happen that it touches earlier the ground than the rotated character
            // Therefore, we have to "teleport" the character to the character controller bottom, e.g. when landing from flying or surfacing from swimming to grounded
            Vector3 delta = _colliderBottom.center + Vector3.down * (_colliderBottom.radius + _colliderBottomOffset) - transform.position;
            Translate(delta);
            // Now rotate the character
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        /// <summary>
        /// Applies sliding to the character if it is standing on too steep terrain
        /// </summary>
        protected virtual void ApplySliding() {
            float slope = 0;
            Vector3 summedNormals = Vector3.zero;

            RaycastHit[] hits = Physics.SphereCastAll(_colliderBottom.center, _colliderBottom.radius, Vector3.down, GroundedTolerance, ~IgnoredLayers, QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits) {
                // Compute the slope in degrees
                slope = Vector3.Angle(hit.normal, Vector3.up);
                // Check if the slope is too steep
                if (Mathf.Round(slope) > _characterController.slopeLimit) {
                    // Add normal that would cause sliding to previous ones
                    summedNormals += hit.normal;
                } else {
                    ResetSliding();
                    // Turn off the anti-stuck mode
                    _antiStuckEnabled = false;
                    return;
                }
            }

            // Compute the slope of the summed normals
            slope = Vector3.Angle(summedNormals, Vector3.up);

            if (Mathf.Round(slope) > _characterController.slopeLimit) {
                // Update the buffer
                _slidingBuffer -= Time.deltaTime;

                if (_antiStuckEnabled) {
                    ResetSliding();
                } else if (_slidingBuffer < 0 /*&& moveDirection != Vector3.zero*/) {
                    // Buffer is depleted
                    _sliding = true;
                    // Compute the sliding direction
                    Vector3 slidingDirection = new Vector3(summedNormals.x, -summedNormals.y, summedNormals.z);
                    // Normalize the sliding direction and make it orthogonal to the hit normal
                    Vector3.OrthoNormalize(ref summedNormals, ref slidingDirection);
                    // Set the movement direction to the combined sliding directions
                    _movementDirection = slidingDirection * slope * 0.2f;
                }
            } else {
                ResetSliding();
                // Turn off the anti-stuck mode
                _antiStuckEnabled = false;
            }
        }

        /// <summary>
        /// Resets all internal variables related to the sliding mechanic
        /// </summary>
        protected virtual void ResetSliding() {
            _sliding = false;
            // Reset the buffer
            _slidingBuffer = SlidingTimeout;
        }

        /// <summary>
        /// Checks if the character is close to the currently set water level
        /// </summary>
        /// <returns>True if the character is close enough to the considered water level, otherwise false</returns>
        public virtual bool IsCloseToWaterSurface() {
            return Mathf.Abs(_currentWaterHeight - (transform.position.y + SwimmingStartHeight)) < 0.1f;
        }

        /// <summary>
        /// Gets the current water height/level based on all touched waters
        /// </summary>
        /// <returns>Maximum water height of all touched waters, -Infinity if no water is touched</returns>
        protected virtual float GetCurrentWaterHeight() {
            return _touchedWaters.Max?.GetHeight() ?? -Mathf.Infinity;
        }

        /// <summary>
        /// Prevents the character from swimming above the surface of the water it is currently swimming in
        /// </summary>
        protected virtual void PreventSwimmingAboveSurface() {
            // Check if the planned move in Y direction would lead to the character being above the current water level
            if (_movementDirection.y * Time.deltaTime + _swimmingStartHeight > _currentWaterHeight) {
                //  Prevent that the character can swim above the water level => cap the movement in Y direction
                _movementDirection.y = Mathf.Min(0, _movementDirection.y);
            }
        }

        /// <summary>
        /// Returns if the character is grounded
        /// </summary>
        /// <returns>True if grounded, otherwise false</returns>
        public virtual bool IsGrounded() {
            return _grounded;
        }

        /// <summary>
        /// Returns if the character is sliding
        /// </summary>
        /// <returns>True if sliding, otherwise false</returns>
        public virtual bool IsSliding() {
            return _sliding;
        }

        /// <summary>
        /// Returns if the character is sprinting
        /// </summary>
        /// <returns>True if sprinting, otherwise false</returns>
        public bool IsSprinting() {
            return _sprinting;
        }

        /// <summary>
        /// Updates variable value _colliderBottom to its current world coordinates 
        /// </summary>
        protected virtual void UpdateColliderBottom() {
            _colliderBottom.center = transform.TransformPoint(_characterController.center) + Vector3.down * (_characterController.height * 0.5f - _characterController.radius);
            _colliderBottom.radius = _characterController.radius;
        }

        /// <summary>
        /// Moves the character in the given direction
        /// </summary>
        /// <param name="direction">Movement direction</param>
        public virtual void Move(Vector3 direction) {
            if (direction.magnitude == 0) {
                return;
            }

            if (_swimming && !_grounded) {
                float yDeltaNeeded = direction.y;
                float yBefore = transform.position.y;

                // Move the character
                _characterController.Move(direction);

                float yDeltaResult = transform.position.y - yBefore;
                if (yDeltaResult > yDeltaNeeded) {
                    // Do not allow inaccuracy by Character Controller's Move method in this case!
                    // Otherwise, it would be possible to swim over the water level
                    Translate(Vector3.down * (yDeltaResult - yDeltaNeeded));
                }
            } else {
                // Move the character
                _characterController.Move(direction);
            }

            // Update the collider bottom world coordinates
            UpdateColliderBottom();
        }

        /// <summary>
        /// Triggers a jump in the current frame if possible
        /// </summary>
        public virtual void Jump() {
            if (_canMove && !_sliding && !_swimming) {
                if (_grounded) {
                    _jump = true;
                } else if (EnableMidairJumps && _midairJumpsCount < AllowedMidairJumps) {
                    _jump = true;
                    _midairJumpsCount++;
                }
            }
        }

        /// <summary>
        /// Lets the character perform a jump in the current frame
        /// </summary>
        protected virtual void PerformJump() {
            _jump = false;

            if (_crouching) {
                // Jumping cancels crouching
                _crouching = false;
            } else {
                _antiStuckEnabled = false;

                if (_inputDirection.x != 0 || _inputDirection.z != 0) {
                    _runningJump = true;
                }

                _movementDirection.y = CalculateJumpHeight();

                if (_animator) {
                    _animator?.SetTrigger("Jump");
                }
            }
        }

        /// <summary>
        /// Enables/Disables sprinting
        /// </summary>
        /// <param name="on">The new value for the internal sprinting variable</param>
        public virtual void Sprint(bool on) {
            _sprinting = on && (_characterInfo?.CanSprint() ?? true);
        }

        /// <summary>
        /// Toggles crouching
        /// </summary>
        /// <param name="toggle">The new value for the internal crouching toggle variable</param>
        public virtual void ToggleCrouching(bool toggle) {
            if (toggle && !_swimming) {
                _crouching = !_crouching;
            }
        }

        /// <summary>
        /// Toggles walking
        /// </summary>
        /// <param name="toggle">The new value for the internal walking toggle variable</param>
        public virtual void ToggleWalking(bool toggle) {
            if (toggle && !_swimming) {
                _walking = !_walking;
            }
        }

        /// <summary>
        /// Toggles autorun
        /// </summary>
        /// <param name="toggle">The new value for the internal autorunning toggle variable</param>
        public virtual void ToggleAutorun(bool toggle) {
            if (toggle) {
                _autorunning = !_autorunning;

                if (_autorunning) {
                    _autorunningStarted = true;
                } else {
                    _autorunningStopped = true;
                }
            }
        }

        /// <summary>
        /// Cancels autorun
        /// </summary>
        /// <param name="stop">The new value for the internal autorunning toggle variable</param>
        public virtual void StopAutorun(bool stop) {
            if (stop && _autorunning) {
                _autorunning = false;
                _autorunningStopped = true;
            }
        }

        /// <summary>
        /// Sets the character's direction input by the player/controller
        /// </summary>
        /// <param name="direction">The new input direction</param>
        public virtual void SetInputDirection(Vector3 direction) {
            _inputDirection = direction;
        }

        /// <summary>
        /// Returns if the character started autorunning this frame
        /// </summary>
        /// <returns>True if the character started autorunning</returns>
        public virtual bool StartedAutorunning() {
            bool result = _autorunningStarted;
            _autorunningStarted = false;
            return result;
        }

        /// <summary>
        /// Returns if the character stopped autorunning this frame
        /// </summary>
        /// <returns>True once if the character stopped autorunning</returns>
        public virtual bool StoppedAutorunning() {
            bool result = _autorunningStopped;
            _autorunningStopped = false;
            return result;
        }

        /// <summary>
        /// Returns if the character is swimming this frame
        /// </summary>
        /// <returns>True if the character is swimming</returns>
        public virtual bool IsSwimming() {
            return _swimming;
        }

        /// <summary>
        /// Sets if the character should surface
        /// </summary>
        /// <param name="surface">If true, the character should surface</param>
        public virtual void Surface(bool surface) {
            _surface = _enable3dMovement && surface && _canMove;
        }

        /// <summary>
        /// Sets if the character should dive
        /// </summary>
        /// <param name="surface">If true, the character should dive</param>
        public virtual void Dive(bool dive) {
            _dive = _enable3dMovement && dive && _canMove;
        }

        /// <summary>
        /// Applies all currently enabled movement speed modifiers to the given value
        /// </summary>
        /// <param name="current">Value to be modified</param>
        /// <returns>Current movement speed modified by active modifiers</returns>
        protected virtual float ApplyMovementSpeedMultipliers(float current) {
            float result = current;
            if (IsFlying()) {
                result = 2.0f * current;
            } else if (_swimming) {
                result *= SwimSpeedMultiplier;
            } else {
                // Adjust the speed if crouching is enabled
                if (_crouching) {
                    result = Mathf.Min(CrouchSpeed, result);
                    // Disable walking while crouching
                    _walking = false;
                }
                // Multiply with the sprint multiplier if sprinting is active
                if (_sprinting) {
                    result *= SprintSpeedMultiplier;
                }
                // Set the speed if walking is enabled
                if (_walking) {
                    result = Mathf.Min(WalkSpeed, result);
                }
            }
            return result;
        }

        /// <summary>
        /// Sets the run and strafe speed of the motor. Also called by the RPGBuilder integration
        /// </summary>
        /// <param name="newSpeed">New speed values to use for running and strafing</param>
        public virtual void SetMovementSpeed(float newSpeed) {
            RunSpeed = StrafeSpeed = newSpeed;
        }

        /// <summary>
        /// Teleports the character to the given Vector3
        /// </summary>
        /// <param name="targetPosition">Position to teleport to</param>
        public virtual void TeleportTo(Vector3 targetPosition) {
            if (!Time.inFixedTimeStep) {
                Debug.LogWarning("Method TeleportTo was called outside of FixedUpdate! This can lead to unexpected side effects (double-click here for more information)");
                // Not called from within FixedUpdate => disable the character controller component so that it does not overwrite the target position
                // The disadvantage is that collision detection callbacks are called again (OnTriggerEnter) or never (OnTriggerExit)
                _characterController.enabled = false;
            }

            transform.position = targetPosition;
            UpdateColliderBottom();

            if (!Time.inFixedTimeStep) {
                // Enable the character controller again
                _characterController.enabled = true;
            }
        }

        /// <summary>
        /// Teleports the character to the given Transform
        /// </summary>
        /// <param name="target">Target Transform</param>
        /// <param name="withYrotation">If true, the character's Y axis rotation is additionally aligned with the Y axis rotation of the given Transform</param>
        public abstract void TeleportTo(Transform target, bool withYrotation = false);

        /// <summary>
        /// Gets this frame's input direction
        /// </summary>
        /// <returns>Input direction of the current frame</returns>
        public virtual Vector3 GetInputDirection() {
            return _inputDirection;
        }

        /// <summary>
        /// Applies the falling threshold to the movement direction
        /// </summary>
        protected virtual void SetFallingThreshold() {
            // Set the falling threshold
            _movementDirection.y = -FallingThreshold;
        }

        /// <summary>
        /// Rotates the character towards a target rotation
        /// </summary>
        /// <param name="lookAtRotation">Look rotation</param>
        public abstract void RotateTowards(Quaternion lookAtRotation);

        /// <summary>
        /// Checks if the character is currently in motion
        /// </summary>
        /// <returns>True if the character is in motion, otherwise false</returns>
        public virtual bool IsInMotion() {
            return _movementDirection.x != 0
                    || _movementDirection.z != 0
                    || IsFalling()
                    || _surface
                    || _dive;
        }

        /// <summary>
        /// Checks if the character is currently flying, i.e. in midair 
        /// </summary>
        /// <returns>True if the character is flying, otherwise false</returns>
        public virtual bool IsFlying() {
            return (_characterInfo?.CanFly() ?? false) && !_grounded;
        }

        /// <summary>
        /// Checks if the character is currently falling, i.e. is neither grounded nor in 3D motion
        /// </summary>
        /// <returns>True if the character is falling, otherwise false</returns>
        public virtual bool IsFalling() {
            return !_grounded && !Is3dMovementEnabled();
        }

        /// <summary>
        /// Checks if movement in all three directions is possible, e.g. while swimming or flying
        /// </summary>
        /// <returns>True if movement in all directions is enabled, otherwise false</returns>
        public virtual bool Is3dMovementEnabled() {
            return _enable3dMovement;
        }

        /// <summary>
        /// Checks if the character is looking in the giving direction
        /// </summary>
        /// <param name="direction">Direction to check</param>
        /// <returns>True if the character is looking in the given direction, otherwise false</returns>
        protected virtual bool IsLookingInDirection(Vector3 direction) {
            return direction == Vector3.zero
                    || Utils.IsAlmostEqual(transform.forward, direction);
        }

        /// <summary>
        /// Checks the character info if a combat lock is active
        /// </summary>
        /// <returns>True if a combat lock is active, otherwise false</returns>
        protected virtual bool IsCombatLockActive() {
            return _characterInfo?.LockOnTarget() ?? false;
        }

        /// <summary>
        /// "OnControllerColliderHit is called when the controller hits a collider while performing a Move" - Unity Documentation
        /// </summary>
        public virtual void OnControllerColliderHit(ControllerColliderHit hit) {
            if (_characterController.collisionFlags == CollisionFlags.Above
                && _movementDirection.y > 0) {
                _movementDirection.y = 0;
            }
        }

        /// <summary>
        /// "OnTriggerEnter happens on the FixedUpdate function when two GameObjects collide" - Unity Documentation
        /// </summary>
        /// <param name="other">Triggering collider</param>
        protected virtual void OnTriggerEnter(Collider other) {
            Water water = other.GetComponent<Water>();
            if (water) {
                // Store the water script for getting the right water level later
                _touchedWaters.Add(water);
            }
        }

        /// <summary>
        /// "OnTriggerExit is called when the Collider other has stopped touching the trigger" - Unity Documentation
        /// </summary>
        /// <param name="other">Left trigger collider</param>
        protected virtual void OnTriggerExit(Collider other) {
            Water water = other.GetComponent<Water>();
            if (water) {
                // Remove the water again since we left it
                _touchedWaters.Remove(water);
            }
        }

        /// <summary>
        /// If Gizmos are enabled, this method draws some debugging gizmos
        /// </summary>
        protected virtual void OnDrawGizmosSelected() {
            Color green = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color yellow = new Color(1.0f, 1.0f, 0.0f, 0.35f);
            Color red = new Color(1.0f, 0.0f, 0.0f, 0.35f);
            Color blue = new Color(0.0f, 0.0f, 1.0f, 0.55f);

            if (!_characterController) {
                _characterController = GetComponent<CharacterController>();
            }
            UpdateColliderBottom();

            if (_sliding) {
                Gizmos.color = yellow;
            } else if (_grounded) {
                Gizmos.color = green;
            } else {
                Gizmos.color = red;
            }

            // Draw sphere for the grounded check area
            Gizmos.DrawSphere(_colliderBottom.center + Vector3.down * GroundedTolerance, _colliderBottom.radius);

            // Draw the local Swimming Start Height
            Gizmos.color = blue;
            Gizmos.DrawCube(transform.position + Vector3.up * SwimmingStartHeight, new Vector3(0.7f, 0.01f, 0.7f));
        }
    }
}
