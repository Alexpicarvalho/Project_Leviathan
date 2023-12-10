﻿using UnityEngine;

namespace FIMSpace
{
    [DefaultExecutionOrder(-18)]
    [AddComponentMenu("FImpossible Creations/Leaning Animator")]
    public class LeaningAnimator : MonoBehaviour
    {
        public enum ELeaningEditorCat { Setup, Leaning, Advanced }
        [HideInInspector] public ELeaningEditorCat _EditorDrawSetup = ELeaningEditorCat.Setup;

        [SerializeField]//[HideInInspector]
        private LeaningProcessor Leaning;

        public LeaningProcessor Parameters { get { return Leaning; } }

        /// <summary> Manually inform leaning that character is accelerating (turn off "TryAutoDetectAcceleration" fist) </summary>
        public bool SetIsAccelerating { set { Parameters.IsCharacterAccelerating = value; } }
        /// <summary> Manually inform leaning that character is on the ground </summary>
        public bool SetIsGrounded { set { Parameters.IsCharacterGrounded = value; } }
        public bool CheckIfIsGrounded { get { return Parameters.IsCharacterGrounded; } }
        public bool CheckIfIsAcccelerating { get { return Parameters.accelerating; } }

        private void Reset()
        {
            if (Leaning == null) Leaning = new LeaningProcessor();
            Leaning.TryAutoFindReferences(transform);
        }

        private void Start()
        {
            Leaning.Initialize(this);
        }

        private void Update()
        {
            Leaning.Update();
        }

        private void FixedUpdate()
        {
            Leaning.FixedUpdate();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.BackQuote)) return; // Debug disable update
#endif
#endif

            Leaning.LateUpdate();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Leaning.DrawGizmos();
        }

        private void OnValidate()
        {
            Leaning.RefreshAnimiatorParams();
        }
#endif

        public void User_RotateSpineStart(Vector3 angles)
        {
            Leaning._UserStartBoneAddAngles = angles;
        }

        public void User_RotateSpineMiddle(Vector3 angles)
        {
            Leaning._UserMidBoneAddAngles = angles;
        }

        public void User_DeliverCustomRaycastHit(RaycastHit hit)
        {
            Leaning._UserUseCustomRaycast = true;
            Leaning._UserCustomRaycast = hit;
        }

        public void User_DeliverSlopeAngle(float angle)
        {
            Leaning._UserCustomSlopeAngle = angle;
        }

        public void User_DeliverAccelerationSpeed(float velocityMagnitude)
        {
            Leaning.customAccelerationVelocity = velocityMagnitude;
        }

        public void User_DeliverIsAccelerating(bool isAccelerating)
        {
            SetIsAccelerating = isAccelerating;
        }

        public void User_DeliverIsGrounded(bool grounded)
        {
            SetIsGrounded = grounded;
        }
    }
}