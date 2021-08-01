using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProfileLoadButton : MonoBehaviour
{
    [SerializeField] GameObject highlightImage;
    [SerializeField] Text titleText;
    [SerializeField] Button button;
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

    public void SetButtonAction(UnityAction _buttonAction)
    {
        button.onClick.AddListener(_buttonAction);
	}

	public void Annihilate()
	{
        Destroy(root);
	}
}
