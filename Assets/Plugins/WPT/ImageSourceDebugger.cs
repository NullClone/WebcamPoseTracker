using UnityEngine;
using UnityEngine.UI;

namespace WPT
{
    public sealed class ImageSourceDebugger : MonoBehaviour
    {
        // Fields

        [SerializeField] private RawImage _image;

        private ImageSource _source;


        // Methods

        void Awake()
        {
            _source = gameObject.GetComponent<ImageSource>();

            if (_image == null || _source == null) return;

            var scale = _image.rectTransform.localScale;
            scale.x *= _source.Resolution.x / _source.Resolution.y;

            _image.rectTransform.localScale = scale;
        }

        void Update()
        {
            if (_image == null || _source == null) return;

            _image.texture = _source.Texture;
        }
    }
}