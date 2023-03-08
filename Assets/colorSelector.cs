using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class colorSelector : MonoBehaviour
{
    
    public GameObject showColor;
    [SerializeField] private Color newColor;
    private Color ButtonColor;
	public GameObject _redButton;
    public GameObject _greenButton;
    public GameObject _blueButton;
    public GameObject _blackButton;
    public DrawableSurface colorScript;

 //    public GameObject _blackButton;
 //    public GameObject _whiteButton;

 


	
    // Start is called before the first frame update
    void Start()
    {
        colorScript._color = _redButton.GetComponent<Image>().color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void ChangeShowColorToRed(){


        showColor.GetComponent<Image>().color = _redButton.GetComponent<Image>().color;
        // colorScript._material.SetColor("_Color", _redButton.GetComponent<Image>().color);



    }

    public void ChangeShowColorToBlue(){


        showColor.GetComponent<Image>().color = _blueButton.GetComponent<Image>().color;

    }

    public void ChangeShowColorToGreen(){


        showColor.GetComponent<Image>().color = _greenButton.GetComponent<Image>().color;

    }

    public void ChangeShowColorToBlack(){


        showColor.GetComponent<Image>().color = _blackButton.GetComponent<Image>().color;

    }
}
