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


        // Methods

        public override void OnInspectorGUI()
        {
            var inferenceRunner = (InferenceRunner)target;

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(_imageSource);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_model);
            EditorGUILayout.Slider(_scoreThreshold, 0f, 1f);
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Debugger"))
            {
                if (inferenceRunner.gameObject.GetComponent<InferenceRunnerDebugger>()) return;

                Undo.AddComponent<InferenceRunnerDebugger>(inferenceRunner.gameObject);
            }

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            _model = serializedObject.FindProperty("_model");
            _imageSource = serializedObject.FindProperty("_imageSource");
            _scoreThreshold = serializedObject.FindProperty("_scoreThreshold");
        }
    }
}
#endif