using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JohnStairs.RCC.Character;

namespace JohnStairs.RCC {
    public class MovingPlatform : MonoBehaviour {
        /// <summary>
        /// Start position
        /// </summary>
        public Transform StartingPoint;
        /// <summary>
        /// Target position
        /// </summary>
        public Transform TurningPoint;
        /// <summary>
        /// Time it takes for moving one way
        /// </summary>
        public float Duration = 2.0f;
        /// <summary>
        /// Time the platform waits before moving again
        /// </summary>
        public float WaitTime = 2.0f;

        /// <summary>
        /// Start position
        /// </summary>
        protected Transform _start;
        /// <summary>
        /// Current target position to move to and align with
        /// </summary>
        protected Transform _target;
        /// <summary>
        /// Parameter for interpolation, here: passed time
        /// </summary>
        protected float _t;
        /// <summary>
        /// If true, the platform waits before moving on
        /// </summary>
        protected bool _waiting;
        /// <summary>
        /// The collider for detecting new or leaving passengers
        /// </summary>
        protected BoxCollider _triggerCollider;
        /// <summary>
        /// Set of all passengers to move (and rotate) with this platform
        /// </summary>
        protected HashSet<RPGMotor> _passengers;
        /// <summary>
        /// Map from passenger RPGMotor script -> passenger position in this object's local space
        /// </summary>
        protected Dictionary<RPGMotor, Vector3> _localPassengerPositions;

        protected virtual void Start() {
            _start = StartingPoint;
            _target = TurningPoint;

            BoxCollider[] boxColliders = GetComponents<BoxCollider>();
            foreach (BoxCollider boxCollider in boxColliders) {
                if (boxCollider.isTrigger) {
                    _triggerCollider = boxCollider;
                    break;
                }
            }

            if (!_triggerCollider) {
                Debug.LogWarning("No trigger collider on game object " + name + " found! Please attach a collider with \"Is Trigger\" = true to make the MovingPlatform component work");
            }

            _passengers = new HashSet<RPGMotor>();
            _localPassengerPositions = new Dictionary<RPGMotor, Vector3>();
        }

        // FixedUpdate because of character controller collision detection
        protected virtual void FixedUpdate() {
            if (Vector3.Distance(transform.position, _target.position) < 0.01f
                && Quaternion.Angle(transform.rotation, _target.rotation) < 0.5f) {
                if (_target == TurningPoint) {
                    // Turning point reached
                    _start = TurningPoint;
                    _target = StartingPoint;
                } else {
                    // Back at starting point
                    _start = StartingPoint;
                    _target = TurningPoint;
                }
                // Reset the interpolation parameter
                _t = 0;
                // Pause movement
                StartCoroutine(WaitingCoroutine());
            }

            if (_waiting) {
                return;
            }

            _t += Time.deltaTime / Duration;

            // Process all passengers to check if everyone is still aboard
            ProcessPassengers();

            Quaternion lastRotation = transform.rotation;
            // Translate and rotate this object
            transform.position = Vector3.Lerp(_start.position, _target.position, _t);
            transform.rotation = Quaternion.Lerp(_start.rotation, _target.rotation, _t);
            // Delta rotation from last to current/new rotation
            Quaternion delta = transform.rotation * Quaternion.Inverse(lastRotation);

            // Move (and rotate) all passengers
            foreach (KeyValuePair<RPGMotor, Vector3> entry in _localPassengerPositions) {
                RPGMotor rpgMotor = entry.Key;
                rpgMotor.TeleportTo(transform.TransformPoint(entry.Value));

                if (rpgMotor.RotateWithRotatingGround) {
                    rpgMotor.Rotate(delta);
                }
            }

            _localPassengerPositions.Clear();
        }

        /// <summary>
        /// Coroutine for waiting WaitTime seconds at the start/turning point
        /// </summary>
        protected virtual IEnumerator WaitingCoroutine() {
            _waiting = true;
            yield return new WaitForSeconds(WaitTime);
            _waiting = false;
        }

        /// <summary>
        /// Processes all passengers and determines the new set of passengers
        /// </summary>
        protected void ProcessPassengers() {
            HashSet<RPGMotor> currentPassengers = new HashSet<RPGMotor>();

            // Go through all passengers
            foreach (RPGMotor passenger in _passengers) {
                if (!passenger.GroundAffectsJumping || IsAbovePlatform(passenger)) {
                    // In the first case, the passenger removal is done via OnTriggerExit
                    // In the second case, the passenger should only be kept when above the platform
                    currentPassengers.Add(passenger);
                    _localPassengerPositions.Add(passenger, transform.InverseTransformPoint(passenger.transform.position));
                }
            }
            // Set the new set of current passengers
            _passengers = currentPassengers;
        }

        /// <summary>
        /// Checks if a passenger is still above the platform. Usually used for checking passengers with "GroundAffectsJumping" to see if 
        /// they left the inertial space via jumping or not
        /// </summary>
        /// <param name="rpgMotor">The RPGMotor component of the passenger to check</param>
        /// <returns>True if the passenger is above this object's trigger collider, otherwise false</returns>
        protected bool IsAbovePlatform(RPGMotor rpgMotor) {
            Vector3 triggerSize = _triggerCollider.size; // local trigger collider size
            Vector3 characterPositionLocal = transform.InverseTransformPoint(rpgMotor.transform.position);

            if (characterPositionLocal.y < _triggerCollider.center.y - triggerSize.y * 0.5f) {
                // Position is below the box collider => passenger left the platform
                return false;
            }

            // RPGMotor has the Character Controller as a prerequisite => get the radius of its collider
            float characterControllerRadius = rpgMotor.GetComponent<CharacterController>().radius;

            if (characterPositionLocal.x - characterControllerRadius <= triggerSize.x * 0.5f
                && characterPositionLocal.x + characterControllerRadius >= -triggerSize.x * 0.5f
                && characterPositionLocal.z - characterControllerRadius <= triggerSize.z * 0.5f
                && characterPositionLocal.z + characterControllerRadius >= -triggerSize.z * 0.5f) {
                // Passenger is between the bounds of the trigger collider in the XZ plane
                return true;
            }

            // Passenger must have left the platform
            return false;
        }

        /// <summary>
        /// "OnTriggerEnter happens on the FixedUpdate function when two GameObjects collide" - Unity Documentation
        /// </summary>
        /// <param name="other">Collider that entered the trigger collider</param>
        protected void OnTriggerEnter(Collider other) {
            RPGMotor rpgMotor = other.GetComponent<RPGMotor>();

            if (rpgMotor?.MoveWithMovingGround ?? false) {
                _passengers.Add(rpgMotor);
            }
        }

        /// <summary>
        /// "OnTriggerExit is called when the Collider other has stopped touching the trigger" - Unity Documentation
        /// </summary>
        /// <param name="other">Left trigger collider</param>
        protected void OnTriggerExit(Collider other) {
            RPGMotor rpgMotor = other.GetComponent<RPGMotor>();

            if (rpgMotor) {
                if (!rpgMotor.GroundAffectsJumping) {
                    _passengers.Remove(rpgMotor);
                }
            }
        }

        /// <summary>
        /// If Gizmos are enabled, this method draws some utility/debugging spheres
        /// </summary>
        protected virtual void OnDrawGizmos() {
            Gizmos.color = Color.gray;

            if (StartingPoint) {
                Gizmos.DrawWireCube(StartingPoint.position, transform.localScale);
            }

            if (TurningPoint) {
                Gizmos.DrawWireCube(TurningPoint.position, transform.localScale);
            }
        }
    }
}
