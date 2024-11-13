using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using UnityEngine.Serialization;
using PopUpWindows;
using static UnityEngine.UI.Dropdown;

public class OscSelectionMenu : OptionsMenu
{
    #region UI elements
    [FormerlySerializedAs("dawCommandsParent")] [SerializeField]
    private RectTransform _dawCommandsParent;
    [FormerlySerializedAs("userCommandsParent")] [SerializeField]
    private RectTransform _userCommandsParent;
    [FormerlySerializedAs("oscSettingsButtonPrefab")] [SerializeField]
    private GameObject _oscSettingsButtonPrefab;
    [FormerlySerializedAs("saveAsButton")] [SerializeField]
    private Button _saveAsButton;
    [FormerlySerializedAs("backButton")] [SerializeField]
    private Button _backButton;
    #endregion UI elements

    #region Built-in OSC Message Option Fields
    [FormerlySerializedAs("midiChannelDropdown")] [SerializeField]
    private Dropdown _midiChannelDropdown;
    [FormerlySerializedAs("addressTypeDropdown")] [SerializeField]
    private Dropdown _addressTypeDropdown;
    [FormerlySerializedAs("valueRangeDropdown")] [SerializeField]
    private Dropdown _valueRangeDropdown;
    [FormerlySerializedAs("ccChannelField")] [SerializeField]
    private InputField _ccChannelField;
    [FormerlySerializedAs("customAddressField")] [SerializeField]
    private InputField _customAddressField;
    [FormerlySerializedAs("oscPreview")] [SerializeField]
    private Text _oscPreview;
    [FormerlySerializedAs("minField")] [SerializeField]
    private InputField _minField;
    [FormerlySerializedAs("maxField")] [SerializeField]
    private InputField _maxField;
    #endregion Built-in OSC Message Option Fields

    public OscControllerSettings OscSettings { get; private set; }
    private OscControllerSettings _originalSettings;

    public ControllerOptionsMenu LastToEdit { get; private set; }

    private const string Subfolder = "OSCSettings";
    private const string FileExtension = ".OSCTemplate";
    private readonly Queue<Action> _actionQueue = new();

    private void Awake()
    {
        InitializeOscEditingUIElements();
        InitializeBuiltInMessagePresets();
        InitializeUserTemplates();
        PopulateDropdowns();
        _saveAsButton.onClick.AddListener(SaveAsButton);
        _backButton.onClick.AddListener(BackButton);
    }

    private void Update()
    {
        while(_actionQueue.TryDequeue(out var action))
            action.Invoke();
    }

    private void DoNextFrame(Action action)
    {
        _actionQueue.Enqueue(action);
    }

    public void Initialize(OscControllerSettings oscSettings, ControllerOptionsMenu optionsMenu)
    {
        OscSettings = new OscControllerSettings(oscSettings); //create copies so any edits aren't automatically applied
        LastToEdit = optionsMenu;
        _originalSettings = new OscControllerSettings(oscSettings);
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

    private void VerifyCustomAddressField(string input)
    {
        var warning = "Address cannot contain the following strings:";
        var containsError = false;
        var bannedStrings = new string[] { OscControllerSettings.CcChannelString , OscControllerSettings.MidiChannelString };

        foreach(var s in bannedStrings)
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

    private void BackButton()
    {
        LastToEdit.StageOscChangesToApply(OscSettings);
        gameObject.SetActive(false);
    }

    private void InitializeBuiltInMessagePresets()
    {
        foreach(var set in OscControllerSettings.DefaultOscTemplates)
        {
            var template = new OscControllerSettingsTemplate(set.Key.GetDescription(), set.Value);
            CreateOscCommandButton(template, _dawCommandsParent);
        }
    }

    private OscCommandButton CreateOscCommandButton(OscControllerSettingsTemplate template, Transform parent)
    {
        var button = Instantiate(_oscSettingsButtonPrefab, parent, false).GetComponent<OscCommandButton>();
        Action longPressAction = parent == _userCommandsParent ? () => DeleteTemplatePrompt(template, button) : null;
        button.Initialize(() => OscSelectionButtonPressed(template), longPressAction, template.Name, template.OscSettings.GetAddress());
        return button;
    }

    private void DeleteTemplatePrompt(OscControllerSettingsTemplate template, OscCommandButton button)
    {
        PopUpController.Instance.ConfirmationWindow($"Delete template \"{template.Name}\"?", () => DeleteTemplate(template, button), null, "Delete", "Cancel");
    }

    private void DeleteTemplate(OscControllerSettingsTemplate template, OscCommandButton button)
    {
        //delete file
        var path = Path.Combine(Application.persistentDataPath, Subfolder, template.Name) + FileExtension;
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

    private void SortUserButtons()
    {
        var userButtons = _userCommandsParent.GetComponentsInChildren<OscCommandButton>();
        userButtons = userButtons.OrderBy(button => button.name).ToArray();

        foreach(var b in userButtons)
        {
            b.transform.SetParent(null);
            b.transform.SetParent(_userCommandsParent);
            Debug.Log($"Sorting {b.name}");
        }
    }

    private void OscSelectionButtonPressed(OscControllerSettingsTemplate template)
    {
        OscSettings = new OscControllerSettings(template.OscSettings);

        UnityAction confirm = () =>
        {
            SetFieldsToControllerValues(OscSettings);
            AddressTypeMenuChange(OscSettings);
            _originalSettings = new OscControllerSettings(OscSettings);
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

    private bool ShouldEnableMinMaxFields(OscControllerSettings settings)
    {
        return settings.Range == ValueRange.CustomFloat || settings.Range == ValueRange.CustomInt;
    }

    private void AddressTypeMenuChange(OscControllerSettings settings)
    {
        var enableMinMax = ShouldEnableMinMaxFields(settings);
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

    private void InitializeUserTemplates()
    {
        var templates = LoadUserTemplates();

        foreach(var t in templates)
        {
            CreateOscCommandButton(t, _userCommandsParent);
        }
    }

    private static List<OscControllerSettingsTemplate> LoadUserTemplates()
    {
        var path = Path.Combine(Application.persistentDataPath, Subfolder);
        var templates = FileHandler.LoadAllJsonObjects<OscControllerSettingsTemplate>(path, FileExtension);
        return templates;
    }

    private void SaveAsButton()
    {
        PopUpController.Instance.TextInputWindow("Enter a name for your OSC template:", SaveAsConfirmed, null, "Save", "Cancel");
    }

    private void SaveAsConfirmed(string input)
    {
        var invalid = FileHandler.ContainsInvalidFileNameCharacters(input, out var invalidCharacters);

        if(!invalid)
        {
            var template = SaveTemplate(input, OscSettings);
           // OSCCommandButton button = CreateOscCommandButton(template, _userCommandsParent);
            DoNextFrame(SortUserButtons);
        }
        else
        {
            var invalidCharString = new string(invalidCharacters.ToArray());
            PopUpController.Instance.ErrorWindow($"Template name contains invalid characters: {invalidCharString}\nPlease use a name suited for a file name.", SaveAsButton);
            Debug.LogError($"Invalid file name: {input}. Offending characters: {invalidCharString}");
        }
    }

    private OscControllerSettingsTemplate SaveTemplate(string templateName, OscControllerSettings settings)
    {
        var template = new OscControllerSettingsTemplate(templateName, settings);
        var path = Path.Combine(Application.persistentDataPath, Subfolder);
        var success = FileHandler.SaveJsonObject(template, path, templateName, FileExtension);

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

    private void ChangeMidiChannel(int val)
    {
        var channel = (MidiChannel)val;
        OscSettings.SetMidiChannel(channel);
        Debug.Log($"Set MIDI Channel as {channel}", this);
    }

    private void ChangeAddressType(int val)
    {
        var type = (OscAddressType)val;
        OscSettings.SetOscAddressType(type);
        AddressTypeMenuChange(OscSettings);
        Debug.Log($"Set Address Type as {type}", this);
    }

    private void ChangeValueRange(int val)
    {
        var range = (ValueRange)val;
        OscSettings.SetRange(range);
        Debug.Log($"Set Value Range as {range}", this);

        var enableMinMax = range == ValueRange.CustomFloat || range == ValueRange.CustomInt;
        ToggleMinMaxFields(enableMinMax);
    }

    private void ChangeCcNumber(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
        {
            return;
        }
        var ccNum = int.Parse(input);
        ccNum = Mathf.Clamp(ccNum, OscControllerSettings.MinCc, OscControllerSettings.MaxCc);
        _ccChannelField.SetTextWithoutNotify(ccNum.ToString());
        OscSettings.SetCcNumber(ccNum);
        Debug.Log($"Set CC Number to {ccNum}", this);
    }

    private void ChangeMin(string valAsString)
    {
        var val = float.Parse(valAsString);
        if(OscSettings.Range == ValueRange.CustomInt)
        {
            var intVal = (int)val;
            OscSettings.SetMin(intVal);
            _minField.SetTextWithoutNotify((intVal).ToString());
            Debug.Log($"Set Min as {intVal}", this);
        }
        else
        {
            OscSettings.SetMin(val);
            Debug.Log($"Set Min as {val}", this);
        }

    }

    private void ChangeMax(string valAsString)
    {
        var val = float.Parse(valAsString);
        if(OscSettings.Range == ValueRange.CustomInt)
        {
            var intVal = (int)val;
            OscSettings.SetMax(intVal);
            _maxField.SetTextWithoutNotify((intVal).ToString());
            Debug.Log($"Set Max as {intVal}", this);
        }
        else
        {
            OscSettings.SetMax(val);
            Debug.Log($"Set Max as {val}", this);
        }
    }

    private void ToggleMinMaxFields(bool on)
    {
        ToggleUIObject(_minField, on);
        ToggleUIObject(_maxField, on);
        
        if(on)
        {
            _minField.SetTextWithoutNotify(OscSettings.Min.ToString());
            _maxField.SetTextWithoutNotify(OscSettings.Max.ToString());
        }
    }

    private void UpdateOscPreview()
    {
        _oscPreview.text = OscSettings.GetAddress();
    }

    private void SetFieldsToControllerValues(OscControllerSettings settings)
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

    private void PopulateDropdowns()
    {
        DropDownEntryNames.Add(_addressTypeDropdown, EnumUtility.GetOscAddressTypeNameArray());
        DropDownEntryNames.Add(_midiChannelDropdown, EnumUtility.GetMidiChannelNameArray());
        DropDownEntryNames.Add(_valueRangeDropdown, EnumUtility.GetValueRangeNameArray());

        foreach(var pair in DropDownEntryNames)
        {
            pair.Key.ClearOptions();
            foreach(var s in pair.Value)
            {
                pair.Key.options.Add(new OptionData(s));
            }

            pair.Key.RefreshShownValue();
        }
    }
}
