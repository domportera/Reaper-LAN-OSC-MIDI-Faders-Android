using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilesManager : MonoBehaviour
{
    [SerializeField] ControlsManager controlMan = null;

    [Header("Profiles Window")]
    [SerializeField] Button toggleProfileWindowButton;
    [SerializeField] GameObject profileWindow;
    [SerializeField] Button closeProfileWindowButton;

    [Header("Profiles")]
    [SerializeField] Button saveButton = null;
    [SerializeField] Button setDefaultButton = null;

    [Header("Delete Profiles")]
    [SerializeField] Button deleteButton = null;

    [Header("Save Profiles As")]
    [SerializeField] Button saveAsButton = null;

    [Header("Dynamic UI Elements")]
    [SerializeField] GameObject profileLoadButtonPrefab = null;
    [SerializeField] Transform profileButtonParent = null;

    Dictionary<string, ProfileLoadButton> profileButtons = new Dictionary<string, ProfileLoadButton>();

    [SerializeField] bool debug = false;


    private void Awake()
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
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DeleteConfirmation()
    {
        if (GetActiveProfile() != ControlsManager.DEFAULT_SAVE_NAME)
        {
            Utilities.instance.VerificationWindow($"Are you sure you want to delete this?", DeleteProfile, null, "Delete", "Cancel");
        }
        else
        {
            Utilities.instance.ErrorWindow($"Can't delete default profile");
		}
    }

    void SaveAsWindow()
    {
        Utilities.instance.VerificationWindow($"Enter a name for your new profile:", SaveAs, null, "Save", "Cancel");
    }

    public void PopulateProfileButtons(List<string> _profileNames, string _defaultProfile)
    {
        ClearAllProfileButtons();
        AddToProfileButtons(ControlsManager.DEFAULT_SAVE_NAME);

        foreach (string pname in _profileNames)
        {
            AddToProfileButtons(pname);
        }

        SetActiveProfile(_defaultProfile);
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
        controlMan.SetActiveProfile(_name);

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
    }

    void Save()
    {
        controlMan.SaveControllers(GetActiveProfile());
    }

    void SaveAs(string _saveName)
    {
        string profileName = _saveName;
        bool canSwitchProfiles = controlMan.SaveControllersAs(profileName);

        if(canSwitchProfiles)
        {
            SetActiveProfile(_saveName);
        }
    }

    void SetDefaultProfile()
    {
        string activeProfile = GetActiveProfile();
        controlMan.SetDefaultProfile(activeProfile);
        Utilities.instance.ConfirmationWindow(activeProfile + " set as default!\nThis will be the patch that loads on startup.");
    }

    void DeleteProfile()
    {
        string activeProfile = GetActiveProfile();
        profileButtons[activeProfile].Annihilate();
        controlMan.DeleteProfile(activeProfile);

        PrintDebug($"Removing profile button {activeProfile}");
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
        return ControlsManager.DEFAULT_SAVE_NAME;
	}
}
