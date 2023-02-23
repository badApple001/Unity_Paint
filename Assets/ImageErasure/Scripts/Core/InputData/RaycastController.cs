using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchCardAsset.Core.InputData
{
    public class RaycastController
    {
        private GameObject scratchSurface;
        private List<CanvasGraphicRaycaster> raycasters;

        public RaycastController(GameObject surfaceObject, Canvas[] canvasesForRaycastsBlocking)
        {
            scratchSurface = surfaceObject;
            raycasters = new List<CanvasGraphicRaycaster>();
            
            if (surfaceObject.TryGetComponent<Image>(out var image) && image.canvas != null)
            {
                if (!image.canvas.TryGetComponent<CanvasGraphicRaycaster>(out var raycaster))
                {
                    raycaster = image.canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                }
                if (raycaster != null)
                {
                    raycasters.Add(raycaster);
                }
            }

            if (canvasesForRaycastsBlocking != null)
            {
                foreach (var canvas in canvasesForRaycastsBlocking)
                {
                    if (canvas != null)
                    {
                        if (!canvas.TryGetComponent<CanvasGraphicRaycaster>(out var raycaster))
                        {
                            raycaster = canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                        }

                        if (raycaster != null)
                        {
                            raycasters.Add(raycaster);
                        }
                    }
                }
            }
        }

        public bool IsBlock(Vector3 position)
        {
            var isBlock = false;
            foreach (var raycaster in raycasters)
            {
                if (raycaster == null)
                    continue;
                
                var result = raycaster.GetRaycasts(position);
                if (result.Count == 0 || result.Count > 0 && result[0].gameObject == scratchSurface)
                    continue;
                
                isBlock = true;
                break;
            }
            return isBlock;
        }
    }
}