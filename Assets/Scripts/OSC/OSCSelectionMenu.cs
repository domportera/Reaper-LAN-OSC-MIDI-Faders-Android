using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
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
    private OSCControllerSettings originalSettings;

    const string SUBFOLDER = "OSCSettings";
    const string FILE_EXTENSION = "OSCTemplate";

    public void Initialize(ControllerSettings _controllerSettings)
    {
        controllerSettings = _controllerSettings;
        OscSettings = _controllerSettings.OscSettings;
        originalSettings = new OSCControllerSettings(_controllerSettings.OscSettings);

        addressTypeDropdown.onValueChanged.AddListener(AddressTypeMenuChange);
        addressTypeDropdown.onValueChanged.AddListener(CheckForCCControl);

        InitializeBuiltInMessagePresets();
        InitializeUserTemplates();
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
            OSCControllerSettingsTemplate template = new OSCControllerSettingsTemplate(set.Key.GetDescription(), set.Value);
            CreateOSCCommandButton(template, dawCommandsParent);
        }
    }

    void CreateOSCCommandButton(OSCControllerSettingsTemplate _template, Transform _parent)
    {
        OSCCommandButton button = Instantiate(oscSettingsButtonPrefab, _parent, false).GetComponentSafer<OSCCommandButton>();
        button.Initialize(() => OSCSelectionButtonPressed(_template), _template.name, _template.oscSettings.GetAddress());
    }

    void OSCSelectionButtonPressed(OSCControllerSettingsTemplate _template)
    {
        OscSettings = _template.oscSettings;

        UnityAction confirm = () =>
        {
            SetFieldsToControllerValues();
            AddressTypeMenuChange(_template.oscSettings.AddressType);
            originalSettings = new OSCControllerSettings(_template.oscSettings);
        };

        if(!_template.oscSettings.Compare(originalSettings))
        {
            UtilityWindows.instance.ConfirmationWindow($"Override current settings to apply {_template.name} template?", confirm);
        }
        else
        {
            confirm.Invoke();
        }
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

    void InitializeUserTemplates()
    {
        List<OSCControllerSettingsTemplate> templates = new List<OSCControllerSettingsTemplate>();
        templates = LoadUserTemplates();

        foreach(OSCControllerSettingsTemplate t in templates)
        {
            CreateOSCCommandButton(t, userCommandsParent);
        }
    }

    List<OSCControllerSettingsTemplate> LoadUserTemplates()
    {
        string path = Path.Combine(Application.persistentDataPath, SUBFOLDER);
        DirectoryInfo info = new DirectoryInfo(path);
        List<OSCControllerSettingsTemplate> templates = new List<OSCControllerSettingsTemplate>();

        foreach(FileInfo f in info.GetFiles())
        {
            OSCControllerSettingsTemplate template = FileHandler.LoadJson<OSCControllerSettingsTemplate>(f.DirectoryName, f.Name, f.Extension);
            templates.Add(template);
        }

        return templates;
    }

    void SaveAsButton()
    {
        UtilityWindows.instance.TextInputWindow("Enter a name for your OSC template:", SaveAsConfirmed, null, "Save", "Cancel");
    }

    void SaveAsConfirmed(string _input)
    {
        List<char> invalidCharacters = new List<char>();
        bool valid = FileHandler.ContainsInvalidFileNameCharacters(_input, out invalidCharacters);

        if(valid)
        {
            OSCControllerSettingsTemplate template = SaveTemplate(_input, OscSettings);
            CreateOSCCommandButton(template, userCommandsParent);
        }
        else
        {
            string invalidCharString = new string(invalidCharacters.ToArray());
            UtilityWindows.instance.ErrorWindow($"Template name contains invalid characters: {invalidCharString}\nPlease use a name suited for a file name.", SaveAsButton);
        }
    }

    OSCControllerSettingsTemplate SaveTemplate(string _name, OSCControllerSettings _settings)
    {
        OSCControllerSettingsTemplate template = new OSCControllerSettingsTemplate(_name, _settings);
        string path = Path.Combine(Application.persistentDataPath, SUBFOLDER);
        bool success = FileHandler.SaveJson(template, path, _name, FILE_EXTENSION);

        if(success)
        {
            UtilityWindows.instance.QuickNoticeWindow($"Saved OSC Template {_name} successfully!");
        }
        else
        {
            UtilityWindows.instance.ErrorWindow($"Error saving OSC Template {_name}.\nYou can check the log for more details.");
        }

        return template;
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
