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
    [SerializeField] float mid;
    [SerializeField] float defaultValue;
    [SerializeField] float smoothTime;
    [SerializeField] CurveType curveType;
    [SerializeField] OSCControllerSettings oscMessage;


    public ReleaseBehaviorType ReleaseBehavior { get { return releaseBehavior; } }
    public InputMethod InputType { get { return inputType; } }
    public DefaultValueType DefaultType { get { return defaultType; } }
    public float Mid { get { return mid; } }
    public float DefaultValue { get { return defaultValue; } }
    public float SmoothTime { get { return smoothTime; } }
    public CurveType Curve { get { return curveType; } }
    public OSCControllerSettings OscMessage { get { return oscMessage; } }

    public ControllerSettings(ControllerSettings _c)
    {
        SetVariables(_c.inputType, _c.releaseBehavior, _c.oscMessage, _c.defaultType, _c.curveType, _c.smoothTime);
    }

    public ControllerSettings(InputMethod _inputType, ReleaseBehaviorType _controlType, OSCControllerSettings _oscMessage, DefaultValueType _defaultValueType, CurveType _curveType,  float _smoothTime = 0.1f)
    {
        SetVariables(_inputType, _controlType, _oscMessage, _defaultValueType, _curveType, _smoothTime);
    }

    public void SetVariables(InputMethod _inputType, ReleaseBehaviorType _controlType, OSCControllerSettings _oscMessage,  DefaultValueType _defaultValueType, CurveType _curveType, float _smoothTime)
    {
        oscMessage = _oscMessage;

        mid = Operations.Average(Controller.MAX, Controller.MIN);

        //sets default value to value type
        SetDefault(_defaultValueType);

        releaseBehavior = _controlType;
        defaultType = _defaultValueType;
        smoothTime = _smoothTime;
        curveType = _curveType;
    }


    void SetDefault(DefaultValueType _defaultValueType)
    {
        switch (_defaultValueType)
        {
            case DefaultValueType.Min:
                defaultValue = Controller.MIN;
                break;
            case DefaultValueType.Mid:
                defaultValue = mid;
                break;
            case DefaultValueType.Max:
                defaultValue = Controller.MAX;
                break;
            default:
                defaultValue = Controller.MIN;
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
