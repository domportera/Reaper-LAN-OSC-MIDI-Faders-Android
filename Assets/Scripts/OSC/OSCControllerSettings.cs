using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;

/// <summary>
/// Used for saving and loading OSC Controller settings templates
/// </summary>
[System.Serializable]
public class OSCControllerSettingsTemplate
{
    public string name;
    public OSCControllerSettings oscSettings;

    public OSCControllerSettingsTemplate(string name, OSCControllerSettings oscSettings)
    {
        this.name = name;
        this.oscSettings = oscSettings;
    }
}

/// <summary>
/// Holds all the data needed to send osc messages with easily built addresses and value ranges
/// </summary>
[System.Serializable]
public class OSCControllerSettings
{
    #region Saved Values
    [SerializeField] MIDIChannel channel;
    [SerializeField] ValueRange range;
    [SerializeField] int ccNumber;
    [SerializeField] OSCAddressType addressType;
    [SerializeField] float min;
    [SerializeField] float max;
    [SerializeField] string customAddress;
    #endregion Saved Values

    #region Properties
    public MIDIChannel MidiChannel { get { return channel; } }
    public ValueRange Range { get { return range; } }
    public OSCAddressType AddressType { get { return addressType; } }
    public string CustomAddress { get { return customAddress; } }
    public int CCNumber { get { return ccNumber; } }
    public float Min { get { return min; } }
    public float Max { get { return max; } }
    #endregion Properties

    public const int MIN_CC = 0;
    public const int MAX_CC = 127;

    #region Built-in addresses
    const string REAPER_MIDI_BASE_ADDRESS = "/vkb_midi/";
    public const string MIDI_CHANNEL_STRING = "MIDI_CHANNEL/"; //string of characters to insert the midi channel
    public const string CC_CHANNEL_STRING = "CC_NUMBER"; //string of characters to insert the CC channel

    static readonly Dictionary<OSCAddressType, string> addressesBuiltIn = new Dictionary<OSCAddressType, string>()
    {
        { OSCAddressType.MidiCC, REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "cc/" + CC_CHANNEL_STRING },
        { OSCAddressType.MidiAftertouch,  REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "channelPressure" },
        { OSCAddressType.MidiPitch, REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "pitch" }
    };

    public static readonly Dictionary<OSCAddressType, OSCAddressMode> addressModes = new Dictionary<OSCAddressType, OSCAddressMode>()
    {
        { OSCAddressType.MidiCC, OSCAddressMode.MIDI },
        { OSCAddressType.MidiAftertouch,  OSCAddressMode.MIDI },
        { OSCAddressType.MidiPitch, OSCAddressMode.MIDI },
        { OSCAddressType.Custom, OSCAddressMode.Custom }
    };

    public static readonly Dictionary<OSCAddressType, OSCControllerSettings> defaultOSCTemplates = new Dictionary<OSCAddressType, OSCControllerSettings>()
    {
        { OSCAddressType.MidiPitch,         new OSCControllerSettings(OSCAddressType.MidiPitch,         MIDIChannel.All, ValueRange.FourteenBit,    0) },
        { OSCAddressType.MidiAftertouch,    new OSCControllerSettings(OSCAddressType.MidiAftertouch,    MIDIChannel.All, ValueRange.SevenBit,       0) },
        { OSCAddressType.MidiCC,            new OSCControllerSettings(OSCAddressType.MidiCC,            MIDIChannel.All, ValueRange.SevenBit,       1) }

    };

    #endregion

    public OSCControllerSettings(OSCAddressType addressType, MIDIChannel channel, ValueRange range, int ccNumber)
    {
        this.channel = channel;
        this.range = range;
        this.ccNumber = ccNumber;
        this.addressType = addressType;
        SetRange(range);
    }

    public OSCControllerSettings(OSCControllerSettings _template)
    {
        this.channel = _template.MidiChannel;
        this.addressType = _template.AddressType;
        this.ccNumber = _template.CCNumber;
        this.range = _template.Range;
        this.customAddress = _template.CustomAddress;
        this.min = _template.Min;
        this.max = _template.Max;
    }

    public bool IsEqualTo(OSCControllerSettings _settings)
    {
        bool identical = _settings.CCNumber == CCNumber && _settings.AddressType == AddressType && _settings.MidiChannel == MidiChannel && _settings.CustomAddress == CustomAddress && _settings.Range == Range && _settings.min == min && _settings.max == max && _settings.MidiChannel == _settings.MidiChannel;
        return identical;
    }

    public void SetOSCAddressType(OSCAddressType _addressType)
    {
        addressType = _addressType;
    }

    public void SetCCNumber(int _cc)
    {
        ccNumber = Mathf.Clamp(_cc, MIN_CC, MAX_CC);
    }

    public void SetMIDIChannel(MIDIChannel _channel)
    {
        channel = _channel;
    }

    public void SetCustomAddress(string _address)
    {
        customAddress = _address;
    }

    public void SetRange(ValueRange _range)
    {
        range = _range;
        switch(_range)
        {
            case ValueRange.SevenBit:
                min = 0;
                max = 127;
                break;
            case ValueRange.FourteenBit:
                min = 0;
                max = 16383;
                break;
            case ValueRange.Float:
                min = 0f;
                max = 1f;
                break;
            case ValueRange.EightBit:
                min = 0;
                max = 255;
                break;
            case ValueRange.CustomInt:
                min = 0;
                max = 127;
                break;
            case ValueRange.CustomFloat:
                min = 0f;
                max = 1f;
                break;
            default:
                min = 0;
                max = 127;
                Debug.LogError("Value range not implemented! Defaulting to 7-bit 0-127");
                break;
        }
    }

    public void SetMin(float _min)
    {
        min = _min;
    }

    public void SetMax(float _max)
    {
        max = _max;
    }

    public string GetAddress()
    {
        if(addressType == OSCAddressType.Custom)
        {
            return customAddress;
        }

        return CreateBuiltInAddress(addressType);
    }

    public int GetValueInt(float _value)
    {
        return Mathf.RoundToInt(GetValueFloat(_value));
    }

    public float GetValueFloat(float _value)
    {
        return Mathf.Clamp(_value.Map(Controller.MIN_CONTROLLER_VALUE, Controller.MAX_CONTROLLER_VALUE, min, max), Min, Max);
    }

    bool AddressTypeIsMIDI(OSCAddressType _addressType)
    {
        return addressModes[_addressType] == OSCAddressMode.MIDI;
    }

    string CreateBuiltInAddress(OSCAddressType _type)
    {
        string address = string.Empty;

        if(addressesBuiltIn.ContainsKey(_type))
        {
            address = addressesBuiltIn[_type];
        }
        else
        {
            Debug.LogError($"OSC Address type {_type} not added to addresses built in!");
        }

        bool shouldReplaceAddressChannels = AddressTypeIsMIDI(_type) && !string.IsNullOrWhiteSpace(address);
        if(shouldReplaceAddressChannels)
        {
            string addressReplacement = MidiChannel == MIDIChannel.All ? "" : ((int)channel).ToString() + '/';
            address = address.Replace(MIDI_CHANNEL_STRING, addressReplacement);

            if(_type == OSCAddressType.MidiCC)
            {
                address = address.Replace(CC_CHANNEL_STRING, ccNumber.ToString());
            }
        }

        return address;
    }
}