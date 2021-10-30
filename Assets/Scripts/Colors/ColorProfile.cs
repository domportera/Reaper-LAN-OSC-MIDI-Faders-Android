using UnityEngine;

[System.Serializable]
public class ColorProfile
{
	[SerializeField] protected string name;
	[SerializeField] protected Color background;
	[SerializeField] protected Color secondary;
	[SerializeField] protected Color primary;
	[SerializeField] protected Color tertiary;

	public string Name { get { return name; } }


	public ColorProfile()
	{
		background = Color.black;
		primary = new Color(.16f, .15f, .34f);
		secondary = new Color(.43f, 0.45f, 0.87f);
		tertiary = secondary;
	}

	/// <summary>
	/// Create a duplicate color profile with a new name
	/// </summary>
	public ColorProfile(ColorProfile _template, string _name = null)
	{
		name = _name == null ? _template.name : _name;
		background = _template.background;
		primary = _template.primary;
		secondary = _template.secondary;
		tertiary = _template.tertiary;
	}

	public ColorProfile(string name, Color background, Color primary, Color secondary, Color tertiary)
	{
		this.name = name;
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

	public static bool Equals(ColorProfile _a, ColorProfile _b)
	{
		return
		!(
			(_a.GetColor(ColorType.Background) != _b.GetColor(ColorType.Background)) ||
			(_a.GetColor(ColorType.Primary) != _b.GetColor(ColorType.Primary))		 ||
			(_a.GetColor(ColorType.Secondary) != _b.GetColor(ColorType.Secondary))	 ||
			(_a.GetColor(ColorType.Tertiary) != _b.GetColor(ColorType.Tertiary))
		);
    }

	protected void SetName(string _name)
    {
		name = _name;
    }

	public enum ColorType { Background, Primary, Secondary, Tertiary }

}


[System.Serializable]
public class ColorPreset : ColorProfile 
{
	public static ColorPreset ProfileToPreset(ColorProfile _profile, string _name = null)
	{
		ColorPreset preset = new ColorPreset();
		preset.background = _profile.GetColor(ColorType.Background);
		preset.primary = _profile.GetColor(ColorType.Primary);
		preset.secondary = _profile.GetColor(ColorType.Secondary);
		preset.tertiary = _profile.GetColor(ColorType.Tertiary);

		preset.SetName(_name == null ? _profile.Name : _name);

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

	public static ColorProfile BuiltInPresetToProfile(ColorPresetBuiltIn _preset)
    {
		return new ColorProfile(_preset.name, _preset.background, _preset.primary, _preset.secondary, _preset.tertiary);
    }
}
