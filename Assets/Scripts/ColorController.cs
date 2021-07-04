using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class ColorController : MonoBehaviour
{
    [SerializeField] Slider hueSlider;
    [SerializeField] Slider saturationSlider;
    [SerializeField] Slider valueSlider;

    [SerializeField] GameObject panel;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;

    [SerializeField] ColorPreview[] colorPreviews;

    enum ColorType {Background, FaderBackground, FaderHandle, Text, ScrollHandle, ScrollBackground }
    ColorType currentColorType = ColorType.Background;

    Dictionary<ColorType, Color> colors = new Dictionary<ColorType, Color>();

	private void Awake()
	{
        InitializeColorDictionary();
        InitializeUI();

        foreach(ColorPreview p in colorPreviews)
        {
            p.selectionButton.onClick.AddListener(ColorPreviewButton);
		}
    }

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
	}

    void SliderChange(float _val)
    {
        ColorPreview preview = GetPreviewFromColorType(currentColorType);
        preview.image.color = GetColorFromSliders();
	}

    Color GetColorFromSliders()
    {
        float hue = ColorIntToFloat((int)hueSlider.value);
        float sat = ColorIntToFloat((int)saturationSlider.value);
        float val = ColorIntToFloat((int)valueSlider.value);

        return Color.HSVToRGB(hue, sat, val);
    }

    void ColorPreviewButton()
    {
        Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        ColorPreview preview = GetPreviewFromButton(button);

        currentColorType = preview.colorType;

        SetSlidersToColor(colors[currentColorType]);
    }

    void SetSlidersToColor(Color _color)
    {
        Vector3 hsv;
        Color.RGBToHSV(_color, out hsv.x, out hsv.y, out hsv.z);

        hsv.x = ColorFloatToInt(hsv.x);
        hsv.y = ColorFloatToInt(hsv.y);
        hsv.z = ColorFloatToInt(hsv.z);

        hueSlider.SetValueWithoutNotify(hsv.x);
        saturationSlider.SetValueWithoutNotify(hsv.y);
        valueSlider.SetValueWithoutNotify(hsv.z);
    }

    void SetPreviewToColor(ColorType _type, Color _color)
    {
        GetPreviewFromColorType(_type).image.color = _color;
	}

    ColorPreview GetPreviewFromColorType(ColorType _type)
    {
        foreach(ColorPreview p in colorPreviews)
        {
            if(p.colorType == _type)
            {
                return p;
			}
		}

        Debug.LogError($"No color preview found for {_type}!");
        return colorPreviews[0];
    }

    ColorPreview GetPreviewFromButton(Button _button)
    {
        foreach (ColorPreview p in colorPreviews)
        {
            if (p.selectionButton == _button)
            {
                return p;
            }
        }

        Debug.LogError($"No button found for {_button.name}!");
        return colorPreviews[0];
    }

    void InitializeColorDictionary()
    {
        foreach (ColorType suit in (ColorType[])Enum.GetValues(typeof(ColorType)))
        {
            colors.Add(suit, Color.white);
        }
    }

    void InitializeUI()
    {
        hueSlider.onValueChanged.AddListener(SliderChange);
        saturationSlider.onValueChanged.AddListener(SliderChange);
        valueSlider.onValueChanged.AddListener(SliderChange);
        openButton.onClick.AddListener(TogglePanel);
        closeButton.onClick.AddListener(TogglePanel);
    }

	#region Color Translation
	Vector3Int ColorFloatToInt(Vector3 _floatColor)
    {
        _floatColor.x = ColorFloatToInt(_floatColor.x);
        _floatColor.y = ColorFloatToInt(_floatColor.y);
        _floatColor.z = ColorFloatToInt(_floatColor.z);

        return new Vector3Int((int)_floatColor.x, (int)_floatColor.y, (int)_floatColor.z);
    }

    Vector3 ColorIntToFloat(Vector3Int _intColor)
    {
        Vector3 col = _intColor;
        col.x = ColorIntToFloat((int)col.x);
        col.y = ColorIntToFloat((int)col.y);
        col.z = ColorIntToFloat((int)col.z);

        return col;
    }

    float ColorIntToFloat(int _int)
    {
        return _int / 255f;
	}

    int ColorFloatToInt(float _float)
    {
        return (int)(_float * 255);
	}
	#endregion Color Translation

	[Serializable]
    struct ColorPreview
    {
        public Button selectionButton;
        public Image image;
        public ColorType colorType;
	}
}
