using System;
using ScratchCardAsset.Core;
using UnityEngine;

namespace ScratchCardAsset.Animation
{
    public class ScratchAnimator : MonoBehaviour
    {
        #region Events

        public event Action OnPlay;
        public event Action OnPause;
        public event Action OnStop;
        public event Action OnCompleted;

        #endregion

        public ScratchCard ScratchCard;
        public ScratchAnimation ScratchAnimation;
        public bool PlayOnStart = true;

        public bool IsPlaying => isPlaying;
        
        private int currentScratchIndex;
        private float progress;
        private float totalTime;
        private Vector2? previousPosition;
        private Vector2 scale = Vector2.one;
        private bool isPlaying;
        
        private void Start()
        {
            if (ScratchAnimation != null && ScratchAnimation.ScratchSpace == ScratchAnimationSpace.UV)
            {
                scale = ScratchCard.ScratchData.TextureSize;
            }

            if (PlayOnStart)
            {
                Play();
            }
        }
        
        private void Update()
        {
            if (isPlaying)
            {
                UpdateScratches();
                totalTime += Time.deltaTime;
            }
        }

        private void OnValidate()
        {
            if (ScratchCard == null)
            {
                if (TryGetComponent(out ScratchCard)) 
                    return;
                
                if (TryGetComponent<ScratchCardManager>(out var scratchCardManager))
                {
                    if (scratchCardManager.Card != null)
                    {
                        ScratchCard = scratchCardManager.Card;
                    }
                }
            }
        }

        private void UpdateScratches()
        {
            if (ScratchAnimation == null || ScratchAnimation.Scratches.Count == 0)
                return;
                
            var scratch = ScratchAnimation.Scratches[currentScratchIndex];
            if (totalTime < scratch.Time)
                return;
            
            if (scratch is LineScratch line)
            {
                var duration = line.TimeEnd - line.Time;
                if (duration == 0f)
                {
                    progress = 1f;
                }
                else
                {
                    progress = totalTime / duration;
                }

                var position = Vector3.Lerp(line.Position, line.PositionEnd, progress) * scale;
                var pressure = Mathf.Lerp(line.BrushScale, line.BrushScaleEnd, progress);
                if (previousPosition == null)
                {
                    previousPosition = line.Position * scale;
                }
                ScratchCard.ScratchLine(previousPosition.Value, position, pressure, pressure);
                previousPosition = position;
            }
            else
            {
                if (scratch.Time == 0f)
                {
                    progress = 1f;
                }
                else
                {
                    progress = totalTime / scratch.Time;
                }

                if (progress >= 1f)
                {
                    var position = scratch.Position * scale;
                    var pressure = scratch.BrushScale;
                    ScratchCard.ScratchHole(position, pressure);
                    previousPosition = null;
                }
            }
            
            if (progress >= 1f)
            {
                currentScratchIndex++;
                progress = 0f;
                previousPosition = null;
                if (currentScratchIndex == ScratchAnimation.Scratches.Count)
                {
                    Stop();
                    OnCompleted?.Invoke();
                }
                else
                {
                    UpdateScratches();
                }
            }
        }

        [ContextMenu("Play")]
        public void Play()
        {
            isPlaying = true;
            OnPlay?.Invoke();
        }

        [ContextMenu("Pause")]
        public void Pause()
        {
            isPlaying = false;
            OnPause?.Invoke();
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            isPlaying = false;
            totalTime = 0f;
            currentScratchIndex = 0;
            progress = 0f;
            previousPosition = null;
            OnStop?.Invoke();
        }
    }
}