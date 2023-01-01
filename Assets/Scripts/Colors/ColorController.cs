using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DomsUnityHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using PopUpWindows;
using static Colors.ColorProfile;

namespace Colors
{
    public class ColorController : MonoBehaviourExtended
    {
        #region Serialized Fields
        [FormerlySerializedAs("controlMan")]
        [Header("Main Color UI")]
        [SerializeField] ControlsManager _controlMan;
        [FormerlySerializedAs("hueSlider")] [SerializeField] Slider _hueSlider;
        [FormerlySerializedAs("saturationSlider")] [SerializeField] Slider _saturationSlider;
        [FormerlySerializedAs("valueSlider")] [SerializeField] Slider _valueSlider;

        [FormerlySerializedAs("panel")] [SerializeField] GameObject _panel;
        [FormerlySerializedAs("openButton")] [SerializeField] Button _openButton;
        [FormerlySerializedAs("closeButton")] [SerializeField] Button _closeButton;

        [FormerlySerializedAs("saveButton")] [SerializeField] Button _saveButton;
        [FormerlySerializedAs("revertButton")] [SerializeField] Button _revertButton;
        [FormerlySerializedAs("setAsDefaultButton")] [SerializeField] Button _setAsDefaultButton;
        #endregion Serialized Fields

        #region Profile Variables
        const string DefaultColorProfile = ProfilesManager.DefaultSaveName + " Colors";
        ColorType _currentColorType = ColorType.Background;
        const string FileExtensionProfiles = ".color";
        string _profilesBasePath;
        ColorProfile _currentColorProfile;

        ColorProfile CurrentColorProfile
        {
            get => _currentColorProfile;
            set {
                _currentColorProfile = value;
                Log($"Set Current Color Profile: {_currentColorProfile.Name}", this);
            }
        }

        #endregion Color Profile Variables

        #region Universal Variables
        public static ColorController Instance;
        static readonly List<ColorSetter> ColorSetters = new();
        const string ColorsFolder = "Colors";
        #endregion Universal Variables

        #region Preset Variables
        string _presetsBasePath;
        const string FileExtensionPresets = ".colorPreset";

        [FormerlySerializedAs("colorPresetPrefab")]
        [Header("Color Preset UI")]
        [SerializeField] GameObject _colorPresetPrefab;
        [FormerlySerializedAs("builtInPresetParent")] [SerializeField] Transform _builtInPresetParent;
        [FormerlySerializedAs("userPresetParent")] [SerializeField] Transform _userPresetParent;
        [FormerlySerializedAs("enablePresetWindowButton")] [SerializeField] Button _enablePresetWindowButton;
        [FormerlySerializedAs("closePresetWindowButton")] [SerializeField] Button _closePresetWindowButton;
        [FormerlySerializedAs("presetWindow")] [SerializeField] GameObject _presetWindow;
        [FormerlySerializedAs("savePresetButton")] [SerializeField] Button _savePresetButton;
        [FormerlySerializedAs("userPresetSorter")] [SerializeField] ColorPresetSelectorSorter _userPresetSorter;

        [FormerlySerializedAs("colorTypeButtons")]
        [Header("Color Change Buttons")]
        [SerializeField] ColorButton[] _colorTypeButtons;

        [FormerlySerializedAs("previewCurrentColorsAsBuiltIn")]
        [Header("Built-in Themes")]
        [Tooltip("This is just used to display the current colors (whatever they are) in the inspector for creating built-in colors")]
        [SerializeField] bool _previewCurrentColorsAsBuiltIn = false;
        [FormerlySerializedAs("inspectorPresetDisplay")] [SerializeField] ColorPresetBuiltIn _inspectorPresetDisplay = new ColorPresetBuiltIn();

        [FormerlySerializedAs("builtInPresets")]
        [Space(10)]
        [Tooltip("The first preset will be the default")]
        [SerializeField] ColorPresetBuiltIn[] _builtInPresets;

        readonly List<ColorPresetSelector> _defaultBuiltInPreset = new();
        readonly List<ColorPresetSelector> _userPresetSelectors = new();
        const uint DefaultBuiltInIndex = 0;

        #endregion Preset Variables

        #region Unity Methods

        void Awake()
        {
            _profilesBasePath = Path.Combine(Application.persistentDataPath, ColorsFolder, "Profiles");
            _presetsBasePath = Path.Combine(Application.persistentDataPath, ColorsFolder, "Presets");

            SingletonSetup();
            InitializeUI();
            InitializePresetUI();

            foreach (ColorButton p in _colorTypeButtons)
            {
                p.SelectionButton.onClick.AddListener(ColorPreviewButtonPress);
            }

            _controlMan.OnProfileLoaded.AddListener(LoadAndSetColorProfile);
        }

        // Start is called before the first frame update
        void Start()
        {
            UpdateAppColors();
        }

        #endregion Unity Methods

        #region Color Application

        public static void AddToControls(ColorSetter setter)
        {
            ColorSetters.Add(setter);

            if (Instance.CurrentColorProfile != null)
            {
                Instance.UpdateAppColors(setter);
            }
        }

        public static void RemoveFromControls(ColorSetter setter)
        {
            ColorSetters.Remove(setter);
        }

        void UpdateAppColors()
        {
            foreach (ColorSetter c in ColorSetters)
            {
                try
                {
                    c.SetColors(CurrentColorProfile);
                }
                catch (Exception e)
                {
                    GameObject obj = c.gameObject;
                    Debug.LogError($"Error setting colors of {obj.name}\n{e}", obj);
                }
            }

            if (_previewCurrentColorsAsBuiltIn)
            {
                UpdateInspectorPresetPreview();
            }
        }

        void UpdateAppColors(ColorSetter setter)
        {
            setter.SetColors(CurrentColorProfile);
        }

        void UpdateColorProfile(ColorType type, Color color)
        {
            CurrentColorProfile.SetColor(type, color);
        }

        void UpdateInspectorPresetPreview()
        {
            _inspectorPresetDisplay.Background = CurrentColorProfile.GetColor(ColorType.Background);
            _inspectorPresetDisplay.Primary = CurrentColorProfile.GetColor(ColorType.Primary);
            _inspectorPresetDisplay.Secondary = CurrentColorProfile.GetColor(ColorType.Secondary);
            _inspectorPresetDisplay.Tertiary = CurrentColorProfile.GetColor(ColorType.Tertiary);
            _inspectorPresetDisplay.Name = CurrentColorProfile.Name;
        }
        #endregion Color Application

        #region Color Profile UI

        [Serializable]
        struct ColorButton
        {
            [FormerlySerializedAs("colorType")] public ColorType ColorType;
            [FormerlySerializedAs("selectionButton")] public Button SelectionButton;
            [FormerlySerializedAs("label")] public Text Label;
        }

        void ChooseColorWindow()
        {
            MultiOptionAction customOption = new MultiOptionAction("Color Editing", () => { ToggleColorControlWindow(true); });
            MultiOptionAction presetOption = new MultiOptionAction("Preset Selection", () => { TogglePresetWindow(true); });
            PopUpController.Instance.MultiOptionWindow("How would you like to select your colors?", customOption, presetOption);
        }

        void ToggleColorControlWindow(bool active)
        {
            _panel.SetActive(active);
        }

        void SliderChange(float _val)
        {
            ColorButton preview = GetPreviewFromColorType(_currentColorType);
            UpdateColorProfile(preview.ColorType, GetColorFromSliders());
            UpdateAppColors();
        }

        Color GetColorFromSliders()
        {
            float hue = ColorIntToFloat((int)_hueSlider.value);
            float sat = ColorIntToFloat((int)_saturationSlider.value);
            float val = ColorIntToFloat((int)_valueSlider.value);
            return Color.HSVToRGB(hue, sat, val);
        }

        void ColorPreviewButtonPress()
        {
            Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            ColorButton preview = GetPreviewFromButton(button);

            _currentColorType = preview.ColorType;
            HighlightSelectedColorType(preview.ColorType);

            SetSlidersToCurrentColor();
        }

        void HighlightSelectedColorType(ColorType colorType)
        {
            foreach (ColorButton butt in _colorTypeButtons)
            {
                if (butt.ColorType != colorType)
                {
                    butt.Label.fontStyle = FontStyle.Normal;
                }
                else
                {
                    butt.Label.fontStyle = FontStyle.Bold;
                }
            }
        }

        void SetSlidersToCurrentColor()
        {
            SetSlidersToColor(CurrentColorProfile.GetColor(_currentColorType));
        }

        void SetSlidersToColor(Color color)
        {
            Vector3 hsv;
            Color.RGBToHSV(color, out hsv.x, out hsv.y, out hsv.z);

            hsv.x = ColorFloatToInt(hsv.x);
            hsv.y = ColorFloatToInt(hsv.y);
            hsv.z = ColorFloatToInt(hsv.z);

            _hueSlider.SetValueWithoutNotify(hsv.x);
            _saturationSlider.SetValueWithoutNotify(hsv.y);
            _valueSlider.SetValueWithoutNotify(hsv.z);
        }

        ColorButton GetPreviewFromColorType(ColorType type)
        {
            foreach (ColorButton p in _colorTypeButtons)
            {
                if (p.ColorType == type)
                {
                    return p;
                }
            }

            Debug.LogError($"No color preview found for {type}!");
            return _colorTypeButtons[0];
        }

        ColorButton GetPreviewFromButton(Button button)
        {
            foreach (ColorButton p in _colorTypeButtons)
            {
                if (p.SelectionButton == button)
                {
                    return p;
                }
            }

            Debug.LogError($"No button found for {button.name}!");
            return _colorTypeButtons[0];
        }

        void InitializeUI()
        {
            _hueSlider.onValueChanged.AddListener(SliderChange);
            _saturationSlider.onValueChanged.AddListener(SliderChange);
            _valueSlider.onValueChanged.AddListener(SliderChange);

            _openButton.onClick.AddListener(ChooseColorWindow);
            _closeButton.onClick.AddListener(() => { ToggleColorControlWindow(false); });

            _setAsDefaultButton.onClick.AddListener(SaveDefaultProfile);
            _revertButton.onClick.AddListener(RevertColorProfile);
            _saveButton.onClick.AddListener(SaveProfile);

            HighlightSelectedColorType(_currentColorType);
        }
        #endregion Color Profile UI

        #region Color Translation
        float ColorIntToFloat(int val)
        {
            return val / 255f;
        }

        int ColorFloatToInt(float val)
        {
            return Mathf.RoundToInt(val * 255);
        }
        #endregion Color Translation

        #region Saving and Loading Color Profiles

        void SaveDefaultProfile()
        {
            ColorProfile defaultColorProfile = new ColorProfile(CurrentColorProfile, DefaultColorProfile);
            SaveProfile(defaultColorProfile, true);
        }

        void SaveProfile()
        {
            SaveProfile(CurrentColorProfile);
        }

        public void SaveColorProfileByName(string profileName)
        {
            ColorProfile profile = new ColorProfile(CurrentColorProfile, profileName);
            SaveProfile(profile);
        }

        void SaveProfile(ColorProfile colorProfile, bool savingDefault = false)
        {
            if (!savingDefault && colorProfile.Name == DefaultColorProfile)
            {
                PopUpController.Instance.ErrorWindow($"Can't save over the Default profile. If you'd like to set the default color palette that will be loaded on this and any new profile you create, click \"Set as Default Color Scheme\"");
                return;
            }

            bool saved = FileHandler.SaveJsonObject(colorProfile, _profilesBasePath, colorProfile.Name, FileExtensionProfiles);

            if (saved)
            {
                if (colorProfile.Name != DefaultColorProfile)
                {
                    PopUpController.Instance.QuickNoticeWindow($"Saved color profile for {colorProfile.Name}!");
                }
                else
                {
                    PopUpController.Instance.QuickNoticeWindow($"Set default colors!");
                }
            }
            else
            {
                PopUpController.Instance.ErrorWindow($"Error saving colors for profile {colorProfile.Name}. Check Log for details.");
            }
        }

        ColorProfile GetDefaultColorProfile()
        {
            ColorProfile defaultProfile = FileHandler.LoadJsonObject<ColorProfile>(_profilesBasePath, DefaultColorProfile, FileExtensionProfiles);

            if (defaultProfile != null)
            {
                //load that file and set as current color profile
                return defaultProfile;
            }
            else
            {
                return ColorPresetBuiltIn.BuiltInPresetToProfile(_builtInPresets[DefaultBuiltInIndex]);
            }
        }

        void LoadAndSetColorProfile(string profile)
        {
            if (profile == ProfilesManager.DefaultSaveName)
            {
                profile = DefaultColorProfile;
            }

            ColorProfile colorProfile = FileHandler.LoadJsonObject<ColorProfile>(_profilesBasePath, profile, FileExtensionProfiles);

            //load that file and set as current color profile. If null, load default
            CurrentColorProfile = colorProfile ?? new ColorProfile(GetDefaultColorProfile(), profile);

            Log($"Loaded Colors: {CurrentColorProfile.Name}\n" + DebugColorProfile(CurrentColorProfile), this);

            UpdateAppColors();
            SetSlidersToCurrentColor();
        }

        void RevertColorProfile()
        {
            LoadAndSetColorProfile(CurrentColorProfile.Name);
            UpdateAppColors();
        }

        #endregion Saving and Loading Color Profiles

        #region Saving and Loading Color Presets

        string[] GetPresetNames()
        {
            if(!Directory.Exists(_presetsBasePath))
            {
                return Array.Empty<string>();
            }

            string[] fileNames = Directory.GetFiles(_presetsBasePath, "*" + FileExtensionPresets);

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
            }

            return fileNames;
        }

        bool DoesPresetExist(string presetName)
        {
            string[] fileNames = GetPresetNames();

            foreach (string s in fileNames)
            {
                if (s == presetName)
                {
                    return true;
                }
            }

            return false;
        }

        ColorPreset LoadPreset(string presetName)
        {
            ColorPreset preset = FileHandler.LoadJsonObject<ColorPreset>(_presetsBasePath, presetName, FileExtensionPresets);

            if (preset != null)
            {
                return preset;
            }
            else
            {
                Debug.LogError($"No preset found named {presetName} in {_presetsBasePath}");
                return (ColorPreset)NewDefaultColorProfile(presetName);
            }
        }

        void SavePreset(string presetName)
        {
            if (DoesPresetExist(presetName))
            {
                PopUpController.Instance.ErrorWindow("Preset with this name already exists, please use another.");
                return;
            }

            List<char> invalidChars = FileHandler.GetInvalidFileNameCharacters(presetName);
            if (invalidChars.Count > 0)
            {
                PopUpController.Instance.ErrorWindow(invalidChars.Count == 1
                    ? $"Chosen preset name contains an invalid character."
                    : $"Chosen preset name contains {invalidChars.Count} invalid characters.");

                return;
            }

            ColorPreset preset = ColorPreset.ProfileToPreset(CurrentColorProfile, presetName);
            bool saved = FileHandler.SaveJsonObject(preset, _presetsBasePath, preset.Name, FileExtensionPresets);

            if (saved)
            {
                PopUpController.Instance.QuickNoticeWindow($"Saved preset {preset.Name}");
            }
            else
            {
                PopUpController.Instance.ErrorWindow($"Error saving preset {preset.Name}. Check the Log for details.");
            }

            AddPresetSelectorAfterSave(preset);
        }

        void AddPresetSelectorAfterSave(ColorPreset preset)
        {
            AddPresetSelector(preset);
            _userPresetSorter.SortChildren(_userPresetSelectors);
        }

        void DeletePreset(ColorPresetSelector selector)
        {
            string presetName = selector.Preset.Name;

            bool deleted = FileHandler.DeleteFile(_presetsBasePath, presetName, FileExtensionPresets);

            if (deleted)
            {
                RemoveUserPresetSelector(selector);
                PopUpController.Instance.QuickNoticeWindow($"{presetName} preset deleted!");
            }
            else
            {
                Debug.LogError($"No preset found to delete with name {presetName}");
                PopUpController.Instance.ErrorWindow($"Error deleting preset {presetName}. Check the Log for details.");
            }
        }

        #endregion Saving and Loading Color Presets

        #region Color Presets
        void InitializePresetUI()
        {
            PopulatePresetSelectors();
            _enablePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(true); });
            _closePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(false); });
            _savePresetButton.onClick.AddListener(CreateSaveWindow);
        }

        void TogglePresetWindow(bool on)
        {
            _presetWindow.SetActive(on);
        }

        void PopulatePresetSelectors()
        {
            //populate built in preset selectors
            _builtInPresets.OrderBy(pre => pre.Name);
            foreach (ColorPresetBuiltIn c in _builtInPresets)
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

        void AddPresetSelector(ColorPreset preset, bool isBuiltIn = false)
        {
            ColorPresetSelector selector = CreatePresetSelector(preset);
            Transform parent = isBuiltIn ? _builtInPresetParent : _userPresetParent;
            selector.transform.SetParent(parent, false);

            selector.Initialize(preset, () => PresetSelection(preset), () => CreateDeleteWindow(selector), isBuiltIn);

            if(!isBuiltIn)
            {
                _userPresetSelectors.Add(selector);
            }
        }

        void RemoveUserPresetSelector(ColorPresetSelector selectorToRemove)
        {
            _userPresetSelectors.Remove(selectorToRemove);
            Destroy(selectorToRemove.gameObject);
        }

        void PresetSelection(ColorPreset preset)
        {
            SetColorsFromPreset(preset);
            SetSlidersToCurrentColor();
        }

        ColorPresetSelector CreatePresetSelector(ColorPreset preset)
        {
            GameObject presetSelector = Instantiate(_colorPresetPrefab);
            presetSelector.SetActive(true); //just in case the prefab is disabled accidentally
            presetSelector.name = $"{preset.Name} Color Preset Selector";
            ColorPresetSelector selector = presetSelector.GetComponent<ColorPresetSelector>();
            return selector;
        }

        void CreateDeleteWindow(ColorPresetSelector selector)
        {
            if (!selector.IsBuiltIn)
            {
                PopUpController.Instance.ConfirmationWindow($"Are you sure you want to delete \"{selector.Preset.Name}\" color preset?", () => DeletePreset(selector), null, "Delete");
            }
        }
        void CreateSaveWindow()
        {
            PopUpController.Instance.TextInputWindow("Enter Name:", SavePreset, null, "Save");
        }

        void SetColorsFromPreset(ColorPreset preset)
        {
            foreach (ColorType t in (ColorType[])Enum.GetValues(typeof(ColorType)))
            {
                CurrentColorProfile.SetColor(t, preset.GetColor(t));
            }

            UpdateAppColors();
        }

        #endregion Color Presets

        #region Utility
        public bool CurrentColorProfileIsDefault()
        {
            return ColorProfile.Equals(CurrentColorProfile, GetDefaultColorProfile());
        }


        #region Singleton
        void SingletonSetup()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError($"Can't have more than one Color Controller. Destroying myself.", this);
                Destroy(gameObject);
            }
        }
        #endregion Singleton

        #endregion Utility

    }
}
