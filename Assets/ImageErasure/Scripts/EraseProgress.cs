using System;
using System.Collections;
using ScratchCardAsset.Core;
using ScratchCardAsset.Tools;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace ScratchCardAsset
{
	/// <summary>
	/// Calculates scratching progress in range from 0 to 1, where 0 - card scratched completely, 1 - scratch surface is whole
	/// </summary>
	public class EraseProgress : MonoBehaviour
	{
		#region Events

		public event Action<float> OnProgress;
		public event Action<float> OnCompleted;

		#endregion

		#region Variables

		[SerializeField, FormerlySerializedAs("Card")] private ScratchCard card;
		public ScratchCard Card
		{
			get => card;
			set => card = value;
		}

		[SerializeField, FormerlySerializedAs("ProgressMaterial")] private Material progressMaterial;
		public Material ProgressMaterial
		{
			get => progressMaterial;
			set => progressMaterial = value;
		}
		
		[SerializeField, FormerlySerializedAs("SampleSourceTexture")] private bool sampleSourceTexture;
		public bool SampleSourceTexture
		{
			get => sampleSourceTexture;
			set => sampleSourceTexture = value;
		}

		[SerializeField] private ProgressAccuracy progressAccuracy;
		public ProgressAccuracy ProgressAccuracy
		{
			get => progressAccuracy;
			set
			{
				progressAccuracy = value;
				UpdateAccuracy();
				if (progressAccuracy == ProgressAccuracy.Default)
				{
					updateProgress = false;
					if (pixelsBuffer.IsCreated)
					{
						if (isCalculating)
						{
							AsyncGPUReadback.WaitAllRequests();
						}
						pixelsBuffer.Dispose();
					}
				}
			}
		}

		private ScratchMode scratchMode;
		private NativeArray<byte> pixelsBuffer;
		private int asyncGPUReadbackFrame;
		private int updateProgressFrame;
		private Color[] sourceSpritePixels;
		private CommandBuffer commandBuffer;
		private Mesh mesh;
		private RenderTexture percentRenderTexture;
		private RenderTargetIdentifier percentTargetIdentifier;
		private Rect percentTextureRect;
		private Texture2D progressTexture;
		private float progress;
		private int bitsPerPixel = 1;
		private bool updateProgress;
		private bool isCalculating;
		private bool isCompleted;

		#endregion

		#region MonoBehaviour Methods

		private void Start()
		{
			Init();
		}

		private void OnDestroy()
		{
			if (progressAccuracy == ProgressAccuracy.High && isCalculating)
			{
				AsyncGPUReadback.WaitAllRequests();
			}
			
			if (pixelsBuffer.IsCreated)
			{
				pixelsBuffer.Dispose();
			}

			if (percentRenderTexture != null && percentRenderTexture.IsCreated())
			{
				percentRenderTexture.Release();
				Destroy(percentRenderTexture);
				percentRenderTexture = null;
			}
			
			if (progressTexture != null)
			{
				Destroy(progressTexture);
				progressTexture = null;
			}

			if (mesh != null)
			{
				Destroy(mesh);
				mesh = null;
			}

			if (commandBuffer != null)
			{
				commandBuffer.Release();
				commandBuffer = null;
			}

			if (card != null)
			{
				card.OnRenderTextureInitialized -= OnCardRenderTextureInitialized;
			}
		}

		private void LateUpdate()
		{
			if (card.Mode != scratchMode)
			{
				scratchMode = card.Mode;
				ResetProgress();
			}
			
			if ((card.IsScratched || updateProgress) && !isCompleted)
			{
				UpdateProgress();
			}
		}

		#endregion

		#region Private Methods

		private void Init()
		{
			if (card == null)
			{
				Debug.LogError("Card field is not assigned!");
				enabled = false;
				return;
			}
			
			if (card.Initialized)
			{
				OnCardRenderTextureInitialized(card.RenderTexture);
			}
			
			card.OnRenderTextureInitialized += OnCardRenderTextureInitialized;
			UpdateAccuracy();
			scratchMode = card.Mode;
			commandBuffer = new CommandBuffer {name = "EraseProgress"};
			mesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
			var renderTextureFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? 
				RenderTextureFormat.R8 : RenderTextureFormat.ARGB32;
			percentRenderTexture = new RenderTexture(1, 1, 0, renderTextureFormat);
			percentTargetIdentifier = new RenderTargetIdentifier(percentRenderTexture);
			percentTextureRect = new Rect(0, 0, percentRenderTexture.width, percentRenderTexture.height);
			var textureFormat = SystemInfo.SupportsTextureFormat(TextureFormat.R8) ? TextureFormat.R8 : TextureFormat.ARGB32;
			progressTexture = new Texture2D(percentRenderTexture.width, percentRenderTexture.height, textureFormat, false, true);
		}
		
		private void OnCardRenderTextureInitialized(RenderTexture renderTexture)
		{
			bitsPerPixel = renderTexture.format == RenderTextureFormat.R8 ? 1 : 4;
		}

		private void UpdateAccuracy()
		{
			if (progressAccuracy == ProgressAccuracy.High && !SystemInfo.supportsAsyncGPUReadback)
			{
				Debug.LogWarning("AsyncGPUReadback is not supported! Switching to ProgressAccuracy.Default.");
				progressAccuracy = ProgressAccuracy.Default;
			}
		}

		/// <summary>
		/// Calculates scratch progress
		/// </summary>
		private IEnumerator CalcProgress()
		{
			if (!isCompleted && !isCalculating)
			{
				isCalculating = true;
				if (progressAccuracy == ProgressAccuracy.High)
				{
					if (!pixelsBuffer.IsCreated)
					{
						var length = card.RenderTexture.width * card.RenderTexture.height * bitsPerPixel;
						pixelsBuffer = new NativeArray<byte>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					}

					asyncGPUReadbackFrame = Time.frameCount;
					var request = AsyncGPUReadback.RequestIntoNativeArray(ref pixelsBuffer, card.RenderTexture);
					yield return new WaitUntil(() => request.done);
					if (request.hasError)
					{
						isCalculating = false;
						updateProgress = false;
						Debug.LogError("GPU readback error detected.");
						yield break;
					}
					
					progress = 0f;
					if (sampleSourceTexture)
					{
						var totalAlpha = 0f;
						for (var i = 0; i < pixelsBuffer.Length; i += bitsPerPixel)
						{
							var sourceAlpha = sourceSpritePixels[i / bitsPerPixel].a;
							totalAlpha += sourceAlpha;
							progress += pixelsBuffer[i] / 255f * sourceAlpha;
						}
						
						var div = pixelsBuffer.Length / (float)bitsPerPixel;
						totalAlpha /= div;
						progress /= div;
						progress /= totalAlpha;
					}
					else
					{
						for (var i = 0; i < pixelsBuffer.Length; i += bitsPerPixel)
						{
							progress += pixelsBuffer[i] / 255f;
						}
						
						progress /= pixelsBuffer.Length / (float)bitsPerPixel;
					}

					if (asyncGPUReadbackFrame > updateProgressFrame)
					{
						updateProgress = false;
					}
				}
				else if (progressAccuracy == ProgressAccuracy.Default)
				{
					var prevRenderTexture = RenderTexture.active;
					RenderTexture.active = percentRenderTexture;
					progressTexture.ReadPixels(percentTextureRect, 0, 0);
					progressTexture.Apply();
					RenderTexture.active = prevRenderTexture;
					var pixel = progressTexture.GetPixel(0, 0);
					progress = pixel.r;
				}
				
				OnProgress?.Invoke(progress);
				if (OnCompleted != null)
				{
					var completeValue = card.Mode == ScratchMode.Erase ? 1f : 0f;
					if (Mathf.Abs(progress - completeValue) < float.Epsilon)
					{
						OnCompleted?.Invoke(progress);
						isCompleted = true;
					}
				}
				isCalculating = false;
			}
			else if (progressAccuracy == ProgressAccuracy.High && isCalculating && card.IsScratched)
			{
				updateProgress = true;
				updateProgressFrame = Time.frameCount;
			}
		}
		
		#endregion
		
		#region Public Methods

		public float GetProgress()
		{
			return progress;
		}

		public void UpdateProgress()
		{
			GL.LoadOrtho();
			commandBuffer.Clear();
			commandBuffer.SetRenderTarget(percentTargetIdentifier);
			commandBuffer.ClearRenderTarget(false, true, Color.clear);
			var pass = sampleSourceTexture ? 1 : 0;
			commandBuffer.DrawMesh(mesh, Matrix4x4.identity, progressMaterial, 0, pass);
			Graphics.ExecuteCommandBuffer(commandBuffer);
			StartCoroutine(CalcProgress());
		}

		public void ResetProgress()
		{
			isCompleted = false;
		}

		public void SetSpritePixels(Color[] pixels)
		{
			sourceSpritePixels = pixels;
		}

		#endregion
	}
}