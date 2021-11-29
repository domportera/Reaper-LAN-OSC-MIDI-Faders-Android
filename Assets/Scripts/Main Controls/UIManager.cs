using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class UIManager : MonoBehaviourExtended
{
    [SerializeField] GameObject optionsPanel = null;
    [SerializeField] GameObject sliderOptionsButtonLayoutPrefab = null;
    [SerializeField] GameObject faderOptionsPrefab = null;
    [SerializeField] GameObject controller2DOptionsPrefab = null;
    [SerializeField] GameObject faderOptionsActivationPrefab = null; //the prefab for the button that opens up fader options
    [SerializeField] GameObject sliderButtonVerticalLayoutParent = null;
    [SerializeField] Button optionsButton = null;
    [SerializeField] Button closeOptionsButton = null;
    [SerializeField] Button newControllerButton = null;

    [Space(10)]
    [SerializeField] Button faderPositionEnableButton = null;
    [SerializeField] Button faderPositionExitButton = null;
    [SerializeField] HorizontalLayoutGroup faderLayoutGroup = null;
    [SerializeField] Button optionsButtonSortingButton = null;

    [SerializeField] Button setupButton;
    [SerializeField] Button closeSetupButton;
    [SerializeField] GameObject setupPanel;

    [Header("Credits")]
    [SerializeField] Button creditsButton;
    [SerializeField] Button closeCreditsButton;
    [SerializeField] GameObject creditsPanel;
    [SerializeField] List<CreditsButton> creditsButtons = new List<CreditsButton>();
    [SerializeField] List<DonationButton> donationButtons = new List<DonationButton>();

    [Serializable]
    struct CreditsButton
    {
        public Button button;
        public string link;
    }

    [Serializable]
    struct DonationButton
    {
        public Button button;
        public string address;
    }

    const int sliderButtonLayoutCapacity = 5;

    List<ControllerUIGroup> controllerUIs = new List<ControllerUIGroup>();
    List<LayoutGroupButtonCount> layoutCounts = new List<LayoutGroupButtonCount>();

    bool positionMode = false;

    public static UIManager instance;

    bool sortOptionsByName = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"There is a second UIManager in the scene!", this);
            Debug.LogError($"This is the first one", instance);
        }

        //options
        optionsButton.onClick.AddListener(ToggleOptionsMenu);
        closeOptionsButton.onClick.AddListener(ToggleOptionsMenu);
        faderPositionExitButton.onClick.AddListener(ToggleEditFaderPositionMode);
        //prevent accidentally leaving stuff on in the scene
        optionsPanel.SetActive(false);


        //if not loaded, use default //when faderwidth is loaded, it will need to change the size of faders. faders should be playerprefs
        faderPositionEnableButton.onClick.AddListener(ToggleEditFaderPositionMode);
        optionsButtonSortingButton.onClick.AddListener(() => SwitchOptionsButtonSorting(!sortOptionsByName));
        SwitchOptionsButtonSorting(false);
        newControllerButton.onClick.AddListener(ControlsManager.instance.NewController);

        UnityAction toggleSetup = () => setupPanel.SetActive(!setupPanel.activeSelf);
        setupButton.onClick.AddListener(toggleSetup);
        closeSetupButton.onClick.AddListener(toggleSetup);

        UnityAction toggleCredits = () => creditsPanel.SetActive(!creditsPanel.activeSelf);
        creditsButton.onClick.AddListener(toggleCredits);
        closeCreditsButton.onClick.AddListener(toggleCredits);

        InitializeCreditsButtons();
        InitializeDonationButtons();
    }

    private void SwitchOptionsButtonSorting(bool _byName)
    {
        sortOptionsByName = _byName;
        optionsButtonSortingButton.GetComponentInChildren<Text>().text = $"Sort Options: {(sortOptionsByName ? "Name" : "Layout")}";
        SortOptionsButtons();
    }

    //used by options button in scene
    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeInHierarchy);
    }

    #region Controller Options
    public void SpawnControllerOptions(ControllerData _config, GameObject _control)
    {
        //check if any other controller buttons exist for this, then destroy all its contents
        //make sure to destroy faderOptions as well
        ControllerUIGroup dupe = GetButtonGroupByConfig(_config);
        if (dupe != null)
        {
            DestroyControllerGroup(dupe);
        }

        InitializeControllerOptions(_config, _control);
        SortOptionsButtons();
    }

    void InitializeControllerOptions(ControllerData _config, GameObject _control)
    {
        ControllerUIGroup buttonGroup;

        switch (_config)
        {
            case FaderData fader:
                GameObject faderOptions = InitializeFaderOptions(fader, _control);
                buttonGroup = new ControllerUIGroup(fader, faderOptionsActivationPrefab, _control, faderOptions);
                break;
            case Controller2DData control2D:
                GameObject controlOptions = InitializeController2DOptions(control2D, _control);
                buttonGroup = new ControllerUIGroup(control2D, faderOptionsActivationPrefab, _control, controlOptions);
                break;
            default:
                buttonGroup = null;
                Debug.LogError($"Unhandled ControllerData type {_config.GetType()}");
                break;
        }

        //buttonGroup.optionsMenu.transform.SetParent(optionsPanel.transform, false);
        if (buttonGroup != null)
        {
            controllerUIs.Add(buttonGroup);
        }
    }

    GameObject InitializeFaderOptions(FaderData _config, GameObject _controlObj)
    {
        GameObject menuObj = Instantiate(faderOptionsPrefab, optionsPanel.transform, false);
        FaderOptions faderOptions = menuObj.GetComponent<FaderOptions>();
        faderOptions.Initialize(_config, _controlObj.GetComponent<RectTransform>());
        faderOptions.gameObject.name = _config.GetName() + " Options Panel";
        return menuObj;
    }

    GameObject InitializeController2DOptions(Controller2DData _config, GameObject _controlObj)
    {
        GameObject menuObj = Instantiate(controller2DOptionsPrefab, optionsPanel.transform, false);
        Controller2DOptions controllerOptions = menuObj.GetComponent<Controller2DOptions>();
        controllerOptions.Initialize(_config, _controlObj.GetComponent<RectTransform>());
        controllerOptions.gameObject.name = _config.GetName() + " Options Panel";
        return menuObj;
    }



    void AddOptionsButtonToLayout(GameObject _button)
    {
        for (int i = 0; i < layoutCounts.Count; i++)
        {
            if (layoutCounts[i].count < sliderButtonLayoutCapacity)
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

    void DestroyEmptyLayouts()
    {
        for (int i = 0; i < layoutCounts.Count; i++)
        {
            if (layoutCounts[i].count == 0)
            {
                //destroy
                Destroy(layoutCounts[i].layoutGroup);
                layoutCounts.RemoveAt(i);
                i--;
            }
        }
    }

    public void DestroyControllerGroup(ControllerData _config)
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

    public void DestroyControllerObjects(ControllerData _config)
    {
        ControllerUIGroup buttonGroup = GetButtonGroupByConfig(_config);
        DestroyControllerGroup(buttonGroup);
    }

    void RemoveFromLayout(ControllerUIGroup _buttonGroup)
    {
        LayoutGroupButtonCount layout = GetLayoutGroupFromObject(_buttonGroup.activateControllerOptionsButton.gameObject);

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

    public void ShowControllerOptions(ControllerData _data)
    {
        ControllerUIGroup uiGroup = GetButtonGroupByConfig(_data);
        uiGroup.SetControllerOptionsActive(true);
    }

    ControllerUIGroup GetButtonGroupByConfig(ControllerData _data)
    {
        foreach (ControllerUIGroup cbg in controllerUIs)
        {
            if (cbg.controllerData == _data)
            {
                return cbg;
            }
        }
        return null;
    }
    #endregion Controller Options
    void InitializeCreditsButtons()
    {
        foreach(CreditsButton b in creditsButtons)
        {
            b.button.onClick.AddListener(() => Application.OpenURL(b.link));
        }
    }

    void InitializeDonationButtons()
    {
        foreach(DonationButton b in donationButtons)
        {
            b.button.onClick.AddListener(() =>
            {
                UniClipboard.SetText(b.address);
                UtilityWindows.instance.QuickNoticeWindow($"Copied {b.address} to clipboard!");
            });
        }
    }

    /// <summary>
    /// This is used to let the controllers use their own width - for some reason this is necessary for the width to update
    /// </summary>
    public void RefreshFaderLayoutGroup()
    {
        faderLayoutGroup.enabled = false;
        faderLayoutGroup.enabled = true;
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
            if (controllerUIs[i].controllerData.GetName() == ControlsManager.GetDefaultControllerName(controllerUIs[i].controllerData))
            {
                unnamedControls.Add(controllerUIs[i]);
                controllerUIs.Remove(controllerUIs[i]);
                i--;
            }
        }

        //sort buttons
        if (sortOptionsByName)
        {
            controllerUIs.Sort((s1, s2) => s1.controllerData.GetName().CompareTo(s2.controllerData.GetName()));
        }
        else
        {
            controllerUIs.Sort((s1, s2) => s1.controllerData.GetPosition().CompareTo(s2.controllerData.GetPosition()));
        }

        //add unnamed ones to the end
        foreach (ControllerUIGroup c in unnamedControls)
        {
            controllerUIs.Add(c);
        }

        //place buttons
        foreach (ControllerUIGroup c in controllerUIs)
        {
            AddOptionsButtonToLayout(c.activateControllerOptionsButton.gameObject);
        }
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
    }

    class LayoutGroupButtonCount
    {
        public GameObject layoutGroup;
        public int count = 0;
    }

    class ControllerUIGroup
    {
        //should be public get private set
        public ButtonExtended activateControllerOptionsButton { get; private set; }
        Toggle activationToggle;
        GameObject controlObject;
        RectTransform controlObjectTransform;
        public ControllerData controllerData { get; private set; }
        GameObject optionsMenu;

        public ControllerUIGroup(ControllerData _config, GameObject _optionsActivateButtonPrefab, GameObject _controlObject, GameObject _optionsMenu)
        {
            controllerData = _config;
            optionsMenu = _optionsMenu;
            controlObject = _controlObject;

            activateControllerOptionsButton = Instantiate(_optionsActivateButtonPrefab).GetComponent<ButtonExtended>();
            activateControllerOptionsButton.gameObject.name = _config.GetName() + " Options";
            activateControllerOptionsButton.GetComponentInChildren<Text>().text = _config.GetName() + " Options"; // change visible button title
            activateControllerOptionsButton.onClick.AddListener(() => { SetControllerOptionsActive(true); });
            activateControllerOptionsButton.OnPointerHeld.AddListener(Delete);
            SetControllerOptionsActive(false);

            activationToggle = activateControllerOptionsButton.GetComponentInChildren<Toggle>();
            activationToggle.onValueChanged.AddListener(ToggleControlVisibility);
            activationToggle.SetIsOnWithoutNotify(_config.GetEnabled());
            controlObjectTransform = _controlObject.GetComponentSafer<RectTransform>();
        }

        void Delete()
        {
            UnityAction deleteAction = () =>
            {
                ControlsManager.instance.DestroyController(controllerData);
                UIManager.instance.DestroyControllerObjects(controllerData);
            };

            UtilityWindows.instance.ConfirmationWindow($"Delete controller\n\"{controllerData.GetName()}\"?", deleteAction, null, "Delete", "Cancel");
        }

        public void SetControllerOptionsActive(bool _active)
        {
            optionsMenu.SetActive(_active);
        }

        public void ToggleControlVisibility(bool _b)
        {
            controlObject.SetActive(_b);
            controllerData.SetEnabled(_b);

            if(!_b)
            {
                controlObjectTransform.SetAsLastSibling();
            }
        }

        public void SetPosition()
        {
            controllerData.SetPosition(controlObjectTransform.GetSiblingIndex());
        }

        public void SetPostionMode(bool _b)
        {
            switch(controllerData)
            {
                case FaderData faderData:
                    FaderControl faderControl = controlObject.GetComponent<FaderControl>();
                    faderControl.SetSortButtonVisibility(_b);
                    break;
                case Controller2DData control2DData:
                    Controller2D control2D = controlObject.GetComponent<Controller2D>();
                    control2D.SetSortButtonVisibility(_b);
                    break;
                default:
                    Debug.LogError($"{controllerData.GetType()} not implemented");
                    break;
            }
        }

        public void SelfDestruct()
        {
            Destroy(optionsMenu);
            Destroy(activateControllerOptionsButton.gameObject);
            Destroy(controlObject);
        }
    }
}
