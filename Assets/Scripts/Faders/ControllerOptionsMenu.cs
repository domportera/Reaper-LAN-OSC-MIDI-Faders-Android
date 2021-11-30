using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class ControllerOptionsMenu : OptionsMenu
{
    ControllerSettings controllerConfig;

    [SerializeField] Slider smoothnessField = null;
    [SerializeField] Dropdown controlTypeDropdown = null;
    [SerializeField] Dropdown defaultValueDropdown = null;
    [SerializeField] Dropdown curveTypeDropdown = null;
    [SerializeField] Button resetValuesButton = null;
    [SerializeField] Button openOSCOptionsButton = null;

    OSCSelectionMenu oscSelectionMenu;
    OSCControllerSettings oscSettingsPendingApplication;

    bool shouldResetOSCMenu = false;

    private void InitializeUI()
    {
        PopulateDropdowns();
        resetValuesButton.onClick.AddListener(ResetValues);
    }

    public void Initialize(ControllerSettings _data, ControllerOptionsPanel _optionsPanel, OSCSelectionMenu _oscMenu)
    {
        controllerConfig = _data;
        oscSelectionMenu = _oscMenu;
        _optionsPanel.OnWake.AddListener(() => shouldResetOSCMenu = true);
        _optionsPanel.OnWake.AddListener(() => oscSettingsPendingApplication = null);
        InitializeUI();
        InitializeOSCSelectionMenu(_data.OscSettings);
        SetFieldsToControllerValues();
    }

    private void InitializeOSCSelectionMenu(OSCControllerSettings _settings)
    {
        openOSCOptionsButton.onClick.AddListener(() => OpenOSCSelectionMenu(_settings));
    }

    void OpenOSCSelectionMenu(OSCControllerSettings _settings)
    {
        if(shouldResetOSCMenu)
        {
            oscSelectionMenu.Initialize(_settings, this);
            shouldResetOSCMenu = false;
        }
        else if(oscSelectionMenu.LastToEdit != this)
        {
            oscSelectionMenu.Initialize(_settings, this);
        }

        oscSelectionMenu.gameObject.SetActive(true);
    }

    void SetFieldsToControllerValues()
    {
        controlTypeDropdown.SetValueWithoutNotify(controllerConfig.ReleaseBehavior.GetInt());
        defaultValueDropdown.SetValueWithoutNotify(controllerConfig.DefaultType.GetInt());
        curveTypeDropdown.SetValueWithoutNotify(controllerConfig.Curve.GetInt());

        smoothnessField.SetValueWithoutNotify(controllerConfig.SmoothTime);
    }

    public void SetControllerValuesToFields()
    {
        ReleaseBehaviorType controlType = (ReleaseBehaviorType)controlTypeDropdown.value;
        DefaultValueType defaultValueType = (DefaultValueType)defaultValueDropdown.value;
        CurveType curveType = (CurveType)curveTypeDropdown.value;
        InputMethod inputType = InputMethod.Touch; //hard-coded for now until other input types are implemented

        float smoothTime = smoothnessField.value;

        OSCControllerSettings oscSettings;

        if(oscSettingsPendingApplication == null)
        {
            oscSettings = controllerConfig.OscSettings;
        }
        else
        {
            oscSettings = new OSCControllerSettings(oscSettingsPendingApplication);
        }

        controllerConfig.SetVariables(inputType, controlType, oscSettings, defaultValueType, curveType, smoothTime);
    }

    public void StageOSCChangesToApply(OSCControllerSettings _settings)
    {
        oscSettingsPendingApplication = new OSCControllerSettings(_settings);
    }

    void PopulateDropdowns()
    {
        dropDownEntryNames.Add(controlTypeDropdown, EnumUtility.GetControllerBehaviorTypeNameArray());
        dropDownEntryNames.Add(defaultValueDropdown, Enum.GetNames(typeof(DefaultValueType)));
        dropDownEntryNames.Add(curveTypeDropdown, Enum.GetNames(typeof(CurveType)));

        foreach (KeyValuePair<Dropdown, string[]> pair in dropDownEntryNames)
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
        oscSelectionMenu.Initialize(controllerConfig.OscSettings, this);
        SetFieldsToControllerValues();
    }
}
