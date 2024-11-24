using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Colors;
using UnityEngine;
using UnityEngine.Serialization;
using PopUpWindows;

public class ControlsManager : MonoBehaviour
{
    //this class needs to create our wheel controls
    [SerializeField] private ProfilesManager _profilesManager;
    [SerializeField] private RectTransform _controllerParent;
    [SerializeField] private ControllerPrefabs[] _controllerPrefabs;

    private List<ControllerData> _controllers = new();
    public ReadOnlyCollection<ControllerData> Controllers { get { return _controllers.AsReadOnly(); } }

    private readonly Dictionary<ControllerData, GameObject> _controllerObjects = new();
    private readonly Queue<Action> _actionQueue = new Queue<Action>();

    #region Default Controller Values

    private static readonly Dictionary<BuiltInOscPreset, OscControllerSettings> DefaultOscSettings = new Dictionary<BuiltInOscPreset, OscControllerSettings>()
    {
        {BuiltInOscPreset.Pitch,            OscControllerSettings.DefaultOscTemplates[OscAddressType.MidiPitch] },
        {BuiltInOscPreset.Mod,              OscControllerSettings.DefaultOscTemplates[OscAddressType.MidiCc] },
        {BuiltInOscPreset.Aftertouch,       OscControllerSettings.DefaultOscTemplates[OscAddressType.MidiAftertouch] },
        {BuiltInOscPreset.FootPedal,        new OscControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       4) },
        {BuiltInOscPreset.Expression,       new OscControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       11) },
        {BuiltInOscPreset.BreathControl,    new OscControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       2) },
        {BuiltInOscPreset.Volume,           new OscControllerSettings(OscAddressType.MidiCc,            MidiChannel.All, ValueRange.SevenBit,       7) },
    };

    private static readonly List<ControllerData> DefaultControllers = new List<ControllerData>
    {
        new FaderData("Pitch",          new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.PitchWheel,    DefaultOscSettings[BuiltInOscPreset.Pitch],         DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Mod",            new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Mod],           DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Foot Pedal",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.FootPedal],     DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Expression",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Expression],    DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Breath Control", new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.BreathControl], DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Aftertouch",     new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Aftertouch],    DefaultValueType.Mid, CurveType.Linear)),
        new FaderData("Volume",         new ControllerSettings(InputMethod.Touch,    ReleaseBehaviorType.Normal,        DefaultOscSettings[BuiltInOscPreset.Volume],        DefaultValueType.Mid, CurveType.Linear))
    };

    private enum BuiltInOscPreset
    {
        Pitch, Mod, FootPedal, Expression, BreathControl, Aftertouch, Volume
    };

    private const string NewFaderName = "New Fader";
    private const string NewController2DName = "New 2D Controller";

    private static readonly Dictionary<ControllerType, string> NewControllerNames = new Dictionary<ControllerType, string>()
    {
        { ControllerType.Fader, NewFaderName },
        { ControllerType.Controller2D, NewController2DName }
    };

    public static string GetDefaultControllerName(ControllerData controller)
    {
        var type = ControllerTypes[controller.GetType()];
        return NewControllerNames[type];
    }

    private readonly FaderData _defaultFader = new FaderData(NewFaderName,
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, DefaultOscSettings[BuiltInOscPreset.Mod],         DefaultValueType.Min, CurveType.Linear));

    private readonly Controller2DData _defaultController2D = new Controller2DData(NewController2DName,
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, DefaultOscSettings[BuiltInOscPreset.Mod],         DefaultValueType.Min, CurveType.Linear),
        new ControllerSettings(InputMethod.Touch, ReleaseBehaviorType.Normal, DefaultOscSettings[BuiltInOscPreset.Expression],  DefaultValueType.Min, CurveType.Linear));
    #endregion Default Controller Values

    public event Action<string> OnProfileLoaded;

    public static readonly Dictionary<Type, ControllerType> ControllerTypes = new()
    {
        {typeof(FaderData), ControllerType.Fader },
        {typeof(Controller2DData), ControllerType.Controller2D }
    };

    public static readonly Dictionary<Type, Type> ControllerClassesByData = new()
    {
        {typeof(FaderData), typeof(FaderControlUi) },
        {typeof(Controller2DData), typeof(Controller2DUi) }
    };

    public void SetActiveProfile(ProfileLoader profile)
    {
        NukeControllers();
        LoadControllers(profile.GetName());
    }

    #region Saving and Loading

    public void LoadControllers(string profileName)
    {
        Debug.Log($"Loading {profileName}", this);

        if (profileName != ProfilesManager.DefaultSaveName)
        {
            var loadedData = _profilesManager.LoadControlsFile(profileName);

            NukeControllers();

            if (loadedData != null && loadedData.GetControllers().Count > 0)
            {
                _controllers = loadedData.GetControllers();
                
                SpawnControllers(_controllers);
                OnProfileLoaded?.Invoke(profileName);
            }
            else
            {
                //spawn defaults if no save data
                SpawnDefaultControllers();
                OnProfileLoaded?.Invoke(ProfilesManager.DefaultSaveName);
                Debug.LogError($"Saved data for {profileName} was empty");
            }
        }
        else
        {
            Debug.Log($"Profile was default", this);
            SpawnDefaultControllers();
            OnProfileLoaded?.Invoke(ProfilesManager.DefaultSaveName);
        }


        ColorController.LoadAndSetColorProfile(profileName);
    }

    private void SpawnDefaultControllers()
    {
        Debug.Log("Spawning Defaults", this);
        NukeControllers();

        for (var i = 0; i < DefaultControllers.Count; i++)
        {
            var c = DefaultControllers[i];
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

    private void NukeControllers()
    {
        foreach(var c in Controllers)
        {
            UIManager.Instance.DestroyControllerGroup(c);
        }

        _controllerObjects.Clear();
        _controllers.Clear();
    }

    #endregion Saving and Loading

    private void SpawnControllers(List<ControllerData> controllers, bool isDefault = false)
    {
        if (!isDefault)
        {
            controllers.Sort((s1, s2) => s1.GetPosition().CompareTo(s2.GetPosition()));
        }

        foreach (var set in controllers)
        {
            SpawnController(set);
        }
    }

    public void NewController()
    {
        var faderAction = new MultiOptionAction("Fader", () => NewController(ControllerType.Fader));
        var controller2DAction = new MultiOptionAction("2D Controller", () => NewController(ControllerType.Controller2D));
        var buttonAction = new MultiOptionAction("Button", () => throw new NotImplementedException());
        var controllerTemplateAction = new MultiOptionAction("From Template", () => throw new NotImplementedException());
        PopUpController.Instance.MultiOptionWindow("Select a controller type", faderAction, controller2DAction, buttonAction, controllerTemplateAction);
    }

    private void NewController(ControllerType type)
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

        var newController = SpawnController(newControl);
        newControl.SetPosition(newController.transform.GetSiblingIndex());
        UIManager.Instance.ShowControllerOptions(newControl);
    }

    private void Update()
    {
        while(_actionQueue.TryDequeue(out var action))
            action.Invoke();
    }

    public GameObject SpawnController (ControllerData data)
    {
        var prefab = GetControllerPrefabFromType(data.GetType());

        //spawn this type
        var control = SpawnControllerObject(data, prefab);
        _controllerObjects.Add(data, control);

        if(!data.GetEnabled())
        {
            //do this after a frame to allow the controller to run its Start method
            _actionQueue.Enqueue(() => control.SetActive(false));
        }

        UIManager.Instance.SpawnControllerOptions(data, control);

        if(!Controllers.Contains(data))
        {
            _controllers.Add(data);
        }

        return control;
    }

    private GameObject SpawnControllerObject(ControllerData config, GameObject controlObject)
    {
        var control = Instantiate(controlObject, _controllerParent, false);

        switch (config)
        {
            case FaderData fader:
                control.GetComponentInChildren<FaderControlUi>().Initialize(fader);
                break;
            case Controller2DData control2D:
                control.GetComponentInChildren<Controller2DUi>().Initialize(control2D);
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
        var control = SpawnController(config);
        control.transform.SetSiblingIndex(config.GetPosition()); //there are bound to be issues here with ordering when faders are deleted and stuff
    }

    public void DestroyController(ControllerData config)
    {
        Destroy(_controllerObjects[config]);
        _controllerObjects.Remove(config);
        _controllers.Remove(config);
    }

    private GameObject GetControllerPrefabFromType(Type type)
    {
        var controllerType = ControllerTypes[type];
        return GetControllerPrefabFromType(controllerType);
       
    }

    private GameObject GetControllerPrefabFromType(ControllerType type)
    {
        foreach (var p in _controllerPrefabs)
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
    private struct ControllerPrefabs
    {
        [FormerlySerializedAs("controlType")] public ControllerType ControlType;
        [FormerlySerializedAs("controlPrefab")] public GameObject ControlPrefab;
    }

}
