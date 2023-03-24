using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    enum Channel
    {
        Hue,
        Saturation,
        Value,
    }

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
        valueSlider.value = 1;

        _surface = GameObject.Find("Board")?.GetComponent<DrawableSurface>();

        UpdateSliderBackground(hueSlider, GenerateTexture(Channel.Hue));
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

    private Texture2D GenerateTexture(Channel channel)
    {
        int width = channel == Channel.Hue ? 360 : 100;
        Color32[] colors = new Color32[width];

        for (int i = 0; i < width; ++i) {
            float h = channel == Channel.Hue ? (float)i / width : 1.0f;
            float s = channel == Channel.Saturation ? (float)i / width : 1.0f;
            float v = channel == Channel.Value ? (float)i / width : 1.0f;
            Color rgb = Color.HSVToRGB(h, s, v);
            byte r = (byte)(rgb.r * 255.0f);
            byte g = (byte)(rgb.g * 255.0f);
            byte b = (byte)(rgb.b * 255.0f);

            colors[i] = new Color32(r, g, b, 255);
        }

        Texture2D tex = new Texture2D(1, width);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(colors);
        tex.Apply();

        return tex;
    }

    private void UpdateSliderBackground(Slider slider, Texture texture)
    {
        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = texture;
        slider.GetComponentInChildren<Image>().material = material;
    }

    private void UpdateColor()
    {
        colorPreview.color = color;

        UpdateSliderBackground(saturationSlider, GenerateTexture(Channel.Saturation));
        UpdateSliderBackground(valueSlider, GenerateTexture(Channel.Value));

        if (_surface != null) {
            _surface.brushColor = color;
        }
    }
}
