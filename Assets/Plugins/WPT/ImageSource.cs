using UnityEngine;
using UnityEngine.Video;

namespace WPT
{
    public sealed class ImageSource : MonoBehaviour
    {
        // Properties

        public Texture Texture => OutputBuffer;

        public Vector2Int OutputResolution => _outputResolution;


        private RenderTexture OutputBuffer => _outputTexture != null ? _outputTexture : _buffer;


        // Fields     

        [SerializeField] private ImageSourceType _sourceType = ImageSourceType.Texture;
        [SerializeField] private Texture2D _texture;
        [SerializeField] private VideoClip _video;
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private string _webcamName = "";
        [SerializeField] private Vector2Int _webcamResolution = new(1920, 1080);
        [SerializeField] private int _webcamFrameRate = 30;
        [SerializeField] private Camera _camera;
        [SerializeField] private RenderTexture _outputTexture;
        [SerializeField] private Vector2Int _outputResolution = new(1920, 1080);

        private WebCamTexture _webcam;
        private RenderTexture _buffer;


        // Methods

        private void Start()
        {
            if (_outputTexture == null)
            {
                _buffer = new RenderTexture(_outputResolution.x, _outputResolution.y, 0);
            }

            switch (_sourceType)
            {
                case ImageSourceType.Video:
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
                        Blit(_texture);

                        break;
                    }
                case ImageSourceType.Video:
                    {
                        Blit(_videoPlayer.texture);

                        break;
                    }
                case ImageSourceType.Webcam:
                    {
                        if (_webcam.didUpdateThisFrame)
                        {
                            Blit(_webcam, _webcam.videoVerticallyMirrored);
                        }

                        break;
                    }
                case ImageSourceType.Camera:
                    {
                        if (_camera.enabled) break;

                        _camera.targetTexture = OutputBuffer;

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


        private void Blit(Texture source, bool vflip = false)
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