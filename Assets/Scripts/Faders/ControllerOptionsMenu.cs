using System;
using UnityEngine;
using UnityEngine.UI;

public class ControllerOptionsMenu : OptionsMenu
{
    private ControllerSettings _controllerConfig;

    [SerializeField] private Slider _smoothnessField;
    [SerializeField] private Dropdown _releaseBehaviourButton;
    [SerializeField] private Dropdown _defaultValueDropdown;
    [SerializeField] private Dropdown _curveTypeDropdown;
    [SerializeField] private Button _resetValuesButton;
    [SerializeField] private Button _openOscOptionsButton;

    private OscSelectionMenu _oscSelectionMenu;
    private OscControllerSettings _oscSettingsPendingApplication;

    private bool _shouldResetOscMenu;
    private bool _initialized;
    private ControllerOptionsPanel _optionsPanel;

    private void OnEnable()
    {
        if (!_initialized) return;
        ResetValues();
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
        _optionsPanel = optionsPanel;
        optionsPanel.OnWake += () => _shouldResetOscMenu = true;
        optionsPanel.OnWake += () => _oscSettingsPendingApplication = null;
        InitializeUI();
        InitializeOscSelectionMenu(data.OscSettings);
        ResetValues();
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

    public void SetControllerValuesToFields()
    {
        var controlType = (ReleaseBehaviorType)_releaseBehaviourButton.value;
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
        DropDownEntryNames.Add(_releaseBehaviourButton, EnumUtility.GetControllerBehaviorTypeNameArray());
        DropDownEntryNames.Add(_defaultValueDropdown, Enum.GetNames(typeof(DefaultValueType)));
        DropDownEntryNames.Add(_curveTypeDropdown, Enum.GetNames(typeof(CurveType)));

        foreach (var pair in DropDownEntryNames)
        {
            pair.Key.ClearOptions();
            foreach (var s in pair.Value)
            {
                pair.Key.options.Add(new Dropdown.OptionData(s));
            }
        }
    }

    public void ResetValues()
    {
        _oscSelectionMenu.Initialize(_controllerConfig.OscSettings, this);
        _releaseBehaviourButton.SetValueWithoutNotify((int)_controllerConfig.ReleaseBehavior);
        _defaultValueDropdown.SetValueWithoutNotify((int)_controllerConfig.DefaultType);
        _curveTypeDropdown.SetValueWithoutNotify((int)_controllerConfig.Curve);

        _smoothnessField.SetValueWithoutNotify(_controllerConfig.SmoothTime);
        _optionsPanel.ResetUiFields();
        UpdateOscPreview(_controllerConfig.OscSettings);
    }
}
