using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [SerializeField] Button saveAsButton;
    [SerializeField] Button backButton;
    #endregion UI elements

    #region Built-in OSC Message Option Fields
    [SerializeField] Dropdown midiChannelDropdown = null;
    [SerializeField] Dropdown addressTypeDropdown = null;
    [SerializeField] Dropdown valueRangeDropdown = null;
    [SerializeField] InputField ccChannelField = null;
    [SerializeField] InputField customAddressField = null;
    [SerializeField] Text oscPreview = null;
    [SerializeField] InputField minField = null;
    [SerializeField] InputField maxField = null;
    #endregion Built-in OSC Message Option Fields

    public OSCControllerSettings OscSettings { get; private set; }
    private OSCControllerSettings originalSettings;

    public ControllerOptionsMenu LastToEdit { get; private set; }

    const string SUBFOLDER = "OSCSettings";
    const string FILE_EXTENSION = ".OSCTemplate";

    private void Awake()
    {
        InitializeOSCEditingUIElements();
        InitializeBuiltInMessagePresets();
        InitializeUserTemplates();
        PopulateDropdowns();
        saveAsButton.onClick.AddListener(SaveAsButton);
        backButton.onClick.AddListener(BackButton);
    }

    public void Initialize(OSCControllerSettings _oscSettings, ControllerOptionsMenu _optionsMenu)
    {
        OscSettings = new OSCControllerSettings(_oscSettings); //create copies so any edits aren't automatically applied
        LastToEdit = _optionsMenu;
        originalSettings = new OSCControllerSettings(_oscSettings);
        SetFieldsToControllerValues(OscSettings);
    }

    private void InitializeOSCEditingUIElements()
    {
        customAddressField.onEndEdit.AddListener(VerifyCustomAddressField);
        minField.onEndEdit.AddListener(ChangeMin);
        maxField.onEndEdit.AddListener(ChangeMax);

        ccChannelField.onEndEdit.AddListener((string val) =>
        {
            ChangeCCNumber(val);
            UpdateOSCPreview();
        });

        addressTypeDropdown.onValueChanged.AddListener((int val) =>
        {
            ChangeAddressType(val);
            UpdateOSCPreview();
        });


        valueRangeDropdown.onValueChanged.AddListener((int val) =>
        {
            ChangeValueRange(val);
            UpdateOSCPreview();
        });

        midiChannelDropdown.onValueChanged.AddListener((int val) =>
        {
            ChangeMIDIChannel(val);
            UpdateOSCPreview();
        });
    }

    void VerifyCustomAddressField(string _input)
    {
        string warning = "Address cannot contain the following strings:";
        bool containsError = false;
        string[] bannedStrings = new string[] { OSCControllerSettings.CC_CHANNEL_STRING , OSCControllerSettings.MIDI_CHANNEL_STRING };

        foreach(string s in bannedStrings)
        {
            if(_input.Contains(s))
            {
                if(containsError)
                    warning += ',';

                warning += " " + s;
                containsError = true;
            }
        }

        if(containsError)
        {
            UtilityWindows.instance.ErrorWindow(warning);
        }
        else
        {
            OscSettings.SetCustomAddress(_input);
        }
    }

    void BackButton()
    {
        LastToEdit.StageOSCChangesToApply(OscSettings);
        gameObject.SetActive(false);
    }

    void InitializeBuiltInMessagePresets()
    {
        foreach(KeyValuePair<OSCAddressType, OSCControllerSettings> set in OSCControllerSettings.defaultOSCTemplates)
        {
            OSCControllerSettingsTemplate template = new OSCControllerSettingsTemplate(set.Key.GetDescription(), set.Value);
            CreateOSCCommandButton(template, dawCommandsParent);
        }
    }

    OSCCommandButton CreateOSCCommandButton(OSCControllerSettingsTemplate _template, Transform _parent)
    {
        OSCCommandButton button = Instantiate(oscSettingsButtonPrefab, _parent, false).GetComponentSafer<OSCCommandButton>();
        Action longPressAction = _parent == userCommandsParent ? () => DeleteTemplatePrompt(_template, button) : null;
        button.Initialize(() => OSCSelectionButtonPressed(_template), longPressAction, _template.name, _template.oscSettings.GetAddress());
        return button;
    }

    void DeleteTemplatePrompt(OSCControllerSettingsTemplate _template, OSCCommandButton _button)
    {
        UtilityWindows.instance.ConfirmationWindow($"Delete template \"{_template.name}\"?", () => DeleteTemplate(_template, _button), null, "Delete", "Cancel");
    }

    void DeleteTemplate(OSCControllerSettingsTemplate _template, OSCCommandButton _button)
    {
        //delete file
        string path = Path.Combine(Application.persistentDataPath, SUBFOLDER, _template.name) + FILE_EXTENSION;
        if(File.Exists(path))
        {
            File.Delete(path);
            _button.DestroySelf();
        }
        else
        {
            Debug.LogError($"Tried to delete OSC template {_template.name} at {path} but could not find it");
        }
    }

    void SortUserButtons()
    {
        OSCCommandButton[] userButtons = userCommandsParent.GetComponentsInChildren<OSCCommandButton>();
        userButtons = userButtons.OrderBy(button => button.name).ToArray();

        foreach(OSCCommandButton b in userButtons)
        {
            b.transform.SetParent(null);
            b.transform.SetParent(userCommandsParent);
            Debug.Log($"Sorting {b.name}");
        }
    }

    void OSCSelectionButtonPressed(OSCControllerSettingsTemplate _template)
    {
        OscSettings = new OSCControllerSettings(_template.oscSettings);

        UnityAction confirm = () =>
        {
            SetFieldsToControllerValues(OscSettings);
            AddressTypeMenuChange(OscSettings);
            originalSettings = new OSCControllerSettings(OscSettings);
        };

        if(!_template.oscSettings.IsEqualTo(originalSettings))
        {
            UtilityWindows.instance.ConfirmationWindow($"Override current settings to apply \"{_template.name}\" template?", confirm);
        }
        else
        {
            confirm.Invoke();
        }
    }

    bool ShouldEnableMinMaxFields(OSCControllerSettings _settings)
    {
        return _settings.Range == ValueRange.CustomFloat || _settings.Range == ValueRange.CustomInt;
    }

    void AddressTypeMenuChange(OSCControllerSettings _settings)
    {
        bool enableMinMax = ShouldEnableMinMaxFields(_settings);
        switch(_settings.AddressType)
        {
            case OSCAddressType.MidiCC:
                valueRangeDropdown.SetValueWithoutNotify((int)_settings.Range);

                ToggleUIObject(valueRangeDropdown, true);
                ToggleUIObject(ccChannelField, true);
                ToggleUIObject(customAddressField, false);
                ToggleUIObject(oscPreview.transform, true);
                ToggleUIObject(midiChannelDropdown, true);

                ToggleMinMaxFields(enableMinMax);
                break;
            case OSCAddressType.MidiPitch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.FourteenBit);
                _settings.SetRange(ValueRange.FourteenBit);

                ToggleUIObject(valueRangeDropdown, false);
                ToggleUIObject(ccChannelField, false);
                ToggleUIObject(customAddressField, false);
                ToggleUIObject(oscPreview.transform, true);
                ToggleUIObject(midiChannelDropdown, true);

                ToggleMinMaxFields(false);
                break;
            case OSCAddressType.MidiAftertouch:
                valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                _settings.SetRange(ValueRange.SevenBit);

                ToggleUIObject(valueRangeDropdown, false);
                ToggleUIObject(ccChannelField, false);
                ToggleUIObject(customAddressField, false);
                ToggleUIObject(oscPreview.transform, true);
                ToggleUIObject(midiChannelDropdown, true);

                ToggleMinMaxFields(false);
                break;
            case OSCAddressType.Custom:
                ToggleUIObject(valueRangeDropdown, true);
                ToggleUIObject(ccChannelField, false);
                ToggleUIObject(midiChannelDropdown, false);
                ToggleUIObject(customAddressField, true);
                ToggleUIObject(oscPreview.transform, false);

                ToggleMinMaxFields(enableMinMax);
                break;
            default:
                Debug.LogError($"OSC Address Type {_settings.AddressType} not implemented for menu change", this);
                return;
        }
    }

    void InitializeUserTemplates()
    {
        List<OSCControllerSettingsTemplate> templates = LoadUserTemplates();

        foreach(OSCControllerSettingsTemplate t in templates)
        {
            CreateOSCCommandButton(t, userCommandsParent);
        }
    }

    List<OSCControllerSettingsTemplate> LoadUserTemplates()
    {
        List<OSCControllerSettingsTemplate> templates;
        string path = Path.Combine(Application.persistentDataPath, SUBFOLDER);
        templates = FileHandler.LoadAllJsonObjects<OSCControllerSettingsTemplate>(path, FILE_EXTENSION);
        return templates;
    }

    void SaveAsButton()
    {
        UtilityWindows.instance.TextInputWindow("Enter a name for your OSC template:", SaveAsConfirmed, null, "Save", "Cancel");
    }

    void SaveAsConfirmed(string _input)
    {
        List<char> invalidCharacters = new List<char>();
        bool invalid = FileHandler.ContainsInvalidFileNameCharacters(_input, out invalidCharacters);

        if(!invalid)
        {
            OSCControllerSettingsTemplate template = SaveTemplate(_input, OscSettings);
            OSCCommandButton button = CreateOSCCommandButton(template, userCommandsParent);
            DoNextFrame(SortUserButtons);
        }
        else
        {
            string invalidCharString = new string(invalidCharacters.ToArray());
            UtilityWindows.instance.ErrorWindow($"Template name contains invalid characters: {invalidCharString}\nPlease use a name suited for a file name.", SaveAsButton);
            Debug.LogError($"Invalid file name: {_input}. Offending characters: {invalidCharString}");
        }
    }

    OSCControllerSettingsTemplate SaveTemplate(string _name, OSCControllerSettings _settings)
    {
        OSCControllerSettingsTemplate template = new OSCControllerSettingsTemplate(_name, _settings);
        string path = Path.Combine(Application.persistentDataPath, SUBFOLDER);
        bool success = FileHandler.SaveJsonObject(template, path, _name, FILE_EXTENSION);

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

    void ChangeMIDIChannel(int _val)
    {
        MIDIChannel channel = (MIDIChannel)_val;
        OscSettings.SetMIDIChannel(channel);
        LogDebug($"Set MIDI Channel as {channel}");
    }

    void ChangeAddressType(int _val)
    {
        OSCAddressType type = (OSCAddressType)_val;
        OscSettings.SetOSCAddressType(type);
        AddressTypeMenuChange(OscSettings);
        LogDebug($"Set Address Type as {type}");
    }

    void ChangeValueRange(int _val)
    {
        ValueRange range = (ValueRange)_val;
        OscSettings.SetRange(range);
        LogDebug($"Set Value Range as {range}");

        bool enableMinMax = range == ValueRange.CustomFloat || range == ValueRange.CustomInt;
        ToggleMinMaxFields(enableMinMax);
    }

    void ChangeCCNumber(string _input)
    {
        if(string.IsNullOrWhiteSpace(_input))
        {
            return;
        }
        int ccNum = int.Parse(_input);
        ccNum = Mathf.Clamp(ccNum, OSCControllerSettings.MIN_CC, OSCControllerSettings.MAX_CC);
        ccChannelField.SetTextWithoutNotify(ccNum.ToString());
        OscSettings.SetCCNumber(ccNum);
        LogDebug($"Set CC Number to {ccNum}");
    }

    void ChangeMin(string _val)
    {
        float val = float.Parse(_val);
        if(OscSettings.Range == ValueRange.CustomInt)
        {
            int intVal = (int)val;
            OscSettings.SetMin(intVal);
            minField.SetTextWithoutNotify((intVal).ToString());
            LogDebug($"Set Min as {intVal}");
        }
        else
        {
            OscSettings.SetMin(val);
            LogDebug($"Set Min as {val}");
        }

    }

    void ChangeMax(string _val)
    {
        float val = float.Parse(_val);
        if(OscSettings.Range == ValueRange.CustomInt)
        {
            int intVal = (int)val;
            OscSettings.SetMax(intVal);
            maxField.SetTextWithoutNotify((intVal).ToString());
            LogDebug($"Set Max as {intVal}");
        }
        else
        {
            OscSettings.SetMax(val);
            LogDebug($"Set Max as {val}");
        }
    }

    void ToggleMinMaxFields(bool _on)
    {
        ToggleUIObject(minField, _on);
        ToggleUIObject(maxField, _on);
        
        if(_on)
        {
            minField.SetTextWithoutNotify(OscSettings.Min.ToString());
            maxField.SetTextWithoutNotify(OscSettings.Max.ToString());
        }
    }

    void UpdateOSCPreview()
    {
        oscPreview.text = OscSettings.GetAddress();
    }

    void SetFieldsToControllerValues(OSCControllerSettings _settings)
    {
        AddressTypeMenuChange(_settings);

        midiChannelDropdown.SetValueWithoutNotify(_settings.MidiChannel.GetInt());
        addressTypeDropdown.SetValueWithoutNotify(_settings.AddressType.GetInt());
        valueRangeDropdown.SetValueWithoutNotify(_settings.Range.GetInt());
        ccChannelField.SetTextWithoutNotify(_settings.CCNumber.ToString());
        minField.SetTextWithoutNotify(_settings.Min.ToString());
        maxField.SetTextWithoutNotify(_settings.Max.ToString());

        UpdateOSCPreview();

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

            pair.Key.RefreshShownValue();
        }
    }
}
