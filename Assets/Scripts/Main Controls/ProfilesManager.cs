using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colors;
using UnityEngine;
using UnityEngine.UI;
using DomsUnityHelper;
using UnityEngine.Serialization;
using PopUpWindows;

public class ProfilesManager : MonoBehaviourExtended
{
    [Header("Profiles Window")]
    [SerializeField]
    Button _toggleProfileWindowButton, _closeProfileWindowButton;
    [SerializeField]
    GameObject _profileWindow;

    [Header("Title Text")]
    [SerializeField]
    Text _titleText;
    [SerializeField]
    bool _boldTitleText = true;

    [Header("Profiles")]
    [SerializeField]
    Button _saveButton, _setDefaultButton;

    [Header("Save Profiles As")]
    [SerializeField]
    Button _saveAsButton;
    
    [Header("Dynamic UI Elements")]
    [SerializeField]
    GameObject _profileLoadButtonPrefab;
    [SerializeField] Transform _profileButtonParent;

    Dictionary <string, ProfileLoader> _profileButtons = new();
    //saving variables
    public const string DefaultSaveName = "Default";
    const string ProfileNameSaveName = "Profiles"; //name of json file that stores all profile names
    ProfilesMetadata _profileNames = null;

    string _basePath;
    const string ControlsExtension = ".controls";
    const string ProfileListExtension = ".profiles";

    void Awake()
    {
        _basePath = Path.Combine(Application.persistentDataPath, "Controllers");

        InitializeUIElements();
        LoadProfileNames();
    }

    void InitializeUIElements()
    {
        //profiles
        _saveButton.onClick.AddListener(Save);

        //save profiles as
        _saveAsButton.onClick.AddListener(SaveAsWindow);

        //set default profile
        _setDefaultButton.onClick.AddListener(SetDefaultProfile);

        //enable disable profile window
        _toggleProfileWindowButton.onClick.AddListener(ToggleProfileWindow);
        _closeProfileWindowButton.onClick.AddListener(ToggleProfileWindow);

        _titleText.supportRichText = _boldTitleText;
    }

    // Start is called before the first frame update
    void Start()
    {
        PopulateProfileSelectionMenu();
    }

    void DeleteConfirmation(ProfileLoader profile)
    {
        if (profile.GetName() != DefaultSaveName)
        {

            string activeProfile = GetActiveProfile();
            string confirmationWindowText;

            if(activeProfile == profile.GetName())
            {
                confirmationWindowText = $"Are you sure you want to delete your current profile? This will automatically load the default profile after deletion.";
            }
            else
            {
                confirmationWindowText = $"Are you sure you want to delete profile {profile.GetName()}?";
            }
            PopUpController.Instance.ConfirmationWindow(confirmationWindowText, () => DeleteProfile(profile), null, "Delete", "Cancel");
        }
        else
        {
            PopUpController.Instance.ErrorWindow($"Can't delete default profile");
		}
    }

    void SaveAsWindow()
    {
        PopUpController.Instance.TextInputWindow($"Enter a name for your new profile:", SaveAs, null, "Save", "Cancel");
    }

    void PopulateProfileButtons(List<string> profileNames, string setActiveProfile)
    {
        AddToProfileButtons(DefaultSaveName);

        foreach (string pname in profileNames)
        {
            AddToProfileButtons(pname);
        }
        SortProfileButtons();
        SetActiveProfile(_profileButtons[setActiveProfile]);
    }

    void PopulateProfileSelectionMenu()
    {
        PopulateProfileButtons(_profileNames.GetNames(), _profileNames.GetDefaultProfileName());
    }

    void AddToProfileButtons(string profileName)
    {
        GameObject obj = Instantiate(_profileLoadButtonPrefab, _profileButtonParent);
        ProfileLoader buttonScript = obj.GetComponent<ProfileLoader>();
        buttonScript.SetText(profileName);
        buttonScript.SetButtonActions(() => SetActiveProfile(_profileButtons[profileName]), () => DeleteConfirmation(buttonScript));
        buttonScript.ToggleHighlight(false);
        _profileButtons.Add(profileName, buttonScript);

        Log($"Adding profile button {profileName}", this);
    }

    void SortProfileButtons()
    {
        RefreshParenting(_profileButtons[DefaultSaveName].transform, _profileButtonParent);

        _profileButtons = _profileButtons.OrderBy(e => e.Key).ToDictionary(x => x.Key, x => x.Value);
        
        foreach(KeyValuePair<string, ProfileLoader> p in _profileButtons)
        {
            if(p.Key != DefaultSaveName)
            {
                RefreshParenting(p.Value.transform, _profileButtonParent);
            }
        }
    }

    void RefreshParenting(Transform obj, Transform newParent)
    {
        obj.SetParent(null);
        obj.SetParent(newParent);
    }

    void SetActiveProfile(ProfileLoader loader)
    {
        ControlsManager.Instance.SetActiveProfile(loader);

        //set highlight color and active status
        foreach(KeyValuePair<string, ProfileLoader> p in _profileButtons)
        {
            p.Value.IsActiveProfile = p.Value == loader;
            p.Value.ToggleHighlight(p.Value == loader);
		}

        _titleText.text = $"<b>{loader.GetName()}</b>";
    }

    void Save()
    {
        _ = SaveProfile(GetActiveProfile());
    }

    void SaveAs(string saveName)
    {
        string profileName = saveName;
        bool canSwitchProfiles = SaveProfileAs(profileName);

        if(canSwitchProfiles)
        {
            SetActiveProfile(_profileButtons[saveName]);
        }
    }

    void SetDefaultProfile()
    {
        string activeProfile = GetActiveProfile();
        _profileNames.SetDefaultProfile(activeProfile);
        SaveProfileNames();
        PopUpController.Instance.QuickNoticeWindow(activeProfile + " set as default!\nThis will be the patch that loads on startup.");
    }

    void DeleteProfile(ProfileLoader profile)
    {
        _profileButtons[profile.GetName()].Annihilate();
        DeleteProfileWithButton(profile);

        Log($"Removing profile button {profile}", this);
    }

    void DeleteProfileWithButton(ProfileLoader button)
    {
        if (button.GetName() == DefaultSaveName)
        {
            PopUpController.Instance.ErrorWindow("Can't delete default profile");
            return;
        }

        //remove profile from current list of profiles
        _profileNames.RemoveProfile(button.GetName());

        //delete file
        FileHandler.DeleteFile(_basePath, button.GetName(), ControlsExtension);

        //Save profiles
        SaveProfileNames();

        //destroy deleted button
        _profileButtons.Remove(button.GetName());
        button.Annihilate();

        //load profile if deleting the currently active one
        if(button.IsActiveProfile)
        {
            SetActiveProfile(_profileButtons[DefaultSaveName]);
            LoadDefaultProfile();
        }
    }

    void ToggleProfileWindow()
    {
        _profileWindow.SetActive(!_profileWindow.activeSelf);
	}
    
    string GetActiveProfile()
    {
        foreach(KeyValuePair<string, ProfileLoader> pair in _profileButtons)
        {
            if(pair.Value.IsActiveProfile)
            {
                return pair.Key;
			}
		}

        Debug.LogError($"No active profile!");
        return DefaultSaveName;
    }

    bool SaveProfile(string profileName, List<ControllerData> controllers)
    {
        if (profileName == DefaultSaveName)
        {
            PopUpController.Instance.ErrorWindow("Can't overwrite defaults, use Save As instead in the Profiles page.");
            return false;
        }

        ProfileSaveData saveData = new ProfileSaveData(controllers, profileName);
        bool success = FileHandler.SaveJsonObject(saveData, _basePath, saveData.GetName(), ControlsExtension);

        if (success)
        {
            PopUpController.Instance.QuickNoticeWindow($"Saved {profileName}");
        }
        else
        {
            PopUpController.Instance.ErrorWindow($"Error saving profile {profileName}. Check the Log for more details.");
        }

        return success;
    }

    void LoadDefaultProfile()
    {
        ControlsManager.Instance.LoadControllers(_profileNames.GetNames().Count == 0
            ? DefaultSaveName
            : _profileNames.GetDefaultProfileName());
    }

    bool SaveProfileAs(string profileName)
    {
        if (profileName == DefaultSaveName || _profileNames.GetNames().Contains(profileName))
        {
            PopUpController.Instance.ErrorWindow("Profile with this name already exists, please use another.");
            return false;
        }

        List<char> invalidChars = FileHandler.GetInvalidFileNameCharacters(profileName);
        if (invalidChars.Count > 0)
        {
            PopUpController.Instance.ErrorWindow(invalidChars.Count == 1
                ? $"Chosen profile name contains an invalid character."
                : $"Chosen profile name contains {invalidChars.Count} invalid characters.");
            return false;
        }

        if (profileName.Length > 0)
        {
            _profileNames.AddProfile(profileName);
            SaveProfileNames();
            AddToProfileButtons(profileName);
            SortProfileButtons();
            
            //add this profile to  working profiles in profile selection ui
            //switch to this profile
            bool saved = SaveProfile(profileName);

            if (!saved) return false;

            ColorController.SaveCurrentColorsWithProfileName(profileName);

            return true;
        }

        PopUpController.Instance.ErrorWindow("Please enter a name.");
        return false;
    }

    bool SaveProfile(string profileName)
    {
        List<ControllerData> controllers = ControlsManager.Instance.Controllers.OrderBy(control => control.GetName()).ToList();
        controllers.Sort((s1, s2) => String.Compare(s1.GetName(), s2.GetName(), StringComparison.Ordinal));
        return SaveProfile(profileName, controllers);
    }

    public ProfileSaveData LoadControlsFile(string fileNameSansExtension)
    {
        return FileHandler.LoadJsonObject<ProfileSaveData>(_basePath, fileNameSansExtension, ControlsExtension);
    }

    void LoadProfileNames()
    {
        ProfilesMetadata loaded = FileHandler.LoadJsonObject<ProfilesMetadata>(_basePath, ProfileNameSaveName, ProfileListExtension);
        _profileNames = loaded ?? new ProfilesMetadata(new List<string>());
    }

    void SaveProfileNames()
    {
        FileHandler.SaveJsonObject(_profileNames, _basePath, ProfileNameSaveName, ProfileListExtension);
    }

    [Serializable]
    public class ProfileSaveData
    {
        [FormerlySerializedAs("_profileName")] [FormerlySerializedAs("name")] [SerializeField]
        string _name = string.Empty;

        //each type of control data has to be split into its own type-specific list for JsonUtility to agree with it
        [FormerlySerializedAs("faderData")] [SerializeField]
        List<FaderData> _faderData = new();
        [FormerlySerializedAs("controller2DData")] [SerializeField]
        List<Controller2DData> _controller2DData = new();

        public ProfileSaveData(List<ControllerData> controllerData, string name)
        {
            foreach (ControllerData data in controllerData)
            {
                switch (data)
                {
                    case FaderData fader:
                        _faderData.Add(fader);
                        break;
                    case Controller2DData control2D:
                        _controller2DData.Add(control2D);
                        break;
                    default:
                        Debug.LogError($"Profile save data does not handle controller type {data.GetType()}");
                        break;
                }
            }

            _name = name;
        }

        public List<ControllerData> GetControllers()
        {
            List<ControllerData> controllers = new();
            controllers.AddRange(_faderData);
            controllers.AddRange(_controller2DData);
            return controllers;
        }

        public string GetName()
        {
            return _name;
        }
    }

    [Serializable]
    class ProfilesMetadata
    {
        [SerializeField]
        List<string> _profileNames;
        [SerializeField]
        string _defaultProfileName;

        public ProfilesMetadata(List<string> profileNames)
        {
            _profileNames = profileNames;
            _defaultProfileName = DefaultSaveName;
        }

        public void AddProfile(string name)
        {
            _profileNames.Add(name);
        }

        public void RemoveProfile(string name)
        {
            _profileNames.Remove(name);

            if (name == _defaultProfileName)
            {
                _defaultProfileName = DefaultSaveName;
            }
        }

        public List<string> GetNames()
        {
            return _profileNames;
        }

        public string GetDefaultProfileName()
        {
            return _defaultProfileName;
        }

        public void SetDefaultProfile(string name)
        {
            _defaultProfileName = name;
        }
    }
}
