using UnityEngine;
using UnityEngine.UI;

namespace ScratchCardAsset.Core.ScratchData
{
    public class ImageData : BaseData
    {
        private readonly Image image;
        private readonly bool isCanvasOverlay;

        public override Vector2 TextureSize => image.sprite.rect.size;
        protected override Rect? SpriteRect => image.sprite.rect;
        protected override Vector2 Bounds => Camera.orthographic || isCanvasOverlay 
            ? Vector2.Scale(image.rectTransform.rect.size, image.rectTransform.lossyScale) 
            : image.rectTransform.rect.size;
        protected override bool IsOrthographic => Camera.orthographic || isCanvasOverlay;
        
        public ImageData(Transform surface, Camera camera) : base(surface, camera)
        {
            if (surface.TryGetComponent(out image))
            {
                isCanvasOverlay = image.canvas.renderMode == RenderMode.ScreenSpaceOverlay;
                InitTriangle();
            }
        }
        
        protected override Vector3 GetClickPosition(Vector2 position)
        {
            return isCanvasOverlay ? (Vector3)position : Camera.ScreenToWorldPoint(position);
        }
    }
}