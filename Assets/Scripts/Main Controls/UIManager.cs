using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Serialization;
using PopUpWindows;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ControlsManager _controlsManager;
    [FormerlySerializedAs("optionsPanel")] [SerializeField] GameObject _optionsPanel = null;
    [FormerlySerializedAs("sliderOptionsButtonLayoutPrefab")] [SerializeField] GameObject _sliderOptionsButtonLayoutPrefab = null;
    [FormerlySerializedAs("faderOptionsPrefab")] [SerializeField] GameObject _faderOptionsPrefab = null;
    [FormerlySerializedAs("controller2DOptionsPrefab")] [SerializeField] GameObject _controller2DOptionsPrefab = null;
    [FormerlySerializedAs("faderOptionsActivationPrefab")] [SerializeField] GameObject _faderOptionsActivationPrefab = null; //the prefab for the button that opens up fader options
    [FormerlySerializedAs("sliderButtonVerticalLayoutParent")] [SerializeField] GameObject _sliderButtonVerticalLayoutParent = null;
    [FormerlySerializedAs("optionsButton")] [SerializeField] Button _optionsButton = null;
    [FormerlySerializedAs("closeOptionsButton")] [SerializeField] Button _closeOptionsButton = null;
    [FormerlySerializedAs("newControllerButton")] [SerializeField] Button _newControllerButton = null;
    [FormerlySerializedAs("oscMenu")] [SerializeField] OSCSelectionMenu _oscMenu;

    [FormerlySerializedAs("faderPositionEnableButton")]
    [Space(10)]
    [SerializeField] Button _faderPositionEnableButton = null;
    [FormerlySerializedAs("faderPositionExitButton")] [SerializeField] Button _faderPositionExitButton = null;
    [FormerlySerializedAs("faderLayoutGroup")] [SerializeField] HorizontalLayoutGroup _faderLayoutGroup = null;
    [FormerlySerializedAs("optionsButtonSortingButton")] [SerializeField] Button _optionsButtonSortingButton = null;

    [FormerlySerializedAs("setupButton")] [SerializeField] Button _setupButton;
    [FormerlySerializedAs("closeSetupButton")] [SerializeField] Button _closeSetupButton;
    [FormerlySerializedAs("setupPanel")] [SerializeField] GameObject _setupPanel;

    [FormerlySerializedAs("creditsButton")]
    [Header("Credits")]
    [SerializeField] Button _creditsButton;
    [FormerlySerializedAs("closeCreditsButton")] [SerializeField] Button _closeCreditsButton;
    [FormerlySerializedAs("creditsPanel")] [SerializeField] GameObject _creditsPanel;
    [FormerlySerializedAs("creditsButtons")] [SerializeField] List<CreditsButton> _creditsButtons = new List<CreditsButton>();
    [FormerlySerializedAs("donationButtons")] [SerializeField] List<DonationButton> _donationButtons = new List<DonationButton>();

    [Serializable]
    struct CreditsButton
    {
        [FormerlySerializedAs("button")] public Button Button;
        [FormerlySerializedAs("link")] public string Link;
    }

    [Serializable]
    struct DonationButton
    {
        [FormerlySerializedAs("button")] public Button Button;
        [FormerlySerializedAs("address")] public string Address;
    }

    const int SliderButtonLayoutCapacity = 5;

    List<ControllerUIGroup> _controllerUIs = new List<ControllerUIGroup>();
    List<LayoutGroupButtonCount> _layoutCounts = new List<LayoutGroupButtonCount>();

    bool _positionMode = false;

    public static UIManager Instance;

    bool _sortOptionsByName = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError($"There is a second UIManager in the scene!", this);
            Debug.LogError($"This is the first one", Instance);
        }

        //options
        _optionsButton.onClick.AddListener(ToggleOptionsMenu);
        _closeOptionsButton.onClick.AddListener(ToggleOptionsMenu);
        _faderPositionExitButton.onClick.AddListener(ToggleEditFaderPositionMode);
        //prevent accidentally leaving stuff on in the scene
        _optionsPanel.SetActive(false);


        //if not loaded, use default //when faderwidth is loaded, it will need to change the size of faders. faders should be playerprefs
        _faderPositionEnableButton.onClick.AddListener(ToggleEditFaderPositionMode);
        _optionsButtonSortingButton.onClick.AddListener(() => SwitchOptionsButtonSorting(!_sortOptionsByName));
        SwitchOptionsButtonSorting(false);
        _newControllerButton.onClick.AddListener(_controlsManager.NewController);

        UnityAction toggleSetup = () => _setupPanel.SetActive(!_setupPanel.activeSelf);
        _setupButton.onClick.AddListener(toggleSetup);
        _closeSetupButton.onClick.AddListener(toggleSetup);

        UnityAction toggleCredits = () => _creditsPanel.SetActive(!_creditsPanel.activeSelf);
        _creditsButton.onClick.AddListener(toggleCredits);
        _closeCreditsButton.onClick.AddListener(toggleCredits);

        InitializeCreditsButtons();
        InitializeDonationButtons();
    }

    private void SwitchOptionsButtonSorting(bool _byName)
    {
        _sortOptionsByName = _byName;
        _optionsButtonSortingButton.GetComponentInChildren<Text>().text = $"Sort Options: {(_sortOptionsByName ? "Name" : "Layout")}";
        SortOptionsButtons();
    }

    //used by options button in scene
    public void ToggleOptionsMenu()
    {
        _optionsPanel.SetActive(!_optionsPanel.activeInHierarchy);
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
                buttonGroup = new ControllerUIGroup(_controlsManager, fader, _faderOptionsActivationPrefab, _control, faderOptions);
                break;
            case Controller2DData control2D:
                GameObject controlOptions = InitializeController2DOptions(control2D, _control);
                buttonGroup = new ControllerUIGroup(_controlsManager, control2D, _faderOptionsActivationPrefab, _control, controlOptions);
                break;
            default:
                buttonGroup = null;
                Debug.LogError($"Unhandled ControllerData type {_config.GetType()}");
                break;
        }

        //buttonGroup.optionsMenu.transform.SetParent(optionsPanel.transform, false);
        if (buttonGroup != null)
        {
            _controllerUIs.Add(buttonGroup);
        }
    }

    GameObject InitializeFaderOptions(FaderData _config, GameObject _controlObj)
    {
        GameObject menuObj = Instantiate(_faderOptionsPrefab, _optionsPanel.transform, false);
        FaderOptions faderOptions = menuObj.GetComponent<FaderOptions>();
        faderOptions.Initialize(_config, _controlObj.GetComponent<RectTransform>(), _oscMenu);
        faderOptions.gameObject.name = _config.GetName() + " Options Panel";
        return menuObj;
    }

    GameObject InitializeController2DOptions(Controller2DData _config, GameObject _controlObj)
    {
        GameObject menuObj = Instantiate(_controller2DOptionsPrefab, _optionsPanel.transform, false);
        Controller2DOptions controllerOptions = menuObj.GetComponent<Controller2DOptions>();
        controllerOptions.Initialize(_config, _controlObj.GetComponent<RectTransform>(), _oscMenu);
        controllerOptions.gameObject.name = _config.GetName() + " Options Panel";
        return menuObj;
    }



    void AddOptionsButtonToLayout(GameObject _button)
    {
        for (int i = 0; i < _layoutCounts.Count; i++)
        {
            if (_layoutCounts[i].Count < SliderButtonLayoutCapacity)
            {
                _button.transform.SetParent(_layoutCounts[i].LayoutGroup.transform);
                _button.transform.SetSiblingIndex(_layoutCounts[i].Count);
                _layoutCounts[i].Count++;
                return;
            }
        }

        //all layouts full, create a new one

        AddNewLayoutCount();
        AddOptionsButtonToLayout(_button);
    }

    void DestroyEmptyLayouts()
    {
        for (int i = 0; i < _layoutCounts.Count; i++)
        {
            if (_layoutCounts[i].Count == 0)
            {
                //destroy
                Destroy(_layoutCounts[i].LayoutGroup);
                _layoutCounts.RemoveAt(i);
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
        _controllerUIs.Remove(_buttonGroup);

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
        LayoutGroupButtonCount layout = GetLayoutGroupFromObject(_buttonGroup.ActivateControllerOptionsButton.gameObject);

        if (layout != null)
        {
            layout.Count--;
        }
        else
        {
            Debug.LogError("Null layout! button didn't find its parent.");
        }
    }

    void AddNewLayoutCount()
    {
        LayoutGroupButtonCount lay = new LayoutGroupButtonCount();
        lay.LayoutGroup = Instantiate(_sliderOptionsButtonLayoutPrefab);
        lay.LayoutGroup.transform.SetParent(_sliderButtonVerticalLayoutParent.transform);
        _layoutCounts.Add(lay);
    }

    public void ShowControllerOptions(ControllerData _data)
    {
        ControllerUIGroup uiGroup = GetButtonGroupByConfig(_data);
        uiGroup.SetControllerOptionsActive(true);
    }

    ControllerUIGroup GetButtonGroupByConfig(ControllerData _data)
    {
        foreach (ControllerUIGroup cbg in _controllerUIs)
        {
            if (cbg.ControllerData == _data)
            {
                return cbg;
            }
        }
        return null;
    }
    #endregion Controller Options
    void InitializeCreditsButtons()
    {
        foreach(CreditsButton b in _creditsButtons)
        {
            b.Button.onClick.AddListener(() => Application.OpenURL(b.Link));
        }
    }

    void InitializeDonationButtons()
    {
        foreach(DonationButton b in _donationButtons)
        {
            b.Button.onClick.AddListener(() =>
            {
                UniClipboard.SetText(b.Address);
                PopUpController.Instance.QuickNoticeWindow($"Copied {b.Address} to clipboard!");
            });
        }
    }

    /// <summary>
    /// This is used to let the controllers use their own width - for some reason this is necessary for the width to update
    /// </summary>
    public void RefreshFaderLayoutGroup()
    {
        _faderLayoutGroup.enabled = false;
        _faderLayoutGroup.enabled = true;
    }

    LayoutGroupButtonCount GetLayoutGroupFromObject(GameObject _button)
    {
        foreach(LayoutGroupButtonCount lay in _layoutCounts)
        {
            Transform[] children = lay.LayoutGroup.GetComponentsInChildren<Transform>();

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
        foreach(LayoutGroupButtonCount layCount in _layoutCounts)
        {
            layCount.Count = 0;
        }

        //get all the unnamed ones out of sorting list
        List<ControllerUIGroup> unnamedControls = new List<ControllerUIGroup>();
        for (int i = 0; i < _controllerUIs.Count; i++)
        {
            if (_controllerUIs[i].ControllerData.GetName() == ControlsManager.GetDefaultControllerName(_controllerUIs[i].ControllerData))
            {
                unnamedControls.Add(_controllerUIs[i]);
                _controllerUIs.Remove(_controllerUIs[i]);
                i--;
            }
        }

        //sort buttons
        if (_sortOptionsByName)
        {
            _controllerUIs.Sort((s1, s2) => s1.ControllerData.GetName().CompareTo(s2.ControllerData.GetName()));
        }
        else
        {
            _controllerUIs.Sort((s1, s2) => s1.ControllerData.GetPosition().CompareTo(s2.ControllerData.GetPosition()));
        }

        //add unnamed ones to the end
        foreach (ControllerUIGroup c in unnamedControls)
        {
            _controllerUIs.Add(c);
        }

        //place buttons
        foreach (ControllerUIGroup c in _controllerUIs)
        {
            AddOptionsButtonToLayout(c.ActivateControllerOptionsButton.gameObject);
        }
    }

    void ToggleEditFaderPositionMode()
    {
        _positionMode = !_positionMode;

        foreach(ControllerUIGroup u in _controllerUIs)
        {
            u.SetPostionMode(_positionMode);
            u.SetPosition();
        }

        //toggle options buttons
        _optionsButton.gameObject.SetActive(!_positionMode);
        _optionsPanel.SetActive(!_positionMode);
        _faderPositionExitButton.gameObject.SetActive(_positionMode);
    }

    class LayoutGroupButtonCount
    {
        public GameObject LayoutGroup;
        public int Count = 0;
    }

    class ControllerUIGroup
    {
        //should be public get private set
        public ButtonExtended ActivateControllerOptionsButton { get; private set; }
        Toggle _activationToggle;
        GameObject _controlObject;
        RectTransform _controlObjectTransform;
        public ControllerData ControllerData { get; private set; }
        GameObject _optionsMenu;
        ControlsManager _controlsManager;

        public ControllerUIGroup(ControlsManager controlsManager, ControllerData _config, GameObject _optionsActivateButtonPrefab, GameObject _controlObject, GameObject _optionsMenu)
        {
            ControllerData = _config;
            this._optionsMenu = _optionsMenu;
            this._controlObject = _controlObject;

            ActivateControllerOptionsButton = Instantiate(_optionsActivateButtonPrefab).GetComponent<ButtonExtended>();
            ActivateControllerOptionsButton.gameObject.name = _config.GetName() + " Options";
            ActivateControllerOptionsButton.GetComponentInChildren<Text>().text = _config.GetName() + " Options"; // change visible button title
            ActivateControllerOptionsButton.OnClick.AddListener(() => { SetControllerOptionsActive(true); });
            ActivateControllerOptionsButton.OnPointerHeld.AddListener(Delete);
            SetControllerOptionsActive(false);

            _activationToggle = ActivateControllerOptionsButton.GetComponentInChildren<Toggle>();
            _activationToggle.onValueChanged.AddListener(ToggleControlVisibility);
            _activationToggle.SetIsOnWithoutNotify(_config.GetEnabled());
            _controlObjectTransform = _controlObject.GetComponent<RectTransform>();
        }

        void Delete()
        {
            UnityAction deleteAction = () =>
            {
                _controlsManager.DestroyController(ControllerData);
                UIManager.Instance.DestroyControllerObjects(ControllerData);
            };

            PopUpController.Instance.ConfirmationWindow($"Delete controller\n\"{ControllerData.GetName()}\"?", deleteAction, null, "Delete", "Cancel");
        }

        public void SetControllerOptionsActive(bool _active)
        {
            _optionsMenu.SetActive(_active);
        }

        public void ToggleControlVisibility(bool _b)
        {
            _controlObject.SetActive(_b);
            ControllerData.SetEnabled(_b);

            if(!_b)
            {
                _controlObjectTransform.SetAsLastSibling();
            }
        }

        public void SetPosition()
        {
            ControllerData.SetPosition(_controlObjectTransform.GetSiblingIndex());
        }

        public void SetPostionMode(bool _b)
        {
            switch(ControllerData)
            {
                case FaderData faderData:
                    FaderControl faderControl = _controlObject.GetComponent<FaderControl>();
                    faderControl.SetSortButtonVisibility(_b);
                    break;
                case Controller2DData control2DData:
                    Controller2D control2D = _controlObject.GetComponent<Controller2D>();
                    control2D.SetSortButtonVisibility(_b);
                    break;
                default:
                    Debug.LogError($"{ControllerData.GetType()} not implemented");
                    break;
            }
        }

        public void SelfDestruct()
        {
            Destroy(_optionsMenu);
            Destroy(ActivateControllerOptionsButton.gameObject);
            Destroy(_controlObject);
        }
    }
}
