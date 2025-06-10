using UnityEngine;
using UnityEngine.UI;

namespace WPT
{
    public sealed class ImageSourceDebugger : MonoBehaviour
    {
        // Fields

        [SerializeField] ImageSource _source;
        [SerializeField] RawImage _image;

        bool isReady = false;


        // Methods

        void Start()
        {
            if (_image == null) return;

            if (_source == null)
            {
                _source = GetComponent<ImageSource>();
            }

            if (_source == null) return;

            isReady = true;
        }

        void Update()
        {
            if (isReady)
            {
                _image.rectTransform.sizeDelta = _source.OutputResolution;
                _image.texture = _source.Texture;
            }
        }
    }
}