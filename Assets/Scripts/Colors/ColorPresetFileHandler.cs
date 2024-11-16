using System;
using System.Collections.Generic;
using System.IO;

using PopUpWindows;
using UnityEngine;

namespace Colors
{
    public static class ColorPresetDataHandler
    {
        private const string FileExtensionPresets = ".json";
        private static readonly string PresetsBasePath = Path.Combine(Application.persistentDataPath, "Colors", "Presets");

        internal static string[] GetPresetNames()
        {
            if(!Directory.Exists(PresetsBasePath))
            {
                return Array.Empty<string>();
            }

            var fileNames = Directory.GetFiles(PresetsBasePath, "*" + FileExtensionPresets);

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
            var preset = FileHandler.LoadJsonObject<ColorProfile>(PresetsBasePath, presetName, FileExtensionPresets);

            if (preset != null)
            {
                return preset;
            }
            else
            {
                Debug.LogError($"No preset found named {presetName} in {PresetsBasePath}");
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

            var invalidChars = FileHandler.GetInvalidFileNameCharactersIn(presetName);
            if (invalidChars.Count > 0)
            {
                PopUpController.Instance.ErrorWindow(invalidChars.Count == 1
                    ? $"Chosen preset name contains an invalid character."
                    : $"Chosen preset name contains {invalidChars.Count.ToString()} invalid characters.");

                return;
            }

            var saved = FileHandler.SaveJsonObject(profileToSave, PresetsBasePath, profileToSave.Name, FileExtensionPresets);

            if (saved)
            {
                PopUpController.Instance.QuickNoticeWindow($"Saved preset {profileToSave.Name}");
                onSave?.Invoke(profileToSave);
            }
            else
            {
                PopUpController.Instance.ErrorWindow($"Error saving preset {profileToSave.Name}. Check the Log for details.");
            }
        }

        internal static void DeletePreset(ColorPresetSelector selector, Action onDeleted)
        {
            var presetName = selector.Preset.Name;

            var deleted = FileHandler.DeleteFile(PresetsBasePath, presetName, FileExtensionPresets);

            if (deleted)
            {
                onDeleted?.Invoke();
                PopUpController.Instance.QuickNoticeWindow($"{presetName} preset deleted!");
            }
            else
            {
                Debug.LogError($"No preset found to delete with name {presetName}");
                PopUpController.Instance.ErrorWindow($"Error deleting preset {presetName}. Check the Log for details.");
            }
        }
    }
}
