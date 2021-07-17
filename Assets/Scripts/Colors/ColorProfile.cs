using UnityEngine;

[System.Serializable]
public class ColorProfile
{
	public string name { get; private set; }
    public ColorSerialized background { get; private set; }
    public ColorSerialized faderBackground { get; private set; }
	public ColorSerialized faderHandle { get; private set; }
	public ColorSerialized text { get; private set; }
	public ColorSerialized scrollHandle { get; private set; }
	public ColorSerialized scrollBackground { get; private set; }
	public ColorSerialized button { get; private set; }

	//default colors
	public static ColorProfile NewDefaultColorProfile(string _name)
	{
		Color background = Color.black;

		Color color1 = new Color(.16f, .15f, .34f);
		Color faderBackground = color1;
		Color scrollBackground = color1;
		Color button = color1;

		Color color2 = new Color(.43f, 0.45f, 0.87f);
		Color faderHandle = color2;
		Color text = color2;
		Color scrollHandle = color2;

		string name = _name;

		return new ColorProfile(name, background, faderBackground, faderHandle, text, scrollHandle, scrollBackground, button);
	}

	public static string DebugColorProfile(ColorProfile _profile, bool _debugLog = false)
	{
		string s = "";
		s += "Name: " + _profile.name;
		s += "\nBackground: " + ColorSerialized.ConvertColor(_profile.background);
		s += "\nFader Background: " + ColorSerialized.ConvertColor(_profile.faderBackground);
		s += "\nFader Handle: " + ColorSerialized.ConvertColor(_profile.faderHandle);
		s += "\nText: " + ColorSerialized.ConvertColor(_profile.text);
		s += "\nScroll Handle: " + ColorSerialized.ConvertColor(_profile.scrollHandle);
		s += "\nScroll Background: " + ColorSerialized.ConvertColor(_profile.scrollBackground);
		s += "\nButton: " + ColorSerialized.ConvertColor(_profile.button);

		if(_debugLog)
		{
			Debug.Log(s);
		}

		return s;
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
		this.background = ColorSerialized.ConvertColor(background);
		this.faderBackground = ColorSerialized.ConvertColor(faderBackground);
		this.faderHandle = ColorSerialized.ConvertColor(faderHandle);
		this.text = ColorSerialized.ConvertColor(text);
		this.scrollHandle = ColorSerialized.ConvertColor(scrollHandle);
		this.scrollBackground = ColorSerialized.ConvertColor(scrollBackground);
		this.button = ColorSerialized.ConvertColor(button);
	}

	public void SetColor(ColorType _type, Color _color)
	{
		switch (_type)
		{
			case ColorType.Background:
				background = ColorSerialized.ConvertColor(_color);
				break;
			case ColorType.FaderBackground:
				faderBackground = ColorSerialized.ConvertColor(_color);
				break;
			case ColorType.FaderHandle:
				faderHandle = ColorSerialized.ConvertColor(_color);
				break;
			case ColorType.Text:
				text = ColorSerialized.ConvertColor(_color);
				break;
			case ColorType.ScrollHandle:
				scrollHandle = ColorSerialized.ConvertColor(_color);
				break;
			case ColorType.ScrollBackground:
				scrollBackground = ColorSerialized.ConvertColor(_color);
				break;
			case ColorType.Button:
				button = ColorSerialized.ConvertColor(_color);
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
				return ColorSerialized.ConvertColor(background);
			case ColorType.FaderBackground:
				return ColorSerialized.ConvertColor(faderBackground);
			case ColorType.FaderHandle:
				return ColorSerialized.ConvertColor(faderHandle);
			case ColorType.Text:
				return ColorSerialized.ConvertColor(text);
			case ColorType.ScrollHandle:
				return ColorSerialized.ConvertColor(scrollHandle);
			case ColorType.ScrollBackground:
				return ColorSerialized.ConvertColor(scrollBackground);
			case ColorType.Button:
				return ColorSerialized.ConvertColor(button);
			default:
				Debug.LogError($"Tried to get {_type} color in profile, but no implementation exists!");
				return Color.white;
		}
	}

	public enum ColorType { Background, FaderBackground, FaderHandle, Text, ScrollHandle, ScrollBackground, Button }

	[System.Serializable]
	public class ColorSerialized
	{
		public float r;
		public float g;
		public float b;
		public float a;

		public ColorSerialized(Color _color)
		{
			r = _color.r;
			g = _color.g;
			b = _color.b;
			a = _color.a;
		}

		public static ColorSerialized ConvertColor(Color _color)
		{
			return new ColorSerialized(_color);
		}

		public static Color ConvertColor(ColorSerialized _color)
		{
			Color c = new Color();
			c.r = _color.r;
			c.g = _color.g;
			c.b = _color.b;
			c.a = _color.a;
			return c;
		}
	}
}
