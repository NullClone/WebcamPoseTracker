using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class ShaderAssetLoader : MonoBehaviour
    {
        [SerializeField] private ComputeShader shader;

        private void Awake()
        {
            ImageUtils.shader = shader;
            ImageUtils.s_ImageSample = shader.FindKernel("ImageSample");
        }

        private void OnDestroy()
        {
            ImageUtils.shader = null;
            ImageUtils.s_ImageSample = 0;
        }
    }
}