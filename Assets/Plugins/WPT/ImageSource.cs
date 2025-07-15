using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using WPT.Utilities;

namespace WPT
{
    public sealed class ImageSource : MonoBehaviour
    {
        // Properties

        public RenderTexture Texture => _buffer;

        public Renderer Renderer => _renderer;

        public Vector2 Resolution => _resolution;


        // Fields

        [SerializeField] private SourceType _sourceType = SourceType.Texture;
        [SerializeField] private Texture2D _texture;
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private string _webcamName = "";
        [SerializeField] private int _webcamFrameRate = 30;
        [SerializeField] private Vector2Int _resolution = new(1920, 1080);
        [SerializeField] private RenderMode _renderMode = RenderMode.None;
        [SerializeField] private RenderTexture _renderTexture;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private string _propertyName;
        [SerializeField] private RawImage _rawImage;

        private WebCamTexture _webcam;
        private RenderTexture _buffer;


        // Methods

        private void Awake()
        {
            _buffer = new RenderTexture(_resolution.x, _resolution.y, 0);

            switch (_sourceType)
            {
                case SourceType.Texture:
                    {
                        if (_texture)
                        {
                            ImageUtils.TextureBlit(_texture, _buffer);
                        }

                        break;
                    }
                case SourceType.Webcam:
                    {
                        _webcam = new WebCamTexture(
                            _webcamName,
                            _resolution.x,
                            _resolution.y,
                            _webcamFrameRate);

                        _webcam.Play();

                        break;
                    }
            }
        }

        private void Update()
        {
            switch (_sourceType)
            {
                case SourceType.Video:
                    {
                        if (_videoPlayer && _videoPlayer.texture)
                        {
                            ImageUtils.TextureBlit(_videoPlayer.texture, _buffer);
                        }

                        break;
                    }
                case SourceType.Webcam:
                    {
                        if (_webcam && _webcam.didUpdateThisFrame)
                        {
                            ImageUtils.TextureBlit(_webcam, _buffer);
                        }

                        break;
                    }
            }

            switch (_renderMode)
            {
                case RenderMode.RenderTexture:
                    {
                        if (_renderTexture)
                        {
                            _renderTexture = _buffer;
                        }

                        break;
                    }
                case RenderMode.Renderer:
                    {
                        if (_renderer && _renderer.material)
                        {
                            _renderer.material.SetTexture(_propertyName, _buffer);

                            var aspect = (float)_resolution.x / _resolution.y;

                            if (_renderer.transform.localScale.x / _renderer.transform.localScale.y != aspect)
                            {
                                _renderer.transform.localScale = new Vector3(
                                _renderer.transform.localScale.x * aspect,
                                _renderer.transform.localScale.y,
                                _renderer.transform.localScale.z);
                            }
                        }

                        break;
                    }
                case RenderMode.RawImage:
                    {
                        if (_rawImage)
                        {
                            _rawImage.texture = _buffer;
                        }
                        break;
                    }
            }
        }

        private void OnDestroy()
        {
            if (_webcam != null)
            {
                Destroy(_webcam);

                _webcam = null;
            }

            if (_buffer != null)
            {
                Destroy(_buffer);

                _buffer = null;
            }
        }
    }

    public enum SourceType
    {
        Texture,
        Video,
        Webcam,
    }

    public enum RenderMode
    {
        None,
        RenderTexture,
        Renderer,
        RawImage,
    }
}