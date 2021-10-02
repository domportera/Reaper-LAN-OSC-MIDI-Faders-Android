using UnityEngine;

[System.Serializable]
public class ColorProfile
{
	public string name;
	public Color background;
	public Color secondary;
	public Color primary;
	public Color tertiary;


	public ColorProfile()
	{
		background = Color.black;
		primary = new Color(.16f, .15f, .34f);
		secondary = new Color(.43f, 0.45f, 0.87f);
		tertiary = secondary;
	}

	//duplicate
	public ColorProfile(ColorProfile _template, string _name = null)
	{
		if (_name == null)
		{
			this.name = _template.name;
		}
		else
        {
			this.name = _name;
        }

		this.background = _template.background;
		this.primary = _template.primary;
		this.secondary = _template.secondary;
		this.tertiary = _template.tertiary;
	}

	public ColorProfile(string _name, ColorProfile _template)
	{
		this.background = _template.background;
		this.primary = _template.primary;
		this.secondary = _template.secondary;
		this.tertiary = _template.tertiary;
		this.name = _name;
	}

	public ColorProfile(string _name, Color background, Color primary, Color secondary, Color tertiary)
	{
		this.name = _name;
		this.background = background;
		this.primary = primary;
		this.secondary = secondary;
		this.tertiary = tertiary;
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
			case ColorType.Tertiary:
				tertiary = _color;
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
			case ColorType.Primary:
				return primary;
			case ColorType.Secondary:
				return secondary;
			case ColorType.Tertiary:
				return tertiary;
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
		s += "\nTertiary: " + _profile.tertiary;

		if (_debugLog)
		{
			Debug.Log(s);
		}

		return s;
	}

	public enum ColorType { Background, Primary, Secondary, Tertiary }

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
		preset.tertiary = _profile.tertiary;
		return preset;
	}

	public static ColorPreset BuiltInToPreset(ColorPresetBuiltIn _preset)
    {
		ColorPreset preset = new ColorPreset();
		preset.background = _preset.background;
		preset.primary = _preset.primary;
		preset.secondary = _preset.secondary;
		preset.tertiary = _preset.tertiary;
		preset.name = _preset.name;
		return preset;
	}
}

[System.Serializable]
public struct ColorPresetBuiltIn
{
	public string name;
	public Color background;
	public Color primary;
	public Color secondary;
	public Color tertiary;
}
