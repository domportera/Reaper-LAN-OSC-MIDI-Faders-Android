using UnityEngine;

[System.Serializable]
public class ColorProfile
{
	public string name { get; private set; }
    public Color background { get; private set; }
    public Color faderBackground { get; private set; }
	public Color faderHandle { get; private set; }
	public Color text { get; private set; }
	public Color scrollHandle { get; private set; }
	public Color scrollBackground { get; private set; }
	public Color button { get; private set; }

	//default
	public ColorProfile()
	{
		this.background = Color.black;

		Color color1 = new Color(.16f, .15f, .34f);
		this.faderBackground = color1;
		this.scrollBackground = color1;
		this.button = color1;

		Color color2 = new Color(.43f, 0.45f, 0.87f);
		this.faderHandle = color2;
		this.text = color2;
		this.scrollHandle = color2;

		this.name = ControlsManager.DEFAULT_SAVE_NAME;
	}

	//duplicate
	public ColorProfile(ColorProfile _template)
	{
		this.name = _template.name;
		this.background = _template.background;
		this.faderBackground = _template.faderBackground;
		this.faderHandle = _template.faderHandle;
		this.text = _template.text;
		this.scrollHandle = _template.scrollHandle;
		this.scrollBackground = _template.scrollBackground;
		this.button = _template.button;
	}

	public ColorProfile(string _name, ColorProfile _template)
	{
		this.background = _template.background;
		this.faderBackground = _template.faderBackground;
		this.faderHandle = _template.faderHandle;
		this.text = _template.text;
		this.scrollHandle = _template.scrollHandle;
		this.scrollBackground = _template.scrollBackground;
		this.button = _template.button;

		this.name = _name;
	}

	public ColorProfile(string _name, Color background, Color faderBackground, Color faderHandle, Color text, Color scrollHandle, Color scrollBackground, Color button)
	{
		this.name = _name;
		this.background = background;
		this.faderBackground = faderBackground;
		this.faderHandle = faderHandle;
		this.text = text;
		this.scrollHandle = scrollHandle;
		this.scrollBackground = scrollBackground;
		this.button = button;
	}

	public void SetColor(ColorType _type, Color _color)
	{
		switch (_type)
		{
			case ColorType.Background:
				background = _color;
				break;
			case ColorType.FaderBackground:
				faderBackground = _color;
				break;
			case ColorType.FaderHandle:
				faderHandle = _color;
				break;
			case ColorType.Text:
				text = _color;
				break;
			case ColorType.ScrollHandle:
				scrollHandle = _color;
				break;
			case ColorType.ScrollBackground:
				scrollBackground = _color;
				break;
			case ColorType.Button:
				button = _color;
				break;
			default:
				Debug.LogError($"Tried to set {_type} color in profile, but no implementation exists!");
				break;
		}
	}

	public Color GetColor(ColorType _type)
	{
		switch (_type)
		{
			case ColorType.Background:
				return background;
			case ColorType.FaderBackground:
				return faderBackground;
			case ColorType.FaderHandle:
				return faderHandle;
			case ColorType.Text:
				return text;
			case ColorType.ScrollHandle:
				return scrollHandle;
			case ColorType.ScrollBackground:
				return scrollBackground;
			case ColorType.Button:
				return button;
			default:
				Debug.LogError($"Tried to get {_type} color in profile, but no implementation exists!");
				return Color.white;
		}
	}

	public enum ColorType { Background, FaderBackground, FaderHandle, Text, ScrollHandle, ScrollBackground, Button }
}
