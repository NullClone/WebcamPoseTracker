using Unity.InferenceEngine;
using Unity.Mathematics;
using UnityEngine;

namespace WPT.Utilities
{
    public static class ImageUtils
    {
        // Fields

        private static readonly ComputeShader _shader = (ComputeShader)Resources.Load("ImageTransform");

        private static readonly int ImageSample = _shader.FindKernel("ImageSample");
        private static readonly int Optr = Shader.PropertyToID("Optr");
        private static readonly int X_tex2D = Shader.PropertyToID("X_tex2D");
        private static readonly int O_height = Shader.PropertyToID("O_height");
        private static readonly int O_width = Shader.PropertyToID("O_width");
        private static readonly int O_channels = Shader.PropertyToID("O_channels");
        private static readonly int X_height = Shader.PropertyToID("X_height");
        private static readonly int X_width = Shader.PropertyToID("X_width");
        private static readonly int affineMatrix = Shader.PropertyToID("affineMatrix");


        // Methods

        public static void SampleImageAffine(Texture srcTexture, Tensor<float> dstTensor, float2x3 M)
        {
            if (_shader == null) return;

            var tensorData = ComputeTensorData.Pin(dstTensor, false);

            _shader.SetTexture(ImageSample, X_tex2D, srcTexture);
            _shader.SetBuffer(ImageSample, Optr, tensorData.buffer);

            _shader.SetInt(O_height, dstTensor.shape[1]);
            _shader.SetInt(O_width, dstTensor.shape[2]);
            _shader.SetInt(O_channels, dstTensor.shape[3]);
            _shader.SetInt(X_height, srcTexture.height);
            _shader.SetInt(X_width, srcTexture.width);

            var matrix = new Matrix4x4(
                new Vector4(M[0][0], M[0][1]),
                new Vector4(M[1][0], M[1][1]),
                new Vector4(M[2][0], M[2][1]),
                Vector4.zero);

            _shader.SetMatrix(affineMatrix, matrix);

            _shader.Dispatch(ImageSample,
                IDivC(dstTensor.shape[1], 8),
                IDivC(dstTensor.shape[1], 8), 1);
        }

        public static void TextureBlit(Texture srcTexture, RenderTexture dstTexture, bool vflip = false)
        {
            if (srcTexture == null) return;

            var aspect1 = (float)srcTexture.width / srcTexture.height;
            var aspect2 = (float)dstTexture.width / dstTexture.height;

            var scale = Vector2.Min(Vector2.one, new Vector2(aspect2 / aspect1, aspect1 / aspect2));
            if (vflip) scale.y *= -1;

            var offset = (Vector2.one - scale) / 2;

            Graphics.Blit(srcTexture, dstTexture, scale, offset);
        }


        private static int IDivC(int v, int div)
        {
            return (v + div - 1) / div;
        }
    }
}