using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class colorSelector : MonoBehaviour
{
    public GameObject showColor;
	public GameObject _redButton;
    public GameObject _greenButton;
    public GameObject _blueButton;
    public GameObject _blackButton;
    private DrawableSurface _surface;

    void Start()
    {
        GameObject go = GameObject.Find("Board");
        _surface = go.GetComponent<DrawableSurface>();
    }

    public void ChangeShowColorToRed(){


        showColor.GetComponent<Image>().color = _redButton.GetComponent<Image>().color;
        _surface.brushColor = _redButton.GetComponent<Image>().color;



    }

    public void ChangeShowColorToBlue(){


        showColor.GetComponent<Image>().color = _blueButton.GetComponent<Image>().color;
        _surface.brushColor = _blueButton.GetComponent<Image>().color;

    }

    public void ChangeShowColorToGreen(){


        showColor.GetComponent<Image>().color = _greenButton.GetComponent<Image>().color;
        _surface.brushColor =_greenButton.GetComponent<Image>().color;

    }

    public void ChangeShowColorToBlack(){


        showColor.GetComponent<Image>().color = _blackButton.GetComponent<Image>().color;
        _surface.brushColor = _blackButton.GetComponent<Image>().color;


    }
}
