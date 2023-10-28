using UnityEditor;

namespace JohnStairs.RCC.Character.MMO {
    [CustomEditor(typeof(RPGCameraMMO))]
    public class RPGCameraEditor : Editor {
        bool showGeneralSettings = true;
        bool showRotationXSettings = true;
        bool showRotationYSettings = true;
        bool showDistanceSettings = true;
        bool showCursorSettings = true;
        bool showAlignmentSettings = true;
        bool showCharacterAlignmentSettings = true;
        bool showUnderwaterSettings = true;

        #region General variables
        SerializedProperty UseNewInputSystem;
        SerializedProperty LogInputWarnings;
        SerializedProperty UsedCamera;
        SerializedProperty CameraToUse;
        SerializedProperty UsedSkybox;
        SerializedProperty CameraPivotLocalPosition;
        SerializedProperty EnableIntelligentPivot;
        SerializedProperty PivotSmoothTime;
        SerializedProperty ActivateCameraControl;
        SerializedProperty AlwaysRotateCamera;
        SerializedProperty RotationSmoothTime;
        SerializedProperty RotateWithCharacter;
        #endregion

        #region Rotation X variables
        SerializedProperty StartRotationX;
        SerializedProperty LockRotationX;
        SerializedProperty InvertRotationX;
        SerializedProperty RotationXSensitivity;
        SerializedProperty ConstrainRotationX;
        SerializedProperty RotationXMin;
        SerializedProperty RotationXMax;
        #endregion

        #region Rotation Y variables
        SerializedProperty StartRotationY;
        SerializedProperty LockRotationY;
        SerializedProperty InvertRotationY;
        SerializedProperty RotationYSensitivity;
        SerializedProperty RotationYMin;
        SerializedProperty RotationYMax;
        #endregion

        #region Distance variables
        SerializedProperty StartDistance;
        SerializedProperty StartZoomOut;
        SerializedProperty ZoomSensitivity;
        SerializedProperty MinDistance;
        SerializedProperty MaxDistance;
        SerializedProperty DistanceSmoothTime;
        #endregion

        #region Cursor variables
        SerializedProperty HideCursor;
        SerializedProperty CursorBehaviorOrbiting;
        #endregion

        #region Alignment variables
        SerializedProperty AlignCameraWhenMoving;
        SerializedProperty SupportWalkingBackwards;
        SerializedProperty AlignCameraSmoothTime;
        #endregion

        #region Character Alignment variables
        SerializedProperty AlignCharacter;
        SerializedProperty AlignCharacterSpeed;
        #endregion

        #region Underwater variables
        SerializedProperty EnableUnderwaterEffect;
        SerializedProperty UnderwaterFogColor;
        SerializedProperty UnderwaterFogDensity;
        SerializedProperty UnderwaterThresholdTuning;
        #endregion

        public void OnEnable() {
            #region General variables
            UseNewInputSystem = serializedObject.FindProperty("UseNewInputSystem");
            LogInputWarnings = serializedObject.FindProperty("LogInputWarnings");
            UsedCamera = serializedObject.FindProperty("UsedCamera");
            CameraToUse = serializedObject.FindProperty("CameraToUse");
            UsedSkybox = serializedObject.FindProperty("UsedSkybox");
            CameraPivotLocalPosition = serializedObject.FindProperty("CameraPivotLocalPosition");
            EnableIntelligentPivot = serializedObject.FindProperty("EnableIntelligentPivot");
            PivotSmoothTime = serializedObject.FindProperty("PivotSmoothTime");
            ActivateCameraControl = serializedObject.FindProperty("ActivateCameraControl");
            AlwaysRotateCamera = serializedObject.FindProperty("AlwaysRotateCamera");
            RotationSmoothTime = serializedObject.FindProperty("RotationSmoothTime");
            RotateWithCharacter = serializedObject.FindProperty("RotateWithCharacter");
            #endregion
            #region Rotation X variables
            StartRotationX = serializedObject.FindProperty("StartRotationX");
            LockRotationX = serializedObject.FindProperty("LockRotationX");
            InvertRotationX = serializedObject.FindProperty("InvertRotationX");
            RotationXSensitivity = serializedObject.FindProperty("RotationXSensitivity");
            ConstrainRotationX = serializedObject.FindProperty("ConstrainRotationX");
            RotationXMin = serializedObject.FindProperty("RotationXMin");
            RotationXMax = serializedObject.FindProperty("RotationXMax");
            #endregion
            #region Rotation Y variables
            StartRotationY = serializedObject.FindProperty("StartRotationY");
            LockRotationY = serializedObject.FindProperty("LockRotationY");
            InvertRotationY = serializedObject.FindProperty("InvertRotationY");
            RotationYSensitivity = serializedObject.FindProperty("RotationYSensitivity");
            RotationYMin = serializedObject.FindProperty("RotationYMin");
            RotationYMax = serializedObject.FindProperty("RotationYMax");
            #endregion
            #region Distance variables
            StartDistance = serializedObject.FindProperty("StartDistance");
            StartZoomOut = serializedObject.FindProperty("StartZoomOut");
            ZoomSensitivity = serializedObject.FindProperty("ZoomSensitivity");
            MinDistance = serializedObject.FindProperty("MinDistance");
            MaxDistance = serializedObject.FindProperty("MaxDistance");
            DistanceSmoothTime = serializedObject.FindProperty("DistanceSmoothTime");
            #endregion
            #region Cursor variables
            HideCursor = serializedObject.FindProperty("HideCursor");
            CursorBehaviorOrbiting = serializedObject.FindProperty("CursorBehaviorOrbiting");
            #endregion
            #region Alignment variables
            AlignCameraWhenMoving = serializedObject.FindProperty("AlignCameraWhenMoving");
            SupportWalkingBackwards = serializedObject.FindProperty("SupportWalkingBackwards");
            AlignCameraSmoothTime = serializedObject.FindProperty("AlignCameraSmoothTime");
            #endregion
            #region Character Alignment variables
            AlignCharacter = serializedObject.FindProperty("AlignCharacter");
            AlignCharacterSpeed = serializedObject.FindProperty("AlignCharacterSpeed");
            #endregion
            #region Underwater variables
            EnableUnderwaterEffect = serializedObject.FindProperty("EnableUnderwaterEffect");
            UnderwaterFogColor = serializedObject.FindProperty("UnderwaterFogColor");
            UnderwaterFogDensity = serializedObject.FindProperty("UnderwaterFogDensity");
            UnderwaterThresholdTuning = serializedObject.FindProperty("UnderwaterThresholdTuning");
            #endregion
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            #region General variables
            showGeneralSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGeneralSettings, "General");
            if (showGeneralSettings) {
                EditorGUILayout.PropertyField(UseNewInputSystem);
                if (!UseNewInputSystem.boolValue) {
                    EditorGUILayout.PropertyField(LogInputWarnings);
                }
                EditorGUILayout.PropertyField(UsedCamera);
                if (UsedCamera.enumValueIndex == (int)RPGCamera.CameraUsage.AssignedCamera) {
                    EditorGUILayout.PropertyField(CameraToUse);
                }
                EditorGUILayout.PropertyField(UsedSkybox);
                EditorGUILayout.PropertyField(CameraPivotLocalPosition);
                if (((RPGCamera)serializedObject.targetObject).HasInternalPivot()) {
                    EditorGUILayout.LabelField("└ Internal pivot logic applies");
                    EditorGUILayout.PropertyField(EnableIntelligentPivot);
                    if (EnableIntelligentPivot.boolValue) {
                        EditorGUILayout.PropertyField(PivotSmoothTime);
                    }
                } else {
                    EditorGUILayout.LabelField("└ External pivot logic applies");
                }
                EditorGUILayout.PropertyField(ActivateCameraControl);
                EditorGUILayout.PropertyField(AlwaysRotateCamera);
                EditorGUILayout.PropertyField(RotationSmoothTime);
                EditorGUILayout.PropertyField(RotateWithCharacter);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Rotation X variables
            showRotationXSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showRotationXSettings, "Rotation X");
            if (showRotationXSettings) {
                EditorGUILayout.PropertyField(StartRotationX);
                EditorGUILayout.PropertyField(LockRotationX);
                EditorGUILayout.PropertyField(InvertRotationX);
                EditorGUILayout.PropertyField(RotationXSensitivity);
                EditorGUILayout.PropertyField(ConstrainRotationX);
                EditorGUILayout.PropertyField(RotationXMin);
                EditorGUILayout.PropertyField(RotationXMax);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Rotation Y variables
            showRotationYSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showRotationYSettings, "Rotation Y");
            if (showRotationYSettings) {
                EditorGUILayout.PropertyField(StartRotationY);
                EditorGUILayout.PropertyField(LockRotationY);
                EditorGUILayout.PropertyField(InvertRotationY);
                EditorGUILayout.PropertyField(RotationYSensitivity);
                EditorGUILayout.PropertyField(RotationYMin);
                EditorGUILayout.PropertyField(RotationYMax);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Distance variables
            showDistanceSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showDistanceSettings, "Distance");
            if (showDistanceSettings) {
                EditorGUILayout.PropertyField(StartDistance);
                EditorGUILayout.PropertyField(StartZoomOut);
                EditorGUILayout.PropertyField(ZoomSensitivity);
                EditorGUILayout.PropertyField(MinDistance);
                EditorGUILayout.PropertyField(MaxDistance);
                EditorGUILayout.PropertyField(DistanceSmoothTime);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Cursor variables
            showCursorSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCursorSettings, "Cursor");
            if (showCursorSettings) {
                EditorGUILayout.PropertyField(HideCursor);
                EditorGUILayout.PropertyField(CursorBehaviorOrbiting);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Character alignment variables
            showAlignmentSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAlignmentSettings, "Alignment with character");
            if (showAlignmentSettings) {
                EditorGUILayout.PropertyField(AlignCameraWhenMoving);
                if (AlignCameraWhenMoving.boolValue) {
                    EditorGUILayout.PropertyField(SupportWalkingBackwards);
                }
                EditorGUILayout.PropertyField(AlignCameraSmoothTime);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Alignment variables
            showCharacterAlignmentSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCharacterAlignmentSettings, "Character alignment");
            if (showCharacterAlignmentSettings) {
                EditorGUILayout.PropertyField(AlignCharacter);
                if (AlignCharacter.enumValueIndex == (int)RPGCameraMMO.CharacterAlignment.OnAlignmentInput) {
                    EditorGUILayout.PropertyField(AlignCharacterSpeed);
                } else if (AlignCharacter.enumValueIndex == (int)RPGCameraMMO.CharacterAlignment.Always) {
                    EditorGUILayout.PropertyField(AlignCharacterSpeed);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Underwater variables
            showUnderwaterSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showUnderwaterSettings, "Underwater");
            if (showUnderwaterSettings) {
                EditorGUILayout.PropertyField(EnableUnderwaterEffect);
                if (EnableUnderwaterEffect.boolValue) {
                    EditorGUILayout.PropertyField(UnderwaterFogColor);
                    EditorGUILayout.PropertyField(UnderwaterFogDensity);
                    EditorGUILayout.PropertyField(UnderwaterThresholdTuning);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }
    }
}