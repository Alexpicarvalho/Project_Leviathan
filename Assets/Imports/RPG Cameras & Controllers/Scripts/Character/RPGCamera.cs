using System.Collections.Generic;
using JohnStairs.RCC.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JohnStairs.RCC.Character {
    public abstract class RPGCamera : MonoBehaviour {
        /// <summary>
        /// Enum value for setting up which camera object should be used by the script, e.g. the main camera of the scene, a newly spawned camera object or the camera assigned to "CameraToUse"
        /// </summary>
        public CameraUsage UsedCamera = CameraUsage.MainCamera;
        /// <summary>
        /// Reference to the camera object used by the script. If no camera is assigned to this variable, a new one will be generated automatically when entering the play mode
        /// </summary>
        public Camera CameraToUse;
        /// <summary>
        /// Skybox which is currently used or should be used by the camera object. The skybox can be changed at runtime by calling the script's method "SetUsedSkybox(material)". Direct assignments to this variable have no effect
        /// </summary>
        public Material UsedSkybox;
        /// <summary>
        /// Position of the camera pivot in local character coordinates. Turn on Gizmos to display it as a small cyan sphere
        /// </summary>
        public Vector3 CameraPivotLocalPosition = new Vector3(0, 0.5f, 0);
        /// <summary>
        /// Enables the intelligent pivot that moves away from obstacles which the player could see through if zooming in enough (internal pivot only)
        /// </summary>
        public bool EnableIntelligentPivot = true;
        /// <summary>
        /// The time needed for moving the intelligent pivot away from the obstacle
        /// </summary>
        public float PivotSmoothTime = 0.7f;
        /// <summary>
        /// If set to false, all camera controls are disabled. Can be used to turn off the camera when interacting with a GUI (e.g. see the demo GUI interaction)
        /// </summary>
        public bool ActivateCameraControl = true;
        /// <summary>
        /// If true, Unity's new input system is used
        /// </summary>
        public bool UseNewInputSystem = true;
        /// <summary>
        /// Logs warnings for legacy inputs which are not set up in the project but used in this script
        /// </summary>
        public bool LogInputWarnings = true;
        /// <summary>
        /// Let the camera always orbit around its pivot without pressing any input
        /// </summary>
        public bool AlwaysRotateCamera = false;
        /// <summary>
        /// Handles when the cursor should be hidden
        /// </summary>
        public CursorHiding HideCursor = CursorHiding.WhenOrbiting;
        /// <summary>
        /// How the cursor should behave when orbiting
        /// </summary>
        public CursorBehavior CursorBehaviorOrbiting = CursorBehavior.Stay;
        /// <summary>
        /// Locks the horizontal orbit axis
        /// </summary>
        public bool LockRotationX = false;
        /// <summary>
        /// Locks the vertical orbit axis
        /// </summary>
        public bool LockRotationY = false;
        /// <summary>
        /// Inverts the horizontal orbit axis
        /// </summary>
        public bool InvertRotationX = true;
        /// <summary>
        /// Inverts the vertical orbit axis
        /// </summary>
        public bool InvertRotationY = true;
        /// <summary>
        /// The sensitivity of orbiting the camera on the horizontal axis
        /// </summary>
        public float RotationXSensitivity = 4.0f;
        /// <summary>
        /// The sensitivity of orbiting the camera on the vertical axis
        /// </summary>
        public float RotationYSensitivity = 4.0f;
        /// <summary>
        /// Constrain the camera's orbit horizontal axis from "RotationXMin" degrees to "RotationXMax" degrees
        /// </summary>
        public bool ConstrainRotationX = false;
        /// <summary>
        /// The minimum degrees for orbiting on the horizontal axis. Needs "ConstrainRotationX" to be true
        /// </summary>
        public float RotationXMin = -90.0f;
        /// <summary>
        /// The maximum degrees for orbiting on the horizontal axis. Needs "ConstrainRotationX" to be true
        /// </summary>
        public float RotationXMax = 90.0f;
        /// <summary>
        /// The minimum degrees for orbiting on the vertical axis
        /// </summary>
        public float RotationYMin = -89.9f;
        /// <summary>
        /// The maximum degrees for orbiting on the vertical axis
        /// </summary>
        public float RotationYMax = 89.9f;
        /// <summary>
        /// The time needed for the camera to orbit around its pivot. The higher the smoother the orbiting
        /// </summary>
        public float RotationSmoothTime = 0.1f;
        /// <summary>
        /// The sensitivity of the zooming input
        /// </summary>
        public float ZoomSensitivity = 15.0f;
        /// <summary>
        /// The minimum distance to zoom in to
        /// </summary>
        public float MinDistance = 0;
        /// <summary>
        /// The maximum distance to zoom out to
        /// </summary>
        public float MaxDistance = 20.0f;
        /// <summary>
        /// The time needed to zoom in and out a step
        /// </summary>
        public float DistanceSmoothTime = 0.7f;
        /// <summary>
        /// The camera's starting degrees on the horizontal axis relative to the character rotation
        /// </summary>
        public float StartRotationX = 0;
        /// <summary>
        /// The camera's starting degrees on the vertical axis relative to the character rotation
        /// </summary>
        public float StartRotationY = 15.0f;
        /// <summary>
        /// The camera's starting distance
        /// </summary>
        public float StartDistance = 7.0f;
        /// <summary>
        /// Enables/disables the initial zoom out from first person view on character spawning
        /// </summary>
        public bool StartZoomOut = false;
        /// <summary>
        /// If true, the below effects (fog color and density) are applied if the camera is underwater
        /// </summary>
        public bool EnableUnderwaterEffect = true;
        /// <summary>
        /// Color of the fog when the camera goes beneath water
        /// </summary>
        /// <returns></returns>
        public Color UnderwaterFogColor = new Color(0, 0.13f, 0.59f);
        /// <summary>
        /// Density of the fog which is activated when the camera is under the water surface
        /// </summary>
        public float UnderwaterFogDensity = 0.06f;
        /// <summary>
        /// For fine-tune the threshold where the camera is considered as underwater. The higher the value, the earlier the underwater effects kick in
        /// </summary>
        public float UnderwaterThresholdTuning = 0.16f;

        /// <summary>
        /// Used view frustum script for camera distance/constraints computations
        /// </summary>
        protected RPGViewFrustum _rpgViewFrustum;
        /// <summary>
        /// Reference to the RPGMotor script
        /// </summary>
        protected RPGMotor _rpgMotor;
        /// <summary>
        /// Collider assigned to the character object for computing the pivot retreat for external pivots
        /// </summary>
        protected Collider _collider;
        /// <summary>
        /// Interface for getting character information, e.g. movement impairing effects
        /// </summary>
        protected ICharacterInfo _characterInfo;
        /// <summary>
        /// Interface for getting pointer information, e.g. if the pointer is over the GUI
        /// </summary>
        protected IPointerInfo _pointerInfo;
        /// <summary>
        /// Reference to the used input actions (Unity's new input system)
        /// </summary>
        protected RPGInputActions _inputActions;
        /// <summary>
        /// True if the current orbiting was started on a GUI, otherwise false
        /// </summary>
        protected bool _orbitingStartedOverGUI;
        /// <summary>
        /// Variable for temporarily storing the cursor position for warping
        /// </summary>
        protected Vector2 _tempCursorPosition;
        /// <summary>
        /// Skybox currently used by the used camera
        /// </summary>
        protected Skybox _skybox;
        /// <summary>
        /// If set to true, the UsedSkybox variable has been changed. Used for updating the used camera's skybox
        /// </summary>
        protected bool _skyboxChanged = false;
        /// <summary>
        /// Stored on start: If true, the pivot is internal, i.e. inside the character collider. Controls the activation of needed functionality for each case
        /// </summary>
        protected bool _internalPivot;
        /// <summary>
        /// Retreat location inside the character controller collider (for external pivots only)
        /// </summary>
        protected Vector3 _pivotRetreat;
        /// <summary>
        /// The position where the player wants the pivot to be (for external pivots only)
        /// </summary>
        protected Vector3 _desiredPivotPosition;
        /// <summary>
        /// Current pivot evasive movement direction (intelligent pivot only)
        /// </summary>
        protected Vector3 _pivotMoveDirection;
        /// <summary>
        /// Current pivot movement smoothing velocity (intelligent pivot only)
        /// </summary>
        protected Vector3 _pivotCurrentVelocity;
        /// <summary>
        /// Current pivot position in world coordinates
        /// </summary>
        protected Vector3 _cameraPivotPosition;
        /// <summary>
        /// For pyramid view frustums, multiple occlusion checks are needed in succession to account for the changing shape (closer distance => less peak stretching)
        /// </summary>
        protected int _maxCheckIterations = 5;
        /// <summary>
        /// Desired camera position, can be unequal to the current position because of ambient occlusion
        /// </summary>
        protected Vector3 _desiredPosition;
        /// <summary>
        /// Desired camera distance, can be unequal to the current position because of ambient occlusion
        /// </summary>
        protected float _desiredDistance;
        /// <summary>
        /// Current camera distance smoothed
        /// </summary>
        protected float _distanceSmooth = 0;
        /// <summary>
        /// Current camera distance smoothing velocity
        /// </summary>
        protected float _distanceCurrentVelocity;
        /// <summary>
        /// Targeted camera X rotation
        /// </summary>
        protected float _rotationX = 0;
        /// <summary>
        /// Current camera X rotation smoothed
        /// </summary>
        protected float _rotationXSmooth = 0;
        /// <summary>
        /// Current camera X rotation smoothing velocity
        /// </summary>
        protected float _rotationXCurrentVelocity;
        /// <summary>
        /// Targeted camera Y rotation
        /// </summary>
        protected float _rotationY = 0;
        /// <summary>
        /// Current camera Y rotation smoothed
        /// </summary>
        protected float _rotationYSmooth = 0;
        /// <summary>
        /// Current camera Y rotation smoothing velocity
        /// </summary>
        protected float _rotationYCurrentVelocity;
        /// <summary>
        /// Desired camera Y rotation, as the Y rotation can be constrained by terrain
        /// </summary>
        protected float _desiredRotationY = 0;
        /// <summary>
        /// Stores all scripts of touched waters, sorted by water height/level
        /// </summary>
        protected SortedSet<Water> _touchedWaters;
        /// <summary>
        /// True if the camera is currently underwater (used for applying/undo the underwater effect)
        /// </summary>
        protected bool _underwater = false;
        /// <summary>
        /// Project setting's fog color at script awakening (used for underwater effect logic)
        /// </summary>
        protected Color _defaultFogColor;
        /// <summary>
        /// Project setting's fog density value at script awakening (used for underwater effect logic)
        /// </summary>
        protected float _defaultFogDensity;
        #region Input values
        /// <summary>
        /// When pressed, camera orbiting is allowed/possible
        /// </summary>
        protected bool _inputAllowOrbiting = false;
        /// <summary>
        /// If true, pressing _inputAllowOrbiting started this frame
        /// </summary>
        protected bool _inputAllowOrbitingStart = false;
        /// <summary>
        /// If true, pressing _inputAllowOrbiting stopped this frame
        /// </summary>
        protected bool _inputAllowOrbitingStop = false;
        /// <summary>
        /// Same as above but additionally enables character rotation while orbiting
        /// </summary>
        protected bool _inputAllowOrbitingWithCharRotation = false;
        /// <summary>
        /// If true, pressing _inputAllowOrbitingWithCharRotation started this frame
        /// </summary>
        protected bool _inputAllowOrbitingWithCharRotationStart = false;
        /// <summary>
        /// If true, pressing _inputAllowOrbitingWithCharRotation stopped this frame
        /// </summary>
        protected bool _inputAllowOrbitingWithCharRotationStop = false;
        /// <summary>
        /// Rotation amount along the camera X axis, i.e. around the character's Y axis, as long as orbiting is allowed/triggered
        /// </summary>
        protected Vector2 _inputRotationAmount;
        /// <summary>
        /// Zoom in/out input axis
        /// </summary>
        protected float _inputZoomAmount = 0;
        /// <summary>
        /// Fast zoom into first person view
        /// </summary>
        protected bool _inputMinDistanceZoom = false;
        /// <summary>
        /// Fast zoom out to max character distance
        /// </summary>
        protected bool _inputMaxDistanceZoom = false;
        #endregion Input values   

        /// <summary>
        /// Enum for controlling which camera GameObject should be used by this script
        /// </summary>
        public enum CameraUsage {
            /// <summary>
            /// Use the camera which is tagged as "MainCamera"
            /// </summary>
            MainCamera,
            /// <summary>
            /// Spawn a new camera game object in the scene to use
            /// </summary>
            SpawnOwnCamera,
            /// <summary>
            /// Camera game object assigned to public variable "CameraToUse"
            /// </summary>
            AssignedCamera
        }

        /// <summary>
        /// Enum for controlling cursor behavior
        /// </summary>
        public enum CursorBehavior {
            /// <summary>
            /// Default, unrestricted cursor movement
            /// </summary>
            Move,
            /// <summary>
            /// Cursor movement is confied by the screen edges
            /// </summary>
            MoveConfined,
            /// <summary>
            /// Cursor position should be stored and reloaded so that it looks like the cursor stays in position
            /// </summary>
            Stay,
            /// <summary>
            /// The cursor is locked in the center of the screen
            /// </summary>
            LockInCenter
        }

        /// <summary>
        /// Enum for controlling when the cursor should be hidden
        /// </summary>
        public enum CursorHiding {
            /// <summary>
            /// Never hide the cursor
            /// </summary>
            Never,
            /// <summary>
            /// Only hide the cursor during orbiting
            /// </summary>
            WhenOrbiting,
            /// <summary>
            /// Always hide the cursor
            /// </summary>
            Always
        }

        protected virtual void Awake() {
            _rpgViewFrustum = GetComponent<RPGViewFrustum>();
            _rpgMotor = GetComponent<RPGMotor>();
            _collider = GetComponent<Collider>();
            _characterInfo = GetComponent<ICharacterInfo>();
            _pointerInfo = GetComponent<IPointerInfo>();
            _inputActions = RPGInputManager.GetInputActions();
            _touchedWaters = new SortedSet<Water>(new Water.WaterComparer());
        }

        protected virtual void Start() {
            if (RenderSettings.fog) {
                _defaultFogDensity = RenderSettings.fogDensity;
            } else {
                _defaultFogDensity = 0;
                RenderSettings.fogDensity = 0;
            }
            RenderSettings.fog = true;
            _defaultFogColor = RenderSettings.fogColor;

            if (UsedCamera == CameraUsage.MainCamera && !Camera.main) {
                Debug.LogWarning("Main Camera should be used but could not be found in the scene! Spawning a camera...");
                UsedCamera = CameraUsage.SpawnOwnCamera;
            } else if (UsedCamera == CameraUsage.AssignedCamera && !CameraToUse) {
                Debug.LogWarning("Assigned Camera should be used but variable \"Camera To Use\" has not been assigned! Spawning a camera...");
                UsedCamera = CameraUsage.SpawnOwnCamera;
            }

            // Check if there is an assigned camera to use
            if (UsedCamera == CameraUsage.SpawnOwnCamera) {
                // Create one for usage in the following code
                GameObject camObject = new GameObject(transform.name + transform.GetInstanceID() + " Camera");
                camObject.AddComponent<Camera>();
                camObject.AddComponent<FlareLayer>();
                camObject.AddComponent<Skybox>();
                CameraToUse = camObject.GetComponent<Camera>();
            } else if (UsedCamera == CameraUsage.MainCamera) {
                CameraToUse = Camera.main;
            }

            _skybox = CameraToUse.GetComponent<Skybox>();
            // Check if the used camera has a skybox attached
            if (_skybox == null) {
                // No skybox attached => add a skybox and assign it to the _skybox variable
                CameraToUse.gameObject.AddComponent<Skybox>();
                _skybox = CameraToUse.gameObject.GetComponent<Skybox>();
            }

            if (UsedSkybox) {
                // Set the used camera's skybox to the user prescribed one
                _skybox.material = UsedSkybox;
            } else {
                UsedSkybox = _skybox.material;
            }

            ResetView(StartZoomOut);

            GetInputs(LogInputWarnings);
            SetUpInputActionCallbacks();

            if (AlwaysRotateCamera && CursorBehaviorOrbiting == CursorBehavior.Stay) {
                Debug.LogWarning("RPGCamera variable \"Cursor Behavior Orbiting\" was set to \"Lock in Center\" to prevent interference with enabled \"Always Rotate Camera\"");
                CursorBehaviorOrbiting = CursorBehavior.LockInCenter;
            }

            _internalPivot = HasInternalPivot();
        }

        protected virtual void LateUpdate() {
            GetInputs();

            HandleCursor();

            CheckForChangedSkybox();

            if (EnableUnderwaterEffect) {
                HandleUnderwaterEffects();
            }
        }

        /// <summary>
        /// Tries to get the inputs used by this script. If logWarnings is true, warnings are logged if inputs of Unity's legacy input system are not set up
        /// </summary>
        /// <param name="logWarnings">If true and Unity's legacy input system is used, warnings are logged whenever an input could not be found</param>
        protected virtual void GetInputs(bool logWarnings = false) {
            if (UseNewInputSystem) {
                // Poll inputs
                // Orbiting input
                _inputAllowOrbiting = _inputActions.Character.AllowOrbiting.ReadValue<float>() > 0;

                // Orbiting with character rotation input
                _inputAllowOrbitingWithCharRotation = _inputActions.Character.AllowOrbitingwithCharacterRotation.ReadValue<float>() > 0;

                // Orbit rotation input
                _inputRotationAmount = _inputActions.Character.RotationAmount.ReadValue<Vector2>();

                // Zoom input
                _inputZoomAmount = _inputActions.Character.Zoom.ReadValue<float>();
            } else {
                // Try to get Unity legacy inputs
                // Orbiting input
                _inputAllowOrbiting = Utils.TryGetButton(Utils.InputPhase.Pressed, "Fire1", logWarnings);
                _inputAllowOrbitingStart = Utils.TryGetButton(Utils.InputPhase.Down, "Fire1", logWarnings);
                _inputAllowOrbitingStop = Utils.TryGetButton(Utils.InputPhase.Up, "Fire1", logWarnings);

                // Orbiting with character rotation input
                _inputAllowOrbitingWithCharRotation = Utils.TryGetButton(Utils.InputPhase.Pressed, "Fire2", logWarnings);
                _inputAllowOrbitingWithCharRotationStart = Utils.TryGetButton(Utils.InputPhase.Down, "Fire2", logWarnings);

                // Orbit rotation input
                float inputRotationAmountX = Utils.TryGetAxis(Utils.InputPhase.Smoothed, "Mouse X", logWarnings);
                float inputRotationAmountY = Utils.TryGetAxis(Utils.InputPhase.Smoothed, "Mouse Y", logWarnings);
                _inputRotationAmount = new Vector2(inputRotationAmountX, inputRotationAmountY);

                // Zoom input
                _inputZoomAmount = Utils.TryGetAxis(Utils.InputPhase.Smoothed, "Mouse ScrollWheel", logWarnings);

                // Input for zooming to min or max distance
                _inputMinDistanceZoom = Utils.TryGetButton(Utils.InputPhase.Pressed, "First Person Zoom", logWarnings);
                _inputMaxDistanceZoom = Utils.TryGetButton(Utils.InputPhase.Pressed, "Maximum Distance Zoom", logWarnings);
            }
        }

        /// <summary>
        /// Sets up all input action callbacks
        /// </summary>
        protected virtual void SetUpInputActionCallbacks() {
            if (!UseNewInputSystem) {
                return;
            }
            // Orbiting input
            _inputActions.Character.AllowOrbiting.started += context => {
                _inputAllowOrbitingStart = true;
                _orbitingStartedOverGUI = ActivateCameraControl && (_pointerInfo?.IsPointerOverGUI() ?? false);
            };
            _inputActions.Character.AllowOrbiting.canceled += context => _inputAllowOrbitingStop = true;

            // Orbiting with character rotation input
            _inputActions.Character.AllowOrbitingwithCharacterRotation.started += context => {
                _inputAllowOrbitingWithCharRotationStart = true;
                _orbitingStartedOverGUI = ActivateCameraControl && (_pointerInfo?.IsPointerOverGUI() ?? false);
            };
            _inputActions.Character.AllowOrbitingwithCharacterRotation.canceled += context => _inputAllowOrbitingWithCharRotationStop = true;

            // Input for zooming to min or max distance
            _inputActions.Character.ZoomToMinDistance.started += context => _inputMinDistanceZoom = true;
            _inputActions.Character.ZoomToMaxDistance.started += context => _inputMaxDistanceZoom = true;
        }

        /// <summary>
        /// Resets all variables which are set by input action callbacks
        /// </summary>
        protected virtual void ConsumeEventInputs() {
            if (!UseNewInputSystem) {
                return;
            }
            _inputAllowOrbitingStart = false;
            _inputAllowOrbitingStop = false;
            _inputAllowOrbitingWithCharRotationStart = false;
            _inputAllowOrbitingWithCharRotationStop = false;
            _inputMinDistanceZoom = false;
            _inputMaxDistanceZoom = false;
        }

        /// <summary>
        /// Handles the cursor behavior and visibility depending on the variables "CursorBehaviorWhileOrbiting" and "HideCursor"
        /// </summary>
        protected virtual void HandleCursor() {
            bool orbitingStart = !_orbitingStartedOverGUI
                                    && (_inputAllowOrbitingStart || _inputAllowOrbitingWithCharRotationStart);
            bool orbitingStop = !_orbitingStartedOverGUI
                                    && (_inputAllowOrbitingStop || _inputAllowOrbitingWithCharRotationStop);

            // Handle cursor visibility
            if (HideCursor == CursorHiding.Always) {
                Cursor.visible = false;
            } else if (HideCursor == CursorHiding.WhenOrbiting) {
                if (orbitingStart) {
                    // Hide the cursor
                    Cursor.visible = false;
                } else if (orbitingStop) {
                    // Show the cursor again
                    Cursor.visible = true;
                }
            } else {
                Cursor.visible = true;
            }

            // Handle cursor behavior while orbiting
            if (CursorBehaviorOrbiting == CursorBehavior.Stay) {
                if (orbitingStart) {
                    // Confine the cursor during orbiting
                    Cursor.lockState = CursorLockMode.Confined;
                    // Store mouse position
                    _tempCursorPosition = Mouse.current.position.ReadValue();
                    // Workaround for a bug in Unity 2019.4
                    // See https://forum.unity.com/threads/mouse-y-position-inverted-in-build-using-mouse-current-warpcursorposition.682627/
#if UNITY_2019_4 && !UNITY_EDITOR
                    _tempCursorPosition.Set(_tempCursorPosition.x, Screen.height - _tempCursorPosition.y);
#endif
                } else if (orbitingStop) {
                    // Release the confinement
                    Cursor.lockState = CursorLockMode.None;
                    // Restore mouse position
                    Mouse.current.WarpCursorPosition(_tempCursorPosition);
                }
            } else if (CursorBehaviorOrbiting == CursorBehavior.LockInCenter) {
                bool orbiting = _inputAllowOrbiting || _inputAllowOrbitingWithCharRotation || AlwaysRotateCamera;
                
                if (orbiting) {
                    // Lock the cursor
                    Cursor.lockState = CursorLockMode.Locked;
                } else if (orbitingStop) {
                    // Unlock the cursor again
                    Cursor.lockState = CursorLockMode.None;
                }
            } else if (CursorBehaviorOrbiting == CursorBehavior.MoveConfined) {
                Cursor.lockState = CursorLockMode.Confined;
            } else {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        /// <summary>
        /// Computes the desired distance based on the given input
        /// </summary>
        protected virtual void ComputeDesiredDistance() {
            if (_pointerInfo?.IsPointerOverGUI() ?? false) {
                // Do not zoom if the cursor is over the GUI
                return;
            }

            // Get camera zoom input
            _desiredDistance = _desiredDistance - _inputZoomAmount * ZoomSensitivity;
            _desiredDistance = Mathf.Clamp(_desiredDistance, MinDistance, MaxDistance);

            // Check if one of the switch buttons is pressed
            if (_inputMinDistanceZoom) {
                _desiredDistance = MinDistance;
            } else if (_inputMaxDistanceZoom) {
                _desiredDistance = MaxDistance;
            }
        }

        /// <summary>
        /// Computes the camera position based on the given parameters as if there were no obstacles
        /// </summary>
        /// <param name="xAxisDegrees">Orbital rotation degrees around the X axis (up/down)</param>
        /// <param name="yAxisDegrees">Orbital rotation degrees around the Y axis (left/right)</param>
        /// <param name="distance">Distance to the character</param>
        /// <returns>Computed orbital camera position</returns>
        protected abstract Vector3 ComputeCameraPosition(float xAxisDegrees, float yAxisDegrees, float distance);

        /// <summary>
        /// Computes the camera's pivot position based on the given parameters as if there were no obstacles
        /// </summary>
        /// <param name="yAxisDegrees">Orbital rotation degrees around the Y axis (left/right)</param>
        /// <returns>Computed orbital pivot position</returns>
        protected abstract Vector3 ComputePivotPosition(float yAxisDegrees);

        /// <summary>
        /// Computes the new camera position based on the desired position. Checks for occlusions between the pivot and the camera
        /// and occlusions between the pivot and the "pivot retreat" inside the character collider
        /// </summary>
        /// <returns>Computed orbital position as close as possible to the desired position</returns>
        protected virtual Vector3 ComputeNewCameraPosition() {
            float closestPivotDistance = Mathf.Infinity;
            // Check if the shape of the view frustum
            bool pyramidViewFrustum = _rpgViewFrustum.Shape == RPGViewFrustum.FrustumShape.Pyramid;

            #region Check pivot occlusion
            // Compute the desired pivot position
            _desiredPivotPosition = ComputePivotPosition(_rotationXSmooth);

            // Check if the pivot was set up to be internal or external, i.e. within the character collider or not
            if (_internalPivot) {
                // INTERNAL PIVOT
                _pivotRetreat = _desiredPivotPosition;
                if (EnableIntelligentPivot) {
                    // Cast rays in all directions according to clip plane height and width
                    float halfHeight = CameraToUse.nearClipPlane * Mathf.Tan(CameraToUse.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    float halfWidth = halfHeight * CameraToUse.aspect;
                    float halfDiagonal = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);

                    float horizontal = Mathf.Sqrt(halfWidth * halfWidth + CameraToUse.nearClipPlane * CameraToUse.nearClipPlane);
                    float vertical = Mathf.Sqrt(halfHeight * halfHeight + CameraToUse.nearClipPlane * CameraToUse.nearClipPlane);
                    float diagonal = Mathf.Sqrt(halfDiagonal * halfDiagonal + CameraToUse.nearClipPlane * CameraToUse.nearClipPlane);

                    Vector3[] rayDirections = { Vector3.forward * horizontal, -Vector3.forward * horizontal, Vector3.left * horizontal, Vector3.right * horizontal, Vector3.up * vertical, Vector3.down * vertical,
                                                   (Vector3.forward + Vector3.up).normalized * diagonal, (-Vector3.forward + Vector3.up).normalized * diagonal, (Vector3.left + Vector3.up).normalized * diagonal, (Vector3.right + Vector3.up).normalized * diagonal,
                                                   (Vector3.forward + Vector3.down).normalized * diagonal, (-Vector3.forward + Vector3.down).normalized * diagonal, (Vector3.left + Vector3.down).normalized * diagonal, (Vector3.right + Vector3.down).normalized * diagonal };

                    Vector3 moveDirection = Vector3.zero;
                    foreach (Vector3 ray in rayDirections) {
                        // Cast the ray
                        //Debug.DrawRay(_desiredPivotPosition, ray, Color.magenta);
                        if (Physics.Raycast(_desiredPivotPosition, ray, out RaycastHit hit, ray.magnitude, _rpgViewFrustum.OccludingLayers, QueryTriggerInteraction.Ignore)) {
                            // Process the hit
                            Vector3 hitDirection = hit.point - _desiredPivotPosition;
                            moveDirection += hitDirection.normalized * (hit.distance - ray.magnitude);
                        }
                    }

                    // Smooth the resulting evasive movement 
                    _pivotMoveDirection = Vector3.SmoothDamp(_pivotMoveDirection, moveDirection, ref _pivotCurrentVelocity, PivotSmoothTime);
                    // Set the camera pivot position
                    _cameraPivotPosition = _desiredPivotPosition + _pivotMoveDirection;
                }
            } else {
                // EXTERNAL PIVOT 
                // Check if there is occlusion between the pivot and the character, i.e. if the pivot should move closer to the character
                _pivotRetreat = transform.position;

                Vector3 colliderHead = GetColliderHeadPosition();

                _pivotRetreat.y = Mathf.Min(_desiredPivotPosition.y, colliderHead.y);

                Vector3 extraLength = Vector3.zero;
                if (pyramidViewFrustum) {
                    // Add extra length to have the full distance checked (no near clip plane subtracted)
                    // Extra length will be handled inside the view frustum for the cuboid view frustum
                    extraLength = (_desiredPivotPosition - _pivotRetreat).normalized * CameraToUse.nearClipPlane;
                }
                // Check for occlusion between the pivot retreat and the desired pivot position
                closestPivotDistance = _rpgViewFrustum.CheckForOcclusion(_pivotRetreat, _desiredPivotPosition + extraLength, CameraToUse);
                // Fade objects between both positions
                _rpgViewFrustum.HandleObjectFading(_pivotRetreat, _desiredPivotPosition, CameraToUse);
            }
            #endregion Check pivot occlusion

            #region Camera position computations
            if (closestPivotDistance == Mathf.Infinity) {
                // Pivot is not occluded => set the pivot position and compute the desired camera position
                if (!_internalPivot || !EnableIntelligentPivot) {
                    // External pivot or intelligent pivot disabled. Otherwise, the pivot position computed by the intelligent pivot above would be overwritten
                    _cameraPivotPosition = _desiredPivotPosition;
                }

                // Compute the desired camera position
                _desiredPosition = ComputeCameraPosition(_rotationYSmooth, _rotationXSmooth, _desiredDistance);
                // Compute the closest possible camera distance by checking if there is something inside the view frustum
                float closestDistance = _rpgViewFrustum.CheckForOcclusion(_cameraPivotPosition, _desiredPosition, CameraToUse);

                if (closestDistance == Mathf.Infinity) {
                    // Camera view at the desired position is not contrained => smooth the distance change
                    _distanceSmooth = Mathf.SmoothDamp(_distanceSmooth, _desiredDistance, ref _distanceCurrentVelocity, DistanceSmoothTime);
                    // Fade objects between the pivot and the desired camera position
                    _rpgViewFrustum.HandleObjectFading(_cameraPivotPosition, _desiredPosition, CameraToUse);
                } else {
                    // Camera view is constrained => set the camera distance to the closest possible distance 
                    Vector3 dir = (_desiredPosition - _cameraPivotPosition).normalized;
                    Vector3 closestPosition;

                    if (pyramidViewFrustum) {
                        // PYRAMID VIEW FRUSTUM
                        float distance = closestDistance;

                        _maxCheckIterations = 0;
                        do {
                            closestDistance = distance;
                            // Compute the closest possible camera position
                            closestPosition = _cameraPivotPosition + dir * closestDistance;
                            distance = _rpgViewFrustum.CheckForOcclusion(_cameraPivotPosition, closestPosition, CameraToUse);
                            _maxCheckIterations++;
                        } while (distance != Mathf.Infinity && _maxCheckIterations < 5);

                        // Camera view is constrained => set the camera distance to the closest possible distance 
                        if (_distanceSmooth < closestDistance) {
                            // Smooth the distance if we move from a smaller constrained distance to a bigger constrained distance
                            _distanceSmooth = Mathf.SmoothDamp(_distanceSmooth, closestDistance, ref _distanceCurrentVelocity, DistanceSmoothTime);
                        } else {
                            // Do not smooth if the new closest distance is smaller than the current distance
                            _distanceSmooth = closestDistance;
                        }
                    } else {
                        // CUBOID VIEW FRUSTUM
                        if (_distanceSmooth < closestDistance) {
                            // Smooth the distance if we move from a smaller constrained distance to a bigger constrained distance
                            _distanceSmooth = Mathf.SmoothDamp(_distanceSmooth, closestDistance, ref _distanceCurrentVelocity, DistanceSmoothTime);
                        } else {
                            // Do not smooth if the new closest distance is smaller than the current distance
                            _distanceSmooth = closestDistance;
                        }
                        // Compute the closest possible camera position
                        closestPosition = _cameraPivotPosition + dir * closestDistance;
                    }

                    // Fade objects between the pivot and the closest possible camera position
                    _rpgViewFrustum.HandleObjectFading(_cameraPivotPosition, closestPosition, CameraToUse);
                }

                // Draw the frustum from pivot to desired camera position inclusive camera plane
                _rpgViewFrustum.DrawFrustum(_cameraPivotPosition, _desiredPosition, CameraToUse, true);
            } else {
                // Pivot is occluded => compute the new pivot position
                _cameraPivotPosition = _pivotRetreat + (_desiredPivotPosition - _pivotRetreat).normalized * closestPivotDistance;
                // Constrain the camera distance to the pivot
                _distanceSmooth = 0;
            }
            #endregion Camera position computations

            // Draw the frustum for the pivot [inclusive camera plane]
            _rpgViewFrustum.DrawFrustum(_pivotRetreat, _desiredPivotPosition, CameraToUse, closestPivotDistance != Mathf.Infinity);

            // Compute the new camera position
            return ComputeCameraPosition(_rotationYSmooth, _rotationXSmooth, _distanceSmooth);
        }

        /// <summary>
        /// Checks if the camera has an internal pivot, i.e. a pivot position inside of the used collider 
        /// </summary>
        /// <returns>True if an pivot inside the assigned collider was found, otherwise false</returns>
        public virtual bool HasInternalPivot() {
            // Check assignment since this method can also be called by an Editor script
            if (!_collider) {
                _collider = GetComponent<Collider>();

                // Check again
                if (!_collider) {
                    // No collider component found at all => assume an external pivot
                    return false;
                }
            }
            // Check if within the character collider or not
            return _collider.bounds.Contains(ComputePivotPosition(0));
        }

        /// <summary>
        /// Checks if the camera object is underwater
        /// </summary>
        /// <returns>True if the camera object is underwater, otherwise false</returns>
        protected virtual bool CheckIfUnderWater() {
            return CameraToUse.transform.position.y < (_touchedWaters.Max?.GetHeight() ?? -Mathf.Infinity) + UnderwaterThresholdTuning;
        }

        /// <summary>
        /// Handles turning on/off underwater effects
        /// </summary>
        protected virtual void HandleUnderwaterEffects() {
            // Check if the camera is underwater
            if (CheckIfUnderWater()) {
                // Change the fog settings only once
                if (!_underwater) {
                    _underwater = true;

                    EnableUnderwaterEffects();
                }
            } else {
                // Change the fog settings only once
                if (_underwater) {
                    _underwater = false;

                    DisableUnderwaterEffects();
                }
            }
        }

        /// <summary>
        /// Enables the visual underwater effects
        /// </summary>
        public virtual void EnableUnderwaterEffects() {
            RenderSettings.fogColor = UnderwaterFogColor;
            RenderSettings.fogDensity = UnderwaterFogDensity;
        }

        /// <summary>
        /// Disables the visual underwater effects
        /// </summary>
        public virtual void DisableUnderwaterEffects() {
            RenderSettings.fogColor = _defaultFogColor;
            RenderSettings.fogDensity = _defaultFogDensity;
        }

        /// <summary>
        /// Gets the internal variable values defining the current camera position
        /// </summary>
        /// <returns>The internal variable values which define the current camera position in a vector</returns>
        public virtual Vector3 GetPosition() {
            return new Vector3(_rotationXSmooth, _rotationYSmooth, _distanceSmooth);
        }

        /// <summary>
        /// Gets the position of the collider head in world space. The collider head is expected to be a safe retreat for the external camera pivot
        /// </summary>
        /// <returns>Collider head position in world coordinates, transform.position if no collider is assigned</returns>
        protected virtual Vector3 GetColliderHeadPosition() {
            if (_collider is CharacterController) {
                CharacterController collider = (CharacterController)_collider;
                return transform.TransformPoint(collider.center) + Vector3.up * collider.height * (0.5f - collider.radius);
            } else if (_collider is CapsuleCollider) {
                CapsuleCollider collider = (CapsuleCollider)_collider;
                return transform.TransformPoint(collider.center) + Vector3.up * collider.height * (0.5f - collider.radius);
            } else if (_collider is BoxCollider) {
                BoxCollider collider = (BoxCollider)_collider;
                return transform.TransformPoint(collider.center);
            } else if (_collider is SphereCollider) {
                SphereCollider collider = (SphereCollider)_collider;
                return transform.TransformPoint(collider.center);
            }

            return transform.position;
        }

        /// <summary>
        /// Sets the internal variable values that define the camera position. If smoothTransition is passed as true, the position change will be immediate
        /// </summary>
        /// <param name="rotationX">Camera rotation on the X axis</param>
        /// <param name="rotationY">Camera rotation on the Y axis</param>
        /// <param name="distance">Camera distance</param>
        /// <param name="smoothTransition">If true, the transition to the newly set internal variables will be smoothed over time. Otherwise, the camera is teleported immediately</param>
        public virtual void SetPosition(float rotationX, float rotationY, float distance, bool smoothTransition = false) {
            _rotationX = rotationX;
            _rotationY = rotationY;
            _desiredRotationY = rotationY;
            if (!smoothTransition) {
                _rotationXSmooth = _rotationX;
                _rotationYSmooth = _rotationY;
            }
            _distanceSmooth = _desiredDistance = distance;
        }

        /// <summary>
        /// Complement to method GetPosition() for setting the internal camera position values
        /// </summary>
        /// <param name="values">Variable values which should be set - see the return vector of GetPosition()</param>
        /// <param name="smoothTransition">If true, the transition to the newly set internal variables will be smoothed over time. Otherwise, the camera is teleported immediately</param>
        public virtual void SetPosition(Vector3 values, bool smoothTransition = false) {
            SetPosition(values.x, values.y, values.z, smoothTransition);
        }

        /// <summary>
        /// Gets the camera's start rotation on the horizontal axis, i.e. left/right rotation 
        /// </summary>
        /// <returns>Start rotation on the horizontal axis in degrees</returns>
        public virtual float GetStartRotationX() {
            return StartRotationX;
        }

        /// <summary>
        /// Moves the camera to the position at newPosition
        /// </summary>
        /// <param name="newPosition">The position the camera should move to</param>
        /// <param name="smoothTransition">If true, the transition to the new position will be smoothed over time. Otherwise, the camera is teleported immediately</param>
        public abstract void MoveTo(Vector3 newPosition, bool smoothTransition = false);

        /// <summary>
        /// Resets the camera view behind the character + starting X rotation, starting Y rotation and starting distance StartDistance
        /// </summary>
        /// <param name="smoothTransition">If true, the transition to the default variable values will be smoothed over time. Otherwise, the camera is teleported immediately</param>        
        public virtual void ResetView(bool smoothTransition = true) {
            _rotationX = GetStartRotationX();
            _rotationY = _desiredRotationY = StartRotationY;
            _desiredDistance = StartDistance;

            if (!smoothTransition) {
                _rotationXSmooth = _rotationX;
                _rotationYSmooth = _rotationY;
                _distanceSmooth = _desiredDistance;
            }
        }

        /// <summary>
        /// Updates the skybox of the camera UsedCamera
        /// </summary>
        /// <param name="skybox">The new skybox to use</param>
        public virtual void SetUsedSkybox(Material skybox) {
            // Set the new skybox
            UsedSkybox = skybox;
            // Signal that the skybox changed for the next frame
            _skyboxChanged = true;
        }

        /// <summary>
        /// Checks for a change of the UsedSkybox via SetUsedSkybox()
        /// </summary>
        protected virtual void CheckForChangedSkybox() {
            // Check if the UsedSkybox changed
            if (_skyboxChanged) {
                // Update the used camera's skybox
                _skybox.material = UsedSkybox;
                _skyboxChanged = false;
            }
        }

        /// <summary>
        /// "OnTriggerEnter happens on the FixedUpdate function when two GameObjects collide" - Unity Documentation
        /// </summary>
        /// <param name="other">Collider that entered the trigger collider</param>
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
        /// If Gizmos are enabled, this method draws some utility/debugging spheres
        /// </summary>
        protected virtual void OnDrawGizmos() {
            Color cyan = new Color(0.0f, 1.0f, 1.0f, 0.65f);

            if (Application.isPlaying) {
                // Draw the camera pivot at its position in cyan
                Gizmos.color = cyan;
                Gizmos.DrawSphere(_cameraPivotPosition, 0.1f);
            } else {
                // Draw the camera pivot at its position in cyan
                Gizmos.color = cyan;
                Gizmos.DrawSphere(transform.TransformPoint(CameraPivotLocalPosition), 0.1f);

                // Draw the currently set up start position of the CameraToUse in yellow
                Gizmos.color = Color.yellow;
                _cameraPivotPosition = transform.TransformPoint(CameraPivotLocalPosition);
                Vector3 cameraStartPosition = ComputeCameraPosition(StartRotationY, GetStartRotationX(), StartDistance);
                Gizmos.DrawSphere(cameraStartPosition, 0.3f);
            }
        }
    }
}
