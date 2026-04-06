using UnityEngine;
using UnityEditor;


namespace GlobalSnowEffect {
    [CustomEditor(typeof(GlobalSnowIgnoreCoverage))]
    public class GlobalSnowIgnoreCoverageEditor : Editor {

        SerializedProperty blockSnow, receiveSnow, useFastMaskShader, exclusionCutOff;

        private void OnEnable() {
            blockSnow = serializedObject.FindProperty("_blockSnow");
            receiveSnow = serializedObject.FindProperty("_receiveSnow");
            useFastMaskShader = serializedObject.FindProperty("_useFastMaskShader");
            exclusionCutOff = serializedObject.FindProperty("_exclusionCutOff");
        }


        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(blockSnow);

            EditorGUILayout.PropertyField(receiveSnow);
            if (!receiveSnow.boolValue) {
                EditorGUILayout.PropertyField(useFastMaskShader);
                if (useFastMaskShader.boolValue) {
                    EditorGUILayout.PropertyField(exclusionCutOff);
                }
            }

            if (serializedObject.ApplyModifiedProperties()) {
                GlobalSnow snow = GlobalSnow.instance;
                if (snow != null) {
                    snow.RefreshExcludedObjects();
                }
            }

        }
    }

}