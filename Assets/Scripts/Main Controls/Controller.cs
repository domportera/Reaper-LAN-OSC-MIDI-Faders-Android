using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [SerializeField] OscPropertySender oscSender;
    public ControllerSettings controllerSettings { get; private set; }

    public float modValue { get; private set; }
    protected float pModValue{ get; private set; }
    protected float targetModValue { get; private set; }

    const int FRAMES_TO_SEND_DUPLICATES = 10;
    int dupeCount = FRAMES_TO_SEND_DUPLICATES; //so it doesnt send anything out before it's touched

    protected virtual void Start()
    {
        IPSetter.instance.TryConnect(oscSender);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        TweenModValue();
        SendValuesWithDuplicates();
    }

    public virtual void Initialize(ControlsManager.ControllerData _controller, int whichIndex = 0)
    {
        if (_controller == null)
        {
            Debug.LogError("Null controller on " + gameObject.name, this);
        }

        controllerSettings = _controller.GetController();

        oscSender.SetAddress(controllerSettings.GetAddress());

        //load default values
        int defaultValue = controllerSettings.DefaultValue;
        modValue = defaultValue;
        pModValue = defaultValue;
        targetModValue = defaultValue;
    }

    #region Mod Value Manipulation
    protected void SetValue(float _val)
    {
        targetModValue = _val;
    }

    protected void SetValue (int _val)
    {
        targetModValue = _val;
    }

    public void SetValueAsPercentage (float _val)
    {
        targetModValue = Mathf.Lerp(controllerSettings.Min, controllerSettings.Max, Mathf.Clamp01(_val));
    }

    protected int MapValueToCurve(float _value, bool _inverse)
    {
        if (controllerSettings.Curve != CurveType.Linear)
        {
            int range = controllerSettings.GetRange();
            float tempVal = _value - controllerSettings.Min;
            float ratio = tempVal / range;
            float mappedRatio;

            if (_inverse)
            {
                mappedRatio = controllerSettings.Curve == CurveType.Logarithmic ? Mathf.Pow(ratio, 2f) : Mathf.Sqrt(ratio);
            }
            else
            {
                mappedRatio = controllerSettings.Curve == CurveType.Logarithmic ? Mathf.Sqrt(ratio) : Mathf.Pow(ratio, 2);
            }

            return (int)(mappedRatio * range + controllerSettings.Min);
        }
        else
        {
            return (int)_value;
        }

    }

    void TweenModValue()
    {
        pModValue = modValue;

        if (modValue == targetModValue)
        {
            return;
        }

        bool shouldSmooth = controllerSettings.SmoothTime <= 0;
        if (shouldSmooth)
        {
            modValue = targetModValue;
        }
        else
        {
            float difference = (controllerSettings.Max - controllerSettings.Min) * Time.deltaTime / controllerSettings.SmoothTime;

            //set to idle if close enough to zero
            if (Mathf.Abs(modValue - targetModValue) < difference)
            {
                modValue = targetModValue;
            }
            else
            {
                //approach target value
                if (modValue > targetModValue)
                {
                    modValue -= difference;
                }
                else
                {
                    modValue += difference;
                }
            }
        }
    }

    public void ReturnToCenter()
    {
        if (controllerSettings.ReleaseBehavior == ReleaseBehaviorType.PitchWheel)
        {
            SetValue(MapValueToCurve(controllerSettings.DefaultValue, true));
        }
    }
    #endregion Mod Value Manipulation

    #region OSC Communication
    void SendValuesWithDuplicates()
    {
        if (modValue == pModValue)
        {
            dupeCount = Mathf.Clamp(++dupeCount, 0, FRAMES_TO_SEND_DUPLICATES);
        }
        else
        {
            dupeCount = 0;
        }

        if (dupeCount < FRAMES_TO_SEND_DUPLICATES)
        {
            SendModValue();
        }
    }

    void SendModValue()
    {
        oscSender.Send(MapValueToCurve(modValue, false));
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
