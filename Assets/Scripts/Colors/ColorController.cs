using System;
using System.Collections.Generic;
using UnityEngine;

namespace Colors
{
    public static class ColorController
    {
        #region Profile Variables

        private static ColorProfile _currentColorProfile;
        private static readonly List<ColorSetter> ColorSetters = new();

        public static event Action ColorsLoaded;
        public static event Action PresetApplied;

        public static ColorProfile CurrentColorProfile
        {
            get
            {
                if(_currentColorProfile != null)
                    return _currentColorProfile;

                // calls set below
                CurrentColorProfile = ColorProfileDataHandler.LoadColorProfile(ControlsManager.ActiveProfile.Name);
                return _currentColorProfile;
            }
            private set {
                _currentColorProfile = value ?? throw new NullReferenceException("Current Color Profile cannot be set to null!");
                UpdateAppColors();
                Debug.Log($"Set Current Color Profile: {_currentColorProfile.Name}");
            }
        }

        #endregion Color Profile Variables

        static ColorController()
        {
            ProfilesManager.ProfileSaved += SaveCurrentColorsWithProfileName;
            ProfilesManager.ProfileChanged += LoadAndSetColorProfile;
        }
        
        public static void AddToControls(ColorSetter setter)
        {
            ColorSetters.Add(setter);
            setter.SetColors(CurrentColorProfile);
        }

        public static void RemoveFromControls(ColorSetter setter)
        {
            ColorSetters.Remove(setter);
        }

        private static void UpdateAppColors()
        {
            foreach (var c in ColorSetters)
            {
                try
                {
                    c.SetColors(CurrentColorProfile);
                }
                catch (Exception e)
                {
                    var obj = c.gameObject;
                    Debug.LogError($"Error setting colors of {obj.name}\n{e}", obj);
                }
            }
        }

        public static void UpdateColorProfile(ColorType type, Color color)
        {
            CurrentColorProfile.SetColor(type, color);
            UpdateAppColors();
        }

        public static void ReloadColorProfile()
        {
            CurrentColorProfile = ColorProfileDataHandler.LoadColorProfile(CurrentColorProfile.Name);
        }

        private static void LoadAndSetColorProfile(ProfileSaveData profile)
        {
            var profileName = profile.Name;
            CurrentColorProfile = ColorProfileDataHandler.LoadColorProfile(profileName);
            Debug.Log($"Loaded Colors: {CurrentColorProfile.Name}\n{ColorProfile.DebugColorProfile(CurrentColorProfile)}");
            ColorsLoaded?.Invoke();
        }

        private static void SaveCurrentColorsWithProfileName(ProfileSaveData profile)
        {
            var profileName = profile.Name;
            
            // only save colors along with profiles if the current color set has been changed from default
            var hasNewColors = !ColorProfileDataHandler.ColorProfileIsDefault(CurrentColorProfile);
            if (!hasNewColors)
                return;

            var colorProfile = CurrentColorProfile;
            if (profileName != CurrentColorProfile.Name)
            {
                colorProfile = new ColorProfile(CurrentColorProfile, profileName);
                CurrentColorProfile = colorProfile;
            }
            
            ColorProfileDataHandler.SaveProfile(colorProfile);
        }

        public static void SetColorsFromPreset(ColorProfile preset)
        {
            foreach (var t in (ColorType[])Enum.GetValues(typeof(ColorType)))
            {
                CurrentColorProfile.SetColor(t, preset.GetColor(t));
            }

            UpdateAppColors();
            PresetApplied?.Invoke();
        }

        public static void SetProfileAsDefault()
        {
            ColorProfileDataHandler.SetProfileDefault(CurrentColorProfile);
        }

        public static void SaveProfile()
        {
            ColorProfileDataHandler.SaveProfile(CurrentColorProfile);
        }
    }
}