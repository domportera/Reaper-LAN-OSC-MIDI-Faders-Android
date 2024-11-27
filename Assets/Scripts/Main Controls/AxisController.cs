using System;
using System.Collections;
using UnityEngine;

public sealed class AxisController
{
    private readonly AxisControlSettings _axisControlSettings;
    private OscControllerSettings OscSettings => _axisControlSettings.OscSettings;

    private float _targetControllerValue;
    private bool _arrivedAtTarget = false;

    private float DefaultValue
    {
        get
        {
            switch (_axisControlSettings.DefaultType)
            {
                case DefaultValueType.Min:
                    return MinControllerValue;
                case DefaultValueType.Mid:
                    return (MinControllerValue + MaxControllerValue) / 2f;
                case DefaultValueType.Max:
                    return MaxControllerValue;
                default:
                    Debug.LogError("Default value type not implemented! Defaulting to min.");
                    return MinControllerValue;
            }
        }
    }


    /// <summary>
    /// returns moving mod value as it approaches target value
    /// </summary>
    public float SmoothValue { get; private set; }

    public const float MinControllerValue = 0f;
    public const float MaxControllerValue = 1f;

    public AxisController(AxisControlSettings settings)
    {
        _axisControlSettings = settings ?? throw new ArgumentNullException(nameof(settings));

        var defaultValue = DefaultValue;
        SmoothValue = defaultValue;
        _targetControllerValue = defaultValue;
        SendCurrentValue();
    }

    #region Mod Value Manipulation
    public void SetValue(float val)
    {
        _targetControllerValue = val;
        _arrivedAtTarget = false;
    }

    private float MapValueToCurve(float value, bool inverse)
    {
        var exponent = _axisControlSettings.Curve.GetExponent();
        if(inverse)
            exponent = 1f/exponent;
        
        const float range = MaxControllerValue - MinControllerValue;
        var ratio = (value - MinControllerValue) / range;
        var mappedRatio = Math.Pow(ratio, exponent);
        return (float)(mappedRatio * range + MinControllerValue);
    }

    public void Update(float deltaTime)
    {
        if (_arrivedAtTarget)
        {
            return;
        }

        var shouldSmooth = _axisControlSettings.SmoothTime > 0;
        if (!shouldSmooth)
        {
            SmoothValue = _targetControllerValue;
            _arrivedAtTarget = true;
        }
        else
        {
            UpdateSmoothly();
        }

        SendCurrentValue();
        return;

        void UpdateSmoothly()
        {
            var difference = (MaxControllerValue - MinControllerValue) * deltaTime / _axisControlSettings.SmoothTime;

            //set to idle if close enough to zero
            if (Mathf.Abs(SmoothValue - _targetControllerValue) < difference)
            {
                SmoothValue = _targetControllerValue;
                _arrivedAtTarget = true;
            }
            else
            {
                //approach target value
                if (SmoothValue > _targetControllerValue)
                {
                    SmoothValue -= difference;
                }
                else
                {
                    SmoothValue += difference;
                }
            }
        }
    }

    public void Release()
    {
        switch (_axisControlSettings.ReleaseBehavior)
        {
            case ReleaseBehaviorType.PitchWheel:
                SetValue(MapValueToCurve(DefaultValue, true));
                break;
            default:
                return;
        }
    }
    
    #endregion Mod Value Manipulation

    #region OSC Communication

    private void SendCurrentValue()
    {
        var curveMappedValue = MapValueToCurve(SmoothValue, false);

        var isFloat = OscSettings.Range is ValueRange.CustomFloat or ValueRange.Float;
        var oscSettings = _axisControlSettings.OscSettings;
        var address = oscSettings.GetAddress();
        if (isFloat)
        {
            OSCSystem.Send(address, oscSettings.GetValueFloat(curveMappedValue));
        }
        else
        {
            OSCSystem.Send(address, oscSettings.GetValueInt(curveMappedValue));
        }
    }

    private IEnumerator SendModValueMultipleTimes(int numberOfTimes)
    {
        for(var i = 0; i < numberOfTimes; i++)
        {
            SendCurrentValue();
            yield return null;
        }
    }

    #endregion OSC Communication

}

internal interface ISortingMember
{
    void SetSortButtonVisibility(bool visible);
    RectTransform RectTransform { get; }
}
