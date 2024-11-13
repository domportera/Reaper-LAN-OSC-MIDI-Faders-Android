using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;

/// <summary>
/// Used for saving and loading OSC Controller settings templates
/// </summary>
[System.Serializable]
public class OscControllerSettingsTemplate
{
    [FormerlySerializedAs("name")] public string Name;
    [FormerlySerializedAs("oscSettings")] public OscControllerSettings OscSettings;

    public OscControllerSettingsTemplate(string name, OscControllerSettings oscSettings)
    {
        this.Name = name;
        this.OscSettings = oscSettings;
    }
}

/// <summary>
/// Holds all the data needed to send osc messages with easily built addresses and value ranges
/// </summary>
[System.Serializable]
public class OscControllerSettings
{
    #region Saved Values
    [FormerlySerializedAs("channel")] [SerializeField]
    private MidiChannel _channel;
    [FormerlySerializedAs("range")] [SerializeField]
    private ValueRange _range;
    [FormerlySerializedAs("ccNumber")] [SerializeField]
    private int _ccNumber;
    [FormerlySerializedAs("addressType")] [SerializeField]
    private OscAddressType _addressType;
    [FormerlySerializedAs("min")] [SerializeField]
    private float _min;
    [FormerlySerializedAs("max")] [SerializeField]
    private float _max;
    [FormerlySerializedAs("customAddress")] [SerializeField]
    private string _customAddress;
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

    private const string ReaperMidiBaseAddress = "/vkb_midi/";
    public const string MidiChannelString = "MIDI_CHANNEL/"; //string of characters to insert the midi channel
    public const string CcChannelString = "CC_NUMBER"; //string of characters to insert the CC channel

    private static readonly Dictionary<OscAddressType, string> AddressesBuiltIn = new Dictionary<OscAddressType, string>()
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

    public static readonly Dictionary<OscAddressType, OscControllerSettings> DefaultOscTemplates = new Dictionary<OscAddressType, OscControllerSettings>()
    {
        { OscAddressType.MidiPitch,         new OscControllerSettings(OscAddressType.MidiPitch,         MidiChannel.All, ValueRange.FourteenBit,    0) },
        { OscAddressType.MidiAftertouch,    new OscControllerSettings(OscAddressType.MidiAftertouch,    MidiChannel.All, ValueRange.SevenBit,       0) },
        { OscAddressType.MidiCc,            new OscControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       1) }

    };

    #endregion

    public OscControllerSettings(OscAddressType addressType, MidiChannel channel, ValueRange range, int ccNumber)
    {
        this._channel = channel;
        this._range = range;
        this._ccNumber = ccNumber;
        this._addressType = addressType;
        SetRange(range);
    }

    public OscControllerSettings(OscControllerSettings template)
    {
        this._channel = template.MidiChannel;
        this._addressType = template.AddressType;
        this._ccNumber = template.CcNumber;
        this._range = template.Range;
        this._customAddress = template.CustomAddress;
        this._min = template.Min;
        this._max = template.Max;
    }

    public bool IsEqualTo(OscControllerSettings settings)
    {
        var identical = settings.CcNumber == CcNumber && settings.AddressType == AddressType && settings.MidiChannel == MidiChannel && settings.CustomAddress == CustomAddress && settings.Range == Range && settings._min == _min && settings._max == _max && settings.MidiChannel == settings.MidiChannel;
        return identical;
    }

    public void SetOscAddressType(OscAddressType addressType)
    {
        this._addressType = addressType;
    }

    public void SetCcNumber(int cc)
    {
        _ccNumber = Mathf.Clamp(cc, MinCc, MaxCc);
    }

    public void SetMidiChannel(MidiChannel channel)
    {
        this._channel = channel;
    }

    public void SetCustomAddress(string address)
    {
        _customAddress = address;
    }

    public void SetRange(ValueRange range)
    {
        this._range = range;
        switch(range)
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

    public void SetMin(float min)
    {
        this._min = min;
    }

    public void SetMax(float max)
    {
        this._max = max;
    }

    public string GetAddress()
    {
        if(_addressType == OscAddressType.Custom)
        {
            return _customAddress;
        }

        return CreateBuiltInAddress(_addressType);
    }

    public int GetValueInt(float value)
    {
        return Mathf.RoundToInt(GetValueFloat(value));
    }

    public float GetValueFloat(float value)
    {
        return Mathf.Clamp(value.Map(Controller.MinControllerValue, Controller.MaxControllerValue, _min, _max), Min, Max);
    }

    private bool AddressTypeIsMidi(OscAddressType addressType)
    {
        return AddressModes[addressType] == OscAddressMode.Midi;
    }

    private string CreateBuiltInAddress(OscAddressType type)
    {
        var address = string.Empty;

        if(AddressesBuiltIn.ContainsKey(type))
        {
            address = AddressesBuiltIn[type];
        }
        else
        {
            Debug.LogError($"OSC Address type {type} not added to addresses built in!");
        }

        var shouldReplaceAddressChannels = AddressTypeIsMidi(type) && !string.IsNullOrWhiteSpace(address);
        if(shouldReplaceAddressChannels)
        {
            var addressReplacement = MidiChannel == MidiChannel.All ? "" : ((int)_channel).ToString() + '/';
            address = address.Replace(MidiChannelString, addressReplacement);

            if(type == OscAddressType.MidiCc)
            {
                address = address.Replace(CcChannelString, _ccNumber.ToString());
            }
        }

        return address;
    }
}