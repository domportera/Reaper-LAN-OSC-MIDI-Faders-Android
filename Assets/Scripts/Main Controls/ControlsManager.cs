using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Colors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DomsUnityHelper;
using UnityEngine.Serialization;
using PopUpWindows;

public class ControlsManager : MonoBehaviourExtended
{
    //this class needs to create our wheel controls
    [SerializeField] ProfilesManager _profilesManager;
    [SerializeField] RectTransform _controllerParent;
    [SerializeField] ControllerPrefabs[] _controllerPrefabs;

    List<ControllerData> _controllers = new();
    public ReadOnlyCollection<ControllerData> Controllers { get { return _controllers.AsReadOnly(); } }

    readonly Dictionary<ControllerData, GameObject> _controllerObjects = new();

    #region Default Controller Values
    static readonly Dictionary<BuiltInOscPreset, OSCControllerSettings> DefaultOscSettings = new Dictionary<BuiltInOscPreset, OSCControllerSettings>()
    {
        {BuiltInOscPreset.Pitch,            OSCControllerSettings.DefaultOscTemplates[OscAddressType.MidiPitch] },
        {BuiltInOscPreset.Mod,              OSCControllerSettings.DefaultOscTemplates[OscAddressType.MidiCc] },
        {BuiltInOscPreset.Aftertouch,       OSCControllerSettings.DefaultOscTemplates[OscAddressType.MidiAftertouch] },
        {BuiltInOscPreset.FootPedal,        new OSCControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       4) },
        {BuiltInOscPreset.Expression,       new OSCControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       11) },
        {BuiltInOscPreset.BreathControl,    new OSCControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       2) },
        {BuiltInOscPreset.Volume,           new OSCControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       7) },
    };

    static readonly List<ControllerData> DefaultControllers = new List<ControllerData>
    {
        new FaderData("Pitch",          new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.PitchWheel,    DefaultOscSettings[BuiltInOscPreset.Pitch],         DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Mod",            new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Mod],           DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Foot Pedal",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.FootPedal],     DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Expression",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Expression],    DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Breath Control", new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.BreathControl], DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Aftertouch",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Aftertouch],    DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Volume",         new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Volume],        DefaultValueType.Mid, CurveType.Linear))
    };

    enum BuiltInOscPreset
    {
        Pitch, Mod, FootPedal, Expression, BreathControl, Aftertouch, Volume
    };

    const string NewFaderName = "New Fader";
    const string NewController2DName = "New 2D Controller";

    static readonly Dictionary<ControllerType, string> NewControllerNames = new Dictionary<ControllerType, string>()
    {
        { ControllerType.Fader, NewFaderName },
        { ControllerType.Controller2D, NewController2DName }
    };

    public static string GetDefaultControllerName(ControllerData _controller)
    {
        ControllerType type = ControllerTypes[_controller.GetType()];
        return NewControllerNames[type];
    }

    readonly FaderData _defaultFader = new FaderData(NewFaderName,
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, DefaultOscSettings[BuiltInOscPreset.Mod],         DefaultValueType.Min, CurveType.Linear));

    readonly Controller2DData _defaultController2D = new Controller2DData(NewController2DName,
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, DefaultOscSettings[BuiltInOscPreset.Mod],         DefaultValueType.Min, CurveType.Linear),
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, DefaultOscSettings[BuiltInOscPreset.Expression],  DefaultValueType.Min, CurveType.Linear));
    #endregion Default Controller Values

    public class ProfileEvent : UnityEvent<string> { }
    public ProfileEvent OnProfileLoaded = new();

    public static readonly Dictionary<Type, ControllerType> ControllerTypes = new()
    {
        {typeof(FaderData), ControllerType.Fader },
        {typeof(Controller2DData), ControllerType.Controller2D }
    };

    public static readonly Dictionary<Type, Type> ControllerClassesByControl = new()
    {
        {typeof(FaderControl), typeof(FaderData) },
        {typeof(Controller2D), typeof(Controller2DData) }
    };

    public static readonly Dictionary<Type, Type> ControllerClassesByData = new()
    {
        {typeof(FaderData), typeof(FaderControl) },
        {typeof(Controller2DData), typeof(Controller2D) }
    };

    public static ControlsManager Instance;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError($"There is a second ControlsManager in the scene!", this);
            Debug.LogError($"This is the first one", Instance);
        }
    }

    public void SetActiveProfile(ProfileLoader _profile)
    {
        NukeControllers();
        LoadControllers(_profile.GetName());
    }

    #region Saving and Loading

    public void LoadControllers(string profileName)
    {
        Log($"Loading {profileName}", this);

        if (profileName != ProfilesManager.DefaultSaveName)
        {
            ProfilesManager.ProfileSaveData loadedData = _profilesManager.LoadControlsFile(profileName);

            NukeControllers();

            if (loadedData != null && loadedData.GetControllers().Count > 0)
            {
                _controllers = loadedData.GetControllers();
                
                SpawnControllers(_controllers);
                OnProfileLoaded.Invoke(profileName);
            }
            else
            {
                //spawn defaults if no save data
                SpawnDefaultControllers();
                OnProfileLoaded.Invoke(ProfilesManager.DefaultSaveName);
                Debug.LogError($"Saved data for {profileName} was empty");
            }
        }
        else
        {
            Log($"Profile was default", this);
            SpawnDefaultControllers();
            OnProfileLoaded.Invoke(ProfilesManager.DefaultSaveName);
        }


        ColorController.LoadAndSetColorProfile(profileName);
    }

    void SpawnDefaultControllers()
    {
        Log("Spawning Defaults", this);
        NukeControllers();

        for (int i = 0; i < DefaultControllers.Count; i++)
        {
            ControllerData c = DefaultControllers[i];
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
            _controllers.Add(c);
        }

        SpawnControllers(_controllers, true);
    }

    void NukeControllers()
    {
        foreach(ControllerData c in Controllers)
        {
            UIManager.Instance.DestroyControllerGroup(c);
        }

        _controllerObjects.Clear();
        _controllers.Clear();
    }

    #endregion Saving and Loading

    void SpawnControllers(List<ControllerData> controllers, bool isDefault = false)
    {
        if (!isDefault)
        {
            controllers.Sort((s1, s2) => s1.GetPosition().CompareTo(s2.GetPosition()));
        }

        foreach (ControllerData set in controllers)
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
        PopUpController.Instance.MultiOptionWindow("Select a controller type", faderAction, controller2DAction, buttonAction, controllerTemplateAction);
    }

    void NewController(ControllerType type)
    {
        ControllerData newControl;

        switch (type)
        {
            case ControllerType.Fader:
                newControl = new FaderData(_defaultFader);
                break;
            case ControllerType.Controller2D:
                newControl = new Controller2DData(_defaultController2D);
                break;
            default:
                Debug.LogError($"Controller type {type} not implemented in ControlsManager", this);
                return;
        }

        GameObject newController = SpawnController(newControl);
        newControl.SetPosition(newController.transform.GetSiblingIndex());
        UIManager.Instance.ShowControllerOptions(newControl);
    }

    public GameObject SpawnController (ControllerData data)
    {
        GameObject prefab = GetControllerPrefabFromType(data.GetType());

        //spawn this type
        GameObject control = SpawnControllerObject(data, prefab);
        _controllerObjects.Add(data, control);

        if(!data.GetEnabled())
        {
            //do this after a frame to allow the controller to run its Start method
            DoNextFrame(() => control.SetActive(false));
        }

        UIManager.Instance.SpawnControllerOptions(data, control);

        if(!Controllers.Contains(data))
        {
            _controllers.Add(data);
        }

        return control;
    }

    GameObject SpawnControllerObject(ControllerData config, GameObject controlObject)
    {
        GameObject control = Instantiate(controlObject, _controllerParent, false);

        switch (config)
        {
            case FaderData fader:
                control.GetComponentInChildren<FaderControl>().Initialize(fader);
                break;
            case Controller2DData control2D:
                control.GetComponentInChildren<Controller2D>().Initialize(control2D);
                break;
            default:
                Debug.Log($"No way to spawn {config.GetType()}", this);
                break;
        }
        return control;
    }

    public void RespawnController(ControllerData config)
    {
        DestroyController(config);
        GameObject control = SpawnController(config);
        control.transform.SetSiblingIndex(config.GetPosition()); //there are bound to be issues here with ordering when faders are deleted and stuff
    }

    public void DestroyController(ControllerData config)
    {
        Destroy(_controllerObjects[config]);
        _controllerObjects.Remove(config);
        _controllers.Remove(config);
    }

    GameObject GetControllerPrefabFromType(Type type)
    {
        ControllerType controllerType = ControllerTypes[type];
        return GetControllerPrefabFromType(controllerType);
       
    }
    
    GameObject GetControllerPrefabFromType(ControllerType type)
    {
        foreach (ControllerPrefabs p in _controllerPrefabs)
        {
            if (p.ControlType == type)
            {
                if (p.ControlPrefab == null)
                {
                    Debug.LogError($"No prefab assigned to controller type {type}", this);
                }

                return p.ControlPrefab;
            }
        }

        Debug.LogError($"No ControllerType implemented in list for type {type}", this);
        return null;
    }

    //used to pair prefabs with their control type
    [Serializable]
    struct ControllerPrefabs
    {
        [FormerlySerializedAs("controlType")] public ControllerType ControlType;
        [FormerlySerializedAs("controlPrefab")] public GameObject ControlPrefab;
    }

}
