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

    static int uniqueIDGen;

    UIManager uiManager;

    void Start()
    {
        //load controllers
        //needs to load defaults

        //spawn defaults if no save data
        foreach(ControllerSettings set in defaultControllers)
        {
            controllers.Add(set);
        }

        uiManager = FindObjectOfType<UIManager>();
        SpawnControllers();
    }

    public static int GetUniqueID()
    {
        return uniqueIDGen++;
    }

    void SpawnControllers()
    {
        foreach(ControllerSettings set in controllers)
        {
            SpawnController(set);
        }
    }

    public void NewController()
    {
        ControllerSettings newControl = new ControllerSettings("New Controller", ControlType.Fader, AddressType.CC, ValueRange.SevenBit, DefaultValueType.Min, MIDIChannel.All, CurveType.Linear);
        controllers.Add(newControl);
        SpawnController(newControl);
    }

    public void SpawnController (ControllerSettings _config)
    {
        bool error = true;
        string errorDebug = "doesn't exist!";
        GameObject control = null;
        foreach (ControllerType t in controllerTypes)
        {
            if (t.controlType == _config.controlType)
            {
                //spawn this type
                if (t.controlObject != null)
                {
                    control = Instantiate(t.controlObject);
                    control.transform.SetParent(controllerParent, false);
                    control.GetComponentInChildren<FaderControl>().Initialize(_config, valueCurves);
                    controllerObjects.Add(_config, control);
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

        if(control == null)
        {
            Debug.LogError("Null control object! not in controller types?");
        }

        uiManager.SpawnFaderOptions(_config, control);
    }

    public void RespawnController(ControllerSettings _config)
    {
        DestroyController(_config);
        SpawnController(_config);
        FindObjectOfType<IPSetter>().TryConnect(); //quick and easy way - reconnect all sliders when done respawning a controller
    }

    public void DestroyController(ControllerSettings _config)
    {
        Destroy(controllerObjects[_config]);
        controllerObjects.Remove(_config);
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