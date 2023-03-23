using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushSizeSelector : MonoBehaviour
{
    
    
    public DrawableSurface brushScript;
    

 //    public GameObject _blackButton;
 //    public GameObject _whiteButton;

 


    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void ChangeBrushSizeToSmall(){

        brushScript.brush_size = 0.005f;


    }
    public void ChangeBrushSizeToMedium(){


        brushScript.brush_size = 0.01f;


    }
    public void ChangeBrushSizeToLarge(){


        brushScript.brush_size = 0.05f;


    }

    
}
