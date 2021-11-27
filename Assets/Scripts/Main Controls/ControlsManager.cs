using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviourExtended
{
    //this class needs to create our wheel controls
    [SerializeField] RectTransform controllerParent = null;

    [SerializeField] ControllerPrefabs[] controllerPrefabs = null;

    List<ControllerData> controllers = new List<ControllerData>();
    public ReadOnlyCollection<ControllerData> Controllers { get { return controllers.AsReadOnly(); } }

    Dictionary<ControllerData, GameObject> controllerObjects = new Dictionary<ControllerData, GameObject>();

    #region Default Controller Values
    static readonly List<ControllerData> defaultControllers = new List<ControllerData>
    {
        new FaderData("Pitch",          new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.PitchWheel,    defaultOSCSettings[BuiltInOSCPreset.Pitch],         DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Mod",            new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        defaultOSCSettings[BuiltInOSCPreset.Mod],           DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Foot Pedal",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        defaultOSCSettings[BuiltInOSCPreset.FootPedal],     DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Expression",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        defaultOSCSettings[BuiltInOSCPreset.Expression],    DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Breath Control", new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        defaultOSCSettings[BuiltInOSCPreset.BreathControl], DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Aftertouch",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        defaultOSCSettings[BuiltInOSCPreset.Aftertouch],    DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Volume",         new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        defaultOSCSettings[BuiltInOSCPreset.Volume],        DefaultValueType.Mid, CurveType.Linear))
    };

    static readonly Dictionary<BuiltInOSCPreset, OSCControllerSettings> defaultOSCSettings = new Dictionary<BuiltInOSCPreset, OSCControllerSettings>()
    {
        {BuiltInOSCPreset.Pitch,            OSCControllerSettings.defaultOSCTemplates[OSCAddressType.MidiPitch] },
        {BuiltInOSCPreset.Mod,              OSCControllerSettings.defaultOSCTemplates[OSCAddressType.MidiCC] },
        {BuiltInOSCPreset.Aftertouch,       OSCControllerSettings.defaultOSCTemplates[OSCAddressType.MidiAftertouch] },
        {BuiltInOSCPreset.FootPedal,        new OSCControllerSettings(OSCAddressType.MidiCC,            MIDIChannel.All, ValueRange.SevenBit,       4) },
        {BuiltInOSCPreset.Expression,       new OSCControllerSettings(OSCAddressType.MidiCC,            MIDIChannel.All, ValueRange.SevenBit,       11) },
        {BuiltInOSCPreset.BreathControl,    new OSCControllerSettings(OSCAddressType.MidiCC,            MIDIChannel.All, ValueRange.SevenBit,       2) },
        {BuiltInOSCPreset.Volume,           new OSCControllerSettings(OSCAddressType.MidiCC,            MIDIChannel.All, ValueRange.SevenBit,       7) },
    };

    enum BuiltInOSCPreset
    {
        Pitch, Mod, FootPedal, Expression, BreathControl, Aftertouch, Volume
    };

    const string NEW_FADER_NAME = "New Fader";
    const string NEW_CONTROLLER_2D_NAME = "New 2D Controller";

    static readonly Dictionary<ControllerType, string> newControllerNames = new Dictionary<ControllerType, string>()
    {
        { ControllerType.Fader, NEW_FADER_NAME },
        { ControllerType.Controller2D, NEW_CONTROLLER_2D_NAME }
    };

    public static string GetDefaultControllerName(ControllerData _controller)
    {
        ControllerType type = controllerTypes[_controller.GetType()];
        return newControllerNames[type];
    }

    readonly FaderData defaultFader = new FaderData(NEW_FADER_NAME,
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, defaultOSCSettings[BuiltInOSCPreset.Mod],         DefaultValueType.Min, CurveType.Linear));

    readonly Controller2DData defaultController2D = new Controller2DData(NEW_CONTROLLER_2D_NAME,
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, defaultOSCSettings[BuiltInOSCPreset.Mod],         DefaultValueType.Min, CurveType.Linear),
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, defaultOSCSettings[BuiltInOSCPreset.Expression],  DefaultValueType.Min, CurveType.Linear));
    #endregion Default Controller Values

    public class ProfileEvent : UnityEvent<string> { }
    public ProfileEvent OnProfileLoaded = new ProfileEvent();

    public static readonly Dictionary<Type, ControllerType> controllerTypes = new Dictionary<Type, ControllerType>()
    {
        {typeof(FaderData), ControllerType.Fader },
        {typeof(Controller2DData), ControllerType.Controller2D }
    };

    public static readonly Dictionary<Type, Type> controllerClassesByControl = new Dictionary<Type, Type>()
    {
        {typeof(FaderControl), typeof(FaderData) },
        {typeof(Controller2D), typeof(Controller2DData) }
    };

    public static readonly Dictionary<Type, Type> controllerClassesByData = new Dictionary<Type, Type>()
    {
        {typeof(FaderData), typeof(FaderControl) },
        {typeof(Controller2DData), typeof(Controller2D) }
    };

    public static ControlsManager instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"There is a second ControlsManager in the scene!", this);
            Debug.LogError($"This is the first one", instance);
        }
    }

    public void SetActiveProfile(string _name)
    {
        NukeControllers();
        LoadControllers(_name);
    }

    #region Saving and Loading

    public void LoadControllers(string _profile)
    {
        LogDebug($"Loading {_profile}");

        if (_profile != ProfilesManager.DEFAULT_SAVE_NAME)
        {
            ProfilesManager.ProfileSaveData loadedData = ProfilesManager.instance.LoadControlsFile(_profile);

            NukeControllers();

            if (loadedData != null || loadedData.GetControllers().Count > 0)
            {
                controllers = loadedData.GetControllers();
                
                SpawnControllers(controllers);
                OnProfileLoaded.Invoke(_profile);
            }
            else
            {
                //spawn defaults if no save data
                SpawnDefaultControllers();
                OnProfileLoaded.Invoke(ProfilesManager.DEFAULT_SAVE_NAME);
                Debug.LogError($"Saved data for {_profile} was empty");
            }
        }
        else
        {
            LogDebug($"Profile was default");
            SpawnDefaultControllers();
            OnProfileLoaded.Invoke(ProfilesManager.DEFAULT_SAVE_NAME);
        }
    }

    void SpawnDefaultControllers()
    {
        LogDebug("Spawning Defaults");
        NukeControllers();

        for (int i = 0; i < defaultControllers.Count; i++)
        {
            ControllerData c = defaultControllers[i];
            switch (c)
            {
                case FaderData fader:
                    c = new FaderData(fader);
                    break;
                case Controller2DData control2D:
                    c = new Controller2DData(control2D);
                    break;
                default:
                    Debug.Log($"No way to spawn {c.GetType()}", this);
                    break;
            }
            c.SetPosition(i);
            controllers.Add(c);
        }

        SpawnControllers(controllers, true);
    }

    void NukeControllers()
    {
        foreach(ControllerData c in Controllers)
        {
            UIManager.instance.DestroyControllerGroup(c);
        }

        controllerObjects.Clear();
        controllers.Clear();
    }

    #endregion Saving and Loading

    void SpawnControllers(List<ControllerData> _controllers, bool _isDefault = false)
    {
        if (!_isDefault)
        {
            _controllers.Sort((s1, s2) => s1.GetPosition().CompareTo(s2.GetPosition()));
        }

        foreach (ControllerData set in _controllers)
        {
            SpawnController(set);
        }
    }

    public void NewController()
    {
        MultiOptionAction faderAction = new MultiOptionAction("Fader", () => NewController(ControllerType.Fader));
        MultiOptionAction controller2DAction = new MultiOptionAction("2D Controller", () => NewController(ControllerType.Controller2D));
        MultiOptionAction buttonAction = new MultiOptionAction("Button", () => throw new NotImplementedException());
        MultiOptionAction controllerTemplateAction = new MultiOptionAction("From Template", () => throw new NotImplementedException());
        UtilityWindows.instance.MultiOptionWindow("Select a controller type", faderAction, controller2DAction, buttonAction, controllerTemplateAction);
    }

    void NewController(ControllerType _type)
    {
        ControllerData newControl;

        switch (_type)
        {
            case ControllerType.Fader:
                newControl = new FaderData(defaultFader);
                break;
            case ControllerType.Controller2D:
                newControl = new Controller2DData(defaultController2D);
                break;
            default:
                Debug.LogError($"Controller type {_type} not implemented in ControlsManager", this);
                return;
        }

        GameObject newController = SpawnController(newControl);
        newControl.SetPosition(newController.transform.GetSiblingIndex());
        UIManager.instance.ShowControllerOptions(newControl);
    }

    public GameObject SpawnController (ControllerData _data)
    {
        GameObject control;
        GameObject prefab = GetControllerPrefabFromType(_data.GetType());

        //spawn this type
        control = SpawnControllerObject(_data, prefab);
        controllerObjects.Add(_data, control);

        if(!_data.GetEnabled())
        {
            //do this after a frame to allow the controller to run its Start method
            DoNextFrame(() => control.SetActive(false));
        }

        UIManager.instance.SpawnControllerOptions(_data, control);

        if(!Controllers.Contains(_data))
        {
            controllers.Add(_data);
        }

        return control;
    }

    GameObject SpawnControllerObject(ControllerData _config, GameObject _controlObject)
    {
        GameObject control = Instantiate(_controlObject);
        control.transform.SetParent(controllerParent, false);

        switch (_config)
        {
            case FaderData fader:
                control.GetComponentInChildren<FaderControl>().Initialize(fader);
                break;
            case Controller2DData control2D:
                control.GetComponentInChildren<Controller2D>().Initialize(control2D);
                break;
            default:
                Debug.Log($"No way to spawn {_config.GetType()}", this);
                break;
        }
        return control;
    }

    public void RespawnController(ControllerData _config)
    {
        DestroyController(_config);
        GameObject control = SpawnController(_config);
        control.transform.SetSiblingIndex(_config.GetPosition()); //there are bound to be issues here with ordering when faders are deleted and stuff
    }

    public void DestroyController(ControllerData _config)
    {
        Destroy(controllerObjects[_config]);
        controllerObjects.Remove(_config);
        controllers.Remove(_config);
    }

    GameObject GetControllerPrefabFromType(Type _type)
    {
        ControllerType controllerType = controllerTypes[_type];
        return GetControllerPrefabFromType(controllerType);
       
    }
    
    GameObject GetControllerPrefabFromType(ControllerType _type)
    {
        foreach (ControllerPrefabs p in controllerPrefabs)
        {
            if (p.controlType == _type)
            {
                if (p.controlPrefab == null)
                {
                    Debug.LogError($"No prefab assigned to controller type {_type}", this);
                }

                return p.controlPrefab;
            }
        }

        Debug.LogError($"No ControllerType implemented in list for type {_type}", this);
        return null;
    }
    public float GetFaderWidth()
    {
        foreach (ControllerData c in controllers)
        {
            if(c.GetType() == typeof(FaderData))
            {
                return c.GetWidth();
            }
        }

        return ControllerData.widthRanges[typeof(FaderData)].defaultValue;
    }

    //used to pair prefabs with their control type
    [Serializable]
    struct ControllerPrefabs
    {
        public ControllerType controlType;
        public GameObject controlPrefab;
    }

}
