using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace ScratchCardAsset.Core.InputData
{
	/// <summary>
	/// Process Input for ScratchCard
	/// </summary>
#if SCRATCHCARD_DEBUG
	[Serializable]
#endif
	public class ScratchCardInput
	{
		#region Events

		public event ScratchHandler OnScratch;
		public event Action<Vector2, float> OnScratchHole;
		public event Action<ScratchCardInputData> OnScratchHoleExtended;
		public event Action<Vector2, float, Vector2, float> OnScratchLine;
		public event Action<ScratchCardInputData, ScratchCardInputData> OnScratchLineExtended;
		public delegate Vector2 ScratchHandler(Vector2 position);
		
		#endregion

		public bool CheckCanvasRaycasts;
		public bool UsePressure;

		public bool IsScratching
		{
			get
			{
				if (isScratching != null)
				{
					foreach (var scratching in isScratching)
					{
						if (scratching)
							return true;
					}
				}
				return false;
			}
		}

		private readonly Func<bool> isScratched;
		private RaycastController raycastController;
		private ScratchCardInputData[] startInputData;
		private ScratchCardInputData[] endInputData;
		private Vector2?[] previousScratchPosition;
		private Vector2 scratchPosition;
		private bool[] isScratching;
		private bool[] isStartPosition;
#if UNITY_WEBGL
		private bool isWebgl = true;
#else
		private bool isWebgl = false;
#endif

		private const int MaxTouchCount = 10;
		private const int ReserveTouchCount = 1; //used for "public void Scratch(Vector2 screenPosition)" method

		public ScratchCardInput(Func<bool> scratched)
		{
			isScratched = scratched;
			isScratching = new bool[MaxTouchCount + ReserveTouchCount];
			isStartPosition = new bool[MaxTouchCount + ReserveTouchCount];
			startInputData = new ScratchCardInputData[MaxTouchCount + ReserveTouchCount];
			endInputData = new ScratchCardInputData[MaxTouchCount + ReserveTouchCount];
			previousScratchPosition = new Vector2?[MaxTouchCount + ReserveTouchCount];
			for (var i = 0; i < isStartPosition.Length; i++)
			{
				isStartPosition[i] = true;
			}
#if ENABLE_INPUT_SYSTEM
			if (!EnhancedTouchSupport.enabled)
			{
				EnhancedTouchSupport.Enable();
			}
#endif
		}
		
		public void InitRaycastsController(GameObject surfaceObject, Canvas[] canvases)
		{
			raycastController = new RaycastController(surfaceObject, canvases);
		}

		public bool TryUpdate()
		{
#if ENABLE_INPUT_SYSTEM
			if (Pen.current != null && (Pen.current.press.isPressed || Pen.current.press.wasReleasedThisFrame))
			{
				if (Pen.current.press.isPressed)
				{
					var position = Pen.current.position.ReadValue();
					var pressure = UsePressure ? Pen.current.pressure.ReadValue() : 1f;

					if (Pen.current.press.wasPressedThisFrame)
					{
						isScratching[0] = false;
						isStartPosition[0] = true;
					}

					if (!Pen.current.press.wasPressedThisFrame)
					{
						SetInputData(0, position, pressure);
					}
				}
				else if (Pen.current.press.wasReleasedThisFrame)
				{
					isScratching[0] = false;
					previousScratchPosition[0] = null;
				}
				
				Scratch();
			}
			else if (Touchscreen.current != null && Touch.activeTouches.Count > 0 && !isWebgl)
			{
				foreach (var touch in Touch.activeTouches)
				{
					var fingerId = touch.finger.index;
					if (fingerId >= MaxTouchCount)
						continue;
					
					var pressure = touch.pressure;
					if (!UsePressure || pressure == 0f)
					{
						pressure = 1f;
					}

					if (touch.phase == TouchPhase.Began)
					{
						isScratching[fingerId] = false;
						isStartPosition[fingerId] = true;
					}
					
					if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
					{
						SetInputData(fingerId, touch.screenPosition, pressure);
					}
					
					if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
					{
						isScratching[fingerId] = false;
						previousScratchPosition[fingerId] = null;
					}
					
					Scratch();
				}
			}
			else if (Mouse.current != null)
			{
				if (Mouse.current.leftButton.wasPressedThisFrame)
				{
					isScratching[0] = false;
					isStartPosition[0] = true;
				}

				if (Mouse.current.leftButton.isPressed)
				{
					SetInputData(0, Mouse.current.position.ReadValue());
				}

				if (Mouse.current.leftButton.wasReleasedThisFrame)
				{
					isScratching[0] = false;
					previousScratchPosition[0] = null;
				}
				
				Scratch();
			}
#elif ENABLE_LEGACY_INPUT_MANAGER
			//Touch / Mouse
			if (Input.touchSupported && Input.touchCount > 0 && !isWebgl)
			{
				foreach (var touch in Input.touches)
				{
					var fingerId = touch.fingerId;
					if (fingerId >= MaxTouchCount)
						continue;

					if (touch.phase == TouchPhase.Began)
					{
						isScratching[fingerId] = false;
						isStartPosition[fingerId] = true;
					}

					if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
					{
						SetInputData(fingerId, touch.position);
					}

					if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
					{
						isScratching[fingerId] = false;
						previousScratchPosition[fingerId] = null;
					}

					Scratch();
				}
			}
			else
			{
				if (Input.GetMouseButtonDown(0))
				{
					isScratching[0] = false;
					isStartPosition[0] = true;
				}

				if (Input.GetMouseButton(0))
				{
					SetInputData(0, Input.mousePosition);
				}

				if (Input.GetMouseButtonUp(0))
				{
					isScratching[0] = false;
					previousScratchPosition[0] = null;
				}

				Scratch();
			}
#endif
			return IsScratching;
		}

		private void SetInputData(int fingerId, Vector2 position, float pressure = 1f)
		{
			if (!isScratching[fingerId] && CheckCanvasRaycasts && raycastController != null && raycastController.IsBlock(position))
				return;

			if (OnScratch != null)
			{
				scratchPosition = OnScratch(position);
			}

			if (isStartPosition[fingerId])
			{
				startInputData[fingerId].Position = scratchPosition;
				startInputData[fingerId].Pressure = pressure;
				startInputData[fingerId].Time = Time.time;
				endInputData[fingerId] = startInputData[fingerId];
				isStartPosition[fingerId] = !isStartPosition[fingerId];
			}
			else
			{
				startInputData[fingerId] = endInputData[fingerId];
				endInputData[fingerId].Position = scratchPosition;
				endInputData[fingerId].Pressure = pressure;
				endInputData[fingerId].Time = Time.time;
			}

			if (!isScratching[fingerId])
			{
				endInputData[fingerId] = startInputData[fingerId];
				isScratching[fingerId] = true;
			}
		}

		private void Scratch()
		{
			for (var i = 0; i < isScratching.Length; i++)
			{
				if (isScratching[i])
				{
					if (startInputData[i].Position == endInputData[i].Position)
					{
						OnScratchHole?.Invoke(endInputData[i].Position, endInputData[i].Pressure);
						if (isScratched != null && isScratched())
						{
							OnScratchHoleExtended?.Invoke(endInputData[i]);
						}
					}
					else
					{
						OnScratchLine?.Invoke(startInputData[i].Position, startInputData[i].Pressure,
							endInputData[i].Position, endInputData[i].Pressure);
						if (isScratched != null && isScratched())
						{
							OnScratchLineExtended?.Invoke(startInputData[i], endInputData[i]);
						}
					}
				}
			}
		}

		public void Scratch(Vector2 screenPosition)
		{
			SetInputData(MaxTouchCount, screenPosition);
		}
	}
}