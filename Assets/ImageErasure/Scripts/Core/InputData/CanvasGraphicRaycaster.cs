using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ScratchCardAsset.Core.InputData
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class CanvasGraphicRaycaster : MonoBehaviour
    {
        private GraphicRaycaster raycaster;
        private EventSystem eventSystem;
        private PointerEventData pointerEventData;
        private List<RaycastResult> raycastResults; 

        void Start()
        {
            raycastResults = new List<RaycastResult>();
            TryGetComponent(out raycaster);
            TryGetComponent(out eventSystem);
            if (eventSystem == null)
            {
                eventSystem = FindObjectOfType<EventSystem>();
            }
        }

        public List<RaycastResult> GetRaycasts(Vector2 position)
        {
            if (raycaster == null)
                return null;
            
            raycastResults.Clear();
            pointerEventData = new PointerEventData(eventSystem) {position = position};
            raycaster.Raycast(pointerEventData, raycastResults);
            return raycastResults;
        }
    }
}