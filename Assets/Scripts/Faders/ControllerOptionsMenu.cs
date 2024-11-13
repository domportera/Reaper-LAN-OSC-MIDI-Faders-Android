using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using static UnityEngine.UI.Dropdown;

public class ControllerOptionsMenu : OptionsMenu
{
    private ControllerSettings _controllerConfig;

    [FormerlySerializedAs("smoothnessField")] [SerializeField]
    private Slider _smoothnessField = null;
    [FormerlySerializedAs("controlTypeDropdown")] [SerializeField]
    private Dropdown _controlTypeDropdown = null;
    [FormerlySerializedAs("defaultValueDropdown")] [SerializeField]
    private Dropdown _defaultValueDropdown = null;
    [FormerlySerializedAs("curveTypeDropdown")] [SerializeField]
    private Dropdown _curveTypeDropdown = null;
    [FormerlySerializedAs("resetValuesButton")] [SerializeField]
    private Button _resetValuesButton = null;
    [FormerlySerializedAs("openOSCOptionsButton")] [SerializeField]
    private Button _openOscOptionsButton = null;

    private OscSelectionMenu _oscSelectionMenu;
    private OscControllerSettings _oscSettingsPendingApplication;

    private bool _shouldResetOscMenu = false;
    private bool _initialized;

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

    public void Initialize(ControllerSettings data, ControllerOptionsPanel optionsPanel, OscSelectionMenu oscMenu)
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

    private void InitializeOscSelectionMenu(OscControllerSettings settings)
    {
        _openOscOptionsButton.onClick.AddListener(() => OpenOscSelectionMenu(settings));
    }

    private void OpenOscSelectionMenu(OscControllerSettings settings)
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

    private void SetFieldsToControllerValues()
    {
        _controlTypeDropdown.SetValueWithoutNotify((int)_controllerConfig.ReleaseBehavior);
        _defaultValueDropdown.SetValueWithoutNotify((int)_controllerConfig.DefaultType);
        _curveTypeDropdown.SetValueWithoutNotify((int)_controllerConfig.Curve);

        _smoothnessField.SetValueWithoutNotify(_controllerConfig.SmoothTime);
        UpdateOscPreview(_controllerConfig.OscSettings);
    }

    public void SetControllerValuesToFields()
    {
        var controlType = (ReleaseBehaviorType)_controlTypeDropdown.value;
        var defaultValueType = (DefaultValueType)_defaultValueDropdown.value;
        var curveType = (CurveType)_curveTypeDropdown.value;
        var inputType = InputMethod.Touch; //hard-coded for now until other input types are implemented

        var smoothTime = _smoothnessField.value;

        OscControllerSettings oscSettings;

        if(_oscSettingsPendingApplication == null)
        {
            oscSettings = _controllerConfig.OscSettings;
        }
        else
        {
            oscSettings = new OscControllerSettings(_oscSettingsPendingApplication);
        }

        _controllerConfig.SetVariables(inputType, controlType, oscSettings, defaultValueType, curveType, smoothTime);
    }

    public void StageOscChangesToApply(OscControllerSettings settings)
    {
        _oscSettingsPendingApplication = new OscControllerSettings(settings);
        UpdateOscPreview(settings);
    }

    private void UpdateOscPreview(OscControllerSettings settings)
    {
        _openOscOptionsButton.GetComponentInChildren<Text>().text = $"<b>OSC Options</b>\n{settings.GetAddress()}";
    }

    private void PopulateDropdowns()
    {
        DropDownEntryNames.Add(_controlTypeDropdown, EnumUtility.GetControllerBehaviorTypeNameArray());
        DropDownEntryNames.Add(_defaultValueDropdown, Enum.GetNames(typeof(DefaultValueType)));
        DropDownEntryNames.Add(_curveTypeDropdown, Enum.GetNames(typeof(CurveType)));

        foreach (var pair in DropDownEntryNames)
        {
            pair.Key.ClearOptions();
            foreach (var s in pair.Value)
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
