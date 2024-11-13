using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using static UnityEngine.UI.Dropdown;

public class ControllerOptionsMenu : OptionsMenu
{
    ControllerSettings _controllerConfig;

    [FormerlySerializedAs("smoothnessField")] [SerializeField] Slider _smoothnessField = null;
    [FormerlySerializedAs("controlTypeDropdown")] [SerializeField] Dropdown _controlTypeDropdown = null;
    [FormerlySerializedAs("defaultValueDropdown")] [SerializeField] Dropdown _defaultValueDropdown = null;
    [FormerlySerializedAs("curveTypeDropdown")] [SerializeField] Dropdown _curveTypeDropdown = null;
    [FormerlySerializedAs("resetValuesButton")] [SerializeField] Button _resetValuesButton = null;
    [FormerlySerializedAs("openOSCOptionsButton")] [SerializeField] Button _openOscOptionsButton = null;

    OSCSelectionMenu _oscSelectionMenu;
    OSCControllerSettings _oscSettingsPendingApplication;

    bool _shouldResetOscMenu = false;
    bool _initialized;

    private void OnEnable()
    {
        if (!_initialized) return;
        SetFieldsToControllerValues();
    }

    private void InitializeUI()
    {
        PopulateDropdowns();
        _resetValuesButton.onClick.AddListener(ResetValues);
    }

    public void Initialize(ControllerSettings data, ControllerOptionsPanel optionsPanel, OSCSelectionMenu oscMenu)
    {
        _controllerConfig = data;
        _oscSelectionMenu = oscMenu;
        optionsPanel.OnWake.AddListener(() => _shouldResetOscMenu = true);
        optionsPanel.OnWake.AddListener(() => _oscSettingsPendingApplication = null);
        InitializeUI();
        InitializeOscSelectionMenu(data.OscSettings);
        SetFieldsToControllerValues();
        _initialized = true;
    }

    private void InitializeOscSelectionMenu(OSCControllerSettings settings)
    {
        _openOscOptionsButton.onClick.AddListener(() => OpenOscSelectionMenu(settings));
    }

    void OpenOscSelectionMenu(OSCControllerSettings settings)
    {
        if(_shouldResetOscMenu)
        {
            _oscSelectionMenu.Initialize(settings, this);
            _shouldResetOscMenu = false;
        }
        else if(_oscSelectionMenu.LastToEdit != this)
        {
            _oscSelectionMenu.Initialize(settings, this);
        }

        _oscSelectionMenu.gameObject.SetActive(true);
    }

    void SetFieldsToControllerValues()
    {
        _controlTypeDropdown.SetValueWithoutNotify((int)_controllerConfig.ReleaseBehavior);
        _defaultValueDropdown.SetValueWithoutNotify((int)_controllerConfig.DefaultType);
        _curveTypeDropdown.SetValueWithoutNotify((int)_controllerConfig.Curve);

        _smoothnessField.SetValueWithoutNotify(_controllerConfig.SmoothTime);
        UpdateOscPreview(_controllerConfig.OscSettings);
    }

    public void SetControllerValuesToFields()
    {
        ReleaseBehaviorType controlType = (ReleaseBehaviorType)_controlTypeDropdown.value;
        DefaultValueType defaultValueType = (DefaultValueType)_defaultValueDropdown.value;
        CurveType curveType = (CurveType)_curveTypeDropdown.value;
        InputMethod inputType = InputMethod.Touch; //hard-coded for now until other input types are implemented

        float smoothTime = _smoothnessField.value;

        OSCControllerSettings oscSettings;

        if(_oscSettingsPendingApplication == null)
        {
            oscSettings = _controllerConfig.OscSettings;
        }
        else
        {
            oscSettings = new OSCControllerSettings(_oscSettingsPendingApplication);
        }

        _controllerConfig.SetVariables(inputType, controlType, oscSettings, defaultValueType, curveType, smoothTime);
    }

    public void StageOscChangesToApply(OSCControllerSettings settings)
    {
        _oscSettingsPendingApplication = new OSCControllerSettings(settings);
        UpdateOscPreview(settings);
    }

    void UpdateOscPreview(OSCControllerSettings settings)
    {
        _openOscOptionsButton.GetComponentInChildren<Text>().text = $"<b>OSC Options</b>\n{settings.GetAddress()}";
    }

    void PopulateDropdowns()
    {
        DropDownEntryNames.Add(_controlTypeDropdown, EnumUtility.GetControllerBehaviorTypeNameArray());
        DropDownEntryNames.Add(_defaultValueDropdown, Enum.GetNames(typeof(DefaultValueType)));
        DropDownEntryNames.Add(_curveTypeDropdown, Enum.GetNames(typeof(CurveType)));

        foreach (KeyValuePair<Dropdown, string[]> pair in DropDownEntryNames)
        {
            pair.Key.ClearOptions();
            foreach (string s in pair.Value)
            {
                pair.Key.options.Add(new OptionData(s));
            }
        }
    }

    public void ResetValues()
    {
        _oscSelectionMenu.Initialize(_controllerConfig.OscSettings, this);
        SetFieldsToControllerValues();
    }
}
