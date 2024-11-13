using System.Collections;
using UnityEngine;
using OscJack;

using UnityEngine.Serialization;

public class Controller : MonoBehaviour
{
    [FormerlySerializedAs("oscSender")] [SerializeField]
    private OscPropertySender _oscSender;
    private ControllerSettings _controllerSettings;
    private OscControllerSettings OscSettings { get { return _controllerSettings.OscSettings; } }

    private float _targetControllerValue;
    private float _defaultValue;

    /// <summary>
    /// returns moving mod value as it approaches target value
    /// </summary>
    public float SmoothValue { get; private set; }

    public const float MinControllerValue = 0f;
    public const float MaxControllerValue = 1f;

    private const int NumberOfFinalMessages = 1;

    private Coroutine _updateModValueCoroutine = null;

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

    private float GetDefault(DefaultValueType defaultValueType)
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
    private IEnumerator UpdateModValueLoop()
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

    private float MapValueToCurve(float value, bool inverse)
    {
        if (_controllerSettings.Curve != CurveType.Linear)
        {
            var range = MaxControllerValue - MinControllerValue;
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

    private bool ModValueCaughtUpToTarget => SmoothValue == _targetControllerValue;

    private void UpdateModValue()
    {
        if (ModValueCaughtUpToTarget)
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
            var difference = (MaxControllerValue - MinControllerValue) * Time.deltaTime / _controllerSettings.SmoothTime;

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

    private void SendModValue()
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

        _oscSender.Send(valueToSend);
    }

    private IEnumerator SendModValueMultipleTimes(int numberOfTimes)
    {
        for(var i = 0; i < numberOfTimes; i++)
        {
            SendModValue();
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
