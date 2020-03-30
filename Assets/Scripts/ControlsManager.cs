using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
    //this class needs to create our wheel controls
    [SerializeField] RectTransform controllerParent = null;
    [SerializeField] GameObject faderOptionsPrefab = null;
    [SerializeField] GameObject optionsPanel = null;

    readonly ControllerSettings[] defaultControllers = new ControllerSettings[]
    {
        new ControllerSettings("Pitch",             ControlType.Wheel, AddressType.Pitch,           ValueRange.FourteenBit, DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear),
        new ControllerSettings("Mod",               ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, CurveType.Linear, 1),
        new ControllerSettings("Foot Pedal",        ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, CurveType.Linear, 4),
        new ControllerSettings("Expression",        ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, CurveType.Linear, 11),
        new ControllerSettings("Breath Control",    ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, CurveType.Linear, 2),
        new ControllerSettings("Aftertouch",        ControlType.Fader, AddressType.Aftertouch,      ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, CurveType.Linear),
        new ControllerSettings("Volume",            ControlType.Fader, AddressType.CC,              ValueRange.SevenBit,    DefaultValueType.Min, MIDIChannel.All, CurveType.Linear, 7)
    };

    [SerializeField] ControllerType[] controllerTypes = null;
    [SerializeField] ValueCurve[] valueCurves = null;

    List<ControllerSettings> controllers = new List<ControllerSettings>();

    Dictionary<ControllerSettings, GameObject> controllerObjects = new Dictionary<ControllerSettings, GameObject>();

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
            SpawnController(set);
        }
    }

    public void SpawnController (ControllerSettings _config)
    {
        bool error = true;
        string errorDebug = "doesn't exist!";
        foreach (ControllerType t in controllerTypes)
        {
            if (t.controlType == _config.controlType)
            {
                //spawn this type
                if (t.controlObject != null)
                {
                    GameObject control = Instantiate(t.controlObject);
                    control.transform.SetParent(controllerParent, false);
                    control.GetComponentInChildren<WheelControl>().Initialize(_config, valueCurves);
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
       
        if (error)
        {
            Debug.LogError($"{typeof(ControllerType).ToString()} for {_config.controlType.ToString()} {errorDebug}!");
        }
        
        GameObject options = Instantiate(faderOptionsPrefab);
        options.name = _config.name + " Options";
        options.transform.SetParent(optionsPanel.transform, false);
        options.GetComponent<FaderOptions>().controllerConfig = _config;
    }

    public void RespawnController(ControllerSettings _config)
    {
        foreach(KeyValuePair<ControllerSettings, GameObject> pair in controllerObjects)
        {
            if(_config == pair.Key)
            {
                Destroy(pair.Value);
                SpawnController(_config);
                return;
            }
        }

        Debug.LogError("Fader didn't exist already!");
    }

    //used to pair prefabs with their control type
    [Serializable]
    public struct ControllerType
    {
        public ControlType controlType;
        public GameObject controlObject;
    }
}

[Serializable]
public struct ValueCurve
{
    public CurveType curveType;
    public AnimationCurve curve;
}