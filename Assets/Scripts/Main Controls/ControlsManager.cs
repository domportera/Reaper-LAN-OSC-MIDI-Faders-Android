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
    public static readonly List<ControllerData> defaultControllers = new List<ControllerData>
    {
        new FaderData("Pitch",          new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.PitchWheel,    OSCAddressType.MidiPitch,           ValueRange.FourteenBit, DefaultValueType.Mid,   MIDIChannel.All,    CurveType.Linear)),
        new FaderData("Mod",            new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        OSCAddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid,   MIDIChannel.All,    CurveType.Linear, 1)),
        new FaderData("Foot Pedal",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        OSCAddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid,   MIDIChannel.All,    CurveType.Linear, 4)),
        new FaderData("Expression",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        OSCAddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid,   MIDIChannel.All,    CurveType.Linear, 11)),
        new FaderData("Breath Control", new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        OSCAddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid,   MIDIChannel.All,    CurveType.Linear, 2)),
        new FaderData("Aftertouch",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        OSCAddressType.MidiAftertouch,      ValueRange.SevenBit,    DefaultValueType.Mid,   MIDIChannel.All,    CurveType.Linear)),
        new FaderData("Volume",         new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        OSCAddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid,   MIDIChannel.All,    CurveType.Linear, 7))
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
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, OSCAddressType.MidiCC, ValueRange.SevenBit, DefaultValueType.Min, MIDIChannel.All, CurveType.Linear));

    readonly Controller2DData defaultController2D = new Controller2DData(NEW_CONTROLLER_2D_NAME,
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, OSCAddressType.MidiCC, ValueRange.SevenBit, DefaultValueType.Min, MIDIChannel.All, CurveType.Linear, 1),
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, OSCAddressType.MidiCC, ValueRange.SevenBit, DefaultValueType.Min, MIDIChannel.All, CurveType.Linear, 11));
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


    public static List<char> GetInvalidFileNameCharacters(string _name)
    {
        //check for invalid characters
        char[] invalidFileChars = Path.GetInvalidFileNameChars();
        List<char> invalidChars = new List<char>();
        foreach (char c in invalidFileChars)
        {
            if (_name.Contains(c.ToString()))
            {
                invalidChars.Add(c);
            }
        }

        return invalidChars;
    }

    public void LoadControllers(string _profile)
    {
        Debug.Log($"Loading {_profile}");

        if (_profile != ProfilesManager.DEFAULT_SAVE_NAME)
        {
            ProfilesManager.ProfileSaveData loadedData = ProfilesManager.instance.LoadControlsFile(_profile);

            NukeControllers();

            if (loadedData != null || loadedData.GetControllers().Count > 0)
            {
                List<ControllerData> temp = loadedData.GetControllers();
                controllers = temp;
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
            Debug.Log($"Profile was default");
            OnProfileLoaded.Invoke(ProfilesManager.DEFAULT_SAVE_NAME);
            SpawnDefaultControllers();
        }
    }

    void SpawnDefaultControllers()
    {
        Debug.Log("Spawning Defaults");
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
        Utilities.instance.MultiOptionWindow("Select a controller type", faderAction, controller2DAction);
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
                newControl = null;
                Debug.LogError($"Controller type {_type} not implemented in ControlsManager", this);
                break;
        }

        GameObject newController = SpawnController(newControl);

        if (newController != null)
        {
            newControl.SetPosition(newController.transform.GetSiblingIndex());
        }
    }

    public GameObject SpawnController (ControllerData _data)
    {
        GameObject control = null;
        GameObject prefab = GetControllerPrefabFromType(_data.GetType());

        //spawn this type
        control = SpawnControllerObject(_data, prefab);
        controllerObjects.Add(_data, control);

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

    //used to pair prefabs with their control type
    [Serializable]
    struct ControllerPrefabs
    {
        public ControllerType controlType;
        public GameObject controlPrefab;
    }

    [Serializable]
    public abstract class ControllerData
    {
        [SerializeField] protected string name;
        [SerializeField] protected List<ControllerSettings> controllers = new List<ControllerSettings>();
        [SerializeField] protected int position = NULL_POSITION;
        [SerializeField] protected bool enabled = true;
        [SerializeField] protected float width = 1f;

        protected const int NULL_POSITION = -1;

        public void SetPosition(int _index)
        {
            position = _index;
        }

        public int GetPosition()
        {
            return position;
        }

        public ControllerSettings GetController()
        {
            if(controllers.Count > 0)
            {
                return controllers[0];
            }

            return null;
        }

        public List<ControllerSettings> GetControllers()
        {
            return controllers;
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string _name)
        {
            name = _name;
        }

        public void SetEnabled(bool _enabled)
        {
            enabled = _enabled;
        }

        public void SetWidth(float value)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class FaderData : ControllerData
    {
        public FaderData(string name, ControllerSettings config)
        {
            controllers.Add(config);
            this.name = name;
        }

        public FaderData(FaderData _data)
        {
            name = _data.name;
            controllers = _data.controllers;
            position = _data.GetPosition();
        }
    }

    [Serializable]
    public class Controller2DData : ControllerData
    {
        public Controller2DData(string name, ControllerSettings horizontalConfig, ControllerSettings verticalConfig)
        {
            controllers.Add(horizontalConfig);
            controllers.Add(verticalConfig);
            this.name = name;
        }
        public Controller2DData(Controller2DData _data)
        {
            name = _data.name;
            controllers = _data.controllers;
            position = _data.GetPosition();
        }

        public ControllerSettings GetHorizontalController()
        {
            return controllers[0];
        }
        public ControllerSettings GetVerticalController()
        {
            return controllers[1];
        }
    }

    
}
