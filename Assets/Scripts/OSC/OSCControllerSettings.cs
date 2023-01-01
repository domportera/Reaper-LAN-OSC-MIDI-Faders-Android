using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using DomsUnityHelper;
using UnityEngine.Serialization;

/// <summary>
/// Used for saving and loading OSC Controller settings templates
/// </summary>
[System.Serializable]
public class OSCControllerSettingsTemplate
{
    [FormerlySerializedAs("name")] public string Name;
    [FormerlySerializedAs("oscSettings")] public OSCControllerSettings OscSettings;

    public OSCControllerSettingsTemplate(string name, OSCControllerSettings oscSettings)
    {
        this.Name = name;
        this.OscSettings = oscSettings;
    }
}

/// <summary>
/// Holds all the data needed to send osc messages with easily built addresses and value ranges
/// </summary>
[System.Serializable]
public class OSCControllerSettings
{
    #region Saved Values
    [FormerlySerializedAs("channel")] [SerializeField] MidiChannel _channel;
    [FormerlySerializedAs("range")] [SerializeField] ValueRange _range;
    [FormerlySerializedAs("ccNumber")] [SerializeField] int _ccNumber;
    [FormerlySerializedAs("addressType")] [SerializeField] OscAddressType _addressType;
    [FormerlySerializedAs("min")] [SerializeField] float _min;
    [FormerlySerializedAs("max")] [SerializeField] float _max;
    [FormerlySerializedAs("customAddress")] [SerializeField] string _customAddress;
    #endregion Saved Values

    #region Properties
    public MidiChannel MidiChannel { get { return _channel; } }
    public ValueRange Range { get { return _range; } }
    public OscAddressType AddressType { get { return _addressType; } }
    public string CustomAddress { get { return _customAddress; } }
    public int CcNumber { get { return _ccNumber; } }
    public float Min { get { return _min; } }
    public float Max { get { return _max; } }
    #endregion Properties

    public const int MinCc = 0;
    public const int MaxCc = 127;

    #region Built-in addresses
    const string ReaperMidiBaseAddress = "/vkb_midi/";
    public const string MidiChannelString = "MIDI_CHANNEL/"; //string of characters to insert the midi channel
    public const string CcChannelString = "CC_NUMBER"; //string of characters to insert the CC channel

    static readonly Dictionary<OscAddressType, string> AddressesBuiltIn = new Dictionary<OscAddressType, string>()
    {
        { OscAddressType.MidiCc, ReaperMidiBaseAddress + MidiChannelString + "cc/" + CcChannelString },
        { OscAddressType.MidiAftertouch,  ReaperMidiBaseAddress + MidiChannelString + "channelPressure" },
        { OscAddressType.MidiPitch, ReaperMidiBaseAddress + MidiChannelString + "pitch" }
    };

    public static readonly Dictionary<OscAddressType, OscAddressMode> AddressModes = new Dictionary<OscAddressType, OscAddressMode>()
    {
        { OscAddressType.MidiCc, OscAddressMode.Midi },
        { OscAddressType.MidiAftertouch,  OscAddressMode.Midi },
        { OscAddressType.MidiPitch, OscAddressMode.Midi },
        { OscAddressType.Custom, OscAddressMode.Custom }
    };

    public static readonly Dictionary<OscAddressType, OSCControllerSettings> DefaultOscTemplates = new Dictionary<OscAddressType, OSCControllerSettings>()
    {
        { OscAddressType.MidiPitch,         new OSCControllerSettings(OscAddressType.MidiPitch,         MidiChannel.All, ValueRange.FourteenBit,    0) },
        { OscAddressType.MidiAftertouch,    new OSCControllerSettings(OscAddressType.MidiAftertouch,    MidiChannel.All, ValueRange.SevenBit,       0) },
        { OscAddressType.MidiCc,            new OSCControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       1) }

    };

    #endregion

    public OSCControllerSettings(OscAddressType addressType, MidiChannel channel, ValueRange range, int ccNumber)
    {
        this._channel = channel;
        this._range = range;
        this._ccNumber = ccNumber;
        this._addressType = addressType;
        SetRange(range);
    }

    public OSCControllerSettings(OSCControllerSettings _template)
    {
        this._channel = _template.MidiChannel;
        this._addressType = _template.AddressType;
        this._ccNumber = _template.CcNumber;
        this._range = _template.Range;
        this._customAddress = _template.CustomAddress;
        this._min = _template.Min;
        this._max = _template.Max;
    }

    public bool IsEqualTo(OSCControllerSettings _settings)
    {
        bool identical = _settings.CcNumber == CcNumber && _settings.AddressType == AddressType && _settings.MidiChannel == MidiChannel && _settings.CustomAddress == CustomAddress && _settings.Range == Range && _settings._min == _min && _settings._max == _max && _settings.MidiChannel == _settings.MidiChannel;
        return identical;
    }

    public void SetOscAddressType(OscAddressType _addressType)
    {
        this._addressType = _addressType;
    }

    public void SetCcNumber(int _cc)
    {
        _ccNumber = Mathf.Clamp(_cc, MinCc, MaxCc);
    }

    public void SetMidiChannel(MidiChannel _channel)
    {
        this._channel = _channel;
    }

    public void SetCustomAddress(string _address)
    {
        _customAddress = _address;
    }

    public void SetRange(ValueRange _range)
    {
        this._range = _range;
        switch(_range)
        {
            case ValueRange.SevenBit:
                _min = 0;
                _max = 127;
                break;
            case ValueRange.FourteenBit:
                _min = 0;
                _max = 16383;
                break;
            case ValueRange.Float:
                _min = 0f;
                _max = 1f;
                break;
            case ValueRange.EightBit:
                _min = 0;
                _max = 255;
                break;
            case ValueRange.CustomInt:
                _min = 0;
                _max = 127;
                break;
            case ValueRange.CustomFloat:
                _min = 0f;
                _max = 1f;
                break;
            default:
                _min = 0;
                _max = 127;
                Debug.LogError("Value range not implemented! Defaulting to 7-bit 0-127");
                break;
        }
    }

    public void SetMin(float _min)
    {
        this._min = _min;
    }

    public void SetMax(float _max)
    {
        this._max = _max;
    }

    public string GetAddress()
    {
        if(_addressType == OscAddressType.Custom)
        {
            return _customAddress;
        }

        return CreateBuiltInAddress(_addressType);
    }

    public int GetValueInt(float _value)
    {
        return Mathf.RoundToInt(GetValueFloat(_value));
    }

    public float GetValueFloat(float _value)
    {
        return Mathf.Clamp(_value.Map(Controller.MinControllerValue, Controller.MaxControllerValue, _min, _max), Min, Max);
    }

    bool AddressTypeIsMidi(OscAddressType _addressType)
    {
        return AddressModes[_addressType] == OscAddressMode.Midi;
    }

    string CreateBuiltInAddress(OscAddressType _type)
    {
        string address = string.Empty;

        if(AddressesBuiltIn.ContainsKey(_type))
        {
            address = AddressesBuiltIn[_type];
        }
        else
        {
            Debug.LogError($"OSC Address type {_type} not added to addresses built in!");
        }

        bool shouldReplaceAddressChannels = AddressTypeIsMidi(_type) && !string.IsNullOrWhiteSpace(address);
        if(shouldReplaceAddressChannels)
        {
            string addressReplacement = MidiChannel == MidiChannel.All ? "" : ((int)_channel).ToString() + '/';
            address = address.Replace(MidiChannelString, addressReplacement);

            if(_type == OscAddressType.MidiCc)
            {
                address = address.Replace(CcChannelString, _ccNumber.ToString());
            }
        }

        return address;
    }
}