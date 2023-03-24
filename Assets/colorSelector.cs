using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class colorSelector : MonoBehaviour
{
    public GameObject showColor;
    public Button[] colorButtons;
    private DrawableSurface _surface;

    void Start()
    {
        GameObject go = GameObject.Find("Board");
        _surface = go.GetComponent<DrawableSurface>();
    }

    public void ChangeColor(Button button){


        showColor.GetComponent<Image>().color = button.GetComponent<Image>().color;
        _surface.brushColor = button.GetComponent<Image>().color;


    }

  
}
