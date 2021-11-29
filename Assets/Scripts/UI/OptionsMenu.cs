using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class OptionsMenu : MonoBehaviourExtended
{
    [SerializeField] bool toggleOptionParent;
    protected Dictionary<Dropdown, string[]> dropDownEntryNames = new Dictionary<Dropdown, string[]>();


    protected void ToggleUIObject(Selectable _object, bool _on)
    {
        if(toggleOptionParent)
        {
            _object.transform.parent.gameObject.SetActive(_on);
        }
        else
        {
            _object.gameObject.SetActive(_on);
        }
    }
}
