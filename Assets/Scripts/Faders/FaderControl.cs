using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
public class FaderControl : MonoBehaviour
{

    [SerializeField] Text label = null;
    [SerializeField] Slider slider = null;
    [SerializeField] Button sortLeftButton = null;
    [SerializeField] Button sortRightButton = null;
    [SerializeField] ColorSetter faderColorController = null;

    OscPropertySender sender = null;

    ControllerSettings myController = null;
    
    float modValue;
    float pModValue;
    float targetModValue;
    float startingPoint;

    const int FRAMES_TO_SEND_DUPLICATES = 10;
    int dupeCount = FRAMES_TO_SEND_DUPLICATES; //so it doesnt send anything out before it's touched

    public void Initialize(ControllerSettings _controller)
    {
        sender = GetComponent<OscPropertySender>();
        myController = _controller;

        sender.SetAddress(myController.GetAddress());

        label.text = myController.name;
        name = myController.name + " " + myController.controlType;

        //load default values
        int defaultValue = myController.defaultValue;
        modValue = defaultValue;
        pModValue = defaultValue;
        targetModValue = defaultValue;

        slider.maxValue = myController.max;
        slider.minValue = myController.min;

        sortLeftButton.onClick.AddListener(SortLeft);
        sortRightButton.onClick.AddListener(SortRight);

        SetSortButtonVisibility(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(myController == null)
        {
            Debug.LogError("Null controller on " + gameObject.name);
            return;
        }

        TweenModValue();

        if (modValue == pModValue)
        {
            dupeCount++;

            if (dupeCount > FRAMES_TO_SEND_DUPLICATES) //prevent int overflow
            {
                dupeCount = FRAMES_TO_SEND_DUPLICATES;
            }

        }
        else
        {
            dupeCount = 0;
        }

        if (dupeCount < FRAMES_TO_SEND_DUPLICATES)
        {
            SendModValue();
        }

        slider.SetValueWithoutNotify(modValue);
    }

    public void StartSliding()
    {

    }

    public void EndSliding()
    {
        if(myController.controlType == ControlType.Wheel)
        {
            targetModValue = MapValueToCurve(myController.defaultValue, true);
        }
    }

    public void SetValue(float _val)
    {
        targetModValue = _val;
    }

    void TweenModValue()
    {
        pModValue = modValue;

        if (modValue == targetModValue)
        {
            return;
        }

        if (myController.smoothTime <= 0)
        {
            modValue = targetModValue;
        }
        else
        {
            float difference = (slider.maxValue - slider.minValue) * Time.deltaTime / myController.smoothTime;

            //set to idle if close enough to zero
            if (Mathf.Abs(modValue - targetModValue) < difference)
            {
                modValue = targetModValue;
            }
            else
            {
                //approach zerovalue
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

    void SortLeft()
    {
        SortPosition(false);
    }
    
    void SortRight()
    {
        SortPosition(true);
    }

    void SortPosition(bool _right)
    {
        transform.SetSiblingIndex(_right ? transform.GetSiblingIndex() + 1 : transform.GetSiblingIndex() - 1);
    }

    public void SetSortButtonVisibility(bool _visible)
    {
        sortLeftButton.gameObject.SetActive(_visible);
        sortRightButton.gameObject.SetActive(_visible);

        Image[] sliderImages = slider.GetComponentsInChildren<Image>();

        foreach(Image i in sliderImages)
        {
            i.enabled = !_visible;
        }
    }

    void SetConnected()
    {
        IPSetter.SetConnected();
    }

    void InvalidClient()
    {
        IPSetter.InvalidClient();
    }

    void SendModValue()
    {
        if (IPSetter.IsConnected())
        {
            sender.Send(MapValueToCurve(modValue, false));
        }
    }

    public ColorSetter GetFaderColorController()
    {
        return faderColorController;
	}

    int MapValueToCurve(float _value, bool _inverse)
    {
        if (myController.curveType != CurveType.Linear)
        {

            int range = myController.GetRange();
            float tempVal = _value - myController.min;
            float ratio = tempVal / range;
            float mappedRatio;

            if (_inverse)
            {
                mappedRatio = myController.curveType == CurveType.Logarithmic ? Mathf.Pow(ratio, 2f) : Mathf.Sqrt(ratio); 
            }
            else
            {
                mappedRatio = myController.curveType == CurveType.Logarithmic ? Mathf.Sqrt(ratio) : Mathf.Pow(ratio, 2);
            }

            return (int)(mappedRatio * range + myController.min);
        }
        else
        {
            return (int)_value;
        }
        
    }
}
