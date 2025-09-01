#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace WPT
{
    [CustomEditor(typeof(PoseDetection))]
    sealed class PoseDetectionEditor : Editor
    {
        // Properties

        private SerializedProperty _backendType;
        private SerializedProperty _performanceLevel;
        private SerializedProperty _imageSource;
        private SerializedProperty _scoreThreshold;
        private SerializedProperty _filterMode;
        private SerializedProperty _timeInterval;
        private SerializedProperty _noise;
        private SerializedProperty _nOrder;
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

            if ((filterMode & FilterMode.KalmanFilter) != 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Kalman Filter", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.Slider(_timeInterval, 0f, 1f);
                EditorGUILayout.Slider(_noise, 0f, 1f);

                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    UpdateKalmanFilter();
                }
            }

            if ((filterMode & FilterMode.LowPassFilter) != 0)
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Low Pass Filter", EditorStyles.boldLabel);
                EditorGUILayout.Slider(_smooth, 0f, 1f);
                EditorGUILayout.IntSlider(_nOrder, 1, 9);

                EditorGUI.EndDisabledGroup();
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
            _timeInterval = serializedObject.FindProperty("_timeInterval");
            _noise = serializedObject.FindProperty("_noise");
            _nOrder = serializedObject.FindProperty("_nOrder");
            _smooth = serializedObject.FindProperty("_smooth");
            _keypoints = serializedObject.FindProperty("_keypoints");
        }

        private void UpdateKalmanFilter()
        {
            var kalmanFilters = ((PoseDetection)target).KalmanFilters;

            for (int i = 0; i < kalmanFilters.Length; i++)
            {
                kalmanFilters[i].SetParameter(_timeInterval.doubleValue, _noise.doubleValue);
            }
        }
    }
}
#endif