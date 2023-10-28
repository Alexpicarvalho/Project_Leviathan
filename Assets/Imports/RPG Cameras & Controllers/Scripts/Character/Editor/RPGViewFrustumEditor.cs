using UnityEditor;

namespace JohnStairs.RCC.Character {
    [CustomEditor(typeof(RPGViewFrustum))]
    public class RPGViewFrustumEditor : Editor {
        bool showOcclusionSettings = true;
        bool showGeneralFadingSettings = true;
        bool showCharacterFadingSettings = true;

        #region Occlusion variables 
        SerializedProperty Shape;
        SerializedProperty RaysPerEdge;
        SerializedProperty OccludingLayers;
        SerializedProperty FadeObjectsBy;
        SerializedProperty _tagsForFading;
        SerializedProperty TagForFadeOut;
        SerializedProperty LayersForFading;
        SerializedProperty ViewportMargin;
        SerializedProperty EnableCameraLookUp;
        SerializedProperty LookUpTrigger;
        SerializedProperty _tagsCausingLookUp;
        SerializedProperty TagCausingLookUp;
        SerializedProperty LayersCausingLookUp;
        #endregion
        #region General fading variables 
        SerializedProperty FadeOutAlpha;
        SerializedProperty FadeInAlpha;
        SerializedProperty FadeOutDuration;
        SerializedProperty FadeInDuration;
        #endregion
        #region Character fading variables 
        SerializedProperty EnableCharacterFading;
        SerializedProperty CharacterFadeOutAlpha;
        SerializedProperty CharacterFadeStartDistance;
        SerializedProperty CharacterFadeEndDistance;
        #endregion

        public void OnEnable() {
            #region Occlusion variables
            Shape = serializedObject.FindProperty("Shape");
            RaysPerEdge = serializedObject.FindProperty("RaysPerEdge");
            OccludingLayers = serializedObject.FindProperty("OccludingLayers");
            FadeObjectsBy = serializedObject.FindProperty("FadeObjectsBy");
            _tagsForFading = serializedObject.FindProperty("TagsForFading");
            LayersForFading = serializedObject.FindProperty("LayersForFading");
            ViewportMargin = serializedObject.FindProperty("ViewportMargin");
            EnableCameraLookUp = serializedObject.FindProperty("EnableCameraLookUp");
            LookUpTrigger = serializedObject.FindProperty("LookUpTrigger");
            _tagsCausingLookUp = serializedObject.FindProperty("TagsCausingLookUp");
            LayersCausingLookUp = serializedObject.FindProperty("LayersCausingLookUp");
            #endregion
            #region General fading variables 
            FadeOutAlpha = serializedObject.FindProperty("FadeOutAlpha");
            FadeInAlpha = serializedObject.FindProperty("FadeInAlpha");
            FadeOutDuration = serializedObject.FindProperty("FadeOutDuration");
            FadeInDuration = serializedObject.FindProperty("FadeInDuration");
            #endregion
            #region Character fading variables
            EnableCharacterFading = serializedObject.FindProperty("EnableCharacterFading");
            CharacterFadeOutAlpha = serializedObject.FindProperty("CharacterFadeOutAlpha");
            CharacterFadeStartDistance = serializedObject.FindProperty("CharacterFadeStartDistance");
            CharacterFadeEndDistance = serializedObject.FindProperty("CharacterFadeEndDistance");
            #endregion
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            #region Occlusion variables
            showOcclusionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showOcclusionSettings, "Occlusion settings");
            if (showOcclusionSettings) {
                EditorGUILayout.PropertyField(Shape);
                if (Shape.enumValueIndex == (int)RPGViewFrustum.FrustumShape.Pyramid) {
                    EditorGUILayout.PropertyField(RaysPerEdge);
                }
                EditorGUILayout.PropertyField(OccludingLayers);

                EditorGUILayout.PropertyField(FadeObjectsBy);
                if (FadeObjectsBy.enumValueIndex == (int)RPGViewFrustum.ObjectTriggerOption.Tag) {
                    #region TagsForFadeOut
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(_tagsForFading.displayName);
                    int numberOfTagsForFading = EditorGUILayout.IntField(_tagsForFading.arraySize);
                    if (numberOfTagsForFading > 0
                        && numberOfTagsForFading != _tagsForFading.arraySize) {
                        // Resize the array
                        _tagsForFading.arraySize = numberOfTagsForFading;
                    }
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < _tagsForFading.arraySize; i++) {
                        TagForFadeOut = _tagsForFading.GetArrayElementAtIndex(i);
                        TagForFadeOut.stringValue = EditorGUILayout.TagField("└ Tag " + i, TagForFadeOut.stringValue);
                    }
                    #endregion
                } else if (LookUpTrigger.enumValueIndex == (int)RPGViewFrustum.ObjectTriggerOption.Layer) {
                    EditorGUILayout.PropertyField(LayersForFading);
                } else {
                    EditorGUILayout.LabelField("Component For Fading", "FadeOut");
                }

                EditorGUILayout.PropertyField(ViewportMargin);
                EditorGUILayout.PropertyField(EnableCameraLookUp);
                if (EnableCameraLookUp.boolValue) {
                    EditorGUILayout.PropertyField(LookUpTrigger);
                    if (LookUpTrigger.enumValueIndex == (int)RPGViewFrustum.ObjectTriggerOption.Tag) {
                        #region TagsForFadeOut
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(_tagsCausingLookUp.displayName);
                        int numberOfTagsCausingLookUp = EditorGUILayout.IntField(_tagsCausingLookUp.arraySize);
                        if (numberOfTagsCausingLookUp > 0
                            && numberOfTagsCausingLookUp != _tagsCausingLookUp.arraySize) {
                            // Resize the array
                            _tagsCausingLookUp.arraySize = numberOfTagsCausingLookUp;
                        }
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < _tagsCausingLookUp.arraySize; i++) {
                            TagCausingLookUp = _tagsCausingLookUp.GetArrayElementAtIndex(i);
                            TagCausingLookUp.stringValue = EditorGUILayout.TagField("└ Tag " + i, TagCausingLookUp.stringValue);
                        }
                        #endregion
                    } else if (LookUpTrigger.enumValueIndex == (int)RPGViewFrustum.ObjectTriggerOption.Layer) {
                        EditorGUILayout.PropertyField(LayersCausingLookUp);
                    } else {
                        EditorGUILayout.LabelField("Comp. Causing Look Up", "CauseCameraLookUp");
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region General fading variables 
            showGeneralFadingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGeneralFadingSettings, "General fading settings");
            if (showGeneralFadingSettings) {
                EditorGUILayout.PropertyField(FadeOutAlpha);
                EditorGUILayout.PropertyField(FadeInAlpha);
                EditorGUILayout.PropertyField(FadeOutDuration);
                EditorGUILayout.PropertyField(FadeInDuration);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            #region Character fading variables
            showCharacterFadingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCharacterFadingSettings, "Character fading settings");
            if (showCharacterFadingSettings) {
                EditorGUILayout.PropertyField(EnableCharacterFading);
                if (EnableCharacterFading.boolValue) {
                    EditorGUILayout.PropertyField(CharacterFadeOutAlpha);
                    EditorGUILayout.PropertyField(CharacterFadeStartDistance);
                    EditorGUILayout.PropertyField(CharacterFadeEndDistance);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }
    }
}