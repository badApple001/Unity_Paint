using System.Collections.Generic;
using ScratchCardAsset.Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScratchCardAsset.Core
{
	/// <summary>
	/// Draws holes and lines into RenderTexture
	/// </summary>
	public class ScratchCardRenderer
	{
		public bool IsScratched;

		private readonly ScratchCard scratchCard;
		private Mesh meshHole;
		private Mesh meshLine;
		private CommandBuffer commandBuffer;
		private Bounds localBounds;
		
		private List<Vector3> positions = new List<Vector3>();
		private List<Color> colors = new List<Color>();
		private List<int> indices = new List<int>();
		private List<Vector2> uv = new List<Vector2>();

		public ScratchCardRenderer(ScratchCard card)
		{
			scratchCard = card;
			localBounds = new Bounds(Vector2.one / 2f, Vector2.one);
			commandBuffer = new CommandBuffer {name = "ScratchCardRenderer"};
			meshHole = MeshGenerator.GenerateQuad(Vector3.zero, Vector2.zero);
		}

		public void Release()
		{
			if (commandBuffer != null)
			{
				commandBuffer.Release();
				commandBuffer = null;
			}
			
			if (meshHole != null)
			{
				Object.Destroy(meshHole);
				meshHole = null;
			}
			
			if (meshLine != null)
			{
				Object.Destroy(meshLine);
				meshLine = null;
			}
		}

		private bool IsInBounds(Rect rect)
		{
			return localBounds.Intersects(new Bounds(rect.center, rect.size));
		}

		/// <summary>
		/// Draws quad into RenderTexture
		/// </summary>
		public void ScratchHole(Vector2 position, float pressure = 1f)
		{
			var positionRect = new Rect(
				(position.x - 0.5f * scratchCard.BrushMaterial.mainTexture.width * scratchCard.BrushSize * pressure) / scratchCard.ScratchData.TextureSize.x,
				(position.y - 0.5f * scratchCard.BrushMaterial.mainTexture.height * scratchCard.BrushSize * pressure) / scratchCard.ScratchData.TextureSize.y,
				scratchCard.BrushMaterial.mainTexture.width * scratchCard.BrushSize * pressure / scratchCard.ScratchData.TextureSize.x,
				scratchCard.BrushMaterial.mainTexture.height * scratchCard.BrushSize * pressure / scratchCard.ScratchData.TextureSize.y);

			if (IsInBounds(positionRect))
			{
				meshHole.vertices = new[]
				{
					new Vector3(positionRect.xMin, positionRect.yMax, 0),
					new Vector3(positionRect.xMax, positionRect.yMax, 0),
					new Vector3(positionRect.xMax, positionRect.yMin, 0),
					new Vector3(positionRect.xMin, positionRect.yMin, 0)
				};

				GL.LoadOrtho();
				commandBuffer.Clear();
				commandBuffer.SetRenderTarget(scratchCard.RenderTarget);
				commandBuffer.DrawMesh(meshHole, Matrix4x4.identity, scratchCard.BrushMaterial);
				Graphics.ExecuteCommandBuffer(commandBuffer);
				IsScratched = true;
			}
		}

		/// <summary>
		/// Draws many quads (line) into RenderTexture
		/// </summary>
		public void ScratchLine(Vector2 startPosition, Vector2 endPosition, float startPressure = 1f, float endPressure = 1f)
		{
			positions.Clear();
			colors.Clear();
			indices.Clear();
			uv.Clear();
			var holesCount = (int)Vector2.Distance(startPosition, endPosition) / (int)scratchCard.RenderTextureQuality;
			holesCount = Mathf.Max(1, holesCount);
			var count = 0;
			for (var i = 0; i < holesCount; i++)
			{
				var t = i / (float)Mathf.Clamp(holesCount - 1, 1, holesCount - 1);
				var pressure = Mathf.Lerp(startPressure, endPressure, t);
				var holePosition = startPosition + (endPosition - startPosition) / holesCount * i;
				
				var positionRect = new Rect(
					(holePosition.x - 0.5f * scratchCard.BrushMaterial.mainTexture.width * scratchCard.BrushSize * pressure) / scratchCard.ScratchData.TextureSize.x,
					(holePosition.y - 0.5f * scratchCard.BrushMaterial.mainTexture.height * scratchCard.BrushSize * pressure) / scratchCard.ScratchData.TextureSize.y,
					scratchCard.BrushMaterial.mainTexture.width * scratchCard.BrushSize * pressure / scratchCard.ScratchData.TextureSize.x,
					scratchCard.BrushMaterial.mainTexture.height * scratchCard.BrushSize * pressure / scratchCard.ScratchData.TextureSize.y);

				if (IsInBounds(positionRect))
				{
					positions.Add(new Vector3(positionRect.xMin, positionRect.yMax, 0));
					positions.Add(new Vector3(positionRect.xMax, positionRect.yMax, 0));
					positions.Add(new Vector3(positionRect.xMax, positionRect.yMin, 0));
					positions.Add(new Vector3(positionRect.xMin, positionRect.yMin, 0));

					colors.Add(Color.white);
					colors.Add(Color.white);
					colors.Add(Color.white);
					colors.Add(Color.white);

					uv.Add(Vector2.up);
					uv.Add(Vector2.one);
					uv.Add(Vector2.right);
					uv.Add(Vector2.zero);

					indices.Add(0 + count * 4);
					indices.Add(1 + count * 4);
					indices.Add(2 + count * 4);
					indices.Add(2 + count * 4);
					indices.Add(3 + count * 4);
					indices.Add(0 + count * 4);

					count++;
				}
			}

			if (positions.Count > 0)
			{
				if (meshLine != null)
				{
					meshLine.Clear(false);
				}
				else
				{
					meshLine = new Mesh();
				}
				meshLine.vertices = positions.ToArray();
				meshLine.uv = uv.ToArray();
				meshLine.triangles = indices.ToArray();
				meshLine.colors = colors.ToArray();
				GL.LoadOrtho();
				commandBuffer.Clear();
				commandBuffer.SetRenderTarget(scratchCard.RenderTarget);
				commandBuffer.DrawMesh(meshLine, Matrix4x4.identity, scratchCard.BrushMaterial);
				Graphics.ExecuteCommandBuffer(commandBuffer);
				IsScratched = true;
			}
		}

		public void FillRenderTextureWithColor(Color color)
		{
			commandBuffer.SetRenderTarget(scratchCard.RenderTarget);
			commandBuffer.ClearRenderTarget(false, true, color);
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}
	}
}