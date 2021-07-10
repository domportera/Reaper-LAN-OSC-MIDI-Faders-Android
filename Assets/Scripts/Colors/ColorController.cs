using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using static ControlsManager;
using static ColorProfile;

public class ColorController : MonoBehaviour
{
    [SerializeField] Slider hueSlider;
    [SerializeField] Slider saturationSlider;
    [SerializeField] Slider valueSlider;

    [SerializeField] GameObject panel;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;

    [SerializeField] ColorButton[] colorPreviews;
    [SerializeField] ControlsManager controlManager;

    ColorType currentColorType = ColorType.Background;

    ColorProfile defaultColorProfile = new ColorProfile();
    ColorProfile currentColorProfile;

    public static ColorController instance;
    static List<ColorSetter> colorSetters = new List<ColorSetter>();

	private void Awake()
    {
        if(instance == null)
        {
            instance = this;
		}
        else
        {
            Debug.LogError($"Can't have more than one Color Controller");
		}

        InitializeUI();

        foreach (ColorButton p in colorPreviews)
        {
            p.selectionButton.onClick.AddListener(ColorPreviewButtonPress);
        }

        Load();
    }

	// Start is called before the first frame update
	void Start()
    {
        UpdateAppColors();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Load()
    {
        currentColorProfile = defaultColorProfile;
	}

    void UpdateAppColors()
    {
        foreach(ColorSetter c in colorSetters)
        {
            c.SetColors(currentColorProfile);
		}
	}

    void UpdateAppColors(ColorSetter _setter)
    {
        _setter.SetColors(currentColorProfile);
	}

    void UpdateColorProfile(ColorType _type, Color _color)
    {
        currentColorProfile.SetColor(_type, _color);
	}

    public static void AddToControls(ColorSetter _setter)
    {
        colorSetters.Add(_setter);
        instance.UpdateAppColors(_setter);
    }

    public static void RemoveFromControls(ColorSetter _setter)
    {
        colorSetters.Remove(_setter);
	}


    void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
	}

    void SliderChange(float _val)
    {
        ColorButton preview = GetPreviewFromColorType(currentColorType);
        UpdateColorProfile(preview.colorType, GetColorFromSliders());
        UpdateAppColors();
	}

    Color GetColorFromSliders()
    {
        float hue = ColorIntToFloat((int)hueSlider.value);
        float sat = ColorIntToFloat((int)saturationSlider.value);
        float val = ColorIntToFloat((int)valueSlider.value);

        return Color.HSVToRGB(hue, sat, val);
    }

    void ColorPreviewButtonPress()
    {
        Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        ColorButton preview = GetPreviewFromButton(button);

        currentColorType = preview.colorType;

        SetSlidersToColor(currentColorProfile.GetColor(currentColorType));
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

    ColorButton GetPreviewFromColorType(ColorType _type)
    {
        foreach(ColorButton p in colorPreviews)
        {
            if(p.colorType == _type)
            {
                return p;
			}
		}

        Debug.LogError($"No color preview found for {_type}!");
        return colorPreviews[0];
    }

    ColorButton GetPreviewFromButton(Button _button)
    {
        foreach (ColorButton p in colorPreviews)
        {
            if (p.selectionButton == _button)
            {
                return p;
            }
        }

        Debug.LogError($"No button found for {_button.name}!");
        return colorPreviews[0];
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
    struct ColorButton
    {
        public Button selectionButton;
        public ColorType colorType;
	}
}
