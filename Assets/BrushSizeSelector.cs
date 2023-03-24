using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushSizeSelector : MonoBehaviour
{
    private DrawableSurface _surface;

    void Start()
    {
        GameObject go = GameObject.Find("Board");
        _surface = go.GetComponent<DrawableSurface>();
    }

    public void ChangeBrushSizeToSmall()
    {
        _surface.brushSize = 0.005f;
    }

    public void ChangeBrushSizeToMedium()
    {
        _surface.brushSize = 0.01f;
    }

    public void ChangeBrushSizeToLarge()
    {
        _surface.brushSize = 0.05f;
    }

    public void Eraser(){

        _surface.brushColor = Color.white;
    }
}
