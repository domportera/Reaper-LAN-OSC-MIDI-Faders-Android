using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
    //this class needs to create our wheel controls
    [SerializeField] RectTransform controllerParent = null;

    readonly ControllerSettings[] defaultControllers = new ControllerSettings[]
    {
        new ControllerSettings("Pitch",             ControlType.Wheel, AddressType.Pitch,           ValueRange.FourteenBit, DefaultValueType.Mid, MIDIChannel.All),
        new ControllerSettings("Mod",               ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, 1),
        new ControllerSettings("Foot Pedal",        ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, 4),
        new ControllerSettings("Expression",        ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, 11),
        new ControllerSettings("Breath Control",    ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, 2),
        new ControllerSettings("Aftertouch",        ControlType.Fader, AddressType.Aftertouch,      ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All),
        new ControllerSettings("Volume",            ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, 7)
    };

    [SerializeField] ControllerType[] controllerTypes = null;

    List<ControllerSettings> controllers = new List<ControllerSettings>();

    void Start()
    {  
        //load controllers
        //needs to load defaults

        //spawn defaults
        foreach(ControllerSettings set in defaultControllers)
        {
            controllers.Add(set);
        }

        SpawnControllers();
    }

    void SpawnControllers()
    {
        foreach(ControllerSettings set in controllers)
        {
            bool error = true;
            string errorDebug = "doesn't exist!";
            foreach (ControllerType t in controllerTypes)
            {
                if(t.controlType == set.controlType)
                {
                    //spawn this type
                    if(t.controlObject != null)
                    {
                        GameObject control = Instantiate(t.controlObject);
                        control.transform.SetParent(controllerParent, false);
                        control.GetComponentInChildren<WheelControl>().Initialize(set);
                        error = false;
                        break;
                    }
                    else
                    {
                        //let it continue looking for one that does have a game object if it exists, but throw an appropriate error
                        errorDebug = "exists, but doesn't have a game object!";
                    }
                }
            }

            if(error)
            {
                Debug.LogError($"{typeof(ControllerType).ToString()} for {set.controlType.ToString()} {errorDebug}!");
            }
        }
    }

    //used to pair prefabs with their control type
    [Serializable]
    public struct ControllerType
    {
        public ControlType controlType;
        public GameObject controlObject;
    }
}