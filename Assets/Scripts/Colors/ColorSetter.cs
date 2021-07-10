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
	bool textIsImage = false;

	private void Awake()
	{
		switch (colorType)
		{
			case ColorProfile.ColorType.Text:
				text = GetComponent<Text>();
				if (text == null)
				{
					Debug.LogWarning($"{name} has no text component", this);
					image = GetComponent<Image>();
					if (image == null)
					{
						Debug.LogWarning($"{name} has no image component for text substitution", this);
						canColor = false;
					}
					else
					{
						canColor = true;
						textIsImage = true;
					}
				}
				break;
			default:
				image = GetComponent<Image>();
				if (image == null)
				{
					Debug.LogWarning($"{name} has no image component", this);
					canColor = false;
				}
				break;
		}
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

		switch (colorType)
		{
			case ColorProfile.ColorType.Background:
				image.color = _colors.background;
				break;
			case ColorProfile.ColorType.FaderBackground:
				image.color = _colors.faderBackground;
				break;
			case ColorProfile.ColorType.FaderHandle:
				image.color = _colors.faderHandle;
				break;
			case ColorProfile.ColorType.Text:;
				if (!textIsImage)
				{
					text.color = _colors.text;
				}
				else
				{
					image.color = _colors.text;
				}
				break;
			case ColorProfile.ColorType.ScrollHandle:
				image.color = _colors.scrollHandle;
				break;
			case ColorProfile.ColorType.ScrollBackground:
				image.color = _colors.scrollBackground;
				break;
			case ColorProfile.ColorType.Button:
				image.color = _colors.button;
				break;
			default:
				Debug.LogError($"Color Type {colorType} not handled in ColorSetter");
				break;
		}
	}
}
