using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DomsUnityHelper;
using UnityEngine.Serialization;

public abstract class OptionsMenu : MonoBehaviourExtended
{
    [FormerlySerializedAs("toggleOptionParent")] [SerializeField] bool _toggleOptionParent;
    protected readonly Dictionary<Dropdown, string[]> DropDownEntryNames = new ();

    protected void ToggleUIObject(Selectable selectable, bool on)
    {
        ToggleUIObject(selectable.gameObject, on);
    }

    protected void ToggleUIObject(GameObject selectable, bool on)
    {
        ToggleUIObject(selectable.transform, on);
    }

    protected void ToggleUIObject(Transform selectable, bool on)
    {
        if(_toggleOptionParent)
        {
            selectable.parent.gameObject.SetActive(on);
        }
        else
        {
            selectable.gameObject.SetActive(on);
        }
    }
}
