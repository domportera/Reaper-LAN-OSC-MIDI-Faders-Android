using System;
using System.IO;
using PopUpWindows;
using UnityEngine;

namespace Colors
{
    public static class ColorProfileDataHandler
    {
        private const string ProfileFolder = "Colors";
        private const string PresetsFolder = "Presets";
        private const string FileExtension = ".json";

        private static string ColorDirectory => Path.Combine(Application.persistentDataPath, "Colors");
        private static string ProfilesDirectory => Path.Combine(ColorDirectory, ProfileFolder);
        private static string PresetsDirectory => Path.Combine(ColorDirectory, PresetsFolder);
        public static readonly BuiltInColorPresets BuiltInColorPresets;
        private const string ProfileMetadataName = "Metadata";


        private static ProfilesMetadata _profilesMetadata;

        private static ProfilesMetadata Metadata
        {
            get
            {
                if (_profilesMetadata != null)
                    return _profilesMetadata;
                
                _profilesMetadata = FileHandler.LoadJsonObject<ProfilesMetadata>(ProfilesDirectory, ProfileMetadataName, ".json");

                if (_profilesMetadata == null)
                {
                    _profilesMetadata = new ProfilesMetadata();
                    FileHandler.SaveJsonObject(_profilesMetadata, ProfilesDirectory, ProfileMetadataName);
                }

                return _profilesMetadata;
            }
        }

        static ColorProfileDataHandler()
        {
            BuiltInColorPresets = Resources.Load<BuiltInColorPresets>("BuiltInColorPresets");
        }

        public static void SaveProfile(ColorProfile colorProfile)
        {
            if (!TryValidateName(colorProfile.Name))
            {
                return;
            }

            SaveAndShowWindow(colorProfile, ProfilesDirectory);
        }

        internal static bool ColorProfileIsDefault(ColorProfile profile)
        {
            var defaultProfile = GetDefaultColorProfile();
            return ColorProfile.Equals(profile, defaultProfile);
        }

        private static ColorProfile GetDefaultColorProfile()
        {
            var defaultProfile = FileHandler.LoadJsonObject<ColorProfile>(
                ProfilesDirectory, Metadata.DefaultProfileName, FileExtension);

            return defaultProfile ?? BuiltInColorPresets[0];
        }

        internal static ColorProfile LoadColorProfile(string profileName)
        {
            if (profileName == ProfilesManager.DefaultSaveName)
            {
                profileName = Metadata.DefaultProfileName;
            }

            var colorProfile =
                FileHandler.LoadJsonObject<ColorProfile>(ProfilesDirectory, profileName, FileExtension);
            return colorProfile ?? new ColorProfile(GetDefaultColorProfile(), profileName);
        }


        [Serializable]
        private class ProfilesMetadata
        {
            [SerializeField] private string _defaultProfileName = "DefaultColorProfile";

            public string DefaultProfileName
            {
                get => _defaultProfileName;
                set => _defaultProfileName = value;
            }
        }

        #region Presets

        internal static string[] GetPresetNames()
        {
            if (!Directory.Exists(PresetsDirectory))
            {
                return Array.Empty<string>();
            }

            var fileNames = Directory.GetFiles(PresetsDirectory, "*" + FileExtension);

            for (var i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
            }

            return fileNames;
        }

        private static bool DoesPresetExist(string presetName)
        {
            var fileNames = GetPresetNames();

            foreach (var s in fileNames)
            {
                if (s == presetName)
                {
                    return true;
                }
            }

            return false;
        }

        internal static ColorProfile LoadPreset(string presetName)
        {
            var preset = FileHandler.LoadJsonObject<ColorProfile>(PresetsDirectory, presetName, FileExtension);

            if (preset != null)
            {
                return preset;
            }
            else
            {
                Debug.LogError($"No preset found named {presetName} in {PresetsDirectory}");
                return ColorProfile.NewDefaultColorProfile(presetName);
            }
        }

        internal static void SavePreset(string presetName, ColorProfile profileToSave, Action<ColorProfile> onSave)
        {
            if (DoesPresetExist(presetName))
            {
                PopUpController.Instance.ErrorWindow("Preset with this name already exists, please use another.");
                return;
            }

            if (!TryValidateName(presetName))
            {
                return;
            }

            var newPreset = new ColorProfile(profileToSave, presetName);
            if (SaveAndShowWindow(newPreset, PresetsDirectory))
                onSave?.Invoke(newPreset);
        }

        private static bool SaveAndShowWindow(ColorProfile profileToSave, string directory)
        {
            if (FileHandler.SaveJsonObject(profileToSave, directory, profileToSave.Name))
            {
                PopUpController.Instance.QuickNoticeWindow($"Saved {profileToSave.Name}");
                return true;
            }

            PopUpController.Instance.ErrorWindow(
                $"Error saving {profileToSave.Name}. Check the Log for details.");

            return false;
        }

        private static bool TryValidateName(string name)
        {
            var invalidChars = FileHandler.GetInvalidFileNameCharactersIn(name);
            if (invalidChars.Count > 0)
            {
                PopUpController.Instance.ErrorWindow(invalidChars.Count == 1
                    ? $"Chosen name contains an invalid character."
                    : $"Chosen name contains {invalidChars.Count.ToString()} invalid characters.");

                return false;
            }

            return true;
        }

        internal static void DeletePreset(ColorProfile preset, Action onDeleted)
        {
            var presetName = preset.Name;

            var deleted = FileHandler.DeleteFile(PresetsDirectory, presetName, FileExtension);

            if (deleted)
            {
                onDeleted?.Invoke();
                PopUpController.Instance.QuickNoticeWindow($"{presetName} preset deleted!");
            }
            else
            {
                Debug.LogError($"No preset found to delete with name {presetName}");
                PopUpController.Instance.ErrorWindow($"Error deleting preset {presetName}. Check the log for details.");
            }
        }

        #endregion

        public static void SetProfileDefault(ColorProfile currentColorProfile)
        {
            Metadata.DefaultProfileName = currentColorProfile.Name;
            Directory.CreateDirectory(ColorDirectory);
            if (FileHandler.SaveJsonObject(Metadata, ProfilesDirectory, ProfileMetadataName))
                return;

            PopUpController.Instance.ErrorWindow("Error setting default profile. Check the log for details.");
        }
    }
}