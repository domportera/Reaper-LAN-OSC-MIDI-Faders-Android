using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static ControlsManager;
using static UnityEngine.UI.Dropdown;

public class UIManager : MonoBehaviour
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

    const int DEFAULT_FADER_WIDTH = 200;
    int faderWidth = DEFAULT_FADER_WIDTH;

    const string FADER_WIDTH_PLAYER_PREF = "Fader Width";
    bool positionMode = false;

    public static UIManager instance;


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

    //used by options button in scene
    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeInHierarchy);
    }


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
        ControllerOptionsMenu faderOptions = menuObj.GetComponent<ControllerOptionsMenu>();
        faderOptions.Initialize(_config, _controlObj.GetComponent<RectTransform>());
        faderOptions.gameObject.name = _config.GetName() + " Options Panel";
        faderOptions.controllerConfig = _config.GetController();
        return menuObj;
    }

    GameObject InitializeController2DOptions(Controller2DData _config, GameObject _control)
    {
        GameObject menuObj = Instantiate(controller2DOptionsPrefab, optionsPanel.transform, false);
        Controller2DOptions controllerOptions = menuObj.GetComponent<Controller2DOptions>();
        controllerOptions.Initialize(_config, _control.GetComponent<RectTransform>());
        controllerOptions.gameObject.name = _config.GetName() + " Options Panel";
        return menuObj;
    }

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
                Utilities.instance.ConfirmationWindow($"Copied {b.address} to clipboard!");
            });
        }
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

    public void DestroyControllerObjects(ControllerData _config)
    {
        ControllerUIGroup buttonGroup = GetButtonGroupByConfig(_config);
        DestroyControllerGroup(buttonGroup);
    }

    void RemoveFromLayout(ControllerUIGroup _buttonGroup)
    {
        LayoutGroupButtonCount layout = GetLayoutGroupFromObject(_buttonGroup.activateFaderOptionsButton.gameObject);

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

    ControllerUIGroup GetButtonGroupByConfig(ControllerData _config)
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
            if(controllerUIs[i].controllerConfig.GetName() == NEW_CONTROLLER_NAME)
            {
                unnamedControls.Add(controllerUIs[i]);
                controllerUIs.Remove(controllerUIs[i]);
                i--;
            }
        }

        //sort buttons
        controllerUIs.Sort((s1, s2) => s1.controllerConfig.GetPosition().CompareTo(s2.controllerConfig.GetPosition()));

        //add unnamed ones to the end
        foreach (ControllerUIGroup c in unnamedControls)
        {
            controllerUIs.Add(c);
        }

        //place buttons
        foreach (ControllerUIGroup c in controllerUIs)
        {
            AddOptionsButtonToLayout(c.activateFaderOptionsButton.gameObject);
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

        if(!positionMode)
        {
            Utilities.instance.ConfirmationWindow("Don't forget to save!");
        }
    }

    public LayoutGroup GetControllerLayoutGroup()
    {
        return faderLayoutGroup;
    }

    class LayoutGroupButtonCount
    {
        public GameObject layoutGroup;
        public int count = 0;
    }

    class ControllerUIGroup
    {
        //should be public get private set
        public Button activateFaderOptionsButton { get; private set; }
        Toggle activationToggle;
        GameObject controlObject;
        RectTransform controlObjectTransform;
        public ControllerData controllerConfig { get; private set; }
        public GameObject optionsMenu { get; private set; }

        public ControllerUIGroup(ControllerData _config, GameObject _optionsActivateButtonPrefab, GameObject _controlObject, GameObject _optionsMenu)
        {
            controllerConfig = _config;
            optionsMenu = _optionsMenu;
            controlObject = _controlObject;

            activateFaderOptionsButton = Instantiate(_optionsActivateButtonPrefab).GetComponent<Button>();
            activateFaderOptionsButton.gameObject.name = _config.GetName() + " Options";
            activateFaderOptionsButton.GetComponentInChildren<Text>().text = _config.GetName() + " Options"; // change visible button title
            activateFaderOptionsButton.onClick.AddListener(DisplayFaderOptions);

            activationToggle = activateFaderOptionsButton.GetComponentInChildren<Toggle>();
            activationToggle.onValueChanged.AddListener(ToggleControlVisibility);
            controlObjectTransform = _controlObject.GetComponent<RectTransform>();
        }

        void DisplayFaderOptions()
        {
            optionsMenu.SetActive(true);
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
            switch(controllerConfig)
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
                    Debug.LogError($"{controllerConfig.GetType()} not implemented");
                    break;
            }
        }

        public void SelfDestruct()
        {
            Destroy(optionsMenu);
            Destroy(activateFaderOptionsButton.gameObject);
            Destroy(controlObject);
        }
    }
}
