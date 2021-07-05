using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ControllerSettings
{
    public ControlType controlType { get; private set; }
    string address;
    public string name { get; private set; }
    string controlObjectName;
    public MIDIChannel channel { get; private set; }
    public ValueRange range { get; private set; }
    public DefaultValueType defaultType { get; private set; }
    public int min { get; private set; }
    public int max { get; private set; }
    public int mid { get; private set; }
    public int defaultValue { get; private set; }
    public float smoothTime { get; private set; }
    public CurveType curveType { get; private set; }
    public UnityEvent OnUpdate = new UnityEvent();
    public AddressType addressType { get; private set; }
    public int ccNumber { get; private set; }
    int id; //not currently in use, could be useful some day?

    public const int NULL_POSITION = -1;

    [SerializeField] int position = NULL_POSITION;

    public string profileName { get; private set; }

    public ControllerSettings(ControllerSettings _c)
    {
        SetVariables(_c.name, _c.profileName, _c.controlType, _c.addressType, _c.range, _c.defaultType, _c.channel, _c.curveType, _c.ccNumber, _c.smoothTime);
        id = ControlsManager.GetUniqueID();
        position = _c.position;
    }

    public ControllerSettings(string _name, string _profileName, ControlType _controlType, AddressType _addressType, ValueRange _range, DefaultValueType _defaultValueType,
        MIDIChannel _channel, CurveType _curveType, int _ccNumber = -1, float _smoothTime = 0.1f)
    {
        SetVariables(_name, _profileName, _controlType, _addressType, _range, _defaultValueType, _channel, _curveType, _ccNumber, _smoothTime);
        id = ControlsManager.GetUniqueID();
    }

    public void SetVariables(string _name, string _profileName, ControlType _controlType, AddressType _addressType, ValueRange _range, DefaultValueType _defaultValueType,
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
        name = _name;
        controlObjectName = _name + " " + _controlType;
        controlType = _controlType;
        range = _range;
        defaultType = _defaultValueType;
        smoothTime = _smoothTime;
        curveType = _curveType;
        addressType = _addressType;
        profileName = _profileName;
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

    public void SetPosition(int _index)
    {
        position = _index;
    }

    public int GetPosition()
    {
        return position;
    }

    public int GetRange()
    {
        return Mathf.Abs(max - min) + 1; //since we're using  ints, 0-127 is actually 128 total values, so +1
    }

    public string GetAddress()
    {
        return address;
    }
}
