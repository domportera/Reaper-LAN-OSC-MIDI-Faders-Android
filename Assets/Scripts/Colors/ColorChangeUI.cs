using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PopUpWindows;

namespace Colors
{
    public class ColorChangeUI : MonoBehaviour
    {
        [SerializeField] ColorPresetUI _presetUI;
        [SerializeField] BuiltInColorPresets _builtInColorPresets;
        
        [SerializeField] Slider _hueSlider;
        [SerializeField] Slider _saturationSlider;
        [SerializeField] Slider _valueSlider;
 
        [SerializeField] GameObject _panel;
        [SerializeField] Button _openButton;
        [SerializeField] Button _closeButton;
 
        [SerializeField] Button _saveButton;
        [SerializeField] Button _revertButton;
        [SerializeField] Button _setAsDefaultButton;

        [Header("Color Change Buttons")]
        [SerializeField] ColorButton[] _colorTypeButtons;

        Dictionary<ColorType, ColorButton> _colorTypeButtonDict;
        Dictionary<Button, ColorButton> _colorPreviewButtonDict;
        ColorType _currentColorType = ColorType.Background;
        
        [Serializable]
        struct ColorButton
        {
            public ColorType ColorType;
            public Button SelectionButton;
            public Text Label;
        }

        void Awake()
        {
            _colorTypeButtonDict = _colorTypeButtons.ToDictionary(x => x.ColorType, x => x);
            _colorPreviewButtonDict = _colorTypeButtons.ToDictionary(x => x.SelectionButton, x => x);
            
            InitializeUI();

            foreach (ColorButton p in _colorTypeButtons)
            {
                p.SelectionButton.onClick.AddListener(OnColorPreviewButtonPress);
            }
            
            ColorController.ColorsLoaded += SetSlidersToCurrentColor;
        }

        // Start is called before the first frame update
        void Start()
        {
            ColorController.UpdateAppColors();
        }

        void InitializeUI()
        {
            _hueSlider.onValueChanged.AddListener((_) => OnSliderChange());
            _saturationSlider.onValueChanged.AddListener((_) => OnSliderChange());
            _valueSlider.onValueChanged.AddListener((_) => OnSliderChange());

            _openButton.onClick.AddListener(ChooseColorWindow);
            _closeButton.onClick.AddListener(() => { ToggleColorControlWindow(false); });

            _setAsDefaultButton.onClick.AddListener(ColorController.SaveDefaultProfile);
            _revertButton.onClick.AddListener(ColorController.RevertColorProfile);
            _saveButton.onClick.AddListener(ColorController.SaveProfile);

            HighlightSelectedColorType(_currentColorType);
        }

        void ChooseColorWindow()
        {
            MultiOptionAction customOption = new MultiOptionAction("Color Editing", () => { ToggleColorControlWindow(true); });
            MultiOptionAction presetOption = new MultiOptionAction("Preset Selection", () => { _presetUI.TogglePresetWindow(true); });
            PopUpController.Instance.MultiOptionWindow("How would you like to select your colors?", customOption, presetOption);
        }

        void ToggleColorControlWindow(bool active)
        {
            _panel.SetActive(active);
        }

        void OnColorPreviewButtonPress()
        {
            Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            ColorButton preview = _colorPreviewButtonDict[button];

            _currentColorType = preview.ColorType;
            HighlightSelectedColorType(preview.ColorType);
            SetSlidersToCurrentColor();
        }

        void HighlightSelectedColorType(ColorType colorType)
        {
            foreach (ColorButton butt in _colorTypeButtons)
            {
                butt.Label.fontStyle = butt.ColorType != colorType ? FontStyle.Normal : FontStyle.Bold;
            }
        }

        internal void SetSlidersToCurrentColor()
        {
            SetSlidersToColor(ColorController.CurrentColorProfile.GetColor(_currentColorType));
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
        
        void OnSliderChange()
        {
            ColorButton preview = _colorTypeButtonDict[_currentColorType];
            ColorController.UpdateColorProfile(preview.ColorType, GetColorFromSliders());
            ColorController.UpdateAppColors();
        }

        Color GetColorFromSliders()
        {
            float hue = ColorIntToFloat((int)_hueSlider.value);
            float sat = ColorIntToFloat((int)_saturationSlider.value);
            float val = ColorIntToFloat((int)_valueSlider.value);
            return Color.HSVToRGB(hue, sat, val);
        }
        
        static float ColorIntToFloat(int val)
        {
            return val / 255f;
        }

        static int ColorFloatToInt(float val)
        {
            return Mathf.RoundToInt(val * 255);
        }
    }
}
