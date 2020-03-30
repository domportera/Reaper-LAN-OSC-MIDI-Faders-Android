using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class FaderOptions : MonoBehaviour
{
    public ControllerSettings controllerConfig;
    ControlsManager manager = null;

    [SerializeField] InputField nameField = null; // must choose a unique name! check for this
    [SerializeField] InputField ccChannelField = null;
    [SerializeField] Slider smoothnessField = null;
    [SerializeField] Dropdown controlTypeDropdown = null;
    [SerializeField] Dropdown midiChannelDropdown = null;
    [SerializeField] Dropdown addressTypeDropdown = null;
    [SerializeField] Dropdown valueRangeDropdown = null;
    [SerializeField] Dropdown defaultValueDropdown = null;
    [SerializeField] Dropdown curveTypeDropdown = null;

    Dictionary<Dropdown, string[]> dropDownEntryNames = new Dictionary<Dropdown, string[]>();

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<ControlsManager>();
        gameObject.SetActive(false);

        PopulateDropdowns();

        if(controllerConfig.addressType != AddressType.CC) //this needs to be re-enabled if CC is selected from Control Type/ MIDI Parameter
        {
            ccChannelField.gameObject.SetActive(false);
        }
    }

    void PopulateDropdowns()
    {
        dropDownEntryNames.Add(controlTypeDropdown, Enum.GetNames(typeof(ControlType)));
        dropDownEntryNames.Add(addressTypeDropdown, Enum.GetNames(typeof(AddressType)));
        dropDownEntryNames.Add(valueRangeDropdown, Enum.GetNames(typeof(ValueRange)));
        dropDownEntryNames.Add(defaultValueDropdown, Enum.GetNames(typeof(DefaultValueType)));
        dropDownEntryNames.Add(curveTypeDropdown, Enum.GetNames(typeof(CurveType)));

        string[] midiChannelNames = new string[]
        {
            "All Channels",
            "Channel 1",
            "Channel 2",
            "Channel 3",
            "Channel 4",
            "Channel 5",
            "Channel 6",
            "Channel 7",
            "Channel 8",
            "Channel 9",
            "Channel 10",
            "Channel 11",
            "Channel 12",
            "Channel 13",
            "Channel 14",
            "Channel 15",
            "Channel 16"
        };

        dropDownEntryNames.Add(midiChannelDropdown, midiChannelNames);

        foreach(KeyValuePair<Dropdown, string[]> pair in dropDownEntryNames)
        {
            pair.Key.ClearOptions();
            foreach (string s in pair.Value)
            {
                pair.Key.options.Add(new OptionData(s));
            }
        }
    }

    public void ApplyAndQuit()
    {
        //validate values, else display an error

        //change values in controllerConfig


        //needs to destroy old slider prefab and create a new one
        //manager.RespawnController(controllerConfig);

        //creating a slider creates an options panel for it, so this one can be destroyed.
        Destroy(this.gameObject);
    }
}
