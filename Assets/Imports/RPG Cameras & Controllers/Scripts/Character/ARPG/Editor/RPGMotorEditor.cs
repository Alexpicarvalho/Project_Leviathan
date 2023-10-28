using UnityEditor;

namespace JohnStairs.RCC.Character.ARPG {
    [CustomEditor(typeof(RPGMotorARPG))]
    public class RPGMotorEditor : Editor {
        bool showMovementSpeedSettings = true;
        bool showRotationSettings = true;
        bool showJumpingSettings = true;
        bool showMovingGroundsSettings = true;
        bool showMiscSettings = true;

        #region Movement speed variables
        SerializedProperty RunSpeed;
        SerializedProperty WalkSpeed;
        SerializedProperty CrouchSpeed;
        SerializedProperty SprintSpeedMultiplier;
        SerializedProperty SwimSpeedMultiplier;
        #endregion

        #region Rotation variables
        SerializedProperty CompleteTurnWhileStanding;
        SerializedProperty RotationTime;
        #endregion

        #region Jumping variables
        SerializedProperty JumpHeight;
        SerializedProperty EnableMidairJumps;
        SerializedProperty AllowedMidairJumps;
        SerializedProperty EnableMidairMovement;
        SerializedProperty MidairSpeed;
        SerializedProperty EnableSwimmingJumps;
        #endregion

        #region Moving grounds variables
        SerializedProperty MoveWithMovingGround;
        SerializedProperty RotateWithRotatingGround;
        SerializedProperty GroundAffectsJumping;
        #endregion

        #region Misc variables
        SerializedProperty IgnoredLayers;
        SerializedProperty EnableSliding;
        SerializedProperty SlidingTimeout;
        SerializedProperty AntiStuckTimeout;
        SerializedProperty EnableCollisionMovement;
        SerializedProperty SwimmingStartHeight;
        SerializedProperty FlyingTimeout;
        SerializedProperty GroundedTolerance;
        SerializedProperty FallingThreshold;
        SerializedProperty Gravity;
        #endregion

        public void OnEnable() {
            #region Movement speed variables 
            RunSpeed = serializedObject.FindProperty("RunSpeed");
            WalkSpeed = serializedObject.FindProperty("WalkSpeed");
            CrouchSpeed = serializedObject.FindProperty("CrouchSpeed");
            SprintSpeedMultiplier = serializedObject.FindProperty("SprintSpeedMultiplier");
            SwimSpeedMultiplier = serializedObject.FindProperty("SwimSpeedMultiplier");
            #endregion
            #region Rotation variables
            CompleteTurnWhileStanding = serializedObject.FindProperty("CompleteTurnWhileStanding");
            RotationTime = serializedObject.FindProperty("RotationTime");
            #endregion
            #region Jumping variables
            JumpHeight = serializedObject.FindProperty("JumpHeight");
            EnableMidairJumps = serializedObject.FindProperty("EnableMidairJumps");
            AllowedMidairJumps = serializedObject.FindProperty("AllowedMidairJumps");
            EnableMidairMovement = serializedObject.FindProperty("EnableMidairMovement");
            MidairSpeed = serializedObject.FindProperty("MidairSpeed");
            EnableSwimmingJumps = serializedObject.FindProperty("EnableSwimmingJumps");
            #endregion
            #region Moving grounds variables
            MoveWithMovingGround = serializedObject.FindProperty("MoveWithMovingGround");
            RotateWithRotatingGround = serializedObject.FindProperty("RotateWithRotatingGround");
            GroundAffectsJumping = serializedObject.FindProperty("GroundAffectsJumping");
            #endregion
            #region Misc variables
            IgnoredLayers = serializedObject.FindProperty("IgnoredLayers");
            EnableSliding = serializedObject.FindProperty("EnableSliding");
            SlidingTimeout = serializedObject.FindProperty("SlidingTimeout");
            AntiStuckTimeout = serializedObject.FindProperty("AntiStuckTimeout");
            EnableCollisionMovement = serializedObject.FindProperty("EnableCollisionMovement");
            SwimmingStartHeight = serializedObject.FindProperty("SwimmingStartHeight");
            FlyingTimeout = serializedObject.FindProperty("FlyingTimeout");
            GroundedTolerance = serializedObject.FindProperty("GroundedTolerance");
            FallingThreshold = serializedObject.FindProperty("FallingThreshold");
            Gravity = serializedObject.FindProperty("Gravity");
            #endregion
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            #region Movement speed variables
            showMovementSpeedSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMovementSpeedSettings, "Movement speed");
            if (showMovementSpeedSettings) {
                EditorGUILayout.PropertyField(RunSpeed);
                EditorGUILayout.PropertyField(WalkSpeed);
                EditorGUILayout.PropertyField(CrouchSpeed);
                EditorGUILayout.PropertyField(SprintSpeedMultiplier);
                EditorGUILayout.PropertyField(SwimSpeedMultiplier);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Rotation variables
            showRotationSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showRotationSettings, "Rotation");
            if (showRotationSettings) {
                EditorGUILayout.PropertyField(CompleteTurnWhileStanding);
                EditorGUILayout.PropertyField(RotationTime);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Jumping variables
            showJumpingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showJumpingSettings, "Jumping");
            if (showJumpingSettings) {
                EditorGUILayout.PropertyField(JumpHeight);
                EditorGUILayout.PropertyField(EnableMidairJumps);
                if (EnableMidairJumps.boolValue) {
                    EditorGUILayout.PropertyField(AllowedMidairJumps);
                }
                EditorGUILayout.PropertyField(EnableMidairMovement);
                if (EnableMidairMovement.enumValueIndex != (int)RPGMotor.MidairMovement.Never) {
                    EditorGUILayout.PropertyField(MidairSpeed);
                }
                EditorGUILayout.PropertyField(EnableSwimmingJumps);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Moving grounds variables
            showMovingGroundsSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMovingGroundsSettings, "Moving grounds");
            if (showMovingGroundsSettings) {
                EditorGUILayout.PropertyField(MoveWithMovingGround);
                if (MoveWithMovingGround.boolValue) {
                    EditorGUILayout.PropertyField(RotateWithRotatingGround);
                    EditorGUILayout.PropertyField(GroundAffectsJumping);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Misc variables
            showMiscSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMiscSettings, "Miscellaneous");
            if (showMiscSettings) {
                EditorGUILayout.PropertyField(IgnoredLayers);
                EditorGUILayout.PropertyField(EnableSliding);
                if (EnableSliding.boolValue) {
                    EditorGUILayout.PropertyField(SlidingTimeout);
                }
                EditorGUILayout.PropertyField(AntiStuckTimeout);
                EditorGUILayout.PropertyField(EnableCollisionMovement);
                EditorGUILayout.PropertyField(SwimmingStartHeight);
                EditorGUILayout.PropertyField(FlyingTimeout);
                EditorGUILayout.PropertyField(GroundedTolerance);
                EditorGUILayout.PropertyField(FallingThreshold);
                EditorGUILayout.PropertyField(Gravity);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }
    }
}