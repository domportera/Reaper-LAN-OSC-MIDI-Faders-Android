using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject optionsPanel = null;
    [SerializeField] GameObject[] sliderOptionsButtonLayoutGroups = null;
    [SerializeField] GameObject faderOptionsPrefab = null;
    [SerializeField] GameObject faderOptionsActivationPrefab = null; //the prefab for the button that opens up fader options

    const int sliderButtonLayoutCapacity = 7;

    List<ControllerButtonGroup> controllerButtons = new List<ControllerButtonGroup>();
    List<LayoutGroupButtonCount> layoutCounts = new List<LayoutGroupButtonCount>();

    // Start is called before the first frame update
    void Start()
    {
        optionsPanel.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //used by options button in scene
    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeInHierarchy);
    }

    public void SpawnFaderOptions(ControllerSettings _config, GameObject _control)
    {
        //check if any other controller buttons exist for this, then destroy all its contents
        //make sure to destroy faderOptions as well
        ControllerButtonGroup dupe = GetButtonGroupByConfig(_config);
        if(dupe != null)
        {
            DestroyControllerGroup(dupe);
        }

        ControllerButtonGroup buttonGroup = new ControllerButtonGroup(_config, faderOptionsPrefab, faderOptionsActivationPrefab, _control);

        buttonGroup.faderOptions.transform.SetParent(optionsPanel.transform, false);
        controllerButtons.Add(buttonGroup);

        AddOptionsButtonToLayout(buttonGroup.faderMenuButton.gameObject);
    }

    void AddOptionsButtonToLayout(GameObject _button)
    {
        for(int i = 0; i < GetLayoutCounts().Count; i++)
        {
            if(GetLayoutCounts()[i].count < sliderButtonLayoutCapacity)
            {
                GetLayoutCounts()[i].count++;
                _button.transform.SetParent(GetLayoutCounts()[i].layoutGroup.transform);
                return;
            }
        }

        Debug.LogError("All layouts are full! Layout count: " + layoutCounts.Count); //this should actually be handled before it ever gets here, or allow layout groups to be infinite
    }

    void DestroyControllerGroup(ControllerButtonGroup _buttonGroup)
    {
        //find layout that its button is in and remove it from
        RemoveFromLayout(_buttonGroup);

        //destroy all params
        _buttonGroup.SelfDestruct();

        //remove from list
        controllerButtons.Remove(_buttonGroup);
    }

    public void DestroyControllerObjects(ControllerSettings _config)
    {
        ControllerButtonGroup buttonGroup = GetButtonGroupByConfig(_config);
        DestroyControllerGroup(buttonGroup);
    }

    void RemoveFromLayout(ControllerButtonGroup _buttonGroup)
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

    List<LayoutGroupButtonCount> GetLayoutCounts()
    {
        if(layoutCounts.Count < sliderOptionsButtonLayoutGroups.Length)
        {
            PopulateLayoutCounts();
        }

        return layoutCounts;
    }

    void PopulateLayoutCounts()
    {
        layoutCounts = new List<LayoutGroupButtonCount>();

        for (int i = 0; i < sliderOptionsButtonLayoutGroups.Length; i++)
        {
            LayoutGroupButtonCount lay = new LayoutGroupButtonCount();
            lay.layoutGroup = sliderOptionsButtonLayoutGroups[i];
            layoutCounts.Add(lay);
        }

        Debug.Log("LayoutCounts: " + layoutCounts.Count);
    }

    bool GetControllerEnabled(FaderOptions _faderOptions)
    {
        ControllerButtonGroup group = GetButtonGroupByFaderOptions(_faderOptions);

        if(group.activationToggle.isOn)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    ControllerButtonGroup GetButtonGroupByConfig(ControllerSettings _config)
    {
        foreach (ControllerButtonGroup cbg in controllerButtons)
        {
            if (cbg.controllerConfig == _config)
            {
                return cbg;
            }
        }

        //if (Time.time > 1f) //if this isn't right in the start of the app. this will always happen for new controls
        //{
        //    Debug.LogError("Didn't find a match for button group! Returning empty.");
        //}
        return null;
    }

    ControllerButtonGroup GetButtonGroupByFaderOptions(FaderOptions _faderOptions)
    {
        foreach (ControllerButtonGroup cbg in controllerButtons)
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
        foreach(LayoutGroupButtonCount lay in GetLayoutCounts())
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

    class LayoutGroupButtonCount
    {
        public GameObject layoutGroup;
        public int count = 0;
    }

    class ControllerButtonGroup
    {
        public FaderOptions faderOptions;
        public Button faderMenuButton;
        public ControllerSettings controllerConfig;
        public Toggle activationToggle;
        public GameObject controlObject;

        public ControllerButtonGroup(ControllerSettings _config, GameObject _faderOptionsPrefab, GameObject _optionsActivateButtonPrefab, GameObject _controlObject)
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
        }

        void DisplayFaderOptions()
        {
            faderOptions.gameObject.SetActive(true);
        }

        void ToggleControlVisibility(bool _b)
        {
            controlObject.SetActive(_b);
        }

        public void SelfDestruct()
        {
            Destroy(faderOptions.gameObject);
            Destroy(faderMenuButton.gameObject);
            Destroy(controlObject);
        }
    }
}
