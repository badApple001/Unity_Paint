using UnityEngine;

namespace ScratchCardAsset.Tools
{
	public static class MeshGenerator
	{
		public static Mesh GenerateQuad(Vector3 size, Vector3 offset)
		{
			var mesh = new Mesh
			{
				vertices = new[]
				{
					new Vector3(0f, 1f * size.y, 0f) - offset,
					new Vector3(1f * size.x, 1f * size.y, 0) - offset,
					new Vector3(1f * size.x, 0f, 0f) - offset,
					new Vector3(0f, 0f, 0f) - offset,
				},
				uv = new[]
				{
					new Vector2(0f, 1f),
					new Vector2(1f, 1f),
					new Vector2(1f, 0f),
					new Vector2(0f, 0f),
				},
				triangles = new[]
				{
					0, 1, 2,
					2, 3, 0
				},
				colors = new[]
				{
					Color.white,
					Color.white,
					Color.white,
					Color.white
				}
			};
			return mesh;
		}
	}
}