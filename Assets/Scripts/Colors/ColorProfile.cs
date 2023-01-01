using UnityEngine;
using UnityEngine.Serialization;

namespace Colors
{
	[System.Serializable]
	public class ColorProfile
	{
		[FormerlySerializedAs("Name")] [FormerlySerializedAs("name")] [SerializeField] protected string ProfileName;
		[FormerlySerializedAs("background")] [SerializeField] protected Color Background;
		[FormerlySerializedAs("secondary")] [SerializeField] protected Color Secondary;
		[FormerlySerializedAs("primary")] [SerializeField] protected Color Primary;
		[FormerlySerializedAs("tertiary")] [SerializeField] protected Color Tertiary;

		public string Name => ProfileName;


		public ColorProfile()
		{
			Background = Color.black;
			Primary = new Color(.16f, .15f, .34f);
			Secondary = new Color(.43f, 0.45f, 0.87f);
			Tertiary = Secondary;
		}

		/// <summary>
		/// Create a duplicate color profile with a new name
		/// </summary>
		public ColorProfile(ColorProfile _template, string _name = null)
		{
			ProfileName = _name == null ? _template.ProfileName : _name;
			Background = _template.Background;
			Primary = _template.Primary;
			Secondary = _template.Secondary;
			Tertiary = _template.Tertiary;
		}

		public ColorProfile(string name, Color background, Color primary, Color secondary, Color tertiary)
		{
			this.ProfileName = name;
			this.Background = background;
			this.Primary = primary;
			this.Secondary = secondary;
			this.Tertiary = tertiary;
		}

		public void SetColor(ColorType type, Color color)
		{
			switch (type)
			{
				case ColorType.Background:
					Background = color;
					break;
				case ColorType.Secondary:
					Secondary = color;
					break;
				case ColorType.Primary:
					Primary = color;
					break;
				case ColorType.Tertiary:
					Tertiary = color;
					break;
				default:
					Debug.LogError($"Tried to set {type} color in profile, but no implementation exists!");
					break;
			}
		}

		public Color GetColor(ColorType type)
		{
			switch (type)
			{
				case ColorType.Background:
					return Background;
				case ColorType.Primary:
					return Primary;
				case ColorType.Secondary:
					return Secondary;
				case ColorType.Tertiary:
					return Tertiary;
				default:
					Debug.LogError($"Tried to get {type} color in profile, but no implementation exists!");
					return Color.white;
			}
		}

		//default colors
		public static ColorProfile NewDefaultColorProfile(string name)
		{
			ColorProfile colorProfile = new ColorProfile
			{
				ProfileName = name
			};
			return colorProfile;
		}

		public static string DebugColorProfile(ColorProfile profile, bool debugLog = false)
		{
			string s = "";
			s += "Name: " + profile.ProfileName;
			s += "\nBackground: " + profile.Background;
			s += "\nPrimary: " + profile.Primary;
			s += "\nSecondary: " + profile.Secondary;
			s += "\nTertiary: " + profile.Tertiary;

			if (debugLog)
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
			ProfileName = _name;
		}

		public enum ColorType { Background, Primary, Secondary, Tertiary }

	}


	[System.Serializable]
	public class ColorPreset : ColorProfile 
	{
		public static ColorPreset ProfileToPreset(ColorProfile profile, string name = null)
		{
			ColorPreset preset = new ColorPreset
			{
				Background = profile.GetColor(ColorType.Background),
				Primary = profile.GetColor(ColorType.Primary),
				Secondary = profile.GetColor(ColorType.Secondary),
				Tertiary = profile.GetColor(ColorType.Tertiary)
			};

			preset.SetName(name ?? profile.Name);

			return preset;
		}

		public static ColorPreset BuiltInToPreset(ColorPresetBuiltIn _preset)
		{
			ColorPreset preset = new()
			{
				Background = _preset.Background,
				Primary = _preset.Primary,
				Secondary = _preset.Secondary,
				Tertiary = _preset.Tertiary,
				ProfileName = _preset.Name
			};
			return preset;
		}
	}

	[System.Serializable]
	public struct ColorPresetBuiltIn
	{
		[FormerlySerializedAs("name")] public string Name;
		[FormerlySerializedAs("background")] public Color Background;
		[FormerlySerializedAs("primary")] public Color Primary;
		[FormerlySerializedAs("secondary")] public Color Secondary;
		[FormerlySerializedAs("tertiary")] public Color Tertiary;

		public static ColorProfile BuiltInPresetToProfile(ColorPresetBuiltIn preset)
		{
			return new (preset.Name, preset.Background, preset.Primary, preset.Secondary, preset.Tertiary);
		}
	}
}