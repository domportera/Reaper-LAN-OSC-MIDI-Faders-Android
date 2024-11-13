using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Colors
{
    public static class ColorController
    {
        #region Profile Variables
        static ColorProfile _currentColorProfile;
        static readonly List<ColorSetter> ColorSetters = new();

        public static event Action ColorsLoaded;

        static BuiltInColorPresets _builtInColorPresets;

        internal static BuiltInColorPresets BuiltInPresets =>
            _builtInColorPresets ??= BuiltInColorPresets.Instance;

        public static ColorProfile CurrentColorProfile
        {
            get => _currentColorProfile ?? throw new NullReferenceException("Current Color Profile is null!");
            set {
                if(value == null)
                    throw new NullReferenceException("Current Color Profile cannot be set to null!");
                _currentColorProfile = value;
                Debug.Log($"Set Current Color Profile: {_currentColorProfile.Name}");
            }
        }

        #endregion Color Profile Variables
        
        public static void AddToControls(ColorSetter setter)
        {
            ColorSetters.Add(setter);

            UpdateAppColors(setter);
        }

        public static void RemoveFromControls(ColorSetter setter)
        {
            ColorSetters.Remove(setter);
        }

        public static void UpdateAppColors()
        {
            foreach (ColorSetter c in ColorSetters)
            {
                try
                {
                    c.SetColors(CurrentColorProfile);
                }
                catch (Exception e)
                {
                    GameObject obj = c.gameObject;
                    Debug.LogError($"Error setting colors of {obj.name}\n{e}", obj);
                }
            }
        }

        public static void UpdateAppColors(ColorSetter setter)
        {
            setter.SetColors(CurrentColorProfile);
        }

        public static void UpdateColorProfile(ColorType type, Color color)
        {
            CurrentColorProfile.SetColor(type, color);
        }

        public static void RevertColorProfile()
        {
            ColorProfileDataHandler.LoadColorProfile(CurrentColorProfile.Name, BuiltInPresets);
            UpdateAppColors();
        }
        

        public static void LoadAndSetColorProfile(string profileName)
        {
            CurrentColorProfile = ColorProfileDataHandler.LoadColorProfile(profileName, BuiltInPresets);
            Debug.Log($"Loaded Colors: {CurrentColorProfile.Name}\n{ColorProfile.DebugColorProfile(CurrentColorProfile)}");
            UpdateAppColors();
            
        }

        public static void SaveCurrentColorsWithProfileName(string profileName)
        {
            // only save colors along with profiles if the current color set has been changed from default
            bool hasNewColors = !ColorProfileDataHandler.ColorProfileIsDefault(CurrentColorProfile, BuiltInPresets);
            if (!hasNewColors)
                return;

            ColorProfileDataHandler.SaveColorProfileByNewName(profileName, CurrentColorProfile);
        }

        public static void SetColorsFromPreset(ColorProfile preset)
        {
            foreach (ColorType t in (ColorType[])Enum.GetValues(typeof(ColorType)))
            {
                CurrentColorProfile.SetColor(t, preset.GetColor(t));
            }

            UpdateAppColors();
        }

        public static void SaveDefaultProfile()
        {
            ColorProfileDataHandler.SaveDefaultProfile(CurrentColorProfile);
        }

        public static void SaveProfile()
        {
            ColorProfileDataHandler.SaveDefaultProfile(CurrentColorProfile);
        }
    }
}