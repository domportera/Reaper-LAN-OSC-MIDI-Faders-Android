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
        [SerializeField] private GameObject _colorPresetPrefab;
        [SerializeField] private GridLayoutGroup _presetParent;
        [SerializeField] private Button _enablePresetWindowButton;
        [SerializeField] private Button _closePresetWindowButton;
        [SerializeField] private GameObject _presetWindow;
        [SerializeField] private Button _savePresetButton;
        [SerializeField] private ColorPresetSelectorSorter _userPresetSorter;

        private readonly List<ColorPresetSelector> _userPresetSelectors = new();

        // Start is called before the first frame update
        private void Awake()
        {
            InitializePresetUI();
        }

        private void InitializePresetUI()
        {
            PopulatePresetSelectors();
            _enablePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(true); });
            _closePresetWindowButton.onClick.AddListener(() => { TogglePresetWindow(false); });
            _savePresetButton.onClick.AddListener(CreateSaveWindow);
            
            var presetPrefabSize = _colorPresetPrefab.GetComponent<RectTransform>().rect.size;
            _presetParent.cellSize = presetPrefabSize;
        }

        internal void TogglePresetWindow(bool on)
        {
            _presetWindow.SetActive(on);
        }

        private void PopulatePresetSelectors()
        {
            //populate built in preset selectors
            foreach (var c in ColorProfileDataHandler.BuiltInColorPresets)
            {
                AddPresetSelector(c, true);
            }

            //populate user presets
            var presetNames = ColorProfileDataHandler.GetPresetNames();
            Array.Sort(presetNames);
            foreach(var presetName in presetNames)
            {
                var preset = ColorProfileDataHandler.LoadPreset(presetName);
                AddPresetSelector(preset, false);
            }
        }

        private void AddPresetSelector(ColorProfile preset, bool isBuiltIn)
        {
            var selector = CreatePresetSelector(preset);
            var parent = _presetParent;
            selector.transform.SetParent(parent.transform, false);

            selector.Initialize(preset, () => ColorController.SetColorsFromPreset(preset), () => CreateDeleteWindow(selector), isBuiltIn);

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
                    confirm: () => ColorProfileDataHandler.DeletePreset(selector.Preset, onDeleted: () => 
                        RemoveUserPresetSelector(selector)), null, "Delete");
            }
            else
            {
                PopUpController.Instance.ErrorWindow($"Can't delete built in color preset.");
            }
        }

        private void CreateSaveWindow()
        {
            PopUpController.Instance.TextInputWindow(inputLabel: "Enter Name:",
                confirm: (presetName) => ColorProfileDataHandler.SavePreset(presetName, ColorController.CurrentColorProfile, AddPresetSelectorAfterSave), 
                cancel: null,
                confirmButtonLabel: "Save");
        }


        private void AddPresetSelectorAfterSave(ColorProfile preset)
        {
            AddPresetSelector(preset, false);
            _userPresetSorter.SortChildren(_userPresetSelectors);
        }
    }
}
