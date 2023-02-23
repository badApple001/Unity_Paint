using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageErasure : MonoBehaviour
{
    [SerializeField,Tooltip("是否需要适配全屏")]
    private bool fitBgOnAwake = false;

    private Image drawingBoard = null;

    private void Awake()
    {
        drawingBoard = GetComponent<Image>();
        if ( null == drawingBoard )
        {
            throw new UnityEngine.UnityException( "You need to drag it onto an object that contains the Image component" );
        }

        //full screen
        if ( fitBgOnAwake )
        {
            RectTransform rectTransform = ( RectTransform ) transform;
            RectTransform parentRectTransform = ( RectTransform ) transform.parent;
            var size = rectTransform.sizeDelta;
            rectTransform.localScale = Vector3.one * ( parentRectTransform.sizeDelta.y / size.y );
        }
    }


}
