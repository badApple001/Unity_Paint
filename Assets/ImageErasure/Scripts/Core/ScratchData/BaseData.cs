using UnityEngine;

namespace ScratchCardAsset.Core.ScratchData
{
    public abstract class BaseData
    {
	    public Camera Camera { protected get; set; }
	    public abstract Vector2 TextureSize { get; }
	    protected abstract Rect? SpriteRect { get; }
	    protected abstract Vector2 Bounds { get; }
	    protected virtual bool IsOrthographic => Camera.orthographic;
	    private Transform Surface { get; set; }
	    private Triangle triangle;

        protected BaseData(Transform surface, Camera camera)
        {
            Surface = surface;
            Camera = camera;
        }
        
        protected void InitTriangle()
        {
	        var bounds = Bounds;
	        //bottom left
	        var position0 = new Vector3(-bounds.x / 2f, -bounds.y / 2f, 0);
	        var uv0 = Vector2.zero;
	        //upper left
	        var position1 = new Vector3(-bounds.x / 2f, bounds.y / 2f, 0);
	        var uv1 = Vector2.up;
	        //upper right
	        var position2 = new Vector3(bounds.x / 2f, bounds.y / 2f, 0);
	        var uv2 = Vector2.one;
	        triangle = new Triangle(position0, position1, position2, uv0, uv1, uv2);
        }

        protected virtual Vector3 GetClickPosition(Vector2 position)
        {
	       return Camera.ScreenToWorldPoint(position);
        }

        public Vector2 GetScratchPosition(Vector2 position)
        {
	        var scratchPosition = Vector2.zero;
	        if (IsOrthographic)
	        {
		        var bounds = Bounds;
		        var clickPosition = GetClickPosition(position);
		        var lossyScale = Surface.lossyScale;
		        var pointLocal = Vector2.Scale(Surface.InverseTransformPoint(clickPosition), lossyScale) + bounds / 2f;
		        if (SpriteRect != null)
		        {
			        var uv = pointLocal / bounds;
			        scratchPosition = new Vector2(SpriteRect.Value.width * uv.x, SpriteRect.Value.height * uv.y);
		        }
		        else
		        {
			        var textureSize = TextureSize;
			        var pixelsPerInch = new Vector2(textureSize.x / bounds.x / lossyScale.x, textureSize.y / bounds.y / lossyScale.y);
			        scratchPosition = Vector2.Scale(Vector2.Scale(pointLocal, lossyScale), pixelsPerInch);
		        }
	        }
	        else
	        {
		        var plane = new Plane(Surface.forward, Surface.position);
		        var ray = Camera.ScreenPointToRay(position);
		        if (plane.Raycast(ray, out var enter))
		        {
			        var point = ray.GetPoint(enter);
			        var pointLocal = Surface.InverseTransformPoint(point);
			        var uv = triangle.GetUV(pointLocal);
			        scratchPosition = Vector2.Scale(TextureSize, uv);
		        }
	        }
	        return scratchPosition;
        }

        public Vector2 GetLocalPosition(Vector2 texturePosition)
        {
	        var textureSize = TextureSize;
	        var bounds = Bounds;
	        if (IsOrthographic)
	        {
		        return (texturePosition - textureSize / 2f) / textureSize * bounds / Surface.lossyScale;
	        }
	        return (texturePosition - textureSize / 2f) / textureSize * bounds;
        }
    }
}