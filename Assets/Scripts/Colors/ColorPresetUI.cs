using System;
using System.Collections.Generic;
using PopUpWindows;
using UnityEngine;
using UnityEngine.UI;

namespace Colors
{
    public class ColorPresetUI : MonoBehaviour
    {
        [Header("Color Preset UI")]
        [SerializeField] GameObject _colorPresetPrefab;
        [SerializeField] Transform _builtInPresetParent;
        [SerializeField] Transform _userPresetParent;
        [SerializeField] Button _enablePresetWindowButton;
        [SerializeField] Button _closePresetWindowButton;
        [SerializeField] GameObject _presetWindow;
        [SerializeField] Button _savePresetButton;
        [SerializeField] ColorPresetSelectorSorter _userPresetSorter;
        [SerializeField] private BuiltInColorPresets _builtInColorPresets;

        [Header("Other UI References")] [SerializeField]
        ColorChangeUI _colorChangeUI;
        
        readonly List<ColorPresetSelector> _userPresetSelectors = new();
        
        // Start is called before the first frame update
        void Awake()
        {
            InitializePresetUI();
        }

        void Start()
        {
            InitializePresetUI();
        }
        
        void InitializePresetUI()
        {
            PopulatePresetSelectors();
            _enablePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(true); });
            _closePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(false); });
            _savePresetButton.onClick.AddListener(CreateSaveWindow);
        }

        internal void TogglePresetWindow(bool on)
        {
            _presetWindow.SetActive(on);
        }

        void PopulatePresetSelectors()
        {
            //populate built in preset selectors
            foreach (ColorProfileStruct c in _builtInColorPresets.ColorProfiles)
            {
                ColorProfile preset = c.ToReferenceType();
                AddPresetSelector(preset, true);
            }

            //populate user presets
            string[] presetNames = ColorPresetDataHandler.GetPresetNames();
            Array.Sort(presetNames);
            foreach(string presetName in presetNames)
            {
                ColorProfile preset = ColorPresetDataHandler.LoadPreset(presetName);
                AddPresetSelector(preset, false);
            }
        }

        void AddPresetSelector(ColorProfile preset, bool isBuiltIn = false)
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

        void PresetSelection(ColorProfile preset)
        {
            ColorController.SetColorsFromPreset(preset);
            _colorChangeUI.SetSlidersToCurrentColor();
        }

        ColorPresetSelector CreatePresetSelector(ColorProfile preset)
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
                PopUpController.Instance.ConfirmationWindow($"Are you sure you want to delete \"{selector.Preset.Name}\" color preset?",
                    confirm: () => ColorPresetDataHandler.DeletePreset(selector, onDeleted: () => 
                        RemoveUserPresetSelector(selector)), null, "Delete");
            }
        }
        void CreateSaveWindow()
        {
            PopUpController.Instance.TextInputWindow(inputLabel: "Enter Name:",
                confirm: (presetName) => ColorPresetDataHandler.SavePreset(presetName, ColorController.CurrentColorProfile, AddPresetSelectorAfterSave), 
                cancel: null,
                confirmButtonLabel: "Save");
        }


        void AddPresetSelectorAfterSave(ColorProfile preset)
        {
            AddPresetSelector(preset);
            _userPresetSorter.SortChildren(_userPresetSelectors);
        }
    }
}
