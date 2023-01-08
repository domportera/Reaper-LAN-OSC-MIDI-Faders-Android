using System.IO;
using DomsUnityHelper;
using PopUpWindows;
using UnityEngine;

namespace Colors
{
    public static class ColorProfileDataHandler
    {        
        static readonly string ProfilesBasePath = Path.Combine(Application.persistentDataPath, ProfileFolder, "Profiles");

        const string ProfileFolder = "Colors";
        const string DefaultColorProfileName = ProfilesManager.DefaultSaveName + " Colors";
        const string FileExtensionProfiles = ".color";

        internal static void SaveDefaultProfile(ColorProfile template)
        {
            ColorProfile defaultColorProfile = new (template, DefaultColorProfileName);
            SaveProfile(defaultColorProfile, true);
        }

        internal static void SaveColorProfileByNewName(string profileName, ColorProfile profileToSave)
        {
            ColorProfile profile = new (profileToSave, profileName);
            SaveProfile(profile);
        }

        const string SavedOverDefaultErrorMsg =
            "Can't save over the Default profile. If you'd like to set the default color palette that will be loaded on " +
            "this and any new profile you create, click \"Set as Default Color Scheme\"";
        public static void SaveProfile(ColorProfile colorProfile, bool savingDefault = false)
        {
            if (!savingDefault && colorProfile.Name == DefaultColorProfileName)
            {
                PopUpController.Instance.ErrorWindow(SavedOverDefaultErrorMsg);
                return;
            }

            bool saved = FileHandler.SaveJsonObject(colorProfile, ProfilesBasePath, colorProfile.Name, FileExtensionProfiles);

            if (!saved)
            {
                PopUpController.Instance.ErrorWindow( $"Error saving colors for profile {colorProfile.Name}. Check Log for details.");
                return;
            }

            PopUpController.Instance.QuickNoticeWindow(colorProfile.Name != DefaultColorProfileName
                ? $"Saved color profile for {colorProfile.Name}!"
                : "Set default colors!");
        }

        internal static bool ColorProfileIsDefault(ColorProfile profile, BuiltInColorPresets presetsBuiltIn)
        {
            ColorProfile defaultProfile = GetDefaultColorProfile(presetsBuiltIn);
            return ColorProfile.Equals(profile, defaultProfile);
        }

        static ColorProfile GetDefaultColorProfile(BuiltInColorPresets presetsBuiltIn)
        {
            ColorProfile defaultProfile = FileHandler.LoadJsonObject<ColorProfile>(
                ProfilesBasePath, DefaultColorProfileName, FileExtensionProfiles);

            return defaultProfile ?? presetsBuiltIn.Default.ToReferenceType();
        }

        internal static ColorProfile LoadColorProfile(string profileName, BuiltInColorPresets presetsBuiltIn)
        {
            if (profileName == ProfilesManager.DefaultSaveName)
            {
                profileName = DefaultColorProfileName;
            }

            var colorProfile = FileHandler.LoadJsonObject<ColorProfile>(ProfilesBasePath, profileName, FileExtensionProfiles);
            return colorProfile ?? new ColorProfile(GetDefaultColorProfile(presetsBuiltIn), profileName);
        }

    }
}