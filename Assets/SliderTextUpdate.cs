using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderTextUpdate : MonoBehaviour
{
    public Text label;

    void Start()
    {
        GetComponent<Slider>().onValueChanged.AddListener(delegate { UpdateText(); });
        UpdateText();
    }

    public void UpdateText()
    {
        label.text = GetComponent<Slider>().value.ToString();
    }
}
