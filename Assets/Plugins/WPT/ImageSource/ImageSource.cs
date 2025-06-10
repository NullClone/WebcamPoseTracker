using UnityEngine;
using UnityEngine.Video;

namespace WPT
{
    public sealed class ImageSource : MonoBehaviour
    {
        // Properties

        public Texture Texture => OutputBuffer;

        public Vector2Int OutputResolution => _outputResolution;


        RenderTexture OutputBuffer => _outputTexture != null ? _outputTexture : _buffer;


        // Fields     

        [SerializeField] SourceType _sourceType = SourceType.Texture;
        [SerializeField] Texture2D _texture = null;
        [SerializeField] VideoClip _video = null;
        [SerializeField] VideoPlayer _videoPlayer = null;
        [SerializeField] string _webcamName = "";
        [SerializeField] Vector2Int _webcamResolution = new(1920, 1080);
        [SerializeField] int _webcamFrameRate = 30;
        [SerializeField] Camera _camera = null;
        [SerializeField] RenderTexture _outputTexture = null;
        [SerializeField] Vector2Int _outputResolution = new(1920, 1080);

        [SerializeField, HideInInspector] Shader _shader = null;


        WebCamTexture _webcam;
        Material _material;
        RenderTexture _buffer;


        // Methods

        void Start()
        {
            if (_outputTexture == null)
            {
                _buffer = new RenderTexture(_outputResolution.x, _outputResolution.y, 0);
            }

            switch (_sourceType)
            {
                case SourceType.Texture:
                    {
                        if (_texture != null) Blit(_texture);

                        break;
                    }
                case SourceType.Video:
                    {
                        if (_videoPlayer == null)
                        {
                            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
                        }

                        _videoPlayer.source = VideoSource.VideoClip;
                        _videoPlayer.clip = _video;
                        _videoPlayer.isLooping = true;
                        _videoPlayer.renderMode = VideoRenderMode.APIOnly;
                        _videoPlayer.Play();

                        break;
                    }
                case SourceType.Webcam:
                    {
                        _webcam = new WebCamTexture(
                            _webcamName,
                            _webcamResolution.x,
                            _webcamResolution.y,
                            _webcamFrameRate);

                        _webcam.Play();

                        break;
                    }
                case SourceType.Card:
                    {
                        var dims = new Vector2(OutputBuffer.width, OutputBuffer.height);

                        _material = new Material(_shader);
                        _material.SetVector("_Resolution", dims);

                        Graphics.Blit(null, OutputBuffer, _material, 0);

                        break;
                    }
                case SourceType.Gradient:
                    {
                        _material = new Material(_shader);

                        break;
                    }
                case SourceType.Camera:
                    {
                        break;
                    }
            }
        }

        void Update()
        {
            switch (_sourceType)
            {
                case SourceType.Video:
                    {
                        Blit(_videoPlayer.texture);

                        break;
                    }
                case SourceType.Webcam:
                    {
                        if (_webcam.didUpdateThisFrame)
                        {
                            Blit(_webcam, _webcam.videoVerticallyMirrored);
                        }

                        break;
                    }
                case SourceType.Gradient:
                    {
                        Graphics.Blit(null, OutputBuffer, _material, 1);

                        break;
                    }
                case SourceType.Camera:
                    {
                        _camera.targetTexture = OutputBuffer;

                        if (!_camera.enabled) _camera.Render();

                        break;
                    }
            }
        }

        void OnDestroy()
        {
            if (_webcam != null) Destroy(_webcam);
            if (_buffer != null) Destroy(_buffer);
            if (_material != null) Destroy(_material);
        }


        void Blit(Texture source, bool vflip = false)
        {
            if (source == null) return;

            var aspect1 = (float)source.width / source.height;
            var aspect2 = (float)OutputBuffer.width / OutputBuffer.height;

            var scale = new Vector2(aspect2 / aspect1, aspect1 / aspect2);
            scale = Vector2.Min(Vector2.one, scale);
            if (vflip) scale.y *= -1;

            var offset = (Vector2.one - scale) / 2;

            Graphics.Blit(source, OutputBuffer, scale, offset);
        }
    }
}