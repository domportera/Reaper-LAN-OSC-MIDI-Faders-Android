using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject optionsPanel = null;
    [SerializeField] GameObject sliderOptionsButtonLayoutPrefab = null;
    [SerializeField] GameObject faderOptionsPrefab = null;
    [SerializeField] GameObject faderOptionsActivationPrefab = null; //the prefab for the button that opens up fader options
    [SerializeField] GameObject sliderButtonVerticalLayoutParent = null;
    [SerializeField] Button optionsButton = null;
    [Space(10)]
    [SerializeField] Slider faderWidthSlider = null;
    [SerializeField] Button faderPositionEnableButton = null;
    [SerializeField] Button faderPositionExitButton = null;
    [SerializeField] HorizontalLayoutGroup faderLayoutGroup = null;

    [Header("Profiles")]
    [SerializeField] Dropdown profileSelectDropDown = null;
    [SerializeField] Button saveButton = null;
    [SerializeField] Button setDefaultButton = null;
    [SerializeField] GameObject setDefaultNotification = null;
    [SerializeField] Text defaultConfirmationText = null;

    [Header("Delete Profiles")]
    [SerializeField] Button deleteButton = null;
    [SerializeField] GameObject deleteConfirmationPanel = null;
    [SerializeField] Button confirmDeleteButton = null;
    [SerializeField] Button cancelDeleteButton = null;
    [SerializeField] Text deleteConfirmationText = null;

    [Header("Save Profiles As")]
    [SerializeField] InputField profileNameInput = null;
    [SerializeField] Button saveAsButton = null;
    [SerializeField] Button confirmSaveAsButton = null;
    [SerializeField] GameObject saveAsPanel = null;
    [SerializeField] Button cancelSaveAsButton = null;

    const int sliderButtonLayoutCapacity = 5;

    List<ControllerUIGroup> controllerUIs = new List<ControllerUIGroup>();
    List<LayoutGroupButtonCount> layoutCounts = new List<LayoutGroupButtonCount>();
    List<GameObject> sliderOptionsButtonLayoutGroups = new List<GameObject>();

    const int DEFAULT_FADER_WIDTH = 200;
    int faderWidth = DEFAULT_FADER_WIDTH;

    const string FADER_WIDTH_PLAYER_PREF = "Fader Width";
    bool positionMode = false;

    ControlsManager controlMan = null;
    Utilities utilities = null;

    // Start is called before the first frame update
    void Start()
    {
        controlMan = FindObjectOfType<ControlsManager>();
        utilities = FindObjectOfType<Utilities>();

        //options
        optionsButton.onClick.AddListener(ToggleOptionsMenu);
        faderPositionExitButton.onClick.AddListener(ToggleEditFaderPositionMode);

        //profiles
        profileSelectDropDown.onValueChanged.AddListener(SetActiveProfile);
        saveButton.onClick.AddListener(Save);

        //delete profiles
        deleteButton.onClick.AddListener(ToggleDeletePanel);
        confirmDeleteButton.onClick.AddListener(DeleteProfile);
        confirmDeleteButton.onClick.AddListener(ToggleDeletePanel);
        cancelDeleteButton.onClick.AddListener(ToggleDeletePanel);

        //save profiles as
        saveAsButton.onClick.AddListener(ToggleSaveAsPanel);
        confirmSaveAsButton.onClick.AddListener(SaveAs);
        cancelSaveAsButton.onClick.AddListener(ToggleSaveAsPanel);

        //set default profile
        setDefaultButton.onClick.AddListener(SetDefaultProfile);
        setDefaultButton.onClick.AddListener(ShowDefaultNotification);

        //prevent accidentally leaving stuff on in the scene
        optionsPanel.SetActive(false);
        saveAsPanel.SetActive(false);
        deleteConfirmationPanel.SetActive(false);
        setDefaultNotification.SetActive(false);

        //if not loaded, use default //when faderwidth is loaded, it will need to change the size of faders. faders should be playerprefs
        if (PlayerPrefs.HasKey(FADER_WIDTH_PLAYER_PREF))
        {
            faderWidth = PlayerPrefs.GetInt(FADER_WIDTH_PLAYER_PREF);
            SetFaderWidths();
            RefreshFaderLayoutGroup();
        }

        faderWidthSlider.SetValueWithoutNotify(faderWidth);
        faderWidthSlider.onValueChanged.AddListener(SetFaderWidthBySlider);
        faderPositionEnableButton.onClick.AddListener(ToggleEditFaderPositionMode);
    }

    //used by options button in scene
    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeInHierarchy);

        deleteConfirmationPanel.SetActive(false); //we don't want this up when the menu is opened again to prevent accidents
    }

    public void SpawnFaderOptions(ControllerSettings _config, GameObject _control)
    {
        //check if any other controller buttons exist for this, then destroy all its contents
        //make sure to destroy faderOptions as well
        ControllerUIGroup dupe = GetButtonGroupByConfig(_config);
        if (dupe != null)
        {
            DestroyControllerGroup(dupe);
        }

        ControllerUIGroup buttonGroup = new ControllerUIGroup(_config, faderOptionsPrefab, faderOptionsActivationPrefab, _control);

        buttonGroup.faderOptions.transform.SetParent(optionsPanel.transform, false);
        controllerUIs.Add(buttonGroup);
        SetFaderWidth(buttonGroup);
        SortOptionsButtons();
        RefreshFaderLayoutGroup();
    }

    string GetSaveNameFromField()
    {
        return profileNameInput.text.Replace(@"\", "").Replace(@"/", "").Trim(); //remove unwanted characters
    }

    public void PopulateProfileDropdown(List<string> _profileNames, string _defaultProfile)
    {
        profileSelectDropDown.ClearOptions();
        AddToPopulateProfileDropdown(ControlsManager.DEFAULT_SAVE_NAME);

        foreach (string pname in _profileNames)
        {
            AddToPopulateProfileDropdown(pname);
        }

        //for some reason, if there is only one entry (in the case where _profileNames is empty),
        //setting value of 0 to dropdown (the correct value) just doesn't properly set the dropdown view -
        //otherwise it's just blank.
        //setting it to literally anything else makes it work. I chose the number 1.
        //I dont make the rules, I just follow them.
        profileSelectDropDown.SetValueWithoutNotify(_profileNames.Count == 0 ? 1 : GetProfileIndex(_defaultProfile));

        SetActiveProfile(_defaultProfile);
    }

    public void AddToPopulateProfileDropdown(string _name)
    {
        profileSelectDropDown.options.Add(new OptionData(_name));
        profileSelectDropDown.SetValueWithoutNotify(profileSelectDropDown.options.Count - 1);
    }

    void SetActiveProfile(int _index)
    {
        if(controlMan == null)
        {
            controlMan = FindObjectOfType<ControlsManager>();
        }
        controlMan.SetActiveProfile(GetNameFromProfileDropdown());
    }

    void SetActiveProfile(string _name)
    {
        if (controlMan == null)
        {
            controlMan = FindObjectOfType<ControlsManager>();
        }

        controlMan.SetActiveProfile(_name);
    }

    void Save()
    {
        controlMan.SaveControllers(GetNameFromProfileDropdown()); ;
    }

    void SaveAs()
    {
        string profileName = GetSaveNameFromField();
        bool canClose = controlMan.SaveControllersAs(profileName);

        if(canClose)
        {
            ToggleSaveAsPanel();
        }
    }

    void SetDefaultProfile()
    {
        string s = GetNameFromProfileDropdown();
        controlMan.SetDefaultProfile(s);

        Debug.Log("Default save: " + s);
    }

    void DeleteProfile()
    {
        controlMan.DeleteProfile(GetNameFromProfileDropdown());
    }

    string GetNameFromProfileDropdown()
    {
        return profileSelectDropDown.options[profileSelectDropDown.value].text;
    }

    int GetProfileIndex(string _profile)
    {
        for(int i = 0; i < profileSelectDropDown.options.Count; i++)
        {
            if(profileSelectDropDown.options[i].text == _profile)
            {
                return i;
            }
        }

        Debug.LogError("Index not found in profile dropdown");
        return -1;
    }

    void ToggleSaveAsPanel()
    {
        saveAsPanel.SetActive(!saveAsPanel.activeInHierarchy);
    }

    void ToggleDeletePanel()
    {
        deleteConfirmationPanel.SetActive(!deleteConfirmationPanel.activeInHierarchy);

        if (deleteConfirmationPanel.activeInHierarchy)
        {
            deleteConfirmationText.text = "Are you sure you want to delete\n" + GetNameFromProfileDropdown() + "?";
        }
    }

    Coroutine showDefaultNotificationRoutine = null;
    void ShowDefaultNotification()
    {
        if(showDefaultNotificationRoutine != null)
        {
            StopCoroutine(showDefaultNotificationRoutine);
        }

        defaultConfirmationText.text = GetNameFromProfileDropdown() + " set as default!\nThis will be the patch that loads on startup.";
        showDefaultNotificationRoutine = StartCoroutine(ShowDefaultNotificationThenHide());
    }
    IEnumerator ShowDefaultNotificationThenHide()
    {
        setDefaultNotification.SetActive(true);
        yield return new WaitForSeconds(3f);
        setDefaultNotification.SetActive(false);
    }

    void AddOptionsButtonToLayout(GameObject _button)
    {
        for(int i = 0; i < layoutCounts.Count; i++)
        {
            if(layoutCounts[i].count < sliderButtonLayoutCapacity)
            {
                _button.transform.SetParent(layoutCounts[i].layoutGroup.transform);
                _button.transform.SetSiblingIndex(layoutCounts[i].count);
                layoutCounts[i].count++;
                return;
            }
        }

        //all layouts full, create a new one

        AddNewLayoutCount();
        AddOptionsButtonToLayout(_button);
    }

    public void DestroyControllerGroup(ControllerSettings _config)
    {
        DestroyControllerGroup(GetButtonGroupByConfig(_config));
    }

    void DestroyControllerGroup(ControllerUIGroup _buttonGroup)
    {
        //destroy all params
        _buttonGroup.SelfDestruct();

        //remove from list
        controllerUIs.Remove(_buttonGroup);

        //check for empty layouts, and destroy it
        DestroyEmptyLayouts();

        SortOptionsButtons();
    }

    void DestroyEmptyLayouts()
    {
        for(int i = 0; i < layoutCounts.Count; i++)
        {
            if(layoutCounts[i].count == 0)
            {
                //destroy
                Destroy(layoutCounts[i].layoutGroup);
                layoutCounts.RemoveAt(i);
                i--;
            }
        }
    }

    public void DestroyControllerObjects(ControllerSettings _config)
    {
        ControllerUIGroup buttonGroup = GetButtonGroupByConfig(_config);
        DestroyControllerGroup(buttonGroup);
    }

    void RemoveFromLayout(ControllerUIGroup _buttonGroup)
    {
        LayoutGroupButtonCount layout = GetLayoutGroupFromObject(_buttonGroup.faderMenuButton.gameObject);

        if (layout != null)
        {
            layout.count--;
        }
        else
        {
            Debug.LogError("Null layout! button didn't find its parent.");
        }
    }

    void AddNewLayoutCount()
    {
        LayoutGroupButtonCount lay = new LayoutGroupButtonCount();
        lay.layoutGroup = Instantiate(sliderOptionsButtonLayoutPrefab);
        lay.layoutGroup.transform.SetParent(sliderButtonVerticalLayoutParent.transform);
        layoutCounts.Add(lay);
    }

    bool GetControllerEnabled(FaderOptions _faderOptions)
    {
        ControllerUIGroup group = GetButtonGroupByFaderOptions(_faderOptions);

        if(group.activationToggle.isOn)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    ControllerUIGroup GetButtonGroupByConfig(ControllerSettings _config)
    {
        foreach (ControllerUIGroup cbg in controllerUIs)
        {
            if (cbg.controllerConfig == _config)
            {
                return cbg;
            }
        }
        return null;
    }

    ControllerUIGroup GetButtonGroupByFaderOptions(FaderOptions _faderOptions)
    {
        foreach (ControllerUIGroup cbg in controllerUIs)
        {
            if(cbg.faderOptions == _faderOptions)
            {
                return cbg;
            }
        }

        Debug.LogError("Didn't find a match for button group! Returning empty.");
        return null;
    }

    LayoutGroupButtonCount GetLayoutGroupFromObject(GameObject _button)
    {
        foreach(LayoutGroupButtonCount lay in layoutCounts)
        {
            Transform[] children = lay.layoutGroup.GetComponentsInChildren<Transform>();

            foreach(Transform child in children)
            {
                if(child.gameObject == _button)
                {
                    return lay;
                }
            }
        }

        return null;
    }

    void SortOptionsButtons()
    {
        foreach(LayoutGroupButtonCount layCount in layoutCounts)
        {
            layCount.count = 0;
        }

        //get all the unnamed ones out of sorting list
        List<ControllerUIGroup> unnamedControls = new List<ControllerUIGroup>();
        for (int i = 0; i < controllerUIs.Count; i++)
        {
            if(controllerUIs[i].controllerConfig.name == ControlsManager.NEW_CONTROLLER_NAME)
            {
                unnamedControls.Add(controllerUIs[i]);
                controllerUIs.Remove(controllerUIs[i]);
                i--;
            }
        }

        //sort buttons
        controllerUIs.Sort((s1, s2) => s1.controllerConfig.name.CompareTo(s2.controllerConfig.name));

        //add unnamed ones to the end
        foreach (ControllerUIGroup c in unnamedControls)
        {
            controllerUIs.Add(c);
        }

        //place buttons
        foreach (ControllerUIGroup c in controllerUIs)
        {
            AddOptionsButtonToLayout(c.faderMenuButton.gameObject);
        }
    }

    void SetFaderWidths()
    {
        //set to fader slider value - this should be set with loaded value, then this can be called.
        foreach(ControllerUIGroup c in controllerUIs)
        {
            SetFaderWidth(c);
        }

        RefreshFaderLayoutGroup();
    }

    void RefreshFaderLayoutGroup()
    {
        faderLayoutGroup.enabled = false;
        faderLayoutGroup.enabled = true;
    }

    void SetFaderWidth(ControllerUIGroup _group)
    {
        _group.SetFaderWidth(faderWidth);
    }

    public void SetFaderWidthBySlider(float _width)
    {
        faderWidth = (int)_width;
        SetFaderWidths();
    }

    public void SaveFaderWidthToPlayerPrefs()
    {
        PlayerPrefs.SetInt(FADER_WIDTH_PLAYER_PREF, faderWidth);
    }

    void ToggleEditFaderPositionMode()
    {
        positionMode = !positionMode;

        foreach(ControllerUIGroup u in controllerUIs)
        {
            u.SetPostionMode(positionMode);
            u.SetPosition();
        }

        //toggle options buttons
        optionsButton.gameObject.SetActive(!positionMode);
        optionsPanel.SetActive(!positionMode);
        faderPositionExitButton.gameObject.SetActive(positionMode);

        if(!positionMode)
        {
            utilities.SetErrorText("Don't forget to save!");
        }
    }

    class LayoutGroupButtonCount
    {
        public GameObject layoutGroup;
        public int count = 0;
    }

    class ControllerUIGroup
    {
        //should be public get private set
        public FaderOptions faderOptions;
        public Button faderMenuButton;
        public ControllerSettings controllerConfig;
        public Toggle activationToggle;
        public GameObject controlObject;
        FaderControl control;
        RectTransform controlObjectTransform;

        public ControllerUIGroup(ControllerSettings _config, GameObject _faderOptionsPrefab, GameObject _optionsActivateButtonPrefab, GameObject _controlObject)
        {
            faderOptions = Instantiate(_faderOptionsPrefab).GetComponent<FaderOptions>();
            faderOptions.gameObject.name = _config.name + " Options Panel";
            faderOptions.controllerConfig = _config;

            controllerConfig = _config;

            faderMenuButton = Instantiate(_optionsActivateButtonPrefab).GetComponent<Button>();
            faderMenuButton.gameObject.name = _config.name + " Options";
            faderMenuButton.GetComponentInChildren<Text>().text = _config.name + " Options"; // change visible button title
            faderMenuButton.onClick.AddListener(DisplayFaderOptions);

            activationToggle = faderMenuButton.GetComponentInChildren<Toggle>();
            activationToggle.onValueChanged.AddListener(ToggleControlVisibility);
            controlObject = _controlObject;
            controlObjectTransform = _controlObject.GetComponent<RectTransform>();

            control = _controlObject.GetComponent<FaderControl>();
        }

        public void SetFaderWidth(float _width)
        {
            controlObjectTransform.sizeDelta = new Vector2(_width, controlObjectTransform.sizeDelta.y);
        }

        void DisplayFaderOptions()
        {
            faderOptions.gameObject.SetActive(true);
        }

        public void ToggleControlVisibility(bool _b)
        {
            controlObject.SetActive(_b);

            if(!_b)
            {
                controlObjectTransform.SetAsLastSibling();
            }
        }

        public void SetPosition()
        {
            controllerConfig.SetPosition(controlObjectTransform.GetSiblingIndex());
        }

        public void SetPostionMode(bool _b)
        {
            control.SetSortButtonVisibility(_b);
        }

        public void SelfDestruct()
        {
            Destroy(faderOptions.gameObject);
            Destroy(faderMenuButton.gameObject);
            Destroy(controlObject);
        }
    }
}
