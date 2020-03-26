using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

public class WheelSender : MonoBehaviour
{
    [SerializeField] OscPropertySender sender;
    [SerializeField] IPSetter ipSetter;
    [SerializeField] Slider slider;

    enum SliderState {Slide, Idle, End};
    SliderState state = SliderState.Idle;

    enum WheelMode { Pitch, Mod };

    [SerializeField] WheelMode mode = WheelMode.Mod;

    float zeroValue; //value slider returns to when released
    float modValue;
    

    [SerializeField] float releaseTime = 0.1f; //time it takes to release from max slide value to zeroValue

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
        SendPitch(modValue);
        slider.value = modValue;
    }

    // Update is called once per frame
    void Update()
    {
        if(state == SliderState.End)
        {
            ReturnToMidpoint();
        }
    }

    void ReturnToMidpoint()
    {
        float difference = (slider.maxValue - zeroValue) * Time.deltaTime / releaseTime;

        //set to idle if close enough to zero
        if (Mathf.Abs(modValue - zeroValue) < difference)
        {
            modValue = zeroValue;
            SendPitch(modValue);
            state = SliderState.Idle;
        }
        else
        {
            //approach zerovalue
            if (modValue > zeroValue)
            {
                modValue -= difference;
            }
            else
            {
                modValue += difference;
            }
        }

        slider.value = modValue;
    }

    public void StartSliding()
    {
        state = SliderState.Slide;
    }

    public void EndSliding()
    {
        state = SliderState.End;
    }


    //needs to tween towards value
    public void SendPitch(float _val)
    {
        modValue = _val;

        if (ipSetter.IsConnected())
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
