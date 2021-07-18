using UnityEngine;

[System.Serializable]
public class ColorProfile
{
	public string name;
	public Color background;
	public Color faderBackground;
	public Color faderHandle;
	public Color text;
	public Color scrollHandle;
	public Color scrollBackground;
	public Color button;


	public ColorProfile()
	{
		background = Color.black;

		Color color1 = new Color(.16f, .15f, .34f);
		faderBackground = color1;
		scrollBackground = color1;
		button = color1;

		Color color2 = new Color(.43f, 0.45f, 0.87f);
		faderHandle = color2;
		text = color2;
		scrollHandle = color2;
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

	//default colors
	public static ColorProfile NewDefaultColorProfile(string _name)
	{
		ColorProfile colorProfile = new ColorProfile();
		colorProfile.name = _name;

		return colorProfile;
	}

	public static string DebugColorProfile(ColorProfile _profile, bool _debugLog = false)
	{
		string s = "";
		s += "Name: " + _profile.name;
		s += "\nBackground: " + _profile.background;
		s += "\nFader Background: " + _profile.faderBackground;
		s += "\nFader Handle: " + _profile.faderHandle;
		s += "\nText: " + _profile.text;
		s += "\nScroll Handle: " + _profile.scrollHandle;
		s += "\nScroll Background: " + _profile.scrollBackground;
		s += "\nButton: " + _profile.button;

		if (_debugLog)
		{
			Debug.Log(s);
		}

		return s;
	}

	public enum ColorType { Background, FaderBackground, FaderHandle, Text, ScrollHandle, ScrollBackground, Button }

}


[System.Serializable]
public class ColorPreset : ColorProfile 
{
	public static ColorPreset ProfileToPreset(ColorProfile _profile)
	{
		ColorPreset preset = new ColorPreset();
		preset.background = _profile.background;
		preset.button = _profile.button;
		preset.faderBackground = _profile.faderBackground;
		preset.faderHandle = _profile.faderHandle;
		preset.scrollBackground = _profile.scrollBackground;
		preset.scrollHandle = _profile.scrollHandle;
		preset.text = _profile.text;
		return preset;
	}
}
