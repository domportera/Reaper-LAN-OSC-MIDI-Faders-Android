using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;

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
    #endregion

    public const float MIN_UNMAPPED = 0f;
    public const float MAX_UNMAPPED = 1f;

    #region Built-in addresses
    const string REAPER_MIDI_BASE_ADDRESS = "/vkb_midi/";
    const string MIDI_CHANNEL_STRING = "#C#$"; //string of characters to insert the midi channel
    const string CC_CHANNEL_STRING = "#CC#$"; //string of characters to insert the CC channel

    readonly Dictionary<OSCAddressType, string> addressesBuiltIn = new Dictionary<OSCAddressType, string>()
    {
        { OSCAddressType.MidiCC, REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "cc/" + CC_CHANNEL_STRING },
        { OSCAddressType.MidiAftertouch,  REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "channelPressure" },
        { OSCAddressType.MidiPitch, REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "pitch" }
    };

    readonly Dictionary<OSCAddressType, OSCAddressMode> addressModes = new Dictionary<OSCAddressType, OSCAddressMode>()
    {
        { OSCAddressType.MidiCC, OSCAddressMode.MIDI },
        { OSCAddressType.MidiAftertouch,  OSCAddressMode.MIDI },
        { OSCAddressType.MidiPitch, OSCAddressMode.MIDI }
    };
    #endregion

    public OSCControllerSettings(OSCAddressType addressType, MIDIChannel channel, ValueRange range, int ccNumber)
    {
        this.channel = channel;
        this.range = range;
        this.ccNumber = ccNumber;
        this.addressType = addressType;
    }

    public void SetOSCAddressType(OSCAddressType _addressType)
    {
        addressType = _addressType;
    }

    public void SetCCNumber(int _cc)
    {
        ccNumber = Mathf.Clamp(_cc, 0, 127);
    }

    public void SetMIDIChannel(MIDIChannel _channel)
    {
        channel = _channel;
    }

    public void SetCustomAddress(string _address)
    {
        customAddress = _address;
    }

    public void SetRange(ValueRange _range, float _minCustom = 0, float _maxCustom = 0)
    {
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
                min = _minCustom;
                break;
            case ValueRange.CustomFloat:
                max = _maxCustom;
                break;
            default:
                min = 0;
                max = 127;
                Debug.LogError("Value range not implemented! Defaulting to 7-bit 0-127");
                break;
        }
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
        return _value.Map(MIN_UNMAPPED, MAX_UNMAPPED, min, max);
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
            Debug.LogError($"OSC Address type {_type} not implemented!");
        }

        if(AddressTypeIsMIDI(_type) && !string.IsNullOrWhiteSpace(address))
        {
            address.Replace(MIDI_CHANNEL_STRING, ((int)channel).ToString());

            if(_type == OSCAddressType.MidiCC)
            {
                address.Replace(CC_CHANNEL_STRING, ccNumber.ToString());
            }
        }

        return address;
    }
}