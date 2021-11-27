using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ControllerSettings
{
    [SerializeField] ReleaseBehaviorType releaseBehavior;
    [SerializeField] InputMethod inputType;
    [SerializeField] DefaultValueType defaultType;
    [SerializeField] float defaultValue;
    [SerializeField] float smoothTime;
    [SerializeField] CurveType curveType;
    [SerializeField] OSCControllerSettings oscSettings;


    public ReleaseBehaviorType ReleaseBehavior { get { return releaseBehavior; } }
    public InputMethod InputType { get { return inputType; } }
    public DefaultValueType DefaultType { get { return defaultType; } }
    public float DefaultValue { get { return defaultValue; } }
    public float SmoothTime { get { return smoothTime; } }
    public CurveType Curve { get { return curveType; } }
    public OSCControllerSettings OscSettings { get { return oscSettings; } }

    public ControllerSettings(ControllerSettings _c)
    {
        SetVariables(_c.inputType, _c.releaseBehavior, _c.oscSettings, _c.defaultType, _c.curveType, _c.smoothTime);
    }

    public ControllerSettings(InputMethod _inputType, ReleaseBehaviorType _controlType, OSCControllerSettings _oscSettings, DefaultValueType _defaultValueType, CurveType _curveType,  float _smoothTime = 0.1f)
    {
        SetVariables(_inputType, _controlType, _oscSettings, _defaultValueType, _curveType, _smoothTime);
    }

    public void SetVariables(InputMethod _inputType, ReleaseBehaviorType _controlType, OSCControllerSettings _oscSettings,  DefaultValueType _defaultValueType, CurveType _curveType, float _smoothTime)
    {
        oscSettings = _oscSettings;

        //sets default value to value type
        SetDefault(_defaultValueType);

        releaseBehavior = _controlType;
        defaultType = _defaultValueType;
        smoothTime = _smoothTime;
        curveType = _curveType;
        inputType = _inputType;
    }

    public string GetAddress()
    {
        return OscSettings.GetAddress();
    }


    void SetDefault(DefaultValueType _defaultValueType)
    {
        switch (_defaultValueType)
        {
            case DefaultValueType.Min:
                defaultValue = OSCControllerSettings.MIN_UNMAPPED;
                break;
            case DefaultValueType.Mid:
                defaultValue = Operations.Average(OSCControllerSettings.MIN_UNMAPPED, OSCControllerSettings.MAX_UNMAPPED);
                break;
            case DefaultValueType.Max:
                defaultValue = OSCControllerSettings.MAX_UNMAPPED;
                break;
            default:
                defaultValue = OSCControllerSettings.MIN_UNMAPPED;
                Debug.LogError("Default value type not implemented! Defaulting to min.");
                break;
        }
    }

    public override string ToString()
    {
        string result = $"Control Type: {releaseBehavior}\n" +
            $"Input Type: {inputType}\n" +
            $"Default Value: {defaultType}\n";

        return result;
    }
}
