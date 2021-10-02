using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ColorProfile;

public class ColorController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Main Color UI")]
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
    #endregion Serialized Fields

    #region Profile Variables
    const string DEFAULT_COLOR_PROFILE = ControlsManager.DEFAULT_SAVE_NAME + " Colors";
    ColorType currentColorType = ColorType.Background;
    string fileExtensionProfiles = ".color";
    string profilesBasePath;
    ColorProfile currentColorProfile;
    ColorProfile CurrentColorProfile
    {
        get { return currentColorProfile; }
        set {
            currentColorProfile = value;
            Debug.Log($"Set Current Color Profile: {currentColorProfile.name}");
        }
    }

    #endregion Color Profile Variables

    #region Universal Variables
    public static ColorController instance;
    static List<ColorSetter> colorSetters = new List<ColorSetter>();
    const string colorsFolder = "Colors";
    #endregion Universal Variables

    #region Preset Variables
    string presetsBasePath;
    const string fileExtensionPresets = ".colorPreset";

    [Header("Color Preset UI")]
    [SerializeField] GameObject colorPresetPrefab;
    [SerializeField] Transform builtInPresetParent;
    [SerializeField] Transform userPresetParent;
    [SerializeField] Button enablePresetWindowButton;
    [SerializeField] Button closePresetWindowButton;
    [SerializeField] GameObject presetWindow;
    [SerializeField] Button deletePresetButton;
    [SerializeField] Button savePresetButton;
    [SerializeField] ColorPresetSelectorSorter userPresetSorter;

    [Header("Color Change Buttons")]
    [SerializeField] ColorButton[] colorTypeButtons;

    [Header("Built-in Themes")]
    [SerializeField] ColorPresetBuiltIn[] builtInPresets;

    List<ColorPresetSelector> builtInPresetSelectors = new List<ColorPresetSelector>();
    List<ColorPresetSelector> userPresetSelectors = new List<ColorPresetSelector>();

    string currentPresetSelection = "";
    #endregion Preset Variables


    #region Unity Methods

    private void Awake()
    {
        profilesBasePath = Path.Combine(Application.persistentDataPath, colorsFolder, "Profiles");
        presetsBasePath = Path.Combine(Application.persistentDataPath, colorsFolder, "Presets");
        CheckBasePath();

        SingletonSetup();
        InitializeUI();
        InitializePresetUI();

        foreach (ColorButton p in colorTypeButtons)
        {
            p.selectionButton.onClick.AddListener(ColorPreviewButtonPress);
        }

        controlMan.OnProfileLoaded.AddListener(LoadAndSetColorProfile);
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateAppColors();
    }

    #endregion Unity Methods

    #region Color Application

    public static void AddToControls(ColorSetter _setter)
    {
        colorSetters.Add(_setter);

        if (instance.CurrentColorProfile != null)
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
        foreach (ColorSetter c in colorSetters)
        {
            c.SetColors(CurrentColorProfile);
        }
    }

    void UpdateAppColors(ColorSetter _setter)
    {
        _setter.SetColors(CurrentColorProfile);
    }

    void UpdateColorProfile(ColorType _type, Color _color)
    {
        CurrentColorProfile.SetColor(_type, _color);
    }
    #endregion Color Application

    #region Color Profile UI

    [Serializable]
    struct ColorButton
    {
        public ColorType colorType;
        public Button selectionButton;
        public Text label;
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

        SetSlidersToCurrentColor();
    }

    void HighlightSelectedColorType(ColorType _colorType)
    {
        foreach (ColorButton butt in colorTypeButtons)
        {
            if (butt.colorType == _colorType)
            {
                butt.label.fontStyle = FontStyle.Bold;
            }
            else
            {
                butt.label.fontStyle = FontStyle.Normal;
            }
        }
    }

    void SetSlidersToCurrentColor()
    {
        SetSlidersToColor(CurrentColorProfile.GetColor(currentColorType));
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
        foreach (ColorButton p in colorTypeButtons)
        {
            if (p.colorType == _type)
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
    #endregion Color Profile UI

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
        return Mathf.RoundToInt(_float * 255);
    }
    #endregion Color Translation

    #region Saving and Loading Color Profiles

    void SaveDefaultProfile()
    {
        SaveProfile(DEFAULT_COLOR_PROFILE, true);
    }

    void SaveProfile()
    {
        SaveProfile(CurrentColorProfile.name);
    }

    void SaveProfile(string _name, bool _savingDefault = false)
    {
        if (!_savingDefault && _name == DEFAULT_COLOR_PROFILE)
        {
            Utilities.instance.ErrorWindow($"Can't save over the Default profile. If you'd like to set the default color palette that will be loaded on this and any new profile you create, click \"Set as Default Color Scheme\"");
            return;
        }

        string path = Path.Combine(profilesBasePath, _name + fileExtensionProfiles);
        string json = JsonUtility.ToJson(CurrentColorProfile, true);
        File.WriteAllText(path, json);

        if (_name != DEFAULT_COLOR_PROFILE)
        {
            Utilities.instance.ConfirmationWindow($"Saved color profile for {_name}!");
        }
        else
        {
            Utilities.instance.ConfirmationWindow($"Set default colors!");
        }


        Debug.Log($"Saved\n" + json);
    }

    ColorProfile GetDefaultColorProfile()
    {
        string path = Path.Combine(profilesBasePath, DEFAULT_COLOR_PROFILE + fileExtensionProfiles);

        if (File.Exists(path))
        {
            //load that file and set as current color profile
            return GetColorProfileFromFile(path);
        }
        else
        {
            return ColorProfile.NewDefaultColorProfile(DEFAULT_COLOR_PROFILE);
        }
    }

    ColorProfile GetColorProfileFromFile(string _path)
    {
        string json = File.ReadAllText(_path);
        return JsonUtility.FromJson<ColorProfile>(json);
    }

    void CheckBasePath()
    {
        if (!Directory.Exists(profilesBasePath))
        {
            Directory.CreateDirectory(profilesBasePath);
        }

        if (!Directory.Exists(presetsBasePath))
        {
            Directory.CreateDirectory(presetsBasePath);
        }
    }

    void LoadAndSetColorProfile(string _profile)
    {
        if (_profile == ControlsManager.DEFAULT_SAVE_NAME)
        {
            _profile = DEFAULT_COLOR_PROFILE;
        }

        string fullPath = Path.Combine(profilesBasePath, _profile + fileExtensionProfiles);

        if (File.Exists(fullPath))
        {
            //load that file and set as current color profile
            CurrentColorProfile = GetColorProfileFromFile(fullPath);
        }
        else
        {
            //load default color profile
            CurrentColorProfile = new ColorProfile(GetDefaultColorProfile(), _profile);
        }

        Debug.Log($"Loaded Colors\n" + ColorProfile.DebugColorProfile(CurrentColorProfile));

        UpdateAppColors();
        SetSlidersToCurrentColor();
    }
    void RevertColorProfile()
    {
        LoadAndSetColorProfile(CurrentColorProfile.name);
        UpdateAppColors();
    }

    #endregion Saving and Loading Color Profiles

    #region Saving and Loading Color Presets

    string[] GetPresetNames()
    {
        string[] fileNames = Directory.GetFiles(presetsBasePath, "*" + fileExtensionPresets);

        for (int i = 0; i < fileNames.Length; i++)
        {
            fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
        }

        return fileNames;
    }

    bool DoesPresetExist(string _name)
    {
        string[] fileNames = GetPresetNames();

        foreach (string s in fileNames)
        {
            if (s == _name)
            {
                return true;
            }
        }

        return false;
    }

    ColorPreset SelectPreset(string _name)
    {
        //check built-ins. if built-ins don't contain this name, then load from file
        foreach(ColorPresetBuiltIn c in builtInPresets)
        {
            if(c.name == _name)
            {
                return ColorPreset.BuiltInToPreset(c);
            }
        }

        return LoadPreset(_name);
    }

    ColorPreset LoadPreset(string _name)
    {
        string path = Path.Combine(presetsBasePath, _name + fileExtensionPresets);
        if (File.Exists(path))
        {
            //load
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ColorPreset>(json);
        }
        else
        {
            Debug.LogError($"No preset found named {_name} in {presetsBasePath}");
            return (ColorPreset)NewDefaultColorProfile(_name);
        }
    }

    void SavePreset(string _name)
    {
        if (DoesPresetExist(_name))
        {
            Utilities.instance.ErrorWindow("Preset with this name already exists, please use another.");
            return;
        }

        List<char> invalidChars = ControlsManager.GetInvalidFileNameCharacters(_name);
        if (invalidChars.Count > 0)
        {
            if (invalidChars.Count == 1)
            {
                Utilities.instance.ErrorWindow($"Chosen preset name contains an invalid character.");
            }
            else
            {
                Utilities.instance.ErrorWindow($"Chosen preset name contains {invalidChars.Count} invalid characters.");
            }

            return;
        }

        ColorPreset preset = ColorPreset.ProfileToPreset(CurrentColorProfile);
        preset.name = _name;
        string json = JsonUtility.ToJson(preset, true);
        string path = Path.Combine(presetsBasePath, preset.name + fileExtensionPresets);
        File.WriteAllText(path, json);
        Utilities.instance.ConfirmationWindow($"Saved preset {preset.name}");

        AddPresetSelectorAfterSave(preset);
    }

    void AddPresetSelectorAfterSave(ColorPreset preset)
    {
        AddPresetSelector(preset);
        userPresetSorter.SortChildren(userPresetSelectors);
        currentPresetSelection = preset.name;
    }

    void DeletePreset()
    {
        string presetName = currentPresetSelection;

        string path = Path.Combine(presetsBasePath, presetName + fileExtensionPresets);

        if (File.Exists(path))
        {
            File.Delete(path);
            RemoveUserPresetSelector(presetName);
            Utilities.instance.ConfirmationWindow($"{presetName} preset deleted!");
        }
        else
        {
            Debug.LogError($"No preset found to delete with name {presetName}");
            Utilities.instance.ErrorWindow($"Error deleting preset {presetName}");
        }
    }

    #endregion Saving and Loading Color Presets

    void InitializePresetUI()
    {
        PopulatePresetSelectors();
        enablePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(true); });
        closePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(false); });
        savePresetButton.onClick.AddListener(CreateSaveWindow);
        deletePresetButton.onClick.AddListener(CreateDeleteWindow);
    }

    void TogglePresetWindow(bool _on)
    {
        presetWindow.SetActive(_on);
    }

    void PopulatePresetSelectors()
    {
        //populate built in preset selectors
        builtInPresets.OrderBy(pre => pre.name);
        foreach (ColorPresetBuiltIn c in builtInPresets)
        {
            ColorPreset preset = ColorPreset.BuiltInToPreset(c);
            AddPresetSelector(preset, true);
        }

        //populate user presets
        string[] presetNames = GetPresetNames();
        Array.Sort(presetNames);
        foreach(string presetName in presetNames)
        {
            ColorPreset preset = LoadPreset(presetName);
            AddPresetSelector(preset, false);
        }
    }

    void AddPresetSelector(ColorPreset _preset, bool _isBuiltIn = false)
    {
        ColorPresetSelector selector = CreatePresetSelector(_preset);
        Transform parent = _isBuiltIn ? builtInPresetParent : userPresetParent;
        selector.transform.SetParent(parent, false);

        selector.Initialize(_preset, () => { PresetSelection(_preset); });

        if(_isBuiltIn)
        {
            builtInPresetSelectors.Add(selector);
        }
        else
        {
            userPresetSelectors.Add(selector);
        }
    }

    void RemoveUserPresetSelector(string _name)
    {
        ColorPresetSelector selectorToRemove = null;
        foreach(ColorPresetSelector c in userPresetSelectors)
        {
            if(c.Preset.name == _name)
            {
                selectorToRemove = c;
                break;
            }
        }

        if(selectorToRemove != null)
        {
            userPresetSelectors.Remove(selectorToRemove);
            Destroy(selectorToRemove.gameObject);
        }
        else
        {
            Debug.LogError($"No preset selector found for {_name}");
        }

        currentPresetSelection = "";
    }

    void PresetSelection(ColorPreset _preset)
    {
        SetColorsFromPreset(_preset);
        SetSlidersToCurrentColor();
        currentPresetSelection = _preset.name;
    }

    ColorPresetSelector CreatePresetSelector(ColorPreset _preset)
    {
        GameObject presetSelector = Instantiate(colorPresetPrefab);
        presetSelector.SetActive(true); //just in case the prefab is disabled accidentally
        presetSelector.name = $"{_preset.name} Color Preset Selector";
        ColorPresetSelector selector = presetSelector.GetComponent<ColorPresetSelector>();
        return selector;
    }

    void CreateDeleteWindow()
    {
        if (string.IsNullOrEmpty(currentPresetSelection))
        {
            Utilities.instance.ErrorWindow($"No preset selected!");
        }
        else if (PresetIsBuiltIn(currentPresetSelection))
        {
            Utilities.instance.ErrorWindow($"Can't delete a built-in preset!");
        }
        else
        {
            Utilities.instance.VerificationWindow($"Are you sure you want to delete {currentPresetSelection} color preset?", DeletePreset, null, "Delete");
        }
    }

    bool PresetIsBuiltIn(string _preset)
    {
        foreach(ColorPresetBuiltIn c in builtInPresets)
        {
            if(c.name == _preset)
            {
                return true;
            }
        }

        return false;
    }

    void CreateSaveWindow()
    {
        Utilities.instance.VerificationWindow("Enter Name:", SavePreset, null, "Save");
    }

    void SetColorsFromPreset(ColorPreset _preset)
    {
        foreach (ColorType t in (ColorType[])Enum.GetValues(typeof(ColorType)))
        {
            CurrentColorProfile.SetColor(t, _preset.GetColor(t));
        }

        UpdateAppColors();
    }

    public Color GetColorFromProfile(ColorType _type)
    {
        return CurrentColorProfile.GetColor(_type);
    }


    #region Singleton
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
    #endregion Singleton

}
