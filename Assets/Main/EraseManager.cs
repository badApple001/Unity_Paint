using ScratchCardAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EraseManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }


    bool chan = false;
    // Update is called once per frame
    void Update()
    {

        if ( Input.GetKeyUp( KeyCode.Space ) && !chan )
        {
            chan = true;
            LoadScratchCard( "Textures/Diaochan" );
        }



        if( Input.GetKeyUp( KeyCode.Q ) )
        {
            LoadScratchCard( "Textures/splash" );
        }
        else if(Input.GetKeyUp( KeyCode.W ) ) { 
        
            LoadScratchCard( "Textures/2" );
        }
    }

    void LoadScratchCard( string img_resource_url )
    {


        var uiRoot = GameObject.Find( "Canvas/UI Form" ).GetComponent<RectTransform>();

        var img = GenImageObj( uiRoot, img_resource_url );

        var scratchCardManager = Instantiate( Resources.Load<GameObject>( "ScratchCard" ) ).GetComponent<ScratchCardManager>();

        scratchCardManager.CanvasRendererCard = img;

        img.raycastTarget = true; //启用 必要条件

        img.transform.localScale = Vector3.one * 0.4f;

        scratchCardManager.ScratchSurfaceSprite = img.sprite;
    }


    Image GenImageObj( Transform parent, string resource_url )
    {
        var obj = new GameObject();
        obj.transform.SetParent( parent, false );
        var img = obj.AddComponent<Image>();
        img.sprite = Resources.Load<Sprite>( resource_url );
        img.SetNativeSize();
        obj.name = img.sprite.name;
        return img;
    }
}
