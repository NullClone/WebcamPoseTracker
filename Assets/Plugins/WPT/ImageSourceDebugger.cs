using UnityEngine;

namespace WPT
{
    public sealed class ImageSourceDebugger : MonoBehaviour
    {
        // Fields

        [SerializeField] private Renderer _renderer;

        private ImageSource _source;


        // Methods

        void Awake()
        {
            _source = gameObject.GetComponent<ImageSource>();

            if (_renderer == null || _source == null) return;

            _renderer.material.color = Color.white;

            var scale = _renderer.gameObject.transform.localScale;
            scale.x *= (float)_source.Resolution.x / _source.Resolution.y;

            _renderer.gameObject.transform.localScale = scale;
            _renderer.gameObject.SetActive(true);
        }

        void Update()
        {
            if (_renderer == null || _source == null) return;

            _renderer.material.mainTexture = _source.Texture;
        }
    }
}