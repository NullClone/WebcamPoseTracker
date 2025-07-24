#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace WPT
{
    [CustomEditor(typeof(InferenceRunner))]
    sealed class InferenceRunnerEditor : Editor
    {
        // Properties

        private SerializedProperty _backendType;
        private SerializedProperty _performanceLevel;
        private SerializedProperty _imageSource;
        private SerializedProperty _scoreThreshold;
        private SerializedProperty _filterMode;
        private SerializedProperty _kalmanParamQ;
        private SerializedProperty _kalmanParamR;
        private SerializedProperty _keypoints;


        // Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(_imageSource);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_performanceLevel);
            EditorGUILayout.PropertyField(_backendType);
            EditorGUILayout.Slider(_scoreThreshold, 0f, 1f);
            EditorGUILayout.Space();

            EditorGUI.EndDisabledGroup();

            var filterMode = (FilterMode)_filterMode.enumValueFlag;

            _filterMode.enumValueFlag = (int)(FilterMode)EditorGUILayout.EnumFlagsField("Filter", filterMode);

            EditorGUI.indentLevel++;

            if ((filterMode & FilterMode.KalmanFilter) != 0)
            {
                EditorGUILayout.PropertyField(_kalmanParamQ);
                EditorGUILayout.PropertyField(_kalmanParamR);
            }
            if ((filterMode & FilterMode.LowPassFilter) != 0)
            {
                EditorGUILayout.HelpBox("Low Pass Filter is not implemented yet.", MessageType.Warning);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_keypoints);

            serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            _backendType = serializedObject.FindProperty("_backendType");
            _performanceLevel = serializedObject.FindProperty("_performanceLevel");
            _imageSource = serializedObject.FindProperty("_imageSource");
            _scoreThreshold = serializedObject.FindProperty("_scoreThreshold");
            _filterMode = serializedObject.FindProperty("_filterMode");
            _kalmanParamQ = serializedObject.FindProperty("_kalmanParamQ");
            _kalmanParamR = serializedObject.FindProperty("_kalmanParamR");
            _keypoints = serializedObject.FindProperty("_keypoints");
        }
    }
}
#endif