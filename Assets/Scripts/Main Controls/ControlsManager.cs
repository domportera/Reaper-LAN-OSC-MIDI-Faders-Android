﻿using System;
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

    List<ControllerSettings> controllers = new List<ControllerSettings>();

    Dictionary<ControllerSettings, GameObject> controllerObjects = new Dictionary<ControllerSettings, GameObject>();

    static int uniqueIDGen = 0;

    UIManager uiManager;
    IPSetter ipSetter;

    public const string NEW_CONTROLLER_NAME = "New Controller";

    //saving variables
    public const string DEFAULT_SAVE_NAME = "Default";
    const string PROFILE_NAME_SAVE_NAME = "Profiles"; //name of json file that stores all profile names
    Profiles profileNames = null;

    public class ProfileEvent : UnityEvent<string> { }
    public ProfileEvent OnProfileLoaded = new ProfileEvent();

    void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        ipSetter = FindObjectOfType<IPSetter>();

        //load profile file names
        LoadProfileNames();

        //populate ui with profile names to allow for saving
        PopulateProfileSelectionMenu();
    }

	private void Start()
	{
        //let color controller know we're ready for default color scheme
        OnProfileLoaded.Invoke(DEFAULT_SAVE_NAME);
	}

	public void SetActiveProfile(string _name)
    {
        NukeControllers();
        LoadControllers(_name);
        ipSetter.TryConnect();
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
            Utilities.instance.SetErrorText("Can't delete default profile");
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
            Utilities.instance.SetErrorText("Can't overwrite defaults, use Save As instead.");
            return;
        }

        controllers.Sort((s1, s2) => s1.name.CompareTo(s2.name));

        string json = JsonUtility.ToJson(new FaderSaver(controllers, _name), true);
        SaveFile(_name, json);

        Utilities.instance.SetConfirmationText($"Saved {_name}");
    }

    public bool SaveControllersAs(string _name)
    {
        if (_name == DEFAULT_SAVE_NAME || profileNames.GetNames().Contains(_name))
        {
            Utilities.instance.SetErrorText("Profile with this name already exists, please use another.");
            return false;
        }

        List<char> invalidChars = GetInvalidFileNameCharacters(_name);
        if(invalidChars.Count > 0)
        {
            if (invalidChars.Count == 1)
            {
                Utilities.instance.SetErrorText($"Chosen profile name contains an invalid character.");
            }
            else
            {
                Utilities.instance.SetErrorText($"Chosen profile name contains {invalidChars.Count} invalid characters.");
            }
            return false;
		}

        if (_name.Length > 0)
        {
            string profileName = _name;
            profileNames.AddProfile(profileName);
            SaveProfileNames();
            uiManager.AddToPopulateProfileDropdown(profileName);
            //add this profile to  working profiles in profile selection ui
            //switch to this profile
            SaveControllers(_name);
            return true;
        }
        else
        {
            Utilities.instance.SetErrorText("Please enter a name.");
            return false;
        }
    }

    List<char> GetInvalidFileNameCharacters(string _name)
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
            string json = LoadFile(_profile);

            if (json != null)
            {
                FaderSaver loadedData = JsonUtility.FromJson<FaderSaver>(json);
                NukeControllers();

                if (loadedData != null || loadedData.GetControllers().Count > 0)
                {
                    List<ControllerSettings> temp = loadedData.GetControllers();

                    //properly initialize each controller settings
                    for (int i = 0; i < temp.Count; i++)
                    {
                        temp[i] = new ControllerSettings(temp[i]);
                    }

                    controllers = temp;
                    OnProfileLoaded.Invoke(_profile);
                    SpawnControllers();
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
        controllers = new List<ControllerSettings>();

        for (int i = 0; i < defaultControllers.Length; i++)
        {
            ControllerSettings c = new ControllerSettings(defaultControllers[i]);
            c.SetPosition(i);
            controllers.Add(c);
        }

        SpawnControllers(true);
    }

    void NukeControllers()
    {
        foreach(ControllerSettings c in controllers)
        {
            uiManager.DestroyControllerGroup(c);
        }

        controllerObjects = new Dictionary<ControllerSettings, GameObject>();

        controllers = new List<ControllerSettings>();
    }

    void PopulateProfileSelectionMenu()
    {
        uiManager.PopulateProfileDropdown(profileNames.GetNames(), profileNames.GetDefaultProfileName());
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
        string json = LoadFile(PROFILE_NAME_SAVE_NAME);

        if (json != null)
        {
            profileNames = JsonUtility.FromJson<Profiles>(json);
        }
        else
        {
            profileNames = new Profiles(new List<string> { });
        }
    }

    void SaveProfileNames()
    {
        string json = JsonUtility.ToJson(profileNames, true);
        SaveFile(PROFILE_NAME_SAVE_NAME, json);
    }

    bool SaveFile(string _fileNameSansExtension, string _data)
    {
        try
        {
            StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/" + _fileNameSansExtension + ".json");
            sw.Write(_data);
            sw.Close();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failure to save {_fileNameSansExtension}!\n" + e);
            return false;
        }
    }

    string LoadFile(string _fileNameSansExtension)
    {
        try
        {
            StreamReader sr = new StreamReader(Application.persistentDataPath + "/" + _fileNameSansExtension + ".json");
            string json = sr.ReadToEnd();
            sr.Close();

            return json;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failure to load {_fileNameSansExtension}!\n" + e);
            return null;
        }
    }

    void DeleteFile(string _fileNameSansExtension)
    {
        string filePath = Application.persistentDataPath + "/" + _fileNameSansExtension + ".json";

        File.Delete(filePath);
    }

    #endregion Saving and Loading

    public static int GetUniqueID()
    {
        return uniqueIDGen++;
    }

    void SpawnControllers(bool _isDefault = false)
    {
        if (!_isDefault)
        {
            controllers.Sort((s1, s2) => s1.GetPosition().CompareTo(s2.GetPosition()));
        }

        foreach (ControllerSettings set in controllers)
        {
            SpawnController(set);
        }

    }

    public void NewController()
    {
        ControllerSettings newControl = new ControllerSettings(NEW_CONTROLLER_NAME, ControlType.Fader, AddressType.CC, ValueRange.SevenBit, DefaultValueType.Min, MIDIChannel.All, CurveType.Linear);
        GameObject newController = SpawnController(newControl);

        if (newController != null)
        {
            newControl.SetPosition(newController.transform.GetSiblingIndex());
        }
    }

    public GameObject SpawnController (ControllerSettings _config)
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
                    control = SpawnControllerObject(_config, t.controlObject);
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

        if(!controllers.Contains(_config))
        {
            controllers.Add(_config);
        }

        return control;
    }

    GameObject SpawnControllerObject(ControllerSettings _config, GameObject _controlObject)
    {
        GameObject control = Instantiate(_controlObject);
        control.transform.SetParent(controllerParent, false);
        control.GetComponentInChildren<FaderControl>().Initialize(_config);
        return control;
    }

    public void RespawnController(ControllerSettings _config)
    {
        DestroyController(_config);
        GameObject control = SpawnController(_config);
        control.transform.SetSiblingIndex(_config.GetPosition()); //there are bound to be issues here with ordering when faders are deleted and stuff
        ipSetter.TryConnect(); //quick and easy way - reconnect all sliders when done respawning a controller
    }

    public void DestroyController(ControllerSettings _config)
    {
        Destroy(controllerObjects[_config]);
        controllerObjects.Remove(_config);
        controllers.Remove(_config);
    }


    [Serializable]
    class FaderSaver
    {
        public List<ControllerSettings> configs;
        public string name = null;

        public FaderSaver(List<ControllerSettings> _controllers, string _name)
        {
            configs = _controllers;
            name = _name;
        }

        public List<ControllerSettings> GetControllers()
        {
            return configs;
        }

        public string GetName()
        {
            return name;
        }
    }

    [Serializable]

    class Profiles
    {
        [SerializeField] List<string> profileNames;
        [SerializeField] string defaultProfileName;

        public Profiles(List<string> _profileNames)
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

//used to pair prefabs with their control type
[Serializable]
struct ControllerType
{
    public ControlType controlType;
    public GameObject controlObject;
}