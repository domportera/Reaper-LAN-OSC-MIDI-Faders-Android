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
    [FormerlySerializedAs("optionsPanel")] [SerializeField]
    private GameObject _optionsPanel = null;
    [FormerlySerializedAs("sliderOptionsButtonLayoutPrefab")] [SerializeField]
    private GameObject _sliderOptionsButtonLayoutPrefab = null;
    [FormerlySerializedAs("faderOptionsPrefab")] [SerializeField]
    private GameObject _faderOptionsPrefab = null;
    [FormerlySerializedAs("controller2DOptionsPrefab")] [SerializeField]
    private GameObject _controller2DOptionsPrefab = null;
    [FormerlySerializedAs("faderOptionsActivationPrefab")] [SerializeField]
    private GameObject _faderOptionsActivationPrefab = null; //the prefab for the button that opens up fader options
    [FormerlySerializedAs("sliderButtonVerticalLayoutParent")] [SerializeField]
    private GameObject _sliderButtonVerticalLayoutParent = null;
    [FormerlySerializedAs("optionsButton")] [SerializeField]
    private Button _optionsButton = null;
    [FormerlySerializedAs("closeOptionsButton")] [SerializeField]
    private Button _closeOptionsButton = null;
    [FormerlySerializedAs("newControllerButton")] [SerializeField]
    private Button _newControllerButton = null;
    [FormerlySerializedAs("oscMenu")] [SerializeField]
    private OscSelectionMenu _oscMenu;

    [FormerlySerializedAs("faderPositionEnableButton")]
    [Space(10)]
    [SerializeField]
    private Button _faderPositionEnableButton = null;
    [FormerlySerializedAs("faderPositionExitButton")] [SerializeField]
    private Button _faderPositionExitButton = null;
    [FormerlySerializedAs("faderLayoutGroup")] [SerializeField]
    private HorizontalLayoutGroup _faderLayoutGroup = null;
    [FormerlySerializedAs("optionsButtonSortingButton")] [SerializeField]
    private Button _optionsButtonSortingButton = null;

    [FormerlySerializedAs("setupButton")] [SerializeField]
    private Button _setupButton;
    [FormerlySerializedAs("closeSetupButton")] [SerializeField]
    private Button _closeSetupButton;
    [FormerlySerializedAs("setupPanel")] [SerializeField]
    private GameObject _setupPanel;

    [FormerlySerializedAs("creditsButton")]
    [Header("Credits")]
    [SerializeField]
    private Button _creditsButton;
    [FormerlySerializedAs("closeCreditsButton")] [SerializeField]
    private Button _closeCreditsButton;
    [FormerlySerializedAs("creditsPanel")] [SerializeField]
    private GameObject _creditsPanel;
    [FormerlySerializedAs("creditsButtons")] [SerializeField]
    private List<CreditsButton> _creditsButtons = new List<CreditsButton>();
    [FormerlySerializedAs("donationButtons")] [SerializeField]
    private List<DonationButton> _donationButtons = new List<DonationButton>();

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
    private List<LayoutGroupButtonCount> _layoutCounts = new List<LayoutGroupButtonCount>();

    private bool _positionMode = false;

    public static UIManager Instance;

    private bool _sortOptionsByName = false;

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

        InitializeCreditsButtons();
        InitializeDonationButtons();
    }

    private void SwitchOptionsButtonSorting(bool byName)
    {
        _sortOptionsByName = byName;
        _optionsButtonSortingButton.GetComponentInChildren<Text>().text = $"Sort Options: {(_sortOptionsByName ? "Name" : "Layout")}";
        SortOptionsButtons();
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
        SortOptionsButtons();
    }

    private void InitializeControllerOptions(ControllerData config, GameObject control)
    {
        ControllerUIGroup buttonGroup;

        switch (config)
        {
            case FaderData fader:
                var faderOptions = InitializeFaderOptions(fader, control);
                buttonGroup = new ControllerUIGroup(_controlsManager, fader, _faderOptionsActivationPrefab, control, faderOptions);
                break;
            case Controller2DData control2D:
                var controlOptions = InitializeController2DOptions(control2D, control);
                buttonGroup = new ControllerUIGroup(_controlsManager, control2D, _faderOptionsActivationPrefab, control, controlOptions);
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
        faderOptions.Initialize(config, controlObj.GetComponent<RectTransform>(), _oscMenu);
        faderOptions.gameObject.name = config.GetName() + " Options Panel";
        return menuObj;
    }

    private GameObject InitializeController2DOptions(Controller2DData config, GameObject controlObj)
    {
        var menuObj = Instantiate(_controller2DOptionsPrefab, _optionsPanel.transform, false);
        var controllerOptions = menuObj.GetComponent<Controller2DOptionsPanel>();
        controllerOptions.Initialize(config, controlObj.GetComponent<RectTransform>(), _oscMenu);
        controllerOptions.gameObject.name = config.GetName() + " Options Panel";
        return menuObj;
    }


    private void AddOptionsButtonToLayout(GameObject button)
    {
        for (var i = 0; i < _layoutCounts.Count; i++)
        {
            if (_layoutCounts[i].Count < SliderButtonLayoutCapacity)
            {
                button.transform.SetParent(_layoutCounts[i].LayoutGroup.transform);
                button.transform.SetSiblingIndex(_layoutCounts[i].Count);
                _layoutCounts[i].Count++;
                return;
            }
        }

        //all layouts full, create a new one

        AddNewLayoutCount();
        AddOptionsButtonToLayout(button);
    }

    private void DestroyEmptyLayouts()
    {
        for (var i = 0; i < _layoutCounts.Count; i++)
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

        //check for empty layouts, and destroy it
        DestroyEmptyLayouts();

        SortOptionsButtons();
    }

    public void DestroyControllerObjects(ControllerData config)
    {
        var buttonGroup = GetButtonGroupByConfig(config);
        DestroyControllerGroup(buttonGroup);
    }

    private void RemoveFromLayout(ControllerUIGroup buttonGroup)
    {
        var layout = GetLayoutGroupFromObject(buttonGroup.ActivateControllerOptionsButton.gameObject);

        if (layout != null)
        {
            layout.Count--;
        }
        else
        {
            Debug.LogError("Null layout! button didn't find its parent.");
        }
    }

    private void AddNewLayoutCount()
    {
        var lay = new LayoutGroupButtonCount();
        lay.LayoutGroup = Instantiate(_sliderOptionsButtonLayoutPrefab);
        lay.LayoutGroup.transform.SetParent(_sliderButtonVerticalLayoutParent.transform);
        _layoutCounts.Add(lay);
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

    private LayoutGroupButtonCount GetLayoutGroupFromObject(GameObject button)
    {
        foreach(var lay in _layoutCounts)
        {
            var children = lay.LayoutGroup.GetComponentsInChildren<Transform>();

            foreach(var child in children)
            {
                if(child.gameObject == button)
                {
                    return lay;
                }
            }
        }

        return null;
    }

    private void SortOptionsButtons()
    {
        foreach(var layCount in _layoutCounts)
        {
            layCount.Count = 0;
        }

        //get all the unnamed ones out of sorting list
        var unnamedControls = new List<ControllerUIGroup>();
        for (var i = 0; i < _controllerUIs.Count; i++)
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
        foreach (var c in unnamedControls)
        {
            _controllerUIs.Add(c);
        }

        //place buttons
        foreach (var c in _controllerUIs)
        {
            AddOptionsButtonToLayout(c.ActivateControllerOptionsButton.gameObject);
        }
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

    private class LayoutGroupButtonCount
    {
        public GameObject LayoutGroup;
        public int Count = 0;
    }

    private class ControllerUIGroup
    {
        //should be public get private set
        public ButtonExtended ActivateControllerOptionsButton { get; private set; }
        private Toggle _activationToggle;
        private GameObject _controlObject;
        private RectTransform _controlObjectTransform;
        public ControllerData ControllerData { get; private set; }
        private GameObject _optionsMenu;
        private ControlsManager _controlsManager;

        public ControllerUIGroup(ControlsManager controlsManager, ControllerData config, GameObject optionsActivateButtonPrefab, GameObject controlObject, GameObject optionsMenu)
        {
            ControllerData = config;
            this._optionsMenu = optionsMenu;
            this._controlObject = controlObject;

            ActivateControllerOptionsButton = Instantiate(optionsActivateButtonPrefab).GetComponent<ButtonExtended>();
            ActivateControllerOptionsButton.gameObject.name = config.GetName() + " Options";
            ActivateControllerOptionsButton.GetComponentInChildren<Text>().text = config.GetName() + " Options"; // change visible button title
            ActivateControllerOptionsButton.OnClick.AddListener(() => { SetControllerOptionsActive(true); });
            ActivateControllerOptionsButton.OnPointerHeld.AddListener(Delete);
            SetControllerOptionsActive(false);

            _activationToggle = ActivateControllerOptionsButton.GetComponentInChildren<Toggle>();
            _activationToggle.onValueChanged.AddListener(ToggleControlVisibility);
            _activationToggle.SetIsOnWithoutNotify(config.GetEnabled());
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
            Destroy(ActivateControllerOptionsButton.gameObject);
            Destroy(_controlObject);
        }
    }
}
