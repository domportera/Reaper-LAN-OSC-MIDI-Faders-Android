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
			case ColorProfile.ColorType.Text:;
				if (!textIsImage)
				{
					text.color = _colors.GetColor(colorType);
				}
				else
				{
					image.color = _colors.GetColor(colorType);
				}
				break;
			default:
				image.color = _colors.GetColor(colorType);
				break;
		}
	}
}
