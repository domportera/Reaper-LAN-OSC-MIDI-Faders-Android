using System.Collections;
using UnityEngine;
using OscJack;

using UnityEngine.Serialization;

public class Controller : MonoBehaviour
{
    [FormerlySerializedAs("oscSender")] [SerializeField] OscPropertySender _oscSender;
    private ControllerSettings _controllerSettings;
    private OSCControllerSettings OscSettings { get { return _controllerSettings.OscSettings; } }

    float _targetControllerValue;
    float _defaultValue;

    /// <summary>
    /// returns moving mod value as it approaches target value
    /// </summary>
    public float SmoothValue { get; private set; }

    public const float MinControllerValue = 0f;
    public const float MaxControllerValue = 1f;

    const int NumberOfFinalMessages = 1;

    Coroutine _updateModValueCoroutine = null;

    protected virtual void Start()
    {
        IPSetter.Instance.TryConnect(_oscSender);
    }

    public virtual void Initialize(ControllerData controller, int whichIndex = 0)
    {
        if (controller == null)
        {
            Debug.LogError("Null controller on " + gameObject.name, this);
        }

        _controllerSettings = controller.GetController();

        _oscSender.SetAddress(_controllerSettings.GetAddress());

        _defaultValue = GetDefault(_controllerSettings.DefaultType);
        SmoothValue = _defaultValue;
        _targetControllerValue = _defaultValue;

        if(_updateModValueCoroutine == null)
        {
            _updateModValueCoroutine = StartCoroutine(UpdateModValueLoop());
        }
        else
        {
            Debug.LogWarning($"Attempted to start the update loop on controller {controller.GetName()} again", this);
        }
    }

    float GetDefault(DefaultValueType defaultValueType)
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
    public void SetValue(float val)
    {
        _targetControllerValue = val;
    }

    float MapValueToCurve(float value, bool inverse)
    {
        if (_controllerSettings.Curve != CurveType.Linear)
        {
            float range = MaxControllerValue - MinControllerValue;
            float tempVal = value - MinControllerValue;
            float ratio = tempVal / range;
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

    bool ModValueCaughtUpToTarget => SmoothValue == _targetControllerValue;
    void UpdateModValue()
    {
        if (ModValueCaughtUpToTarget)
        {
            return;
        }

        bool shouldSmooth = _controllerSettings.SmoothTime > 0;
        if (!shouldSmooth)
        {
            SmoothValue = _targetControllerValue;
        }
        else
        {
            float difference = (MaxControllerValue - MinControllerValue) * Time.deltaTime / _controllerSettings.SmoothTime;

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

        if(ModValueCaughtUpToTarget)
        {
            StartCoroutine(SendModValueMultipleTimes(NumberOfFinalMessages));
        }
        else
        {
            SendModValue();
        }

    }

    public void ReturnToCenter()
    {
        if (_controllerSettings.ReleaseBehavior == ReleaseBehaviorType.PitchWheel)
        {
            SetValue(MapValueToCurve(_defaultValue, true));
        }
    }
    #endregion Mod Value Manipulation

    #region OSC Communication

    void SendModValue()
    {
        float curveMappedValue = MapValueToCurve(SmoothValue, false);
        float valueToSend;

        bool isFloat = OscSettings.Range is ValueRange.CustomFloat or ValueRange.Float;
        if(isFloat)
        {
            valueToSend = OscSettings.GetValueFloat(curveMappedValue);
        }
        else
        {
            valueToSend = OscSettings.GetValueInt(curveMappedValue);
        }

        _oscSender.Send(valueToSend);
    }

    IEnumerator SendModValueMultipleTimes(int numberOfTimes)
    {
        for(int i = 0; i < numberOfTimes; i++)
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
    void SetSortButtonVisibility(bool visible);
    void SortLeft();
    void SortRight();
    void SortPosition(bool right);
}
