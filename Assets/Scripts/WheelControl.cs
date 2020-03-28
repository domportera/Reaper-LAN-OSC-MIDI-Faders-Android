using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
public class WheelControl : MonoBehaviour
{
    OscPropertySender sender = null;
    [SerializeField] Slider slider = null;

    enum SliderState {Slide, Idle};
    SliderState state = SliderState.Idle;

    enum WheelMode { Pitch, CC, ChannelPressure };

    [SerializeField] WheelMode mode = WheelMode.CC;

    float defaultValue; //value slider returns to when released
    float modValue;
    float pModValue;
    float targetModValue;
    float startingPoint;
    
    [SerializeField] float releaseTime = 0.05f; //time it takes to release from max slide value to zeroValue
    [SerializeField] float rampUpTime = 0.05f;

    const int FRAMES_TO_SEND_DUPLICATES = 10;
    int dupeCount = 10; //so it doesnt send anything out before it's touched

    // Start is called before the first frame update
    void Start()
    {
        sender = GetComponent<OscPropertySender>();

        if(mode == WheelMode.CC)
        {
            slider.minValue = 0;
            slider.maxValue = 127;
            defaultValue = 0;
        }
        else if (mode == WheelMode.ChannelPressure)
        {
            slider.minValue = 0;
            slider.maxValue = 127;
            defaultValue = 0;
        }
        else if (mode == WheelMode.Pitch)
        {
            slider.minValue = 0;
            slider.maxValue = 16383;
            defaultValue = 8191;
        }

        modValue = defaultValue;
        pModValue = modValue;
        targetModValue = modValue;
        slider.value = modValue;
    }

    // Update is called once per frame
    void Update()
    {
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
        state = SliderState.Slide;
    }

    public void EndSliding()
    {
        state = SliderState.Idle;

        if(mode == WheelMode.Pitch)
        {
            targetModValue = defaultValue;
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

        float time = state == SliderState.Idle ? releaseTime : rampUpTime;
        float difference = (slider.maxValue - defaultValue) * Time.deltaTime / time;

        //set to idle if close enough to zero
        if (Mathf.Abs(modValue - targetModValue) < difference)
        {
            modValue = targetModValue;
            state = SliderState.Idle;
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
