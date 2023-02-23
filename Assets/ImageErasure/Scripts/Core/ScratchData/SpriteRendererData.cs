using UnityEngine;

namespace ScratchCardAsset.Core.ScratchData
{
    public class SpriteRendererData : BaseData
    {
        private readonly SpriteRenderer renderer;

        public override Vector2 TextureSize => renderer.sprite.rect.size;
        protected override Rect? SpriteRect => renderer.sprite.rect;
        protected override Vector2 Bounds => renderer != null && !Camera.orthographic ? renderer.sprite.bounds.size : renderer.bounds.size;
        
        public SpriteRendererData(Transform surface, Camera camera) : base(surface, camera)
        {
            if (surface.TryGetComponent(out renderer))
            {
                InitTriangle();
            }
        }
    }
}