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
        private SerializedProperty _timeInterval;
        private SerializedProperty _noise;
        private SerializedProperty _order;
        private SerializedProperty _smooth;
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

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Slider(_scoreThreshold, 0f, 1f);
            EditorGUILayout.Space();

            var filterMode = (FilterMode)_filterMode.enumValueFlag;

            _filterMode.enumValueFlag = (int)(FilterMode)EditorGUILayout.EnumFlagsField("Filter", filterMode);

            EditorGUI.indentLevel++;

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            if ((filterMode & FilterMode.KalmanFilter) != 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Kalman Filter", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_timeInterval);
                EditorGUILayout.PropertyField(_noise);
            }

            if ((filterMode & FilterMode.LowPassFilter) != 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Low Pass Filter", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_order);
                EditorGUILayout.PropertyField(_smooth);
            }

            EditorGUI.EndDisabledGroup();

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
            _timeInterval = serializedObject.FindProperty("_timeInterval");
            _noise = serializedObject.FindProperty("_noise");
            _order = serializedObject.FindProperty("_order");
            _smooth = serializedObject.FindProperty("_smooth");
            _keypoints = serializedObject.FindProperty("_keypoints");
        }
    }
}
#endif