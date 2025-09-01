#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace WPT
{
    [CustomEditor(typeof(PoseDetection))]
    sealed class PoseDetectionEditor : Editor
    {
        // Properties

        private SerializedProperty _imageSource;
        private SerializedProperty _keypoints;
        private SerializedProperty _performanceLevel;
        private SerializedProperty _backendType;
        private SerializedProperty _scoreThreshold;


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
            EditorGUILayout.PropertyField(_keypoints);

            serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            _imageSource = serializedObject.FindProperty("_imageSource");
            _keypoints = serializedObject.FindProperty("_keypoints");
            _performanceLevel = serializedObject.FindProperty("_performanceLevel");
            _backendType = serializedObject.FindProperty("_backendType");
            _scoreThreshold = serializedObject.FindProperty("_scoreThreshold");
        }
    }
}
#endif