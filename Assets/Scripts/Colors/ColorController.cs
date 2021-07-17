using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using static ColorProfile;
using System.IO;

public class ColorController : MonoBehaviour
{
    [SerializeField] ControlsManager controlMan;
    [SerializeField] Slider hueSlider;
    [SerializeField] Slider saturationSlider;
    [SerializeField] Slider valueSlider;

    [SerializeField] GameObject panel;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;

    [SerializeField] Button saveButton;
    [SerializeField] Button revertButton;
    [SerializeField] Button setAsDefaultButton;

    [SerializeField] ColorButton[] colorTypeButtons;

    public static ColorController instance;
    static List<ColorSetter> colorSetters = new List<ColorSetter>();

    const string DEFAULT_COLOR_PROFILE = ControlsManager.DEFAULT_SAVE_NAME + " Colors";

    ColorType currentColorType = ColorType.Background;
    ColorProfile currentColorProfile;

    string fileExtension = ".colors";
    string basePath;

	private void Awake()
    {
        SingletonSetup();
        InitializeUI();

        foreach (ColorButton p in colorTypeButtons)
        {
            p.selectionButton.onClick.AddListener(ColorPreviewButtonPress);
        }

        basePath = Application.persistentDataPath + "/Color Profiles/";
        CheckBasePath();
        controlMan.OnProfileLoaded.AddListener(LoadAndSetColorProfile);
    }

	// Start is called before the first frame update
	void Start()
    {
        UpdateAppColors();
    }

    public static void AddToControls(ColorSetter _setter)
    {
        colorSetters.Add(_setter);

        if (instance.currentColorProfile != null)
        {
            instance.UpdateAppColors(_setter);
        }
    }

    public static void RemoveFromControls(ColorSetter _setter)
    {
        colorSetters.Remove(_setter);
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
        HighlightSelectedColorType(preview.colorType);

        SetSlidersToColor(currentColorProfile.GetColor(currentColorType));
    }

    void HighlightSelectedColorType(ColorType _colorType)
    {
        foreach(ColorButton butt in colorTypeButtons)
        {
            if(butt.colorType == _colorType)
            {
                butt.label.fontStyle = FontStyle.Bold;
			}
            else
            {
                butt.label.fontStyle = FontStyle.Normal;
            }
		}
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
        foreach(ColorButton p in colorTypeButtons)
        {
            if(p.colorType == _type)
            {
                return p;
			}
		}

        Debug.LogError($"No color preview found for {_type}!");
        return colorTypeButtons[0];
    }

    ColorButton GetPreviewFromButton(Button _button)
    {
        foreach (ColorButton p in colorTypeButtons)
        {
            if (p.selectionButton == _button)
            {
                return p;
            }
        }

        Debug.LogError($"No button found for {_button.name}!");
        return colorTypeButtons[0];
    }

    void InitializeUI()
    {
        hueSlider.onValueChanged.AddListener(SliderChange);
        saturationSlider.onValueChanged.AddListener(SliderChange);
        valueSlider.onValueChanged.AddListener(SliderChange);
        openButton.onClick.AddListener(TogglePanel);
        closeButton.onClick.AddListener(TogglePanel);

        setAsDefaultButton.onClick.AddListener(SaveDefaultProfile);
        revertButton.onClick.AddListener(RevertColorProfile);
        saveButton.onClick.AddListener(SaveProfile);

        HighlightSelectedColorType(currentColorType);
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

    #region Saving and Loading Color Profiles

    void SaveDefaultProfile()
    {
        SaveProfile(DEFAULT_COLOR_PROFILE, true);
    }

    void SaveProfile()
    {
        SaveProfile(currentColorProfile.name);
    }

    void SaveProfile(string _name, bool _savingDefault = false)
    {
        if(!_savingDefault && _name == DEFAULT_COLOR_PROFILE)
        {
            Utilities.instance.SetErrorText($"Can't save over the Default profile. If you'd like to set the default color palette that will be loaded on this and any new profile you create, click \"Set as Default Color Scheme\"");
            return;
        }

        string path = basePath + _name + fileExtension;
        string json = JsonUtility.ToJson(currentColorProfile, true);
        File.WriteAllText(path, json);

        if (_name != DEFAULT_COLOR_PROFILE)
        {
            Utilities.instance.SetConfirmationText($"Saved color profile for {_name}!");
        }
        else
        {
            Utilities.instance.SetConfirmationText($"Set default colors!");
        }


        Debug.Log($"Saved\n" + json);
    }

    ColorProfile GetDefaultColorProfile(string _profile)
    {
        string path = basePath + DEFAULT_COLOR_PROFILE + fileExtension;

        if (File.Exists(path))
        {
            //load that file and set as current color profile
            return GetColorProfileFromFile(path);
        }
        else
        {
            return ColorProfile.NewDefaultColorProfile(_profile);
        }
    }

    ColorProfile GetColorProfileFromFile(string _path)
    {
        string json = File.ReadAllText(_path);
        return JsonUtility.FromJson<ColorProfile>(json);
    }

    void CheckBasePath()
    {
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
    }

    void LoadAndSetColorProfile(string _profile)
    {
        if(_profile == ControlsManager.DEFAULT_SAVE_NAME)
        {
            _profile = DEFAULT_COLOR_PROFILE;
		}

        string fullPath = basePath + _profile + fileExtension;

        if (File.Exists(fullPath))
        {
            //load that file and set as current color profile
            currentColorProfile = GetColorProfileFromFile(fullPath);
        }
        else
        {
            //load default color profile
            currentColorProfile = GetDefaultColorProfile(_profile);
        }

        Debug.Log($"Loaded Colors\n" + ColorProfile.DebugColorProfile(currentColorProfile));

        UpdateAppColors();
    }
    void RevertColorProfile()
    {
        LoadAndSetColorProfile(currentColorProfile.name);
        UpdateAppColors();
    }

    #endregion Saving and Loading Color Profiles

    void SingletonSetup()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"Can't have more than one Color Controller. Destroying myself.", this);
            Destroy(gameObject);
        }
    }

    [Serializable]
    struct ColorButton
    {
        public ColorType colorType;
        public Button selectionButton;
        public Text label;
	}
}
