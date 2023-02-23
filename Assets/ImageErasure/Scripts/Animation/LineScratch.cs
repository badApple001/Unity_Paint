using System;
using UnityEngine;

namespace ScratchCardAsset.Animation
{
    [Serializable]
    public class LineScratch : BaseScratch
    {
        public Vector2 PositionEnd;
        public float BrushScaleEnd;
        public float TimeEnd = 1f;
    }
}