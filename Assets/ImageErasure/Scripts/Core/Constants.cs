using UnityEngine;

namespace ScratchCardAsset.Core
{
    public static class Constants
    {
        public static class General
        {
            public const float PixelsPerUnit = 100f;
        }
        
        public static class BrushShader
        {
            public const string BlendOpShaderParam = "_BlendOpValue";

            public const string SrcColorBlend = "_SrcColorBlend";
            public const string DstColorBlend = "_DstColorBlend";
            public const string SrcAlphaBlend = "_SrcAlphaBlend";
            public const string DstAlphaBlend = "_DstAlphaBlend";
        }

        public static class ProgressShader
        {
            private const string SourceTextureParam = "_SourceTex";
            public static readonly int SourceTexture = Shader.PropertyToID(SourceTextureParam);
        }

        public static class MaskShader
        {
            private const string MaskTextureParam = "_MaskTex";
            private const string OffsetParam = "_Offset";
            public static readonly int MaskTexture = Shader.PropertyToID(MaskTextureParam);
            public static readonly int Offset = Shader.PropertyToID(OffsetParam);
        }
    }
}