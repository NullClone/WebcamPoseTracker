using UnityEngine;
using UnityEngine.Video;
using WPT.Utilities;

namespace WPT
{
    public sealed class ImageSource : MonoBehaviour
    {
        // Properties

        public Texture Texture => _buffer;

        public RenderTexture RenderTexture => _buffer;

        public Vector2Int Resolution => _resolution;


        // Fields     

        [SerializeField] private ImageSourceType _sourceType = ImageSourceType.Texture;
        [SerializeField] private Texture2D _texture;
        [SerializeField] private VideoClip _video;
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private string _webcamName = "";
        [SerializeField] private Vector2Int _webcamResolution = new(1920, 1080);
        [SerializeField] private int _webcamFrameRate = 30;
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector2Int _resolution = new(1920, 1080);

        private WebCamTexture _webcam;
        private RenderTexture _buffer;


        // Methods

        private void Start()
        {
            _buffer = new RenderTexture(_resolution.x, _resolution.y, 0);

            switch (_sourceType)
            {
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
                        Application.RequestUserAuthorization(UserAuthorization.WebCam);

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

        private void Update()
        {
            switch (_sourceType)
            {
                case ImageSourceType.Texture:
                    {
                        if (_texture)
                        {
                            ImageUtils.TextureBlit(_texture, _buffer);
                        }

                        break;
                    }
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
                        if (_webcam.didUpdateThisFrame)
                        {
                            ImageUtils.TextureBlit(_webcam, _buffer, _webcam.videoVerticallyMirrored);
                        }

                        break;
                    }
                case ImageSourceType.Camera:
                    {
                        if (_camera == null || _camera.enabled) break;

                        _camera.targetTexture = _buffer;
                        _camera.Render();

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
}