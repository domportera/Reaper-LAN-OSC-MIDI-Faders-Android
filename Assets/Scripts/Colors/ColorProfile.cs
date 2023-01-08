﻿using UnityEngine;
using UnityEngine.Serialization;

namespace Colors
{
	[System.Serializable]
	public class ColorProfile
	{
		[FormerlySerializedAs("Name")] [SerializeField] protected string ProfileName;
		[SerializeField] protected Color Background;
		[SerializeField] protected Color Secondary;
		[SerializeField] protected Color Primary;
		[SerializeField] protected Color Tertiary;

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
		public ColorProfile(ColorProfile template, string name = null)
		{
			ProfileName = name ?? template.ProfileName;
			Background = template.Background;
			Primary = template.Primary;
			Secondary = template.Secondary;
			Tertiary = template.Tertiary;
		}

		public ColorProfile(string name, Color background, Color primary, Color secondary, Color tertiary)
		{
			ProfileName = name;
			Background = background;
			Primary = primary;
			Secondary = secondary;
			Tertiary = tertiary;
		}

		internal void SetColor(ColorType type, Color color)
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
					Debug.LogError($"Tried to set {nameof(type)} color in profile, but no implementation exists!");
					break;
			}
		}

		internal Color GetColor(ColorType type)
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
					Debug.LogError($"Tried to get {nameof(type)} color in profile, but no implementation exists!");
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

		protected void SetName(string name)
		{
			ProfileName = name;
		}


	}

	public enum ColorType { Background, Primary, Secondary, Tertiary }

	[System.Serializable]
	internal struct ColorProfileStruct
	{
		[SerializeField] internal string Name;
		[SerializeField] internal Color Background;
		[SerializeField] internal Color Primary;
		[SerializeField] internal Color Secondary;
		[SerializeField] internal Color Tertiary;

		internal ColorProfile ToReferenceType() => new(Name, Background, Primary, Secondary, Tertiary);
	}
}