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
        [SerializeField]
        private GameObject _colorPresetPrefab;
        [SerializeField] private Transform _builtInPresetParent;
        [SerializeField] private Transform _userPresetParent;
        [SerializeField] private Button _enablePresetWindowButton;
        [SerializeField] private Button _closePresetWindowButton;
        [SerializeField] private GameObject _presetWindow;
        [SerializeField] private Button _savePresetButton;
        [SerializeField] private ColorPresetSelectorSorter _userPresetSorter;
        [SerializeField] private BuiltInColorPresets _builtInColorPresets;

        [Header("Other UI References")] [SerializeField]
        private ColorChangeUI _colorChangeUI;

        private readonly List<ColorPresetSelector> _userPresetSelectors = new();
        
        // Start is called before the first frame update
        private void Awake()
        {
            InitializePresetUI();
        }

        private void Start()
        {
            InitializePresetUI();
        }

        private void InitializePresetUI()
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

        private void PopulatePresetSelectors()
        {
            //populate built in preset selectors
            foreach (var c in _builtInColorPresets.ColorProfiles)
            {
                var preset = c.ToReferenceType();
                AddPresetSelector(preset, true);
            }

            //populate user presets
            var presetNames = ColorPresetDataHandler.GetPresetNames();
            Array.Sort(presetNames);
            foreach(var presetName in presetNames)
            {
                var preset = ColorPresetDataHandler.LoadPreset(presetName);
                AddPresetSelector(preset, false);
            }
        }

        private void AddPresetSelector(ColorProfile preset, bool isBuiltIn = false)
        {
            var selector = CreatePresetSelector(preset);
            var parent = isBuiltIn ? _builtInPresetParent : _userPresetParent;
            selector.transform.SetParent(parent, false);

            selector.Initialize(preset, () => PresetSelection(preset), () => CreateDeleteWindow(selector), isBuiltIn);

            if(!isBuiltIn)
            {
                _userPresetSelectors.Add(selector);
            }
        }

        private void RemoveUserPresetSelector(ColorPresetSelector selectorToRemove)
        {
            _userPresetSelectors.Remove(selectorToRemove);
            Destroy(selectorToRemove.gameObject);
        }

        private void PresetSelection(ColorProfile preset)
        {
            ColorController.SetColorsFromPreset(preset);
            _colorChangeUI.SetSlidersToCurrentColor();
        }

        private ColorPresetSelector CreatePresetSelector(ColorProfile preset)
        {
            var presetSelector = Instantiate(_colorPresetPrefab);
            presetSelector.SetActive(true); //just in case the prefab is disabled accidentally
            presetSelector.name = $"{preset.Name} Color Preset Selector";
            var selector = presetSelector.GetComponent<ColorPresetSelector>();
            return selector;
        }

        private void CreateDeleteWindow(ColorPresetSelector selector)
        {
            if (!selector.IsBuiltIn)
            {
                PopUpController.Instance.ConfirmationWindow($"Are you sure you want to delete \"{selector.Preset.Name}\" color preset?",
                    confirm: () => ColorPresetDataHandler.DeletePreset(selector, onDeleted: () => 
                        RemoveUserPresetSelector(selector)), null, "Delete");
            }
        }

        private void CreateSaveWindow()
        {
            PopUpController.Instance.TextInputWindow(inputLabel: "Enter Name:",
                confirm: (presetName) => ColorPresetDataHandler.SavePreset(presetName, ColorController.CurrentColorProfile, AddPresetSelectorAfterSave), 
                cancel: null,
                confirmButtonLabel: "Save");
        }


        private void AddPresetSelectorAfterSave(ColorProfile preset)
        {
            AddPresetSelector(preset);
            _userPresetSorter.SortChildren(_userPresetSelectors);
        }
    }
}
