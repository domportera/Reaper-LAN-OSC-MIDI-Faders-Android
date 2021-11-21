using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;

[System.Serializable]
public class OSCControllerSettings
{
    [SerializeField] MIDIChannel channel;
    [SerializeField] ValueRange range;
    [SerializeField] int ccNumber;
    [SerializeField] OSCAddressType addressType;

    [SerializeField] string address;

    [SerializeField] int min;
    [SerializeField] int max;

    const string REAPER_MIDI_BASE_ADDRESS = "/vkb_midi/";
    const string MIDI_CHANNEL_STRING = "#C#"; //string of characters to insert the midi channel
    const string CC_CHANNEL_STRING = "#CC#"; //string of characters to insert the CC channel

    readonly Dictionary<OSCAddressType, string> addressesBuiltIn = new Dictionary<OSCAddressType, string>()
    {
        { OSCAddressType.MidiCC, REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "cc/" + CC_CHANNEL_STRING },
        { OSCAddressType.MidiAftertouch,  REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "channelPressure" },
        { OSCAddressType.MidiPitch, REAPER_MIDI_BASE_ADDRESS + MIDI_CHANNEL_STRING + "pitch" }
    };

    public OSCControllerSettings(MIDIChannel channel, ValueRange range, OSCAddressType addressType, int ccNumber, string address)
    {
        this.channel = channel;
        this.range = range;
        this.ccNumber = ccNumber;
        this.address = address;
        this.addressType = addressType;
    }

    public string GetAddress()
    {
        return address;
    }

    void SetOSCAddressType(OSCAddressType _addressType)
    {
        addressType = _addressType;
        if(addressesBuiltIn.ContainsKey(_addressType))
        {
            address = addressesBuiltIn[_addressType];
        }
        else
        {
            address = ""; //set to nothing - this is a custom address
        }
    }

    void SetCCNumber(int _cc)
    {
        if (addressType != OSCAddressType.MidiCC)
        {
            Debug.LogError($"Can't set CC value if OSC address type is not CC");
            return;
        }

        address = address.Replace(CC_CHANNEL_STRING, _cc.ToString());
    }

    void SetMIDIChannel(MIDIChannel _channel)
    {
        if()
    }

    bool AddressTypeIsMIDI(OSCAddressType _addressType)
    {
        return addressType == OSCAddressType.MidiCC || addressType == OSCAddressType.MidiAftertouch || _addressType == OSCAddressType.MidiPitch;
    }

    void SetMIDIAddress(OSCAddressType _type, MIDIChannel _channel, int _ccNumber)
    {
        //add channel if not set to all channels
        address = "/vkb_midi/" + (_channel == MIDIChannel.All ? "" : (int)_channel + "/");

        //Add additional channel information for each midi address type
        switch (_type)
        {
            case OSCAddressType.MidiCC:
                if (_ccNumber < 0 || _ccNumber > 127)
                {
                    _ccNumber = 127;
                }
                address += "cc/" + _ccNumber;
                break;
            case OSCAddressType.MidiAftertouch:
                address += "channelPressure";
                break;
            case OSCAddressType.MidiPitch:
                address += "pitch";
                break;
            default:
                Debug.LogError("MIDI Address type not implemented!");
                break;
        }

        ccNumber = _ccNumber;
    }


    void SetRange(ValueRange _range)
    {
        switch (_range)
        {
            case ValueRange.SevenBit:
                min = 0;
                max = 127;
                break;
            case ValueRange.FourteenBit:
                min = 0;
                max = 16383;
                break;
            default:
                min = 0;
                max = 127;
                Debug.LogError("Value range not implemented! Defaulting to 7-bit 0-127");
                break;
        }
    }
}
