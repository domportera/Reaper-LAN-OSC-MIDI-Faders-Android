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
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private GameObject _faderOptionsPrefab;
    [SerializeField] private GameObject _controller2DOptionsPrefab;

    [SerializeField]
    private GameObject _faderOptionsActivationPrefab; //the prefab for the button that opens up fader options
    [SerializeField] private OscSelectionMenu _oscMenu;

    [SerializeField] private GridLayoutGroup _sliderOptionsButtonLayout;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _closeOptionsButton;
    [SerializeField] private Button _newControllerButton;
    [SerializeField] private Button _faderPositionEnableButton;
    [SerializeField] private Button _faderPositionExitButton;
    [SerializeField] private HorizontalLayoutGroup _faderLayoutGroup;
    [SerializeField] private Button _optionsButtonSortingButton;

    [SerializeField] private Button _setupButton;
    [SerializeField] private Button _closeSetupButton;
    [SerializeField] private GameObject _setupPanel;

    [Header("Credits")] [SerializeField] private Button _creditsButton;
    [SerializeField] private Button _closeCreditsButton;
    [SerializeField] private GameObject _creditsPanel;
    [SerializeField] private List<CreditsButton> _creditsButtons = new List<CreditsButton>();
    [SerializeField] private List<DonationButton> _donationButtons = new List<DonationButton>();

    [Serializable]
    private struct CreditsButton
    {
        [FormerlySerializedAs("button")] public Button Button;
        [FormerlySerializedAs("link")] public string Link;
    }

    [Serializable]
    private struct DonationButton
    {
        [FormerlySerializedAs("button")] public Button Button;
        [FormerlySerializedAs("address")] public string Address;
    }

    private const int SliderButtonLayoutCapacity = 5;

    private List<ControllerUIGroup> _controllerUIs = new List<ControllerUIGroup>();

    private bool _positionMode;
    private bool _sortOptionsByName;

    public static UIManager Instance;

    // Start is called before the first frame update
    private void Awake()
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

        _sliderOptionsButtonLayout.cellSize = _faderOptionsActivationPrefab.GetComponent<RectTransform>().rect.size;

        InitializeCreditsButtons();
        InitializeDonationButtons();
    }

    private void SwitchOptionsButtonSorting(bool byName)
    {
        _sortOptionsByName = byName;
        _optionsButtonSortingButton.GetComponentInChildren<Text>().text =
            $"Sort Options: {(byName ? "Name" : "Layout")}";
        SortOptionsButtons(byName, _controllerUIs);
        return;

        static void SortOptionsButtons(bool sortOptionsByName, List<ControllerUIGroup> controllerUIs)
        {
            //get all the unnamed ones out of sorting list
            var unnamedControls = new List<ControllerUIGroup>();
            for (var i = 0; i < controllerUIs.Count; i++)
            {
                if (controllerUIs[i].ControllerData.GetName() ==
                    ControlsManager.GetDefaultControllerName(controllerUIs[i].ControllerData))
                {
                    unnamedControls.Add(controllerUIs[i]);
                    controllerUIs.Remove(controllerUIs[i]);
                    i--;
                }
            }

            //sort buttons
            if (sortOptionsByName)
            {
                controllerUIs.Sort((s1, s2) => 
                    string.Compare(s1.ControllerData.GetName(), s2.ControllerData.GetName(), StringComparison.Ordinal));
            }
            else
            {
                controllerUIs.Sort((s1, s2) =>
                    s1.ControllerData.GetPosition().CompareTo(s2.ControllerData.GetPosition()));
            }
            
            //add unnamed ones to the end
            foreach (var c in unnamedControls)
            {
                controllerUIs.Add(c);
            }
            
            for (var i = 0; i < controllerUIs.Count; i++)
            {
                controllerUIs[i].OptionButtonTransform.SetSiblingIndex(i);
            }
        }
    }

    //used by options button in scene
    public void ToggleOptionsMenu()
    {
        _optionsPanel.SetActive(!_optionsPanel.activeInHierarchy);
    }

    #region Controller Options
    public void SpawnControllerOptions(ControllerData config, GameObject control)
    {
        //check if any other controller buttons exist for this, then destroy all its contents
        //make sure to destroy faderOptions as well
        var dupe = GetButtonGroupByConfig(config);
        if (dupe != null)
        {
            DestroyControllerGroup(dupe);
        }

        InitializeControllerOptions(config, control);
        SwitchOptionsButtonSorting(_sortOptionsByName);
    }

    private void InitializeControllerOptions(ControllerData config, GameObject control)
    {
        ControllerUIGroup buttonGroup;

        var parent = (RectTransform)_sliderOptionsButtonLayout.transform;

        switch (config)
        {
            case FaderData fader:
                var faderOptions = InitializeFaderOptions(fader, control);
                buttonGroup = new ControllerUIGroup(_controlsManager, fader, _faderOptionsActivationPrefab, parent, control, faderOptions);
                break;
            case Controller2DData control2D:
                var controlOptions = InitializeController2DOptions(control2D, control);
                buttonGroup = new ControllerUIGroup(_controlsManager, control2D, _faderOptionsActivationPrefab, parent, control, controlOptions);
                break;
            default:
                buttonGroup = null;
                Debug.LogError($"Unhandled ControllerData type {config.GetType()}");
                break;
        }

        //buttonGroup.optionsMenu.transform.SetParent(optionsPanel.transform, false);
        if (buttonGroup != null)
        {
            _controllerUIs.Add(buttonGroup);
        }
    }

    private GameObject InitializeFaderOptions(FaderData config, GameObject controlObj)
    {
        var menuObj = Instantiate(_faderOptionsPrefab, _optionsPanel.transform, false);
        var faderOptions = menuObj.GetComponent<FaderOptions>();
        faderOptions.Initialize(config, _oscMenu);
        faderOptions.gameObject.name = config.GetName() + " Options Panel";
        return menuObj;
    }

    private GameObject InitializeController2DOptions(Controller2DData config, GameObject controlObj)
    {
        var menuObj = Instantiate(_controller2DOptionsPrefab, _optionsPanel.transform, false);
        var controllerOptions = menuObj.GetComponent<Controller2DOptionsPanel>();
        controllerOptions.Initialize(config, _oscMenu);
        controllerOptions.gameObject.name = config.GetName() + " Options Panel";
        return menuObj;
    }

    public void DestroyControllerGroup(ControllerData config)
    {
        DestroyControllerGroup(GetButtonGroupByConfig(config));
    }

    private void DestroyControllerGroup(ControllerUIGroup buttonGroup)
    {
        //destroy all params
        buttonGroup.SelfDestruct();

        //remove from list
        _controllerUIs.Remove(buttonGroup);
    }

    public void DestroyControllerObjects(ControllerData config)
    {
        var buttonGroup = GetButtonGroupByConfig(config);
        DestroyControllerGroup(buttonGroup);
    }

    public void ShowControllerOptions(ControllerData data)
    {
        var uiGroup = GetButtonGroupByConfig(data);
        uiGroup.SetControllerOptionsActive(true);
    }

    private ControllerUIGroup GetButtonGroupByConfig(ControllerData data)
    {
        foreach (var cbg in _controllerUIs)
        {
            if (cbg.ControllerData == data)
            {
                return cbg;
            }
        }
        return null;
    }
    #endregion Controller Options

    private void InitializeCreditsButtons()
    {
        foreach(var b in _creditsButtons)
        {
            b.Button.onClick.AddListener(() => Application.OpenURL(b.Link));
        }
    }

    private void InitializeDonationButtons()
    {
        foreach(var b in _donationButtons)
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

    private void ToggleEditFaderPositionMode()
    {
        _positionMode = !_positionMode;

        foreach(var u in _controllerUIs)
        {
            u.SetPostionMode(_positionMode);
            u.SetPosition();
        }

        //toggle options buttons
        _optionsButton.gameObject.SetActive(!_positionMode);
        _optionsPanel.SetActive(!_positionMode);
        _faderPositionExitButton.gameObject.SetActive(_positionMode);
    }

    private class ControllerUIGroup
    {
        //should be public get private set
        public RectTransform OptionButtonTransform { get; }
        public ButtonExtended ActivateOptionsButton { get; private set; }
        private readonly GameObject _controlObject;
        private readonly RectTransform _controlObjectTransform;
        public ControllerData ControllerData { get; private set; }
        private readonly GameObject _optionsMenu;
        private readonly ControlsManager _controlsManager;

        public ControllerUIGroup(ControlsManager controlsManager, ControllerData config, GameObject optionsActivateButtonPrefab, RectTransform optionsButtonParent, GameObject controlObject, GameObject optionsMenu)
        {
            ControllerData = config;
            _controlsManager = controlsManager;
            _optionsMenu = optionsMenu;
            _controlObject = controlObject;

            var buttonObj = Instantiate(optionsActivateButtonPrefab, optionsButtonParent, false);
            OptionButtonTransform = (RectTransform)buttonObj.transform;
            ActivateOptionsButton = buttonObj.GetComponentInChildren<ButtonExtended>();
            ActivateOptionsButton.gameObject.name = config.GetName() + " Options Button";
            ActivateOptionsButton.GetComponentInChildren<Text>().text = config.GetName(); // change visible button title
            ActivateOptionsButton.OnClick.AddListener(() => { SetControllerOptionsActive(true); });
            ActivateOptionsButton.OnPointerHeld.AddListener(Delete);
            SetControllerOptionsActive(false);

            var activationToggle = buttonObj.GetComponentInChildren<Toggle>();
            activationToggle.onValueChanged.AddListener(ToggleControlVisibility);
            activationToggle.SetIsOnWithoutNotify(config.GetEnabled());
            _controlObjectTransform = controlObject.GetComponent<RectTransform>();
        }

        private void Delete()
        {
            UnityAction deleteAction = () =>
            {
                _controlsManager.DestroyController(ControllerData);
                UIManager.Instance.DestroyControllerObjects(ControllerData);
            };

            PopUpController.Instance.ConfirmationWindow($"Delete controller\n\"{ControllerData.GetName()}\"?", deleteAction, null, "Delete", "Cancel");
        }

        public void SetControllerOptionsActive(bool active)
        {
            _optionsMenu.SetActive(active);
        }

        public void ToggleControlVisibility(bool b)
        {
            _controlObject.SetActive(b);
            ControllerData.SetEnabled(b);

            if(!b)
            {
                _controlObjectTransform.SetAsLastSibling();
            }
        }

        public void SetPosition()
        {
            ControllerData.SetPosition(_controlObjectTransform.GetSiblingIndex());
        }

        public void SetPostionMode(bool b)
        {
            switch(ControllerData)
            {
                case FaderData faderData:
                    var faderControl = _controlObject.GetComponent<FaderControlUi>();
                    faderControl.SetSortButtonVisibility(b);
                    break;
                case Controller2DData control2DData:
                    var control2D = _controlObject.GetComponent<Controller2DUi>();
                    control2D.SetSortButtonVisibility(b);
                    break;
                default:
                    Debug.LogError($"{ControllerData.GetType()} not implemented");
                    break;
            }
        }

        public void SelfDestruct()
        {
            Destroy(_optionsMenu);
            Destroy(ActivateOptionsButton.gameObject);
            Destroy(_controlObject);
        }
    }
}
