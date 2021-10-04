using System;
using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Awake()
    {
        manager = FindObjectOfType<ControlsManager>();
        uiManager = FindObjectOfType<UIManager>();
        gameObject.SetActive(false);

        PopulateDropdowns();
        SetFieldsToControllerValues();
        addressTypeDropdown.onValueChanged.AddListener(AddressTypeMenuChange);

        addressTypeDropdown.onValueChanged.AddListener(CheckForCCControl);

        if(controllerConfig.addressType != AddressType.CC) //this needs to be re-enabled if CC is selected from Control Type/ MIDI Parameter
        {
            ccChannelField.gameObject.SetActive(false);
        }

        applyAndCloseButton.onClick.AddListener(Apply);
        closeButton.onClick.AddListener(Close);
        resetValuesButton.onClick.AddListener(ResetValues);
        deleteButton.onClick.AddListener(Delete);
        nameField.onValueChanged.AddListener(RemoveProblemCharactersInNameField);
    }

    public void CheckForCCControl(int _value)
    {
        if ((AddressType)_value == AddressType.CC)
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
        List<ControllerSettings> controllers = manager.GetAllControllers();

        foreach(ControllerSettings set in controllers)
        {
            if(set.name == _s)
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
            case AddressType.CC:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                valueRangeDropdown.gameObject.SetActive(true);
                ccChannelField.SetTextWithoutNotify("");
                ccChannelField.gameObject.SetActive(true);
                break;
            case AddressType.Pitch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.FourteenBit);
                valueRangeDropdown.gameObject.SetActive(true);
                ccChannelField.SetTextWithoutNotify("");
                ccChannelField.gameObject.SetActive(false);
                break;
            case AddressType.Aftertouch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                valueRangeDropdown.gameObject.SetActive(false);
                ccChannelField.SetTextWithoutNotify("");
                ccChannelField.gameObject.SetActive(false);
                break;
        }
    }
    
    private void OnEnable()
    {
        SetFieldsToControllerValues();
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
        nameField.SetTextWithoutNotify(controllerConfig.name);
        ccChannelField.SetTextWithoutNotify(controllerConfig.ccNumber.ToString());

        AddressTypeMenuChange((int)controllerConfig.addressType);
    }

    void SetControllerValuesToFields()
    {
        //_ = VerifyUniqueName(nameField.text); //not sure if this is necessary - should be tested. Disabling for now
        ControlType controlType = (ControlType)controlTypeDropdown.value;
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

        controllerConfig.SetVariables(controllerName, inputType, controlType, addressType, valueRange, defaultValueType, midiChannel, curveType, ccNumber, smoothTime);

        manager.RespawnController(controllerConfig);
    }

    void PopulateDropdowns()
    {
        dropDownEntryNames.Add(controlTypeDropdown, Enum.GetNames(typeof(ControlType)));
        dropDownEntryNames.Add(addressTypeDropdown, Enum.GetNames(typeof(AddressType)));
        dropDownEntryNames.Add(defaultValueDropdown, Enum.GetNames(typeof(DefaultValueType)));
        dropDownEntryNames.Add(curveTypeDropdown, Enum.GetNames(typeof(CurveType)));

        string[] midiChannelNames = new string[] //pair with enum
        {
            "All Channels",
            "Channel 1",
            "Channel 2",
            "Channel 3",
            "Channel 4",
            "Channel 5",
            "Channel 6",
            "Channel 7",
            "Channel 8",
            "Channel 9",
            "Channel 10",
            "Channel 11",
            "Channel 12",
            "Channel 13",
            "Channel 14",
            "Channel 15",
            "Channel 16"
        };

        string[] valueRangeNames = new string[]
        {
            "7-bit (0-127)",
            "14-bit (0-16383)",
            "8-bit (0-255)",
            "7-bit (-64-63)",
            "14-bit(-16384-16383)",
            "8-bit(-128-127)"
        };

        dropDownEntryNames.Add(midiChannelDropdown, midiChannelNames);
        dropDownEntryNames.Add(valueRangeDropdown, valueRangeNames);

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
        manager.DestroyController(controllerConfig);
        uiManager.DestroyControllerObjects(controllerConfig);
    }

    public void Apply()
    {
        SetControllerValuesToFields();
        Utilities.instance.ConfirmationWindow("Settings applied!");
    }
}
