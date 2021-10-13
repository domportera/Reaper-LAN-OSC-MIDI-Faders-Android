using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class ControllerOptionsMenu : MonoBehaviour
{
    public ControllerSettings controllerConfig;

    //[SerializeField] InputField nameField = null;
    [SerializeField] InputField ccChannelField = null;
    [SerializeField] Slider smoothnessField = null;
    [SerializeField] Dropdown controlTypeDropdown = null;
    [SerializeField] Dropdown midiChannelDropdown = null;
    [SerializeField] Dropdown addressTypeDropdown = null;
    [SerializeField] Dropdown valueRangeDropdown = null;
    [SerializeField] Dropdown defaultValueDropdown = null;
    [SerializeField] Dropdown curveTypeDropdown = null;
    [SerializeField] Button resetValuesButton = null;

    Dictionary<Dropdown, string[]> dropDownEntryNames = new Dictionary<Dropdown, string[]>();

    ControlsManager.ControllerData controlData;

    // Start is called before the first frame update
    void Start()
    {
        InitializeUI();
        CheckForCCControl((int)controllerConfig.addressType);
        gameObject.SetActive(false);
    }

    private void InitializeUI()
    {
        PopulateDropdowns();
        addressTypeDropdown.onValueChanged.AddListener(AddressTypeMenuChange);
        addressTypeDropdown.onValueChanged.AddListener(CheckForCCControl);
        resetValuesButton.onClick.AddListener(ResetValues);
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
        ccChannelField.SetTextWithoutNotify(controllerConfig.ccNumber.ToString());

        AddressTypeMenuChange((int)controllerConfig.addressType);
    }

    public void SetControllerValuesToFields()
    {
        ControlBehaviorType controlType = (ControlBehaviorType)controlTypeDropdown.value;
        AddressType addressType = (AddressType)addressTypeDropdown.value;
        DefaultValueType defaultValueType = (DefaultValueType)defaultValueDropdown.value;
        CurveType curveType = (CurveType)curveTypeDropdown.value;
        MIDIChannel midiChannel = (MIDIChannel)(midiChannelDropdown.value - 1); //-1 because all channels is -1 in the enum, channel 1 is 0 in the enum, etc
        ValueRange valueRange = (ValueRange)(valueRangeDropdown.value);
        InputType inputType = InputType.Touch; //hard-coded for now until other input types are implemented

        float smoothTime = smoothnessField.value;

        int result;
        int ccNumber = int.TryParse(ccChannelField.text, out result) ? result : -1; //this number should be validated by text field, so it should always be ok if text field is set up properly

        controllerConfig.SetVariables(inputType, controlType, addressType, valueRange, defaultValueType, midiChannel, curveType, ccNumber, smoothTime);
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

    public void ResetValues()
    {
        SetFieldsToControllerValues();
    }
}
