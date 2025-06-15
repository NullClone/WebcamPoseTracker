using UnityEngine;
using UnityEngine.Video;
using WPT.Utilities;

namespace WPT
{
    public sealed class ImageSource : MonoBehaviour
    {
        // Properties

        public RenderTexture Texture => _buffer;

        public Vector2Int Resolution => _resolution;


        // Fields     

        [SerializeField] private ImageSourceType _sourceType = ImageSourceType.Texture;
        [SerializeField] private Texture2D _texture;
        [SerializeField] private VideoClip _video;
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private string _webcamName = "";
        [SerializeField] private Vector2Int _webcamResolution = new(1920, 1080);
        [SerializeField] private int _webcamFrameRate = 30;
        [SerializeField] private bool _playOnAwake = true;
        [SerializeField] private Vector2Int _resolution = new(1920, 1080);

        private WebCamTexture _webcam;
        private RenderTexture _buffer;


        // Methods

        private void Awake()
        {
            if (_playOnAwake)
            {
                Initialize();
            }
        }

        private void Start()
        {
            if (!_playOnAwake)
            {
                Initialize();
            }
        }

        private void Update()
        {
            switch (_sourceType)
            {
                case ImageSourceType.Video:
                    {
                        if (_videoPlayer && _videoPlayer.texture)
                        {
                            ImageUtils.TextureBlit(_videoPlayer.texture, _buffer);
                        }

                        break;
                    }
                case ImageSourceType.Webcam:
                    {
                        if (_webcam && _webcam.didUpdateThisFrame)
                        {
                            ImageUtils.TextureBlit(_webcam, _buffer);
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

        private void Initialize()
        {
            _buffer = new RenderTexture(_resolution.x, _resolution.y, 0);

            switch (_sourceType)
            {
                case ImageSourceType.Texture:
                    {
                        if (_texture == null) break;

                        ImageUtils.TextureBlit(_texture, _buffer);

                        break;
                    }
                case ImageSourceType.Video:
                    {
                        if (_videoPlayer == null) break;

                        if (_video)
                        {
                            _videoPlayer.clip = _video;
                        }

                        _videoPlayer.renderMode = VideoRenderMode.APIOnly;
                        _videoPlayer.Play();

                        break;
                    }
                case ImageSourceType.Webcam:
                    {
                        _webcam = new WebCamTexture(
                            _webcamName,
                            _webcamResolution.x,
                            _webcamResolution.y,
                            _webcamFrameRate);

                        _webcam.Play();

                        break;
                    }
            }
        }
    }
}