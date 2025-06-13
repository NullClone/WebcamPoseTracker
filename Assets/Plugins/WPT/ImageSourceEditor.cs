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
        private SerializedProperty _resolution;


        // Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(_sourceType);

            EditorGUI.indentLevel++;

            var type = (ImageSourceType)_sourceType.enumValueIndex;

            switch (type)
            {
                case ImageSourceType.Texture:
                    {
                        EditorGUILayout.PropertyField(_texture);

                        break;
                    }
                case ImageSourceType.Video:
                    {
                        EditorGUILayout.PropertyField(_video);
                        EditorGUILayout.PropertyField(_videoPlayer);

                        break;
                    }
                case ImageSourceType.Webcam:
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUI.BeginDisabledGroup(true);

                        EditorGUILayout.PropertyField(_webcamName, new GUIContent("Device Name"));

                        EditorGUI.EndDisabledGroup();

                        var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(60));

                        if (EditorGUI.DropdownButton(rect, new GUIContent("Select"), FocusType.Keyboard))
                        {
                            var menu = new GenericMenu();

                            foreach (var device in WebCamTexture.devices)
                            {
                                menu.AddItem(new GUIContent(device.name), false,
                                    () =>
                                    {
                                        serializedObject.Update();

                                        _webcamName.stringValue = device.name;

                                        serializedObject.ApplyModifiedProperties();
                                    });
                            }

                            menu.DropDown(rect);
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(_webcamResolution, new GUIContent("Resolution"));
                        EditorGUILayout.PropertyField(_webcamFrameRate, new GUIContent("FrameRate"));

                        break;
                    }
                case ImageSourceType.Camera:
                    {
                        EditorGUILayout.PropertyField(_camera);

                        break;
                    }
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_resolution, new GUIContent("Output Resolution"));

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
            _resolution = serializedObject.FindProperty("_resolution");
        }
    }
}
#endif