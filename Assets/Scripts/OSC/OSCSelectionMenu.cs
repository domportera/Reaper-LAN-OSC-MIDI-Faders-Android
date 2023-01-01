using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DomsUnityHelper;
using UnityEngine.Serialization;
using PopUpWindows;
using static UnityEngine.UI.Dropdown;

public class OSCSelectionMenu : OptionsMenu
{
    #region UI elements
    [FormerlySerializedAs("dawCommandsParent")] [SerializeField] RectTransform _dawCommandsParent;
    [FormerlySerializedAs("userCommandsParent")] [SerializeField] RectTransform _userCommandsParent;
    [FormerlySerializedAs("oscSettingsButtonPrefab")] [SerializeField] GameObject _oscSettingsButtonPrefab;
    [FormerlySerializedAs("saveAsButton")] [SerializeField] Button _saveAsButton;
    [FormerlySerializedAs("backButton")] [SerializeField] Button _backButton;
    #endregion UI elements

    #region Built-in OSC Message Option Fields
    [FormerlySerializedAs("midiChannelDropdown")] [SerializeField] Dropdown _midiChannelDropdown;
    [FormerlySerializedAs("addressTypeDropdown")] [SerializeField] Dropdown _addressTypeDropdown;
    [FormerlySerializedAs("valueRangeDropdown")] [SerializeField] Dropdown _valueRangeDropdown;
    [FormerlySerializedAs("ccChannelField")] [SerializeField] InputField _ccChannelField;
    [FormerlySerializedAs("customAddressField")] [SerializeField] InputField _customAddressField;
    [FormerlySerializedAs("oscPreview")] [SerializeField] Text _oscPreview;
    [FormerlySerializedAs("minField")] [SerializeField] InputField _minField;
    [FormerlySerializedAs("maxField")] [SerializeField] InputField _maxField;
    #endregion Built-in OSC Message Option Fields

    public OSCControllerSettings OscSettings { get; private set; }
    OSCControllerSettings _originalSettings;

    public ControllerOptionsMenu LastToEdit { get; private set; }

    const string Subfolder = "OSCSettings";
    const string FileExtension = ".OSCTemplate";

    private void Awake()
    {
        InitializeOscEditingUIElements();
        InitializeBuiltInMessagePresets();
        InitializeUserTemplates();
        PopulateDropdowns();
        _saveAsButton.onClick.AddListener(SaveAsButton);
        _backButton.onClick.AddListener(BackButton);
    }

    public void Initialize(OSCControllerSettings oscSettings, ControllerOptionsMenu optionsMenu)
    {
        OscSettings = new OSCControllerSettings(oscSettings); //create copies so any edits aren't automatically applied
        LastToEdit = optionsMenu;
        _originalSettings = new OSCControllerSettings(oscSettings);
        SetFieldsToControllerValues(OscSettings);
    }

    private void InitializeOscEditingUIElements()
    {
        _customAddressField.onEndEdit.AddListener(VerifyCustomAddressField);
        _minField.onEndEdit.AddListener(ChangeMin);
        _maxField.onEndEdit.AddListener(ChangeMax);

        _ccChannelField.onEndEdit.AddListener(val =>
        {
            ChangeCcNumber(val);
            UpdateOscPreview();
        });

        _addressTypeDropdown.onValueChanged.AddListener(val =>
        {
            ChangeAddressType(val);
            UpdateOscPreview();
        });


        _valueRangeDropdown.onValueChanged.AddListener(val =>
        {
            ChangeValueRange(val);
            UpdateOscPreview();
        });

        _midiChannelDropdown.onValueChanged.AddListener(val =>
        {
            ChangeMidiChannel(val);
            UpdateOscPreview();
        });
    }

    void VerifyCustomAddressField(string input)
    {
        string warning = "Address cannot contain the following strings:";
        bool containsError = false;
        string[] bannedStrings = new string[] { OSCControllerSettings.CcChannelString , OSCControllerSettings.MidiChannelString };

        foreach(string s in bannedStrings)
        {
            if(input.Contains(s))
            {
                if(containsError)
                    warning += ',';

                warning += " " + s;
                containsError = true;
            }
        }

        if(containsError)
        {
            PopUpController.Instance.ErrorWindow(warning);
        }
        else
        {
            OscSettings.SetCustomAddress(input);
        }
    }

    void BackButton()
    {
        LastToEdit.StageOscChangesToApply(OscSettings);
        gameObject.SetActive(false);
    }

    void InitializeBuiltInMessagePresets()
    {
        foreach(KeyValuePair<OscAddressType, OSCControllerSettings> set in OSCControllerSettings.DefaultOscTemplates)
        {
            OSCControllerSettingsTemplate template = new OSCControllerSettingsTemplate(set.Key.GetDescription(), set.Value);
            CreateOscCommandButton(template, _dawCommandsParent);
        }
    }

    OSCCommandButton CreateOscCommandButton(OSCControllerSettingsTemplate template, Transform parent)
    {
        OSCCommandButton button = Instantiate(_oscSettingsButtonPrefab, parent, false).GetComponent<OSCCommandButton>();
        Action longPressAction = parent == _userCommandsParent ? () => DeleteTemplatePrompt(template, button) : null;
        button.Initialize(() => OscSelectionButtonPressed(template), longPressAction, template.Name, template.OscSettings.GetAddress());
        return button;
    }

    void DeleteTemplatePrompt(OSCControllerSettingsTemplate template, OSCCommandButton button)
    {
        PopUpController.Instance.ConfirmationWindow($"Delete template \"{template.Name}\"?", () => DeleteTemplate(template, button), null, "Delete", "Cancel");
    }

    void DeleteTemplate(OSCControllerSettingsTemplate template, OSCCommandButton button)
    {
        //delete file
        string path = Path.Combine(Application.persistentDataPath, Subfolder, template.Name) + FileExtension;
        if(File.Exists(path))
        {
            File.Delete(path);
            button.DestroySelf();
        }
        else
        {
            Debug.LogError($"Tried to delete OSC template {template.Name} at {path} but could not find it");
        }
    }

    void SortUserButtons()
    {
        OSCCommandButton[] userButtons = _userCommandsParent.GetComponentsInChildren<OSCCommandButton>();
        userButtons = userButtons.OrderBy(button => button.name).ToArray();

        foreach(OSCCommandButton b in userButtons)
        {
            b.transform.SetParent(null);
            b.transform.SetParent(_userCommandsParent);
            Debug.Log($"Sorting {b.name}");
        }
    }

    void OscSelectionButtonPressed(OSCControllerSettingsTemplate template)
    {
        OscSettings = new OSCControllerSettings(template.OscSettings);

        UnityAction confirm = () =>
        {
            SetFieldsToControllerValues(OscSettings);
            AddressTypeMenuChange(OscSettings);
            _originalSettings = new OSCControllerSettings(OscSettings);
        };

        if(!template.OscSettings.IsEqualTo(_originalSettings))
        {
            PopUpController.Instance.ConfirmationWindow($"Override current settings to apply \"{template.Name}\" template?", confirm);
        }
        else
        {
            confirm.Invoke();
        }
    }

    bool ShouldEnableMinMaxFields(OSCControllerSettings settings)
    {
        return settings.Range == ValueRange.CustomFloat || settings.Range == ValueRange.CustomInt;
    }

    void AddressTypeMenuChange(OSCControllerSettings settings)
    {
        bool enableMinMax = ShouldEnableMinMaxFields(settings);
        switch(settings.AddressType)
        {
            case OscAddressType.MidiCc:
                _valueRangeDropdown.SetValueWithoutNotify((int)settings.Range);

                ToggleUIObject(_valueRangeDropdown, true);
                ToggleUIObject(_ccChannelField, true);
                ToggleUIObject(_customAddressField, false);
                ToggleUIObject(_oscPreview.transform, true);
                ToggleUIObject(_midiChannelDropdown, true);

                ToggleMinMaxFields(enableMinMax);
                break;
            case OscAddressType.MidiPitch:
                _valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.FourteenBit);
                settings.SetRange(ValueRange.FourteenBit);

                ToggleUIObject(_valueRangeDropdown, false);
                ToggleUIObject(_ccChannelField, false);
                ToggleUIObject(_customAddressField, false);
                ToggleUIObject(_oscPreview.transform, true);
                ToggleUIObject(_midiChannelDropdown, true);

                ToggleMinMaxFields(false);
                break;
            case OscAddressType.MidiAftertouch:
                _valueRangeDropdown.SetValueWithoutNotify((int)ValueRange.SevenBit);
                settings.SetRange(ValueRange.SevenBit);

                ToggleUIObject(_valueRangeDropdown, false);
                ToggleUIObject(_ccChannelField, false);
                ToggleUIObject(_customAddressField, false);
                ToggleUIObject(_oscPreview.transform, true);
                ToggleUIObject(_midiChannelDropdown, true);

                ToggleMinMaxFields(false);
                break;
            case OscAddressType.Custom:
                ToggleUIObject(_valueRangeDropdown, true);
                ToggleUIObject(_ccChannelField, false);
                ToggleUIObject(_midiChannelDropdown, false);
                ToggleUIObject(_customAddressField, true);
                ToggleUIObject(_oscPreview.transform, false);

                ToggleMinMaxFields(enableMinMax);
                break;
            default:
                Debug.LogError($"OSC Address Type {settings.AddressType} not implemented for menu change", this);
                return;
        }
    }

    void InitializeUserTemplates()
    {
        List<OSCControllerSettingsTemplate> templates = LoadUserTemplates();

        foreach(OSCControllerSettingsTemplate t in templates)
        {
            CreateOscCommandButton(t, _userCommandsParent);
        }
    }

    static List<OSCControllerSettingsTemplate> LoadUserTemplates()
    {
        string path = Path.Combine(Application.persistentDataPath, Subfolder);
        List<OSCControllerSettingsTemplate> templates = FileHandler.LoadAllJsonObjects<OSCControllerSettingsTemplate>(path, FileExtension);
        return templates;
    }

    void SaveAsButton()
    {
        PopUpController.Instance.TextInputWindow("Enter a name for your OSC template:", SaveAsConfirmed, null, "Save", "Cancel");
    }

    void SaveAsConfirmed(string input)
    {
        bool invalid = FileHandler.ContainsInvalidFileNameCharacters(input, out List<char> invalidCharacters);

        if(!invalid)
        {
            OSCControllerSettingsTemplate template = SaveTemplate(input, OscSettings);
           // OSCCommandButton button = CreateOscCommandButton(template, _userCommandsParent);
            DoNextFrame(SortUserButtons);
        }
        else
        {
            string invalidCharString = new string(invalidCharacters.ToArray());
            PopUpController.Instance.ErrorWindow($"Template name contains invalid characters: {invalidCharString}\nPlease use a name suited for a file name.", SaveAsButton);
            Debug.LogError($"Invalid file name: {input}. Offending characters: {invalidCharString}");
        }
    }

    OSCControllerSettingsTemplate SaveTemplate(string templateName, OSCControllerSettings settings)
    {
        var template = new OSCControllerSettingsTemplate(templateName, settings);
        string path = Path.Combine(Application.persistentDataPath, Subfolder);
        bool success = FileHandler.SaveJsonObject(template, path, templateName, FileExtension);

        if(success)
        {
            PopUpController.Instance.QuickNoticeWindow($"Saved OSC Template {templateName} successfully!");
        }
        else
        {
            PopUpController.Instance.ErrorWindow($"Error saving OSC Template {templateName}.\nYou can check the log for more details.");
        }

        return template;
    }

    void ChangeMidiChannel(int val)
    {
        MidiChannel channel = (MidiChannel)val;
        OscSettings.SetMidiChannel(channel);
        Log($"Set MIDI Channel as {channel}", this);
    }

    void ChangeAddressType(int val)
    {
        OscAddressType type = (OscAddressType)val;
        OscSettings.SetOscAddressType(type);
        AddressTypeMenuChange(OscSettings);
        Log($"Set Address Type as {type}", this);
    }

    void ChangeValueRange(int val)
    {
        ValueRange range = (ValueRange)val;
        OscSettings.SetRange(range);
        Log($"Set Value Range as {range}", this);

        bool enableMinMax = range == ValueRange.CustomFloat || range == ValueRange.CustomInt;
        ToggleMinMaxFields(enableMinMax);
    }

    void ChangeCcNumber(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
        {
            return;
        }
        int ccNum = int.Parse(input);
        ccNum = Mathf.Clamp(ccNum, OSCControllerSettings.MinCc, OSCControllerSettings.MaxCc);
        _ccChannelField.SetTextWithoutNotify(ccNum.ToString());
        OscSettings.SetCcNumber(ccNum);
        Log($"Set CC Number to {ccNum}", this);
    }

    void ChangeMin(string valAsString)
    {
        float val = float.Parse(valAsString);
        if(OscSettings.Range == ValueRange.CustomInt)
        {
            int intVal = (int)val;
            OscSettings.SetMin(intVal);
            _minField.SetTextWithoutNotify((intVal).ToString());
            Log($"Set Min as {intVal}", this);
        }
        else
        {
            OscSettings.SetMin(val);
            Log($"Set Min as {val}", this);
        }

    }

    void ChangeMax(string valAsString)
    {
        float val = float.Parse(valAsString);
        if(OscSettings.Range == ValueRange.CustomInt)
        {
            int intVal = (int)val;
            OscSettings.SetMax(intVal);
            _maxField.SetTextWithoutNotify((intVal).ToString());
            Log($"Set Max as {intVal}", this);
        }
        else
        {
            OscSettings.SetMax(val);
            Log($"Set Max as {val}", this);
        }
    }

    void ToggleMinMaxFields(bool on)
    {
        ToggleUIObject(_minField, on);
        ToggleUIObject(_maxField, on);
        
        if(on)
        {
            _minField.SetTextWithoutNotify(OscSettings.Min.ToString());
            _maxField.SetTextWithoutNotify(OscSettings.Max.ToString());
        }
    }

    void UpdateOscPreview()
    {
        _oscPreview.text = OscSettings.GetAddress();
    }

    void SetFieldsToControllerValues(OSCControllerSettings settings)
    {
        AddressTypeMenuChange(settings);

        _midiChannelDropdown.SetValueWithoutNotify((int)settings.MidiChannel);
        _addressTypeDropdown.SetValueWithoutNotify((int)settings.AddressType);
        _valueRangeDropdown.SetValueWithoutNotify((int)settings.Range);
        _ccChannelField.SetTextWithoutNotify(settings.CcNumber.ToString());
        _minField.SetTextWithoutNotify(settings.Min.ToString(CultureInfo.InvariantCulture));
        _maxField.SetTextWithoutNotify(settings.Max.ToString(CultureInfo.InvariantCulture));

        UpdateOscPreview();

    }

    void PopulateDropdowns()
    {
        DropDownEntryNames.Add(_addressTypeDropdown, EnumUtility.GetOscAddressTypeNameArray());
        DropDownEntryNames.Add(_midiChannelDropdown, EnumUtility.GetMidiChannelNameArray());
        DropDownEntryNames.Add(_valueRangeDropdown, EnumUtility.GetValueRangeNameArray());

        foreach(KeyValuePair<Dropdown, string[]> pair in DropDownEntryNames)
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
