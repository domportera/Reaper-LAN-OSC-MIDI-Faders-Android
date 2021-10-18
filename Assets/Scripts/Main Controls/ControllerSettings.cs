using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ControllerSettings
{
    [SerializeField] ControlBehaviorType controlType;
    [SerializeField] InputMethod inputType;
    [SerializeField] MIDIChannel channel;
    [SerializeField] ValueRange range;
    [SerializeField] DefaultValueType defaultType;
    [SerializeField] int min;
    [SerializeField] int max;
    [SerializeField] int mid;
    [SerializeField] int defaultValue;
    [SerializeField] float smoothTime;
    [SerializeField] CurveType curveType;
    [SerializeField] int ccNumber;
    [SerializeField] OSCAddressType addressType;

    public ControlBehaviorType ControlType { get { return controlType; } }
    public InputMethod InputType { get { return inputType; } }
    public MIDIChannel Channel { get { return channel; } }
    public ValueRange Range { get { return range; } }
    public DefaultValueType DefaultType { get { return defaultType; } }
    public int Min { get { return min; } }
    public int Max { get { return max; } }
    public int Mid { get { return mid; } }
    public int DefaultValue { get { return defaultValue; } }
    public float SmoothTime { get { return smoothTime; } }
    public CurveType Curve { get { return curveType; } }
    public int CCNumber { get { return ccNumber; } }
    public OSCAddressType AddressType { get { return addressType; } }

    string address;

    public ControllerSettings(ControllerSettings _c)
    {
        SetVariables(_c.inputType, _c.controlType, _c.addressType, _c.range, _c.defaultType, _c.channel, _c.curveType, _c.ccNumber, _c.smoothTime);
    }

    public ControllerSettings(InputMethod _inputType, ControlBehaviorType _controlType, OSCAddressType _addressType, ValueRange _range, DefaultValueType _defaultValueType,
        MIDIChannel _channel, CurveType _curveType, int _ccNumber = -1, float _smoothTime = 0.1f)
    {
        SetVariables(_inputType, _controlType, _addressType, _range, _defaultValueType, _channel, _curveType, _ccNumber, _smoothTime);
    }

    public void SetVariables(InputMethod _inputType, ControlBehaviorType _controlType, OSCAddressType _addressType, ValueRange _range, DefaultValueType _defaultValueType,
        MIDIChannel _channel, CurveType _curveType, int _ccNumber, float _smoothTime)
    {
        //add channel if not set to all channels
        address = "/vkb_midi/" + (_channel == MIDIChannel.All ? "" : (int)_channel + "/");

        //sets address based on address type
        SetAddress(_addressType, _ccNumber);

        //assign min and max values from value range
        SetRange(_range);

        //sets default value to value type
        SetDefault(_defaultValueType);

        channel = _channel;
        controlType = _controlType;
        range = _range;
        defaultType = _defaultValueType;
        smoothTime = _smoothTime;
        curveType = _curveType;
        addressType = _addressType;
    }

    void SetAddress(OSCAddressType _type, int _ccNumber)
    {
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
                Debug.LogError("Address type not implemented!");
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

        mid = (max + min) / 2;
    }

    void SetDefault(DefaultValueType _defaultValueType)
    {
        switch (_defaultValueType)
        {
            case DefaultValueType.Min:
                defaultValue = min;
                break;
            case DefaultValueType.Mid:
                defaultValue = mid;
                break;
            case DefaultValueType.Max:
                defaultValue = max;
                break;
            default:
                defaultValue = min;
                Debug.LogError("Default value type not implemented! Defaulting to min.");
                break;
        }
    }

    #region Getters
    public int GetRange()
    {
        return Mathf.Abs(max - min) + 1; //since we're using  ints, 0-127 is actually 128 total values, so +1
    }

    public string GetAddress()
    {
        return address;
    }
    #endregion Getters
}
