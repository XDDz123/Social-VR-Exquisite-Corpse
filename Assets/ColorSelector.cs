using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    public Image colorPreview;
    public Slider hueSlider;
    public Slider saturationSlider;
    public Slider valueSlider;

    private DrawableSurface _surface;

    public Color color
    {
        get => Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
    }

    void Start()
    {
        SetupSlider(hueSlider);
        SetupSlider(saturationSlider);
        SetupSlider(valueSlider);

        _surface = GameObject.Find("Board")?.GetComponent<DrawableSurface>();

        UpdateColor();
    }

    private void SetupSlider(Slider slider)
    {
        if (slider == null) {
            return;
        }

        slider.onValueChanged.AddListener(delegate { UpdateColor(); });
        slider.minValue = 0;
        slider.maxValue = 1;
    }

    private void UpdateColor()
    {
        colorPreview.color = color;

        if (_surface != null) {
            _surface.brushColor = color;
        }
    }
}
