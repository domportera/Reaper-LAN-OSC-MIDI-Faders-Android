using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSetter : MonoBehaviour
{
    [SerializeField]
    ColorProfile.ColorType colorType;

    Image image;
	Text text;

	bool canColor = true;
	bool isImage = false;

	private void Awake()
	{
		GetColoredComponents();
	}

	private void Start()
	{
		ColorController.AddToControls(this);
	}

	private void OnDestroy()
	{
		ColorController.RemoveFromControls(this);
	}

	public void SetColors(ColorProfile _colors)
	{
		if (!canColor) return;

		if (!isImage)
		{
			text.color = _colors.GetColor(colorType);
		}
		else
		{
			image.color = _colors.GetColor(colorType);
		}
	}

	void GetColoredComponents()
    {
		text = GetComponent<Text>();
		if (text == null)
		{
			image = GetComponent<Image>();
			if (image == null)
			{
				Debug.LogWarning($"{name} has no component to color", this);
				canColor = false;
			}
			else
			{
				canColor = true;
				isImage = true;
			}
		}
	}
}
