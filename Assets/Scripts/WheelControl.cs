using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
[RequireComponent(typeof(Slider))]
public class WheelControl : MonoBehaviour
{

    [SerializeField] Text label = null;

    OscPropertySender sender = null;
    Slider slider = null;

    ControllerSettings myController = null;
    
    float modValue;
    float pModValue;
    float targetModValue;
    float startingPoint;

    const int FRAMES_TO_SEND_DUPLICATES = 10;
    int dupeCount = FRAMES_TO_SEND_DUPLICATES; //so it doesnt send anything out before it's touched

    public void Initialize(ControllerSettings _controller)
    {
        slider = GetComponent<Slider>();
        sender = GetComponent<OscPropertySender>();

        myController = _controller;

        sender.SetAddress(myController.address);
        label.text = myController.name;
        name = myController.name + " " + myController.controlType;

        //load default values
        int defaultValue = myController.defaultValue;
        modValue = defaultValue;
        pModValue = defaultValue;
        targetModValue = defaultValue;

        slider.maxValue = myController.max;
        slider.minValue = myController.min;
        slider.SetValueWithoutNotify(defaultValue);
    }

    // Update is called once per frame
    void Update()
    {
        if(myController == null)
        {
            Debug.LogError("Null controller!");
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
    }

    public void StartSliding()
    {
        
    }

    public void EndSliding()
    {

        if(myController.controlType == ControlType.Wheel)
        {
            targetModValue = myController.defaultValue;
        }
    }

    public void SetPitch(float _val)
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

        slider.SetValueWithoutNotify(modValue);
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
            sender.Send((int)modValue);
        }
    }
}
