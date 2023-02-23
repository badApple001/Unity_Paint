using UnityEngine;

namespace ScratchCardAsset.Core.ScratchData
{
    public class MeshRendererData : BaseData
    {
        private readonly MeshRenderer renderer;
        private readonly MeshFilter filter;

        public override Vector2 TextureSize { get; }
        protected override Rect? SpriteRect => null;
        protected override Vector2 Bounds => filter != null && !Camera.orthographic ? filter.sharedMesh.bounds.size : renderer.bounds.size;

        public MeshRendererData(Transform surface, Camera camera) : base(surface, camera)
        {
            if (surface.TryGetComponent(out renderer) && surface.TryGetComponent(out filter))
            {
                InitTriangle();
                var sharedMaterial = renderer.sharedMaterial;
                var offset = sharedMaterial.GetVector(Constants.MaskShader.Offset);
                TextureSize = new Vector2(sharedMaterial.mainTexture.width * offset.z, sharedMaterial.mainTexture.height * offset.w);
            }
        }
    }
}