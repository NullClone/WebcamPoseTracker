#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace WPT
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ImageSource))]
    sealed class ImageSourceEditor : Editor
    {
        // Properties

        private SerializedProperty _sourceType;
        private SerializedProperty _texture;
        private SerializedProperty _video;
        private SerializedProperty _videoPlayer;
        private SerializedProperty _webcamName;
        private SerializedProperty _webcamResolution;
        private SerializedProperty _webcamFrameRate;
        private SerializedProperty _camera;
        private SerializedProperty _outputTexture;
        private SerializedProperty _outputResolution;


        // Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(_sourceType);

            EditorGUI.indentLevel++;

            var type = (ImageSourceType)_sourceType.enumValueIndex;

            if (type == ImageSourceType.Texture)
            {
                EditorGUILayout.PropertyField(_texture, Labels.Asset);
            }

            if (type == ImageSourceType.Video)
            {
                EditorGUILayout.PropertyField(_video, Labels.Asset);
                EditorGUILayout.PropertyField(_videoPlayer, Labels.Player);
            }

            if (type == ImageSourceType.Webcam)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.PropertyField(_webcamName, Labels.DeviceName);

                EditorGUI.EndDisabledGroup();

                var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(60));

                if (EditorGUI.DropdownButton(rect, Labels.Select, FocusType.Keyboard))
                {
                    var menu = new GenericMenu();

                    foreach (var device in WebCamTexture.devices)
                    {
                        menu.AddItem(new GUIContent(device.name), false, () => ChangeWebcam(device.name));
                    }

                    menu.DropDown(rect);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(_webcamResolution, Labels.Resolution);
                EditorGUILayout.PropertyField(_webcamFrameRate, Labels.FrameRate);
            }

            if (type == ImageSourceType.Camera)
            {
                EditorGUILayout.PropertyField(_camera);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_outputTexture);

            if (_outputTexture.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(_outputResolution);
            }

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            _sourceType = serializedObject.FindProperty("_sourceType");
            _texture = serializedObject.FindProperty("_texture");
            _video = serializedObject.FindProperty("_video");
            _videoPlayer = serializedObject.FindProperty("_videoPlayer");
            _webcamName = serializedObject.FindProperty("_webcamName");
            _webcamResolution = serializedObject.FindProperty("_webcamResolution");
            _webcamFrameRate = serializedObject.FindProperty("_webcamFrameRate");
            _camera = serializedObject.FindProperty("_camera");
            _outputTexture = serializedObject.FindProperty("_outputTexture");
            _outputResolution = serializedObject.FindProperty("_outputResolution");
        }

        private void ChangeWebcam(string name)
        {
            serializedObject.Update();

            _webcamName.stringValue = name;

            serializedObject.ApplyModifiedProperties();
        }
    }

    static class Labels
    {
        public static GUIContent Asset = new("Asset");
        public static GUIContent Player = new("Player");
        public static GUIContent DeviceName = new("Device Name");
        public static GUIContent FrameRate = new("Frame Rate");
        public static GUIContent Resolution = new("Resolution");
        public static GUIContent Select = new("Select");
        public static GUIContent URL = new("URL");
    }
}
#endif