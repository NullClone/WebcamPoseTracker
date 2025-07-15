#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace WPT
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(InferenceRunner))]
    sealed class InferenceRunnerEditor : Editor
    {
        // Properties

        private SerializedProperty _model;
        private SerializedProperty _imageSource;
        private SerializedProperty _scoreThreshold;
        private SerializedProperty _kalmanParamQ;
        private SerializedProperty _kalmanParamR;
        private SerializedProperty _keypoints;


        // Methods

        public override void OnInspectorGUI()
        {
            var instance = (InferenceRunner)target;

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(_model);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_imageSource);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.Slider(_scoreThreshold, 0f, 1f);
            EditorGUILayout.Space();

            instance._filterMode = (FilterMode)EditorGUILayout.EnumFlagsField("Filter", instance._filterMode);

            EditorGUI.indentLevel++;

            if ((instance._filterMode & FilterMode.KalmanFilter) != 0)
            {
                EditorGUILayout.PropertyField(_kalmanParamQ);
                EditorGUILayout.PropertyField(_kalmanParamR);
                EditorGUILayout.Space();
            }
            if ((instance._filterMode & FilterMode.LowPassFilter) != 0)
            {
                EditorGUILayout.HelpBox("Low Pass Filter is not implemented yet.", MessageType.Warning);
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(_keypoints);

            serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            _model = serializedObject.FindProperty("_model");
            _imageSource = serializedObject.FindProperty("_imageSource");
            _scoreThreshold = serializedObject.FindProperty("_scoreThreshold");
            _kalmanParamQ = serializedObject.FindProperty("_kalmanParamQ");
            _kalmanParamR = serializedObject.FindProperty("_kalmanParamR");
            _keypoints = serializedObject.FindProperty("_keypoints");
        }
    }
}
#endif