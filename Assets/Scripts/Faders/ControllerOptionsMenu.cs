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

    [SerializeField] GameObject oscSelectionMenuPreset;
    OSCSelectionMenu oscSelectionMenu;

    private void InitializeUI()
    {
        PopulateDropdowns();
        resetValuesButton.onClick.AddListener(ResetValues);
    }

    public void Initialize(ControllerSettings _data)
    {
        controllerConfig = _data;
        InitializeUI();

        oscSelectionMenu = Instantiate(oscSelectionMenuPreset, transform).GetComponentSafer<OSCSelectionMenu>();
        oscSelectionMenu.Initialize(_data);

        SetFieldsToControllerValues();
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

        OSCControllerSettings oscSettings = oscSelectionMenu.OscSettings;

        controllerConfig.SetVariables(inputType, controlType, oscSettings, defaultValueType, curveType, smoothTime);
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
        SetFieldsToControllerValues();
    }
}
