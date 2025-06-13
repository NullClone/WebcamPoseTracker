using UnityEngine;
using UnityEngine.UI;

namespace WPT
{
    public sealed class ImageSourceDebugger : MonoBehaviour
    {
        // Fields

        [SerializeField] private ImageSource _source;
        [SerializeField] private RawImage _image;

        private bool isReady = false;


        // Methods

        void Start()
        {
            if (_image == null) return;

            if (_source == null)
            {
                _source = gameObject.GetComponent<ImageSource>();
            }

            if (_source == null) return;

            isReady = true;
        }

        void Update()
        {
            if (isReady)
            {
                _image.texture = _source.Texture;
            }
        }
    }
}