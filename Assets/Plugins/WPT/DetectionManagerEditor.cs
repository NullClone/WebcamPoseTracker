#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace WPT
{
    [CustomEditor(typeof(DetectionManager))]
    sealed class DetectionManagerEditor : Editor
    {
        // Properties

        private SerializedProperty _poseDetection;
        private SerializedProperty _filterMode;
        private SerializedProperty _timeInterval;
        private SerializedProperty _noise;
        private SerializedProperty _nOrder;
        private SerializedProperty _smooth;


        // Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_poseDetection);
            EditorGUILayout.Space();

            var filterMode = (FilterMode)_filterMode.enumValueFlag;

            _filterMode.enumValueFlag = (int)(FilterMode)EditorGUILayout.EnumFlagsField("Filter", filterMode);

            EditorGUI.indentLevel++;

            if ((filterMode & FilterMode.KalmanFilter) != 0)
            {
                EditorGUILayout.LabelField("Kalman Filter", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.Slider(_timeInterval, 0f, 1f);
                EditorGUILayout.Slider(_noise, 0f, 1f);

                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    UpdateKalmanFilter();
                }

                EditorGUILayout.Space();
            }

            if ((filterMode & FilterMode.LowPassFilter) != 0)
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);

                EditorGUILayout.LabelField("Low Pass Filter", EditorStyles.boldLabel);
                EditorGUILayout.Slider(_smooth, 0f, 1f);
                EditorGUILayout.IntSlider(_nOrder, 1, 9);

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            _poseDetection = serializedObject.FindProperty("_poseDetection");
            _filterMode = serializedObject.FindProperty("_filterMode");
            _timeInterval = serializedObject.FindProperty("_timeInterval");
            _noise = serializedObject.FindProperty("_noise");
            _nOrder = serializedObject.FindProperty("_nOrder");
            _smooth = serializedObject.FindProperty("_smooth");
        }

        private void UpdateKalmanFilter()
        {
            var kalmanFilters = ((DetectionManager)target).KalmanFilters;

            for (int i = 0; i < kalmanFilters.Length; i++)
            {
                kalmanFilters[i].SetParameter(_timeInterval.doubleValue, _noise.doubleValue);
            }
        }
    }
}

#endif