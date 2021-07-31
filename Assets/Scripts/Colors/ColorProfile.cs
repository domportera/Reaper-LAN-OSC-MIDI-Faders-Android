using UnityEngine;

[System.Serializable]
public class ColorProfile
{
	public string name;
	public Color background;
	public Color secondary;
	public Color primary;


	public ColorProfile()
	{
		background = Color.black;
		primary = new Color(.16f, .15f, .34f);
		secondary = new Color(.43f, 0.45f, 0.87f);
	}

	//duplicate
	public ColorProfile(ColorProfile _template)
	{
		this.name = _template.name;
		this.background = _template.background;
		this.secondary = _template.secondary;
		this.primary = _template.primary;
	}

	public ColorProfile(string _name, ColorProfile _template)
	{
		this.background = _template.background;
		this.secondary = _template.secondary;
		this.primary = _template.primary;

		this.name = _name;
	}

	public ColorProfile(string _name, Color background, Color faderBackground, Color faderHandle, Color text, Color scrollHandle, Color scrollBackground, Color button)
	{
		this.name = _name;
		this.background = background;
		this.secondary = text;
		this.primary = button;
	}

	public void SetColor(ColorType _type, Color _color)
	{
		switch (_type)
		{
			case ColorType.Background:
				background = _color;
				break;
			case ColorType.Secondary:
				secondary = _color;
				break;
			case ColorType.Primary:
				primary = _color;
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
			case ColorType.Secondary:
				return secondary;
			case ColorType.Primary:
				return primary;
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
		s += "\nPrimary: " + _profile.primary;
		s += "\nSecondary: " + _profile.secondary;

		if (_debugLog)
		{
			Debug.Log(s);
		}

		return s;
	}

	public enum ColorType { Background, Primary, Secondary }

}


[System.Serializable]
public class ColorPreset : ColorProfile 
{
	public static ColorPreset ProfileToPreset(ColorProfile _profile)
	{
		ColorPreset preset = new ColorPreset();
		preset.background = _profile.background;
		preset.primary = _profile.primary;
		preset.secondary = _profile.secondary;
		return preset;
	}
}
