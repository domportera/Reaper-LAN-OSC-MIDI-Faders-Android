using System;
using System.Collections;
using UnityEngine;

public sealed class RangeController
{
    private ControllerSettings _controllerSettings;
    private OscControllerSettings OscSettings => _controllerSettings.OscSettings;

    private float _targetControllerValue;
    private float _defaultValue;

    /// <summary>
    /// returns moving mod value as it approaches target value
    /// </summary>
    public float SmoothValue { get; private set; }

    public const float MinControllerValue = 0f;
    public const float MaxControllerValue = 1f;

    public RangeController(ControllerSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _controllerSettings = settings;

        _defaultValue = GetDefault(_controllerSettings.DefaultType);
        SmoothValue = _defaultValue;
        _targetControllerValue = _defaultValue;
        SendCurrentValue();
    }

    private static float GetDefault(DefaultValueType defaultValueType)
    {
        switch(defaultValueType)
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

    #region Mod Value Manipulation
    public void SetValue(float val)
    {
        _targetControllerValue = val;
    }

    private float MapValueToCurve(float value, bool inverse)
    {
        if (_controllerSettings.Curve != CurveType.Linear)
        {
            const float range = MaxControllerValue - MinControllerValue;
            var tempVal = value - MinControllerValue;
            var ratio = tempVal / range;
            float mappedRatio;

            if (inverse)
            {
                mappedRatio = _controllerSettings.Curve == CurveType.Logarithmic ? Mathf.Pow(ratio, 2) : Mathf.Sqrt(ratio);
            }
            else
            {
                mappedRatio = _controllerSettings.Curve == CurveType.Logarithmic ? Mathf.Sqrt(ratio) : Mathf.Pow(ratio, 2);
            }

            return mappedRatio * range + MinControllerValue;
        }
        else
        {
            return value;
        }
    }

    public void Update(float deltaTime)
    {
        if (Mathf.Approximately(SmoothValue, _targetControllerValue))
        {
            return;
        }

        var shouldSmooth = _controllerSettings.SmoothTime > 0;
        if (!shouldSmooth)
        {
            SmoothValue = _targetControllerValue;
        }
        else
        {
            UpdateSmoothly();
        }

        SendCurrentValue();
        return;

        void UpdateSmoothly()
        {
            var difference = (MaxControllerValue - MinControllerValue) * deltaTime / _controllerSettings.SmoothTime;

            //set to idle if close enough to zero
            if (Mathf.Abs(SmoothValue - _targetControllerValue) < difference)
            {
                SmoothValue = _targetControllerValue;
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
        if (_controllerSettings.ReleaseBehavior == ReleaseBehaviorType.PitchWheel)
        {
            SetValue(MapValueToCurve(_defaultValue, true));
        }
    }
    #endregion Mod Value Manipulation

    #region OSC Communication

    private void SendCurrentValue()
    {
        var curveMappedValue = MapValueToCurve(SmoothValue, false);
        float valueToSend;

        var isFloat = OscSettings.Range is ValueRange.CustomFloat or ValueRange.Float;
        if(isFloat)
        {
            valueToSend = OscSettings.GetValueFloat(curveMappedValue);
        }
        else
        {
            valueToSend = OscSettings.GetValueInt(curveMappedValue);
        }

        OSCSystem.Send(_controllerSettings.OscSettings.GetAddress(), valueToSend);
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
    void InitializeSorting();
    void SetSortButtonVisibility(bool visible);
    void SortLeft();
    void SortRight();
    void SortPosition(bool right);
}
