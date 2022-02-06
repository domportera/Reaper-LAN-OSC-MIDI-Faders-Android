using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DomsUnityHelper;

public abstract class OptionsMenu : MonoBehaviourExtended
{
    [SerializeField] bool toggleOptionParent;
    protected Dictionary<Dropdown, string[]> dropDownEntryNames = new Dictionary<Dropdown, string[]>();


    protected void ToggleUIObject(Selectable _object, bool _on)
    {
        ToggleUIObject(_object.gameObject, _on);
    }

    protected void ToggleUIObject(GameObject _object, bool _on)
    {
        ToggleUIObject(_object.transform, _on);
    }

    protected void ToggleUIObject(Transform _object, bool _on)
    {
        if(toggleOptionParent)
        {
            _object.parent.gameObject.SetActive(_on);
        }
        else
        {
            _object.gameObject.SetActive(_on);
        }
    }
}
