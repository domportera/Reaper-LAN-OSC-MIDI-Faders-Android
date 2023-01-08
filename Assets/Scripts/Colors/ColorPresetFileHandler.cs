using System;
using System.Collections.Generic;
using System.IO;
using DomsUnityHelper;
using PopUpWindows;
using UnityEngine;

namespace Colors
{
    public static class ColorPresetDataHandler
    {
        const string FileExtensionPresets = ".colorPreset";
        
        const string PresetFolder = "Colors";
        static readonly string PresetsBasePath = Path.Combine(Application.persistentDataPath, PresetFolder, "Presets");

        internal static string[] GetPresetNames()
        {
            if(!Directory.Exists(PresetsBasePath))
            {
                return Array.Empty<string>();
            }

            string[] fileNames = Directory.GetFiles(PresetsBasePath, "*" + FileExtensionPresets);

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
            }

            return fileNames;
        }

        static bool DoesPresetExist(string presetName)
        {
            string[] fileNames = GetPresetNames();

            foreach (string s in fileNames)
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
            ColorProfile preset = FileHandler.LoadJsonObject<ColorProfile>(PresetsBasePath, presetName, FileExtensionPresets);

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

            List<char> invalidChars = FileHandler.GetInvalidFileNameCharacters(presetName);
            if (invalidChars.Count > 0)
            {
                PopUpController.Instance.ErrorWindow(invalidChars.Count == 1
                    ? $"Chosen preset name contains an invalid character."
                    : $"Chosen preset name contains {invalidChars.Count.ToString()} invalid characters.");

                return;
            }

            bool saved = FileHandler.SaveJsonObject(profileToSave, PresetsBasePath, profileToSave.Name, FileExtensionPresets);

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
            string presetName = selector.Preset.Name;

            bool deleted = FileHandler.DeleteFile(PresetsBasePath, presetName, FileExtensionPresets);

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
