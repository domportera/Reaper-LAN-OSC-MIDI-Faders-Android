using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

public class WheelSender : MonoBehaviour
{
    [SerializeField] OscPropertySender sender;
    [SerializeField] Slider slider;

    enum SliderState {Slide, Idle};
    SliderState state = SliderState.Idle;
    [SerializeField] int zeroValue = 64; //value slider returns to when released
    float modValue;

    [SerializeField] float releaseTime = 0.1f; //time it takes to release from max slide value to zeroValue

    // Start is called before the first frame update
    void Start()
    {
        modValue = zeroValue;
    }

    // Update is called once per frame
    void Update()
    {
        if(state == SliderState.Idle && modValue != zeroValue)
        {
            ReturnToMidpoint();
        }
    }

    void ReturnToMidpoint()
    {
        if(slider != null)
        {
     //       float newPitch = modValue + (slider - zeroValue) * Time.deltaTime / releaseTime;
        //    slider.value = newPitch;
        }

    }

    public void StartSliding()
    {
        state = SliderState.Slide;
    }

    public void EndSliding()
    {
        state = SliderState.Idle;
    }

    public void SendPitch(float _val)
    {
        modValue = _val;
        sender.Send((int)modValue);
        Debug.Log("SENT " + (int)modValue);
    }
}
