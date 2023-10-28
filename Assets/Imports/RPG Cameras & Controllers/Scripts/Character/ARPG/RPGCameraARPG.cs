using UnityEngine;

namespace JohnStairs.RCC.Character.ARPG {
    [RequireComponent(typeof(RPGViewFrustum))]
    public class RPGCameraARPG : RPGCamera {

        protected override void LateUpdate() {
            base.LateUpdate();            

            // Check if the camera's Y rotation is contrained by terrain
            bool enableCameraLookUp = !(_rpgMotor?.IsSwimming() ?? false) 
                                        && _rpgViewFrustum.IsTouchingGround(CameraToUse, _cameraPivotPosition);

            float smoothTime = RotationSmoothTime;
            float rotationYMinLimit = _rotationY;

            #region Process rotation input axes
            if (ActivateCameraControl
                && !_orbitingStartedOverGUI
                && (_inputAllowOrbiting || _inputAllowOrbitingWithCharRotation || AlwaysRotateCamera)) {

                #region Rotation X input processing
                if (!LockRotationX) {
                    float rotationXinput = 0;

                    rotationXinput = (InvertRotationX ? 1 : -1) * _inputRotationAmount.x;
                    _rotationX += rotationXinput * RotationXSensitivity;

                    if (ConstrainRotationX) {
                        // Clamp the rotation in X axis direction
                        _rotationX = Mathf.Clamp(_rotationX, RotationXMin, RotationXMax);
                    }
                }
                #endregion Rotation X input processing

                #region Rotation Y input processing
                // Get rotation Y axis input
                if (!LockRotationY) {
                    _desiredRotationY += (InvertRotationY ? -1 : 1) * _inputRotationAmount.y * RotationYSensitivity;
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

            if (_characterInfo?.LockOnTarget() ?? false) {
                Vector3 cameraToTarget = _characterInfo.GetTargetPosition() - CameraToUse.transform.position;
                cameraToTarget.y = 0;
                Vector3 cameraToCharacter = transform.position - CameraToUse.transform.position;
                cameraToCharacter.y = 0;
                if (!Utils.IsAlmostEqual(cameraToTarget, cameraToCharacter)) {
                    float delta = Utils.SignedAngle(cameraToCharacter.normalized, cameraToTarget.normalized, Vector3.up);
                    _rotationX = _rotationXSmooth + delta;
                }
            }

            if (ActivateCameraControl) {
                ComputeDesiredDistance();
            }

            _rotationXSmooth = Mathf.SmoothDamp(_rotationXSmooth, _rotationX, ref _rotationXCurrentVelocity, smoothTime);
            _rotationYSmooth = Mathf.SmoothDamp(_rotationYSmooth, _rotationY, ref _rotationYCurrentVelocity, smoothTime);

            #region Update the camera transform
            // Compute the new camera position            
            CameraToUse.transform.position = ComputeNewCameraPosition();
            
            // Check if we are in third or first person and adjust the camera rotation behavior
            if (_distanceSmooth > 0.1f) {
                // In third person => orbit camera
                CameraToUse.transform.LookAt(_cameraPivotPosition);
            } else {
                // In first person => normal camera rotation
                CameraToUse.transform.rotation = Quaternion.Euler(new Vector3(_rotationYSmooth, _rotationXSmooth, 0));
            }

            if (enableCameraLookUp /*|| _distanceSmooth <= 0.1f*/) {
                // Camera lies on terrain => enable looking up			
                float lookUpDegrees = _desiredRotationY - _rotationY;
                CameraToUse.transform.Rotate(Vector3.right, lookUpDegrees);
            }
            #endregion Update the camera transform

            ConsumeEventInputs();
        }

        public override float GetStartRotationX() {
            // Offset by the current character rotation so that it is relative to the character forward
            return StartRotationX + transform.eulerAngles.y;
        }
        
        protected override Vector3 ComputeCameraPosition(float xAxisDegrees, float yAxisDegrees, float distance) {
            Vector3 offset = -Vector3.forward * distance;
            // Create the combined rotation of X and Y axis rotation
            Quaternion rotXaxis = Quaternion.AngleAxis(xAxisDegrees, Vector3.right);
            Quaternion rotYaxis = Quaternion.AngleAxis(yAxisDegrees, Vector3.up);
            Quaternion rotation = rotYaxis * rotXaxis;

            return _cameraPivotPosition + rotation * offset;
        }

        protected override Vector3 ComputePivotPosition(float yAxisDegrees) {
            Quaternion rotYaxis = Quaternion.AngleAxis(yAxisDegrees, Vector3.up);
            return transform.position + rotYaxis * CameraPivotLocalPosition;
        }

        public override void MoveTo(Vector3 newPosition, bool smoothTransition = false) {
            Vector3 lookDirection = _cameraPivotPosition - newPosition;
            float distance = lookDirection.magnitude;

            float rotationX = Utils.SignedAngle(-Vector3.forward, lookDirection, Vector3.up);
            float rotationY = Vector3.Angle(Vector3.up, lookDirection) - 90.0f;

            SetPosition(rotationX, rotationY, distance, smoothTransition);
        }
    }
}
