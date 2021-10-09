using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class FaderOptions : MonoBehaviour
{
    public ControllerSettings controllerConfig;
    ControlsManager manager = null;
    UIManager uiManager = null;

    [SerializeField] InputField nameField = null;
    [SerializeField] InputField ccChannelField = null;
    [SerializeField] Slider smoothnessField = null;
    [SerializeField] Dropdown controlTypeDropdown = null;
    [SerializeField] Dropdown midiChannelDropdown = null;
    [SerializeField] Dropdown addressTypeDropdown = null;
    [SerializeField] Dropdown valueRangeDropdown = null;
    [SerializeField] Dropdown defaultValueDropdown = null;
    [SerializeField] Dropdown curveTypeDropdown = null;
    [SerializeField] Button applyAndCloseButton = null;
    [SerializeField] Button closeButton = null;
    [SerializeField] Button resetValuesButton = null;
    [SerializeField] Button deleteButton = null;

    Dictionary<Dropdown, string[]> dropDownEntryNames = new Dictionary<Dropdown, string[]>();

    ControlsManager.ControllerData controlData;

    // Start is called before the first frame update
    void Awake()
    {
        manager = FindObjectOfType<ControlsManager>();
        uiManager = FindObjectOfType<UIManager>();
        gameObject.SetActive(false);

        PopulateDropdowns();
        addressTypeDropdown.onValueChanged.AddListener(AddressTypeMenuChange);

        addressTypeDropdown.onValueChanged.AddListener(CheckForCCControl);

        if(controllerConfig.addressType != AddressType.MidiCC) //this needs to be re-enabled if CC is selected from Control Type/ MIDI Parameter
        {
            ccChannelField.gameObject.SetActive(false);
        }

        applyAndCloseButton.onClick.AddListener(Apply);
        closeButton.onClick.AddListener(Close);
        resetValuesButton.onClick.AddListener(ResetValues);
        deleteButton.onClick.AddListener(Delete);
        nameField.onValueChanged.AddListener(RemoveProblemCharactersInNameField);
    }

    public void Initialize(ControlsManager.ControllerData _data)
    {
        controlData = _data;
        SetFieldsToControllerValues();
    }

    public void CheckForCCControl(int _value)
    {
        if ((AddressType)_value == AddressType.MidiCC)
        {
            ccChannelField.gameObject.SetActive(true);
        }
        else
        {
            ccChannelField.gameObject.SetActive(false);
        }
    }

    bool VerifyUniqueName(string _s)
    {
        bool valid = true;
        List<ControlsManager.ControllerData> controllers = manager.GetAllControllers();

        foreach(ControlsManager.ControllerData set in controllers)
        {
            if(set.GetName() == _s)
            {
                valid = false;
                break;
            }
        }

        if(!valid)
        {
            Utilities.instance.ErrorWindow("Name should be unique - no two controllers in the same profile can have the same name.");
            return false;
        }

        return true;
    }

    void RemoveProblemCharactersInNameField(string _input)
    {
        _input.Replace("\"", "");
        _input.Replace("\\", "");
        nameField.SetTextWithoutNotify(_input);
    }

    void AddressTypeMenuChange(int _val)
    {
        switch((AddressType)_val)
        {
            case AddressType.MidiCC:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                valueRangeDropdown.gameObject.SetActive(true);
                ccChannelField.SetTextWithoutNotify("");
                ccChannelField.gameObject.SetActive(true);
                break;
            case AddressType.MidiPitch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.FourteenBit);
                valueRangeDropdown.gameObject.SetActive(true);
                ccChannelField.SetTextWithoutNotify("");
                ccChannelField.gameObject.SetActive(false);
                break;
            case AddressType.MidiAftertouch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                valueRangeDropdown.gameObject.SetActive(false);
                ccChannelField.SetTextWithoutNotify("");
                ccChannelField.gameObject.SetActive(false);
                break;
        }
    }
    
    private void OnEnable()
    {
        if (controlData != null)
        {
            SetFieldsToControllerValues();
        }
    }

    void SetFieldsToControllerValues()
    {
        controlTypeDropdown.SetValueWithoutNotify((int)controllerConfig.controlType);
        midiChannelDropdown.SetValueWithoutNotify((int)controllerConfig.channel);
        addressTypeDropdown.SetValueWithoutNotify((int)controllerConfig.addressType);
        valueRangeDropdown.SetValueWithoutNotify((int)controllerConfig.range);
        defaultValueDropdown.SetValueWithoutNotify((int)controllerConfig.defaultType);
        curveTypeDropdown.SetValueWithoutNotify((int)controllerConfig.curveType);

        smoothnessField.SetValueWithoutNotify(controllerConfig.smoothTime);
        nameField.SetTextWithoutNotify(controlData.GetName());
        ccChannelField.SetTextWithoutNotify(controllerConfig.ccNumber.ToString());

        AddressTypeMenuChange((int)controllerConfig.addressType);
    }

    void SetControllerValuesToFields()
    {
        //_ = VerifyUniqueName(nameField.text); //not sure if this is necessary - should be tested. Disabling for now
        ControlBehaviorType controlType = (ControlBehaviorType)controlTypeDropdown.value;
        AddressType addressType = (AddressType)addressTypeDropdown.value;
        DefaultValueType defaultValueType = (DefaultValueType)defaultValueDropdown.value;
        CurveType curveType = (CurveType)curveTypeDropdown.value;
        MIDIChannel midiChannel = (MIDIChannel)(midiChannelDropdown.value - 1); //-1 because all channels is -1 in the enum, channel 1 is 0 in the enum, etc
        ValueRange valueRange = (ValueRange)(valueRangeDropdown.value);
        InputType inputType = InputType.Touch; //hard-coded for now until other input types are implemented

        float smoothTime = smoothnessField.value;
        string controllerName = nameField.text;

        int result;
        int ccNumber = int.TryParse(ccChannelField.text, out result) ? result : -1; //this number should be validated by text field, so it should always be ok if text field is set up properly

        controllerConfig.SetVariables(inputType, controlType, addressType, valueRange, defaultValueType, midiChannel, curveType, ccNumber, smoothTime);
        controlData.SetName(controllerName);

        manager.RespawnController(controlData);
    }

    void PopulateDropdowns()
    {
        dropDownEntryNames.Add(controlTypeDropdown, Enum.GetNames(typeof(ControlBehaviorType)));
        dropDownEntryNames.Add(addressTypeDropdown, Enum.GetNames(typeof(AddressType)));
        dropDownEntryNames.Add(defaultValueDropdown, Enum.GetNames(typeof(DefaultValueType)));
        dropDownEntryNames.Add(curveTypeDropdown, Enum.GetNames(typeof(CurveType)));
        dropDownEntryNames.Add(midiChannelDropdown, EnumUtility.GetMidiChannelNameArray());
        dropDownEntryNames.Add(valueRangeDropdown, EnumUtility.GetValueRangeNameArray());

        foreach (KeyValuePair<Dropdown, string[]> pair in dropDownEntryNames)
        {
            pair.Key.ClearOptions();
            foreach (string s in pair.Value)
            {
                pair.Key.options.Add(new OptionData(s));
            }
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void ResetValues()
    {
        SetFieldsToControllerValues();
    }

    public void Delete()
    {
        //delete from ControlsManager and destroy objects
        manager.DestroyController(controlData);
        uiManager.DestroyControllerObjects(controlData);
    }

    public void Apply()
    {
        SetControllerValuesToFields();
        Utilities.instance.ConfirmationWindow("Settings applied!");
    }
}
