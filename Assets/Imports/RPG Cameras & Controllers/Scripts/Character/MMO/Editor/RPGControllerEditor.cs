using UnityEditor;

namespace JohnStairs.RCC.Character.MMO {
    [CustomEditor(typeof(RPGControllerMMO))]
    public class RPGControllerEditor : Editor {
        SerializedProperty ActivateCharacterControl;
        SerializedProperty UseNewInputSystem;
        SerializedProperty LogInputWarnings;

        public void OnEnable() {
            ActivateCharacterControl = serializedObject.FindProperty("ActivateCharacterControl");
            UseNewInputSystem = serializedObject.FindProperty("UseNewInputSystem");
            LogInputWarnings = serializedObject.FindProperty("LogInputWarnings");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(ActivateCharacterControl);
            EditorGUILayout.PropertyField(UseNewInputSystem);
            if (!UseNewInputSystem.boolValue) {
                EditorGUILayout.PropertyField(LogInputWarnings);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}