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
    ControllerSettings controllerConfig;

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

    [SerializeField] bool toggleOptionParent = false;

    private void InitializeUI()
    {
        PopulateDropdowns();
        addressTypeDropdown.onValueChanged.AddListener(AddressTypeMenuChange);
        addressTypeDropdown.onValueChanged.AddListener(CheckForCCControl);
        resetValuesButton.onClick.AddListener(ResetValues);

        CheckForCCControl((int)controllerConfig.AddressType);
    }

    public void Initialize(ControllerSettings _data)
    {
        controllerConfig = _data;
        InitializeUI();
        SetFieldsToControllerValues();
    }

    public void CheckForCCControl(int _value)
    {
        if ((OSCAddressType)_value == OSCAddressType.MidiCC)
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
        switch((OSCAddressType)_val)
        {
            case OSCAddressType.MidiCC:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                ToggleUIObject(valueRangeDropdown, false);
                ToggleUIObject(valueRangeDropdown, true);
                ccChannelField.SetTextWithoutNotify("");
                ToggleUIObject(ccChannelField, true);
                break;
            case OSCAddressType.MidiPitch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.FourteenBit);
                ToggleUIObject(valueRangeDropdown, true);
                ccChannelField.SetTextWithoutNotify("");
                ToggleUIObject(ccChannelField, false);
                break;
            case OSCAddressType.MidiAftertouch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                ToggleUIObject(valueRangeDropdown, false);
                ccChannelField.SetTextWithoutNotify("");
                ToggleUIObject(ccChannelField, false);
                break;
        }
    }

    void ToggleUIObject(Selectable _object, bool _on)
    {
        if (toggleOptionParent)
        {
            _object.transform.parent.gameObject.SetActive(_on);
        }
        else
        {
            _object.gameObject.SetActive(_on);
        }
    }

    void SetFieldsToControllerValues()
    {
        AddressTypeMenuChange((int)controllerConfig.AddressType);

        controlTypeDropdown.SetValueWithoutNotify(controllerConfig.ReleaseBehavior.GetInt());
        midiChannelDropdown.SetValueWithoutNotify(controllerConfig.Channel.GetInt());
        addressTypeDropdown.SetValueWithoutNotify(controllerConfig.AddressType.GetInt());
        valueRangeDropdown.SetValueWithoutNotify(controllerConfig.Range.GetInt());
        defaultValueDropdown.SetValueWithoutNotify(controllerConfig.DefaultType.GetInt());
        curveTypeDropdown.SetValueWithoutNotify(controllerConfig.Curve.GetInt());

        smoothnessField.SetValueWithoutNotify(controllerConfig.SmoothTime);
        ccChannelField.SetTextWithoutNotify(controllerConfig.CCNumber.ToString());
    }

    public void SetControllerValuesToFields()
    {
        ReleaseBehaviorType controlType = (ReleaseBehaviorType)controlTypeDropdown.value;
        OSCAddressType addressType = (OSCAddressType)addressTypeDropdown.value;
        DefaultValueType defaultValueType = (DefaultValueType)defaultValueDropdown.value;
        CurveType curveType = (CurveType)curveTypeDropdown.value;
        MIDIChannel midiChannel = (MIDIChannel)(midiChannelDropdown.value - 1); //-1 because all channels is -1 in the enum, channel 1 is 0 in the enum, etc
        ValueRange valueRange = (ValueRange)(valueRangeDropdown.value);
        InputMethod inputType = InputMethod.Touch; //hard-coded for now until other input types are implemented

        float smoothTime = smoothnessField.value;

        int result;
        int ccNumber = int.TryParse(ccChannelField.text, out result) ? result : -1; //this number should be validated by text field, so it should always be ok if text field is set up properly

        controllerConfig.SetVariables(inputType, controlType, addressType, valueRange, defaultValueType, midiChannel, curveType, ccNumber, smoothTime);
    }

    void PopulateDropdowns()
    {
        dropDownEntryNames.Add(controlTypeDropdown, EnumUtility.GetControllerBehaviorTypeNameArray());
        dropDownEntryNames.Add(addressTypeDropdown, EnumUtility.GetOSCAddressTypeNameArray());
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
