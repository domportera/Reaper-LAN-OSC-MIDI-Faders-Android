using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [SerializeField] OscPropertySender oscSender;
    private ControllerSettings controllerSettings;
    private OSCControllerSettings OscSettings { get { return controllerSettings.OscSettings; } }

    float smoothValue; //the value we're actually sending
    float targetControllerValue;
    float defaultValue;

    /// <summary>
    /// returns moving mod value as it approaches target value
    /// </summary>
    public float SmoothValue { get { return smoothValue; } }

    public const float MIN_CONTROLLER_VALUE = 0f;
    public const float MAX_CONTROLLER_VALUE = 1f;

    const int NUMBER_OF_FINAL_MESSAGES = 1;

    Coroutine updateModValueCoroutine = null;

    protected virtual void Start()
    {
        IPSetter.instance.TryConnect(oscSender);
    }

    public virtual void Initialize(ControllerData _controller, int whichIndex = 0)
    {
        if (_controller == null)
        {
            Debug.LogError("Null controller on " + gameObject.name, this);
        }

        controllerSettings = _controller.GetController();

        oscSender.SetAddress(controllerSettings.GetAddress());

        defaultValue = GetDefault(controllerSettings.DefaultType);
        smoothValue = defaultValue;
        targetControllerValue = defaultValue;

        if(updateModValueCoroutine == null)
        {
            updateModValueCoroutine = StartCoroutine(UpdateModValueLoop());
        }
        else
        {
            Debug.LogWarning($"Attempted to start the update loop on controller {_controller.GetName()} again", this);
        }
    }

    float GetDefault(DefaultValueType _defaultValueType)
    {
        switch(_defaultValueType)
        {
            case DefaultValueType.Min:
                return MIN_CONTROLLER_VALUE;
            case DefaultValueType.Mid:
                return Operations.Average(MIN_CONTROLLER_VALUE, MAX_CONTROLLER_VALUE);
            case DefaultValueType.Max:
                return MAX_CONTROLLER_VALUE;
            default:
                Debug.LogError("Default value type not implemented! Defaulting to min.");
                return MIN_CONTROLLER_VALUE;
        }
    }

    /// <summary>
    /// This exists to allow this base class to have an "update" loop that always has to be run and can never be overridden by child class
    /// </summary>
    IEnumerator UpdateModValueLoop()
    {
        while(true)
        {
            UpdateModValue();
            yield return null;
        }
    }

    #region Mod Value Manipulation
    public void SetValue(float _val)
    {
        targetControllerValue = _val;
    }

    protected float MapValueToCurve(float _value, bool _inverse)
    {
        if (controllerSettings.Curve != CurveType.Linear)
        {
            float range = MAX_CONTROLLER_VALUE - MIN_CONTROLLER_VALUE;
            float tempVal = _value - MIN_CONTROLLER_VALUE;
            float ratio = tempVal / range;
            float mappedRatio;

            if (_inverse)
            {
                mappedRatio = controllerSettings.Curve == CurveType.Logarithmic ? Mathf.Pow(ratio, 2) : Mathf.Sqrt(ratio);
            }
            else
            {
                mappedRatio = controllerSettings.Curve == CurveType.Logarithmic ? Mathf.Sqrt(ratio) : Mathf.Pow(ratio, 2);
            }

            return mappedRatio * range + MIN_CONTROLLER_VALUE;
        }
        else
        {
            return _value;
        }
    }

    bool ModValueCaughtUpToTarget { get { return smoothValue == targetControllerValue; } }
    void UpdateModValue()
    {
        if (ModValueCaughtUpToTarget)
        {
            return;
        }

        bool shouldSmooth = controllerSettings.SmoothTime > 0;
        if (!shouldSmooth)
        {
            smoothValue = targetControllerValue;
        }
        else
        {
            float difference = (MAX_CONTROLLER_VALUE - MIN_CONTROLLER_VALUE) * Time.deltaTime / controllerSettings.SmoothTime;

            //set to idle if close enough to zero
            if (Mathf.Abs(smoothValue - targetControllerValue) < difference)
            {
                smoothValue = targetControllerValue;
            }
            else
            {
                //approach target value
                if (smoothValue > targetControllerValue)
                {
                    smoothValue -= difference;
                }
                else
                {
                    smoothValue += difference;
                }
            }
        }

        if(ModValueCaughtUpToTarget)
        {
            StartCoroutine(SendModValueMultipleTimes(NUMBER_OF_FINAL_MESSAGES));
        }
        else
        {
            SendModValue();
        }

    }

    public void ReturnToCenter()
    {
        if (controllerSettings.ReleaseBehavior == ReleaseBehaviorType.PitchWheel)
        {
            SetValue(MapValueToCurve(defaultValue, true));
        }
    }
    #endregion Mod Value Manipulation

    #region OSC Communication

    void SendModValue()
    {
        float curveMappedValue = MapValueToCurve(smoothValue, false);
        float valueToSend;

        bool isFloat = OscSettings.Range == ValueRange.CustomFloat || OscSettings.Range == ValueRange.Float;
        if(isFloat)
        {
            valueToSend = OscSettings.GetValueFloat(curveMappedValue);
        }
        else
        {
            valueToSend = OscSettings.GetValueInt(curveMappedValue);
        }

        oscSender.Send(valueToSend);
    }

    IEnumerator SendModValueMultipleTimes(int _numberOfTimes)
    {
        for(int i = 0; i < _numberOfTimes; i++)
        {
            SendModValue();
            yield return null;
        }
    }

    #endregion OSC Communication
}

interface ISortingMember
{
    void InitializeSorting();
    void SetSortButtonVisibility(bool _visible);
    void SortLeft();
    void SortRight();
    void SortPosition(bool _right);
}
