using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProfileLoader : MonoBehaviour
{
    [SerializeField] GameObject highlightImage;
    [SerializeField] Text titleText;
    [SerializeField] ButtonExtended button;
    [SerializeField] GameObject root;

    bool active;
    public bool isActiveProfile { get { return active; } set { active = value; } }

    public void ToggleHighlight(bool _enabled)
    {
        highlightImage.SetActive(_enabled);
	}

    public void SetText(string _profileName)
    {
        titleText.text = _profileName;
	}

    public void SetButtonActions(UnityAction _buttonAction, UnityAction _holdAction)
    {
        button.onClick.AddListener(_buttonAction);
        button.OnPointerHeld.AddListener(_holdAction);
	}

	public void Annihilate()
	{
        Destroy(root);
	}

    public string GetName()
    {
        return titleText.text;
    }
}
