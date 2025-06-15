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
        }

        void Update()
        {
            if (_image == null || _source == null) return;

            _image.texture = _source.Texture;
        }
    }
}