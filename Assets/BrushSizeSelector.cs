using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BrushSizeSelector : MonoBehaviour
{
	// public GameObject smallBrush, mediumBrush, largeBrush;
    public DrawableSurface brushSize;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void ChangeBrushSize(){
    GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
    if(clickedObject != null){
        string clickedObjectName = clickedObject.name;
        Debug.Log("Clicked Object Name: " + clickedObjectName);
    }
}

}
