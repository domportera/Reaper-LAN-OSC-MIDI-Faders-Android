using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProfileLoadButton : MonoBehaviour
{
    [SerializeField] Image highlightImage;
    [SerializeField] Text titleText;
    [SerializeField] Button button;
    [SerializeField] GameObject root;

    bool active;
    public bool isActiveProfile { get { return active; } set { active = value; } }

    public void SetHighlightColor(Color _c)
    {
        highlightImage.color = _c;
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
