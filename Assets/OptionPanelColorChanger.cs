using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanelColorChanger : MonoBehaviour
{
    public GameObject _showColor;
    public GameObject _small;
    public GameObject _meidum;
    public GameObject _large;
    private Color newColor;
    private DrawableSurface _surface;

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = GameObject.Find("Board");
        _surface = go.GetComponent<DrawableSurface>();
    }

    // Update is called once per frame
    void Update()
    {
        Color newColor = _surface.brushColor;
        _showColor.GetComponent<Image>().color = newColor;
        _small.GetComponent<Image>().color = newColor;
        _meidum.GetComponent<Image>().color = newColor;
        _large.GetComponent<Image>().color = newColor;
    }
}
