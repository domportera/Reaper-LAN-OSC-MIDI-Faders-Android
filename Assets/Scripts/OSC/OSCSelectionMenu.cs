using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class OSCSelectionMenu : OptionsMenu
{
    #region UI elements
    [SerializeField] RectTransform dawCommandsParent;
    [SerializeField] RectTransform userCommandsParent;
    [SerializeField] GameObject oscSettingsButtonPrefab;
    #endregion UI elements

    #region Built-in OSC Message Option Fields
    [SerializeField] Dropdown midiChannelDropdown = null;
    [SerializeField] Dropdown addressTypeDropdown = null;
    [SerializeField] Dropdown valueRangeDropdown = null;
    [SerializeField] InputField ccChannelField = null;
    #endregion Built-in OSC Message Option Fields

    ControllerSettings controllerSettings;
    public OSCControllerSettings OscSettings { get; private set; }

    const string SUBFOLDER = "OSCSettings";
    const string FILE_EXTENSION = "OSCTemplate";

    public void Initialize(ControllerSettings _controllerSettings)
    {
        controllerSettings = _controllerSettings;
        OscSettings = _controllerSettings.OscSettings;

        addressTypeDropdown.onValueChanged.AddListener(AddressTypeMenuChange);
        addressTypeDropdown.onValueChanged.AddListener(CheckForCCControl);
    }


    public void CheckForCCControl(int _value)
    {
        if((OSCAddressType)_value == OSCAddressType.MidiCC)
        {
            ccChannelField.gameObject.SetActive(true);
        }
        else
        {
            ccChannelField.gameObject.SetActive(false);
        }
    }

    void InitializeBuiltInMessagePresets()
    {
        foreach(KeyValuePair<OSCAddressType, OSCControllerSettings> set in OSCControllerSettings.defaultOSCTemplates)
        {
            CreateOSCCommandButton(set.Value, dawCommandsParent);
        }
    }

    void CreateOSCCommandButton(OSCControllerSettings _settings, Transform _parent)
    {
        OSCCommandButton button = Instantiate(oscSettingsButtonPrefab, _parent, false).GetComponentSafer<OSCCommandButton>();
        button.Initialize(() => OSCSelectionButtonPressed(_settings), _settings.AddressType.GetDescription(), _settings.GetAddress());
    }

    void OSCSelectionButtonPressed(OSCControllerSettings _settings)
    {
        OscSettings = _settings;
        SetFieldsToControllerValues();
        AddressTypeMenuChange(_settings.AddressType);
    }

    void AddressTypeMenuChange(OSCAddressType _type)
    {
        switch(_type)
        {
            case OSCAddressType.MidiCC:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                //ToggleUIObject(valueRangeDropdown, false); //was this for a unity UI thing where I needed to disable and then re-enable the dropdown for it to show the new value?
                ToggleUIObject(valueRangeDropdown, true);
                ccChannelField.SetTextWithoutNotify("1");
                ToggleUIObject(ccChannelField, true);
                break;
            case OSCAddressType.MidiPitch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.FourteenBit);
                ToggleUIObject(valueRangeDropdown, false);
                ccChannelField.SetTextWithoutNotify("");
                ToggleUIObject(ccChannelField, false);
                break;
            case OSCAddressType.MidiAftertouch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                ToggleUIObject(valueRangeDropdown, false);
                ccChannelField.SetTextWithoutNotify("");
                ToggleUIObject(ccChannelField, false);
                break;
            case OSCAddressType.Custom:
                ToggleUIObject(valueRangeDropdown, false);
                ToggleUIObject(ccChannelField, false);
                ToggleUIObject(midiChannelDropdown, false);
                ToggleUIObject(addressTypeDropdown, true);
                break;
            default:
                Debug.LogError($"OSC Address Type {_type} not implemented for menu change", this);
                return;
        }
    }

    void SaveTemplate(string _name, OSCControllerSettings _settings)
    {
        OSCControllerSettingsTemplate template = new OSCControllerSettingsTemplate(_name, _settings);
        string path = Path.Combine(Application.persistentDataPath, SUBFOLDER);
        bool success = FileHandler.SaveJson(template, path, _name, FILE_EXTENSION);

        if(success)
        {
            UtilityWindows.instance.ConfirmationWindow($"Saved OSC Template {_name} successfully!");
        }
        else
        {
            UtilityWindows.instance.ErrorWindow($"Error saving OSC Template {_name}.\nYou can check the log for more details.");
        }
    }

    void AddressTypeMenuChange(int _val)
    {
        AddressTypeMenuChange((OSCAddressType)_val);
    }

    void SetFieldsToControllerValues()
    {
        AddressTypeMenuChange((int)OscSettings.AddressType);

        midiChannelDropdown.SetValueWithoutNotify(OscSettings.Channel.GetInt());
        addressTypeDropdown.SetValueWithoutNotify(OscSettings.AddressType.GetInt());
        valueRangeDropdown.SetValueWithoutNotify(OscSettings.Range.GetInt());
        ccChannelField.SetTextWithoutNotify(OscSettings.CCNumber.ToString());

    }

    void PopulateDropdowns()
    {
        dropDownEntryNames.Add(addressTypeDropdown, EnumUtility.GetOSCAddressTypeNameArray());
        dropDownEntryNames.Add(midiChannelDropdown, EnumUtility.GetMidiChannelNameArray());
        dropDownEntryNames.Add(valueRangeDropdown, EnumUtility.GetValueRangeNameArray());

        foreach(KeyValuePair<Dropdown, string[]> pair in dropDownEntryNames)
        {
            pair.Key.ClearOptions();
            foreach(string s in pair.Value)
            {
                pair.Key.options.Add(new OptionData(s));
            }
        }
    }
}
