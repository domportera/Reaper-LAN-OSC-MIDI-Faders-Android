using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

public class WheelControl : MonoBehaviour
{
    [SerializeField] OscPropertySender sender;
    [SerializeField] Slider slider;

    enum SliderState {Slide, Idle};
    SliderState state = SliderState.Idle;

    enum WheelMode { Pitch, Mod };

    [SerializeField] WheelMode mode = WheelMode.Mod;

    float zeroValue; //value slider returns to when released
    float modValue;
    float targetModValue;
    

    [SerializeField] float releaseTime = 0.05f; //time it takes to release from max slide value to zeroValue
    [SerializeField] float rampUpTime = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        if(mode == WheelMode.Mod)
        {
            slider.minValue = 0;
            slider.maxValue = 16383;
            zeroValue = 8191;
        }
        else if (mode == WheelMode.Pitch)
        {
            slider.minValue = 0;
            slider.maxValue = 16383;
            zeroValue = 8191;
        }

        modValue = zeroValue;
        SendPitch();
        slider.value = modValue;
    }

    // Update is called once per frame
    void Update()
    {
        TweenModValue();
    }

    public void StartSliding()
    {
        state = SliderState.Slide;
    }

    public void EndSliding()
    {
        state = SliderState.Idle;
        targetModValue = zeroValue;
    }

    public void SetPitch(float _val)
    {
        targetModValue = _val;
    }

    void TweenModValue()
    {
        if(modValue == targetModValue)
        {
            return;
        }

        float time = state == SliderState.Idle ? releaseTime : rampUpTime;
        float difference = (slider.maxValue - zeroValue) * Time.deltaTime / time;

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

        SendPitch();
    }

    void SetConnected()
    {
        IPSetter.SetConnected();
    }

    void InvalidClient()
    {
        IPSetter.InvalidClient();
    }

    void SendPitch()
    {
        if (IPSetter.IsConnected())
        {
            if (mode == WheelMode.Pitch)
            {
                sender.Send((int)modValue);
            }
            else if (mode == WheelMode.Mod)
            {
                sender.Send((int)modValue);
            }
        }
    }
    
}
