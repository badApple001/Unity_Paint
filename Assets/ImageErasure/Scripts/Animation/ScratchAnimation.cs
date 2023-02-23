using System.Collections.Generic;
using ScratchCardAsset.Core;
using UnityEngine;

namespace ScratchCardAsset.Animation
{
    [CreateAssetMenu(menuName = "Scratch Card/Scratch Animation", fileName = nameof(ScratchAnimation))]
    public class ScratchAnimation : ScriptableObject
    {
        [SerializeReference] public List<BaseScratch> Scratches;
        public ScratchAnimationSpace ScratchSpace;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}