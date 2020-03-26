using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

public class WheelSender : MonoBehaviour
{
    [SerializeField] OscPropertySender sender;
    [SerializeField] Slider slider;

    enum SliderState {Slide, Idle, End};
    SliderState state = SliderState.Idle;

    enum ValueMode { Integer, FloatingPoint };

    [SerializeField] ValueMode mode = ValueMode.FloatingPoint;

    float zeroValue; //value slider returns to when released
    float modValue;
    

    [SerializeField] float releaseTime = 0.1f; //time it takes to release from max slide value to zeroValue

    // Start is called before the first frame update
    void Start()
    {
        if(mode == ValueMode.FloatingPoint)
        {
            slider.minValue = 0;
            slider.maxValue = 1;
            zeroValue = 0.5f;
        }
        else if (mode == ValueMode.Integer)
        {
            slider.minValue = 0;
            slider.maxValue = 127;
            zeroValue = 64;
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

    public void SendPitch(float _val)
    {
        modValue = _val;

        if (mode == ValueMode.Integer)
        {
            sender.Send((int)modValue);
        }
        else if (mode == ValueMode.FloatingPoint)
        {
            sender.Send(modValue);
        }
    }
}
