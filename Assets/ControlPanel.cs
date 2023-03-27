using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
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
    public Slider brushSizeSlider;

    public RectTransform paletteTransform;
    public int paletteSize = 10;
    public GameObject paletteButtonPrefab;

    private Button[] _palette;
    private int _oldestColor = 0;

    private DrawableSurface _surface;
    private LaserPointer _laser_pointer;

    public Color color
    {
        get => Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
        set
        {
            float H;
            float S;
            float V;

            Color.RGBToHSV(value, out H, out S, out V);
            hueSlider.value = H;
            saturationSlider.value = S;
            valueSlider.value = V;
        }
    }

    void Start()
    {
        SetupColorSlider(hueSlider);
        SetupColorSlider(saturationSlider);
        SetupColorSlider(valueSlider);
        SetupHistory();
        SetupBrushSize();

        _surface = GameObject.Find("Board")?.GetComponent<DrawableSurface>();
        _laser_pointer = GameObject.Find("Laser Pointer")?.GetComponent<LaserPointer>();

        UpdateSliderBackground(hueSlider, GenerateTexture(Channel.Hue));
        UpdateColor();
    }

    public void SaveCurrentColor()
    {
        Button button = _palette[_oldestColor];

        button.GetComponent<Image>().color = color;
        button.gameObject.SetActive(true);

        if (++_oldestColor >= _palette.Length) {
            _oldestColor = 0;
        }
    }

    public void UseBrush()
    {
        _surface.brushColor = color;
        _laser_pointer.color = color;
    }

    public void UseEraser()
    {
        _surface.brushColor = Color.white;
        _laser_pointer.color = Color.white;
    }

    private void SetupColorSlider(Slider slider)
    {
        if (slider == null) {
            return;
        }

        slider.onValueChanged.AddListener(delegate { UpdateColor(); });
        slider.minValue = 0;
        slider.maxValue = 1;

        // set initial value to 0
        slider.value = 0;
    }

    private void SetupHistory()
    {
        paletteButtonPrefab.SetActive(false);

        _palette = new Button[paletteSize];

        for(int i = 0; i < _palette.Length; i++)
        {
            GameObject goButton = (GameObject)Instantiate(paletteButtonPrefab);
            goButton.transform.SetParent(paletteTransform, false);
            goButton.SetActive(false);

            Button button = goButton.GetComponent<Button>();
            button.onClick.AddListener(() => color = button.GetComponent<Image>().color);
            _palette[i] = button;
        }
    }

    private void SetupBrushSize()
    {
        if (brushSizeSlider == null) {
            return;
        }

        brushSizeSlider.onValueChanged.AddListener(delegate { UpdateBrushSize(); });
        brushSizeSlider.minValue = 1;
        brushSizeSlider.maxValue = 5;
    }

    private Texture2D GenerateTexture(Channel channel)
    {
        int width = channel == Channel.Hue ? 360 : 100;
        Color32[] colors = new Color32[width];

        for (int i = 0; i < width; ++i) {
            float h = hueSlider.value;
            float s = 1.0f;
            float v = 1.0f;

            switch(channel) {
                case Channel.Hue:
                    h = (float)i / width;
                    break;

                case Channel.Saturation:
                    s = (float)i / width;
                    break;

                case Channel.Value:
                    v = (float)i / width;
                    break;
            }

            Color rgb = Color.HSVToRGB(h, s, v);
            byte r = (byte)(rgb.r * 255.0f);
            byte g = (byte)(rgb.g * 255.0f);
            byte b = (byte)(rgb.b * 255.0f);

            colors[i] = new Color32(r, g, b, 255);
        }

        Texture2D tex = new Texture2D(width, 1);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(colors);
        tex.Apply();

        return tex;
    }

    private void UpdateSliderBackground(Slider slider, Texture texture)
    {
        Material material = new Material(Shader.Find("UI/Default"));
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

        if (_laser_pointer != null)
        {
            _laser_pointer.color = color;
        }
    }

    private void UpdateBrushSize()
    {
        if (_surface != null) {
            _surface.brushSize = 0.01f * brushSizeSlider.value;
        }

        if (_laser_pointer != null)
        {
            _laser_pointer.scale = brushSizeSlider.value;
        }
    }
}
