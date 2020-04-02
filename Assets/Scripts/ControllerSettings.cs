using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ControllerSettings
{
    public ControlType controlType;
    string address;
    public string name;
    string controlObjectName;
    public MIDIChannel channel;
    public ValueRange range;
    public DefaultValueType defaultType;
    public int min;
    public int max;
    public int mid;
    public int defaultValue;
    public float smoothTime;
    public CurveType curveType;
    public AnimationCurve valueCurve;
    public UnityEvent OnUpdate = new UnityEvent();
    public AddressType addressType;
    public int ccNumber;
    int id;

    public ControllerSettings(string _name, ControlType _controlType, AddressType _addressType, ValueRange _range, DefaultValueType _defaultValueType,
        MIDIChannel _channel, CurveType _curveType, int _ccNumber = -1, float _smoothTime = 0.1f)
    {
        SetVariables(_name, _controlType, _addressType, _range, _defaultValueType, _channel, _curveType, _ccNumber, _smoothTime);

        id = ControlsManager.GetUniqueID();
    }

    public void SetVariables(string _name, ControlType _controlType, AddressType _addressType, ValueRange _range, DefaultValueType _defaultValueType,
        MIDIChannel _channel, CurveType _curveType, int _ccNumber = -1, float _smoothTime = 0.1f)
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
        name = _name;
        controlObjectName = _name + " " + _controlType;
        controlType = _controlType;
        range = _range;
        defaultType = _defaultValueType;
        smoothTime = _smoothTime;
        curveType = _curveType;
        addressType = _addressType;
    }

    public int GetID()
    {
        return id;
    }

    public void SetNewID()
    {
        id = ControlsManager.GetUniqueID();
    }

    void SetAddress(AddressType _type, int _ccNumber)
    {
        switch (_type)
        {
            case AddressType.CC:
                if (_ccNumber < 0 || _ccNumber > 127)
                {
                    Debug.LogError("Invalid CC value! Setting to 127.");
                    _ccNumber = 127;
                }
                address += "cc/" + _ccNumber;
                break;
            case AddressType.Aftertouch:
                address += "channelPressure";
                break;
            case AddressType.Pitch:
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
            case ValueRange.EightBit:
                min = 0;
                max = 255;
                Debug.LogError("Untested! not recommended, please use only if values 0-255 are what you need.");
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

    public int GetRange()
    {
        return Mathf.Abs(max - mid);
    }

    public string GetAddress()
    {
        return address;
    }
}
