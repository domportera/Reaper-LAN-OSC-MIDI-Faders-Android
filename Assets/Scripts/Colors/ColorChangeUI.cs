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
        [SerializeField] private ColorPresetUI _presetUI;
        [SerializeField] private BuiltInColorPresets _builtInColorPresets;
        
        [SerializeField] private Slider _hueSlider;
        [SerializeField] private Slider _saturationSlider;
        [SerializeField] private Slider _valueSlider;
 
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
 
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _revertButton;
        [SerializeField] private Button _setAsDefaultButton;

        [Header("Color Change Buttons")]
        [SerializeField]
        private ColorButton[] _colorTypeButtons;

        private Dictionary<ColorType, ColorButton> _colorTypeButtonDict;
        private Dictionary<Button, ColorButton> _colorPreviewButtonDict;
        private ColorType _currentColorType = ColorType.Background;
        
        [Serializable]
        private struct ColorButton
        {
            public ColorType ColorType;
            public Button SelectionButton;
            public Text Label;
        }

        private void Awake()
        {
            _colorTypeButtonDict = _colorTypeButtons.ToDictionary(x => x.ColorType, x => x);
            _colorPreviewButtonDict = _colorTypeButtons.ToDictionary(x => x.SelectionButton, x => x);
            
            InitializeUI();

            foreach (var p in _colorTypeButtons)
            {
                p.SelectionButton.onClick.AddListener(OnColorPreviewButtonPress);
            }
            
            ColorController.ColorsLoaded += SetSlidersToCurrentColor;
        }

        // Start is called before the first frame update
        private void Start()
        {
            ColorController.UpdateAppColors();
        }

        private void InitializeUI()
        {
            _hueSlider.onValueChanged.AddListener((_) => OnSliderChange());
            _saturationSlider.onValueChanged.AddListener((_) => OnSliderChange());
            _valueSlider.onValueChanged.AddListener((_) => OnSliderChange());
            
            _hueSlider.minValue = 0;
            _saturationSlider.minValue = 0;
            _valueSlider.minValue = 0;
            
            _hueSlider.maxValue = 255;
            _saturationSlider.maxValue = 255;
            _valueSlider.maxValue = 255;
            
            _hueSlider.wholeNumbers = true;
            _saturationSlider.wholeNumbers = true;
            _valueSlider.wholeNumbers = true;

            _openButton.onClick.AddListener(ChooseColorWindow);
            _closeButton.onClick.AddListener(() => { ToggleColorControlWindow(false); });

            _setAsDefaultButton.onClick.AddListener(ColorController.SaveDefaultProfile);
            _revertButton.onClick.AddListener(ColorController.RevertColorProfile);
            _saveButton.onClick.AddListener(ColorController.SaveProfile);

            HighlightSelectedColorType(_currentColorType);
        }

        private void ChooseColorWindow()
        {
            var customOption = new MultiOptionAction("Color Editing", () => { ToggleColorControlWindow(true); });
            var presetOption = new MultiOptionAction("Preset Selection", () => { _presetUI.TogglePresetWindow(true); });
            PopUpController.Instance.MultiOptionWindow("How would you like to select your colors?", customOption, presetOption);
        }

        private void ToggleColorControlWindow(bool active)
        {
            _panel.SetActive(active);
        }

        private void OnColorPreviewButtonPress()
        {
            var button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            var preview = _colorPreviewButtonDict[button];

            _currentColorType = preview.ColorType;
            HighlightSelectedColorType(preview.ColorType);
            SetSlidersToCurrentColor();
        }

        private void HighlightSelectedColorType(ColorType colorType)
        {
            foreach (var butt in _colorTypeButtons)
            {
                butt.Label.fontStyle = butt.ColorType != colorType ? FontStyle.Normal : FontStyle.Bold;
            }
        }

        internal void SetSlidersToCurrentColor()
        {
            SetSlidersToColor(ColorController.CurrentColorProfile.GetColor(_currentColorType));
        }

        private void SetSlidersToColor(Color color)
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

        private void OnSliderChange()
        {
            var preview = _colorTypeButtonDict[_currentColorType];
            ColorController.UpdateColorProfile(preview.ColorType, GetColorFromSliders());
            ColorController.UpdateAppColors();
        }

        private Color GetColorFromSliders()
        {
            var hue = ColorIntToFloat((int)_hueSlider.value);
            var sat = ColorIntToFloat((int)_saturationSlider.value);
            var val = ColorIntToFloat((int)_valueSlider.value);
            return Color.HSVToRGB(hue, sat, val);
        }

        private static float ColorIntToFloat(int val)
        {
            return val / 255f;
        }

        private static int ColorFloatToInt(float val)
        {
            return Mathf.RoundToInt(val * 255);
        }
    }
}
