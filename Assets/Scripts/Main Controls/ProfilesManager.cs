using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProfilesManager : MonoBehaviour
{
    [Header("Profiles Window")]
    [SerializeField] Button toggleProfileWindowButton;
    [SerializeField] GameObject profileWindow;
    [SerializeField] Button closeProfileWindowButton;

    [Header("Title Text")]
    [SerializeField] Text titleText;
    [SerializeField] bool boldTitleText = true;

    [Header("Profiles")]
    [SerializeField] Button saveButton;
    [SerializeField] Button setDefaultButton;

    [Header("Delete Profiles")]
    [SerializeField] Button deleteButton;

    [Header("Save Profiles As")]
    [SerializeField] Button saveAsButton;

    [Header("Dynamic UI Elements")]
    [SerializeField] GameObject profileLoadButtonPrefab;
    [SerializeField] Transform profileButtonParent;

    Dictionary<string, ProfileLoadButton> profileButtons = new Dictionary<string, ProfileLoadButton>();

    [SerializeField] bool debug = false;

    public static ProfilesManager instance;


    //saving variables
    public const string DEFAULT_SAVE_NAME = "Default";
    const string PROFILE_NAME_SAVE_NAME = "Profiles"; //name of json file that stores all profile names
    ProfilesMetadata profileNames = null;

    string basePath;
    const string CONTROLS_EXTENSION = ".controls";
    const string PROFILE_LIST_EXTENSION = ".profiles";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"There is a second ProfilesManager in the scene!", this);
            Debug.LogError($"This is the first one", instance);
        }

        basePath = Path.Combine(Application.persistentDataPath, "Controllers");

        InitializeUIElements();
        LoadProfileNames();
    }

    private void InitializeUIElements()
    {
        //profiles
        saveButton.onClick.AddListener(Save);

        //delete profiles
        deleteButton.onClick.AddListener(DeleteConfirmation);

        //save profiles as
        saveAsButton.onClick.AddListener(SaveAsWindow);

        //set default profile
        setDefaultButton.onClick.AddListener(SetDefaultProfile);

        //enable disable profile window
        toggleProfileWindowButton.onClick.AddListener(ToggleProfileWindow);
        closeProfileWindowButton.onClick.AddListener(ToggleProfileWindow);

        titleText.supportRichText = boldTitleText;
    }

    // Start is called before the first frame update
    void Start()
    {
        PopulateProfileSelectionMenu();
    }

    void DeleteConfirmation()
    {
        if (GetActiveProfile() != DEFAULT_SAVE_NAME)
        {
            UtilityWindows.instance.VerificationWindow($"Are you sure you want to delete this?", DeleteProfile, null, "Delete", "Cancel");
        }
        else
        {
            UtilityWindows.instance.ErrorWindow($"Can't delete default profile");
		}
    }

    void SaveAsWindow()
    {
        UtilityWindows.instance.VerificationWindow($"Enter a name for your new profile:", SaveAs, null, "Save", "Cancel");
    }

    public void PopulateProfileButtons(List<string> _profileNames, string _defaultProfile)
    {
        ClearAllProfileButtons();
        AddToProfileButtons(DEFAULT_SAVE_NAME);

        foreach (string pname in _profileNames)
        {
            AddToProfileButtons(pname);
        }

        SetActiveProfile(_defaultProfile);
    }

    void PopulateProfileSelectionMenu()
    {
        PopulateProfileButtons(profileNames.GetNames(), profileNames.GetDefaultProfileName());
    }

    public void AddToProfileButtons(string _name)
    {
        GameObject obj = Instantiate(profileLoadButtonPrefab, profileButtonParent);
        ProfileLoadButton buttonScript = obj.GetComponent<ProfileLoadButton>();
        buttonScript.SetText(_name);
        buttonScript.SetButtonAction(() => SetActiveProfile(_name));
        buttonScript.ToggleHighlight(false);
        profileButtons.Add(_name, buttonScript);

        PrintDebug($"Adding profile button {_name}");
    }

    void SetActiveProfile(string _name)
    {
        ControlsManager.instance.SetActiveProfile(_name);

        //make all profiles inactive
        foreach (KeyValuePair<string, ProfileLoadButton> pair in profileButtons)
		{
            pair.Value.isActiveProfile = false;
		}

        profileButtons[_name].isActiveProfile = true;

        //set highlight color
        foreach(KeyValuePair<string, ProfileLoadButton> pair in profileButtons)
        {
            pair.Value.ToggleHighlight(pair.Key == _name);
		}

        titleText.text = $"<b>{_name}</b>";
    }

    void Save()
    {
        _ = SaveProfile(GetActiveProfile());
    }

    void SaveAs(string _saveName)
    {
        string profileName = _saveName;
        bool canSwitchProfiles = SaveProfileAs(profileName);

        if(canSwitchProfiles)
        {
            SetActiveProfile(_saveName);
        }
    }

    void SetDefaultProfile()
    {
        string activeProfile = GetActiveProfile();
        profileNames.SetDefaultProfile(activeProfile);
        SaveProfileNames();
        UtilityWindows.instance.ConfirmationWindow(activeProfile + " set as default!\nThis will be the patch that loads on startup.");
    }

    void DeleteProfile()
    {
        string activeProfile = GetActiveProfile();
        profileButtons[activeProfile].Annihilate();
        DeleteProfileByName(activeProfile);

        PrintDebug($"Removing profile button {activeProfile}");
    }

    public void DeleteProfileByName(string _name)
    {
        if (_name == DEFAULT_SAVE_NAME)
        {
            UtilityWindows.instance.ErrorWindow("Can't delete default profile");
            return;
        }
        //remove profile from current list of profiles
        profileNames.RemoveProfile(_name);

        //delete file
        FileHandler.DeleteFile(basePath, _name, CONTROLS_EXTENSION);

        //Save profiles
        SaveProfileNames();

        //repopulate dropdown
        PopulateProfileSelectionMenu();

        //load controller profile
        LoadDefaultProfile();
    }

    void ToggleProfileWindow()
    {
        profileWindow.SetActive(!profileWindow.activeSelf);
	}

    void ClearAllProfileButtons()
    {
        foreach(KeyValuePair<string, ProfileLoadButton> pair in profileButtons)
        {
            pair.Value.Annihilate();
		}

        profileButtons.Clear();

        PrintDebug($"Clearing Profile Buttons");
	}

    void PrintDebug(string _text)
    {
        if(debug)
        {
            Debug.Log(_text, this);
		}
	}

    string GetActiveProfile()
    {
        foreach(KeyValuePair<string, ProfileLoadButton> pair in profileButtons)
        {
            if(pair.Value.isActiveProfile)
            {
                return pair.Key;
			}
		}

        Debug.LogError($"No active profile!");
        return DEFAULT_SAVE_NAME;
    }

    public bool SaveProfile(string _name, List<ControllerData> _controllers)
    {
        if (_name == DEFAULT_SAVE_NAME)
        {
            UtilityWindows.instance.ErrorWindow("Can't overwrite defaults, use Save As instead in the Profiles page.");
            return false;
        }

        ProfileSaveData saveData = new ProfileSaveData(_controllers, _name);
        bool success = FileHandler.SaveJson(saveData, basePath, saveData.GetName(), CONTROLS_EXTENSION);

        if (success)
        {
            UtilityWindows.instance.ConfirmationWindow($"Saved {_name}");
        }
        else
        {
            UtilityWindows.instance.ErrorWindow($"Error saving profile {_name}. Check the Log for more details.");
        }

        return success;
    }

    void LoadDefaultProfile()
    {
        if (profileNames.GetNames().Count < 1)
        {
            ControlsManager.instance.LoadControllers(DEFAULT_SAVE_NAME);
        }
        else
        {
            ControlsManager.instance.LoadControllers(profileNames.GetDefaultProfileName());
        }
    }

    public bool SaveProfileAs(string _name)
    {
        if (_name == ProfilesManager.DEFAULT_SAVE_NAME || profileNames.GetNames().Contains(_name))
        {
            UtilityWindows.instance.ErrorWindow("Profile with this name already exists, please use another.");
            return false;
        }

        List<char> invalidChars = GetInvalidFileNameCharacters(_name);
        if (invalidChars.Count > 0)
        {
            if (invalidChars.Count == 1)
            {
                UtilityWindows.instance.ErrorWindow($"Chosen profile name contains an invalid character.");
            }
            else
            {
                UtilityWindows.instance.ErrorWindow($"Chosen profile name contains {invalidChars.Count} invalid characters.");
            }
            return false;
        }

        if (_name.Length > 0)
        {
            string profileName = _name;
            profileNames.AddProfile(profileName);
            SaveProfileNames();
            ProfilesManager.instance.AddToProfileButtons(profileName);
            //add this profile to  working profiles in profile selection ui
            //switch to this profile
            bool saved = SaveProfile(_name);

            if(saved)
            {
                if(!ColorController.instance.CurrentColorProfileIsDefault())
                {
                    ColorController.instance.SaveColorProfileByName(_name);
                }
            }

            return saved;
        }
        else
        {
            UtilityWindows.instance.ErrorWindow("Please enter a name.");
            return false;
        }
    }

    public bool SaveProfile(string _name)
    {
        List<ControllerData> controllers = ControlsManager.instance.Controllers.OrderBy(control => control.GetName()).ToList();
        controllers.Sort((s1, s2) => s1.GetName().CompareTo(s2.GetName()));
        return SaveProfile(_name, controllers);
    }

    public ProfileSaveData LoadControlsFile(string _fileNameSansExtension)
    {
        return FileHandler.LoadJson<ProfileSaveData>(basePath, _fileNameSansExtension, CONTROLS_EXTENSION);
    }

    void LoadProfileNames()
    {
        ProfilesMetadata loaded = FileHandler.LoadJson<ProfilesMetadata>(basePath, PROFILE_NAME_SAVE_NAME, PROFILE_LIST_EXTENSION);

        if (loaded != null)
        {
            profileNames = loaded;
        }
        else
        {
            profileNames = new ProfilesMetadata(new List<string> { });
        }
    }

    void SaveProfileNames()
    {
        FileHandler.SaveJson<ProfilesMetadata>(profileNames, basePath, PROFILE_NAME_SAVE_NAME, PROFILE_LIST_EXTENSION);
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

    [Serializable]
    public class ProfileSaveData
    {
        [SerializeField] string name = string.Empty;

        //each type of control data has to be split into its own type-specific list for JsonUtility to agree with it
        [SerializeField] List<FaderData> faderData = new List<FaderData>();
        [SerializeField] List<Controller2DData> controller2DData = new List<Controller2DData>();

        public ProfileSaveData(List<ControllerData> _controllerData, string _name)
        {
            foreach (ControllerData data in _controllerData)
            {
                switch (data)
                {
                    case FaderData fader:
                        faderData.Add(fader);
                        break;
                    case Controller2DData control2D:
                        controller2DData.Add(control2D);
                        break;
                    default:
                        Debug.LogError($"Profile save data does not handle controller type {data.GetType()}");
                        break;
                }
            }

            name = _name;
        }

        public List<ControllerData> GetControllers()
        {
            List<ControllerData> controllers = new List<ControllerData>();

            foreach (FaderData f in faderData)
            {
                controllers.Add(f);
            }

            foreach (Controller2DData c2d in controller2DData)
            {
                controllers.Add(c2d);
            }

            return controllers;
        }

        public string GetName()
        {
            return name;
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

            if (_name == defaultProfileName)
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
}
