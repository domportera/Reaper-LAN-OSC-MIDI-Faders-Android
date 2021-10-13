using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
    //this class needs to create our wheel controls
    [SerializeField] RectTransform controllerParent = null;

    public static readonly List<ControllerData> defaultControllers = new List<ControllerData>
    {
        new FaderData("Pitch",          new ControllerSettings(InputType.Touch,    ControlBehaviorType.ReturnToCenter,     AddressType.MidiPitch,           ValueRange.FourteenBit, DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear)),
        new FaderData("Mod",            new ControllerSettings(InputType.Touch,    ControlBehaviorType.Normal,             AddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear, 1)),
        new FaderData("Foot Pedal",     new ControllerSettings(InputType.Touch,    ControlBehaviorType.Normal,             AddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear, 4)),
        new FaderData("Expression",     new ControllerSettings(InputType.Touch,    ControlBehaviorType.Normal,             AddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear, 11)),
        new FaderData("Breath Control", new ControllerSettings(InputType.Touch,    ControlBehaviorType.Normal,             AddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear, 2)),
        new FaderData("Aftertouch",     new ControllerSettings(InputType.Touch,    ControlBehaviorType.Normal,             AddressType.MidiAftertouch,      ValueRange.SevenBit,    DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear)),
        new FaderData("Volume",         new ControllerSettings(InputType.Touch,    ControlBehaviorType.Normal,             AddressType.MidiCC,              ValueRange.SevenBit,    DefaultValueType.Mid, MIDIChannel.All, CurveType.Linear, 7))
    };

    [SerializeField] ControllerPrefabs[] controllerPrefabs = null;

    List<ControllerData> controllers = new List<ControllerData>();

    Dictionary<ControllerData, GameObject> controllerObjects = new Dictionary<ControllerData, GameObject>();

    static int uniqueIDGen = 0;

    UIManager uiManager;
    IPSetter ipSetter;
    ProfilesManager profileManager;

    public const string NEW_CONTROLLER_NAME = "New Controller";

    //saving variables
    public const string DEFAULT_SAVE_NAME = "Default";
    const string PROFILE_NAME_SAVE_NAME = "Profiles"; //name of json file that stores all profile names
    ProfilesMetadata profileNames = null;

    string basePath;
    const string CONTROLS_EXTENSION = ".controls";
    const string PROFILES_EXTENSION = ".profiles";

    public class ProfileEvent : UnityEvent<string> { }
    public ProfileEvent OnProfileLoaded = new ProfileEvent();

    readonly Dictionary<Type, ControllerType> controllerTypes = new Dictionary<Type, ControllerType>()
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

        basePath = Path.Combine(Application.persistentDataPath, "Controllers");
        uiManager = FindObjectOfType<UIManager>();
        ipSetter = FindObjectOfType<IPSetter>();
        profileManager = FindObjectOfType<ProfilesManager>();

        //load profile file names
        LoadProfileNames();

    }

	private void Start()
    {
        //populate ui with profile names to allow for saving
        PopulateProfileSelectionMenu();

        //let color controller know we're ready for default color 
        //OnProfileLoaded.Invoke(DEFAULT_SAVE_NAME);
	}

	public void SetActiveProfile(string _name)
    {
        NukeControllers();
        LoadControllers(_name);
        ipSetter.TryConnect();
    }

    public List<ControllerData> GetAllControllers()
    {
        return controllers;
    }

    public void SetDefaultProfile(string _profile)
    {
        profileNames.SetDefaultProfile(_profile);
        SaveProfileNames();
    }

    public void DeleteProfile(string _name)
    {
        if(_name == DEFAULT_SAVE_NAME)
        {
            Utilities.instance.ErrorWindow("Can't delete default profile");
            return;
        }
        //remove profile from current list of profiles
        profileNames.RemoveProfile(_name);

        //delete file
        DeleteFile(_name);

        //Save profiles
        SaveProfileNames();

        //reload profiles
        //LoadProfileNames();

        //repopulate dropdown
        PopulateProfileSelectionMenu();

        //load controller profile
        LoadDefaultProfile();
    }

    #region Saving and Loading

    public void SaveControllers(string _name)
    {
        if(_name == DEFAULT_SAVE_NAME)
        {
            Utilities.instance.ErrorWindow("Can't overwrite defaults, use Save As instead in the Profiles page.");
            return;
        }

        controllers.Sort((s1, s2) => s1.GetName().CompareTo(s2.GetName()));

        string json = JsonUtility.ToJson(new ProfileSaveData(controllers, _name), true);
        SaveControlsFile(_name, json);

        Utilities.instance.ConfirmationWindow($"Saved {_name}");
    }

    public bool SaveControllersAs(string _name)
    {
        if (_name == DEFAULT_SAVE_NAME || profileNames.GetNames().Contains(_name))
        {
            Utilities.instance.ErrorWindow("Profile with this name already exists, please use another.");
            return false;
        }

        List<char> invalidChars = GetInvalidFileNameCharacters(_name);
        if(invalidChars.Count > 0)
        {
            if (invalidChars.Count == 1)
            {
                Utilities.instance.ErrorWindow($"Chosen profile name contains an invalid character.");
            }
            else
            {
                Utilities.instance.ErrorWindow($"Chosen profile name contains {invalidChars.Count} invalid characters.");
            }
            return false;
		}

        if (_name.Length > 0)
        {
            string profileName = _name;
            profileNames.AddProfile(profileName);
            SaveProfileNames();
            profileManager.AddToProfileButtons(profileName);
            //add this profile to  working profiles in profile selection ui
            //switch to this profile
            SaveControllers(_name);
            return true;
        }
        else
        {
            Utilities.instance.ErrorWindow("Please enter a name.");
            return false;
        }
    }

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

    void LoadControllers(string _profile)
    {
        Debug.Log($"Loading {_profile}");

        if (_profile != DEFAULT_SAVE_NAME)
        {
            string json = LoadControlsFile(_profile);

            if (json != null)
            {
                ProfileSaveData loadedData = JsonUtility.FromJson<ProfileSaveData>(json);
                NukeControllers();

                if (loadedData != null || loadedData.GetControllers().Count > 0)
                {
                    List<ControllerData> temp = loadedData.GetControllers();

                    //properly initialize each controller settings
                    //for (int i = 0; i < temp.Count; i++)
                    //{
                    //    temp[i] = new ControllerData(temp[i]);
                    //}

                    controllers = temp;
                    SpawnControllers(temp);
                    OnProfileLoaded.Invoke(_profile);
                }
                else
                {
                    //spawn defaults if no save data
                    SpawnDefaultControllers();
                    OnProfileLoaded.Invoke(DEFAULT_SAVE_NAME);
                    Debug.LogError($"Saved data for {_profile} was empty");
                }
            }
            else
            {
                Debug.LogError($"JSON object for {_profile} was null");
                OnProfileLoaded.Invoke(DEFAULT_SAVE_NAME);
                SpawnDefaultControllers();
            }
        }
        else
        {
            Debug.Log($"Profile was default");
            OnProfileLoaded.Invoke(DEFAULT_SAVE_NAME);
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
        foreach(ControllerData c in controllers)
        {
            uiManager.DestroyControllerGroup(c);
        }

        controllerObjects = new Dictionary<ControllerData, GameObject>();

        controllers.Clear();
    }

    void PopulateProfileSelectionMenu()
    {
        profileManager.PopulateProfileButtons(profileNames.GetNames(), profileNames.GetDefaultProfileName());
    }

    void LoadDefaultProfile ()
    {
        if (profileNames.GetNames().Count < 1)
        {
            LoadControllers(DEFAULT_SAVE_NAME);
        }
        else
        {
            LoadControllers(profileNames.GetDefaultProfileName());
        }
    }

    void LoadProfileNames()
    {
        string json = LoadFile(PROFILE_NAME_SAVE_NAME, PROFILES_EXTENSION);

        if (json != null)
        {
            profileNames = JsonUtility.FromJson<ProfilesMetadata>(json);
        }
        else
        {
            profileNames = new ProfilesMetadata(new List<string> { });
        }
    }

    void SaveProfileNames()
    {
        string json = JsonUtility.ToJson(profileNames, true);
        SaveFile(PROFILE_NAME_SAVE_NAME, PROFILES_EXTENSION, json);
    }

    void SaveControlsFile(string _fileNameSansExtension, string _data)
    {
        if(!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
		}

        SaveFile(_fileNameSansExtension, CONTROLS_EXTENSION, _data);
    }

    void SaveFile(string _fileNameSansExtension, string _fileExtension, string _data)
    {
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        string path = Path.Combine(basePath, _fileNameSansExtension + _fileExtension);
        File.WriteAllText(path, _data);
    }

    string LoadControlsFile(string _fileNameSansExtension)
    {
        return LoadFile(_fileNameSansExtension, CONTROLS_EXTENSION);
    }

    string LoadFile(string _fileNameSansExtension, string _fileNameExtension)
    {
        try
        {
            string path = Path.Combine(basePath, _fileNameSansExtension + _fileNameExtension);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return json;
            }
            else
            {
                Debug.LogWarning($"No file at {path} exists");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failure to load {_fileNameSansExtension}!\n" + e);
            return null;
        }
    }

    void DeleteFile(string _fileNameSansExtension)
    {
        string filePath = basePath + _fileNameSansExtension + CONTROLS_EXTENSION;

        File.Delete(filePath);
    }

    #endregion Saving and Loading

    public static int GetUniqueID()
    {
        return uniqueIDGen++;
    }

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
        
    }

    void NewFader()
    {
        FaderData newControl = new FaderData(NEW_CONTROLLER_NAME, new ControllerSettings(InputType.Touch, ControlBehaviorType.Normal, AddressType.MidiCC, ValueRange.SevenBit, DefaultValueType.Min, MIDIChannel.All, CurveType.Linear));
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

        uiManager.SpawnControllerOptions(_data, control);

        if(!controllers.Contains(_data))
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
        ipSetter.TryConnect(); //quick and easy way - reconnect all sliders when done respawning a controller
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


    [Serializable]
    class ProfileSaveData
    {
        public List<ControllerData> controllers;
        public string name = null;

        public ProfileSaveData(List<ControllerData> _controllerData, string _name)
        {
            controllers = _controllerData;
            name = _name;
        }

        public List<ControllerData> GetControllers()
        {
            return controllers;
        }

        public string GetName()
        {
            return name;
        }
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
        [SerializeField] protected List<ControllerSettings> controllers;
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
    }

    [Serializable]
    public class FaderData : ControllerData
    {
        public FaderData(string name, ControllerSettings config)
        {
            controllers = new List<ControllerSettings>();
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

    [Serializable]
    class ProfilesMetadata
    {
        [SerializeField] List<string> profileNames;
        [SerializeField] string defaultProfileName;

        public ProfilesMetadata(List<string> _profileNames)
        {
            profileNames = _profileNames;
            defaultProfileName = DEFAULT_SAVE_NAME;
        }

        public void AddProfile(string _name)
        {
            profileNames.Add(_name);
        }

        public void RemoveProfile(string _name)
        {
            profileNames.Remove(_name);

            if(_name == defaultProfileName)
            {
                defaultProfileName = DEFAULT_SAVE_NAME;
            }
        }

        public List<string> GetNames()
        {
            return profileNames;
        }

        public string GetDefaultProfileName()
        {
            return defaultProfileName;
        }

        public void SetDefaultProfile(string _name)
        {
            defaultProfileName = _name;
        }
    }

    public class Profile
    {
        public string name;

        public Profile(string _name)
        {
            name = _name;
		}
	}
}
