using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
[RequireComponent(typeof(Slider))]
public class WheelControl : MonoBehaviour
{

    [SerializeField] Text label;

    OscPropertySender sender = null;
    Slider slider = null;

    enum SliderState {Slide, Idle};
    SliderState state = SliderState.Idle;

    enum ControlType { Pitch, CC, ChannelPressure };

    enum MIDIControl { Pitch, Mod, FootPedal, Expression, BreathControl, ChannelPressure, Volume};

    Dictionary<MIDIControl, string> addresses = new Dictionary<MIDIControl, string>()
    {
        { MIDIControl.Pitch, "/vkb_midi/pitch"},
        { MIDIControl.Mod, "/vkb_midi/cc/1"},
        { MIDIControl.FootPedal, "/vkb_midi/cc/4"},
        { MIDIControl.Expression, "/vkb_midi/cc/11"},
        { MIDIControl.BreathControl, "/vkb_midi/cc/2"},
        { MIDIControl.ChannelPressure, "/vkb_midi/channelpressure"},
        { MIDIControl.Volume, "/vkb_midi/cc/7"}
    };

    Dictionary<MIDIControl, string> labels = new Dictionary<MIDIControl, string>()
    {
        { MIDIControl.Pitch, "Pitch"},
        { MIDIControl.Mod, "Mod"},
        { MIDIControl.FootPedal, "Foot Pedal"},
        { MIDIControl.Expression, "Expression"},
        { MIDIControl.BreathControl, "Breath Control"},
        { MIDIControl.ChannelPressure, "Channel Pressure"},
        { MIDIControl.Volume, "Volume"}
    };

    [SerializeField] MIDIControl midiMode;

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
        slider = GetComponent<Slider>();
        sender = GetComponent<OscPropertySender>();

        sender.SetAddress(addresses[midiMode]);
        label.text = labels[midiMode];
        name = labels[midiMode] + " Slider";

        SetControlMode();

        //load default values
        modValue = defaultValue;
        pModValue = defaultValue;
        targetModValue = defaultValue;
        slider.value = defaultValue;
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

        if(midiMode == MIDIControl.Pitch)
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

    void SetControlMode()
    {
        switch(midiMode)
        {
            case MIDIControl.Pitch:
                slider.minValue = 0;
                slider.maxValue = 16383;
                defaultValue = 8191;
                break;

            default:
                slider.minValue = 0;
                slider.maxValue = 127;
                defaultValue = 0;
                break;
        }
    }
}
